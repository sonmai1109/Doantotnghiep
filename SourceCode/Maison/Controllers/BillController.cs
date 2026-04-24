using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using System.Data.Entity;
using System.IO;
using System.Text.RegularExpressions;

namespace Maison.Controllers
{
    public class BillController : Controller
    {
        shopdb db = new shopdb();

        // 1. DANH SÁCH ĐƠN HÀNG CỦA KHÁCH
        [HttpGet]
        public ActionResult ListBills()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            var list = db.HoaDons.Where(p => p.MaTK == tk.MaTK).OrderByDescending(x => x.NgayDat).ToList();
            return View(list);
        }
        // Đổi thành HttpPost để chống lưu cache
        [HttpPost]
        public JsonResult CheckPaymentStatus(int maHD)
        {
            // Thêm AsNoTracking() để lấy dữ liệu tươi nhất trực tiếp từ SQL Server
            var hd = db.HoaDons.AsNoTracking().FirstOrDefault(x => x.MaHD == maHD);

            if (hd != null && hd.TrangThaiThanhToan == 1)
            {
                return Json(new { isPaid = true });
            }
            return Json(new { isPaid = false });
        }

        // 2. CHI TIẾT HÓA ĐƠN
        [HttpGet]
        public ActionResult Details(int id)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            // Include sâu vào các bảng công nghệ: ChiTietHoaDon -> BienThe -> Sanpham & ChiTietBT
            var hd = db.HoaDons
                .Include(x => x.TaiKhoanNguoiDung)
                .Include(x => x.ChiTietHoaDons.Select(c => c.BienThe.Sanpham))
                .Include(x => x.ChiTietHoaDons.Select(c => c.BienThe.ChiTietBTs.Select(ct => ct.GiaTriTT.ThuocTinh)))
                .FirstOrDefault(x => x.MaHD == id && x.MaTK == tk.MaTK);

            if (hd == null) return RedirectToAction("PageNotFound", "Error");

            return View(hd);
        }

        // 3. TẠO ĐƠN HÀNG MỚI (TỪ NÚT XÁC NHẬN ĐẶT HÀNG)
        [HttpPost]
        public JsonResult CreateBill(HoaDon hd, string PhuongThuc)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, message = "Vui lòng đăng nhập!" });

            // Dùng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Lưu Hóa Đơn
                    hd.MaTK = tk.MaTK;
                    hd.NgayDat = DateTime.Now;
                    hd.TrangThai = 1; // 1: Đang chuẩn bị / Chờ duyệt
                   // Cần SaveChanges để lấy MaHD phát sinh tự động
                    hd.PhuongThucThanhToan = string.IsNullOrEmpty(PhuongThuc) ? "COD" : PhuongThuc;
                    hd.TrangThaiThanhToan = 0; // 0 = Chưa thanh toán
                    db.HoaDons.Add(hd);
                    db.SaveChanges();
                    // 2. Lấy giỏ hàng từ Database (kèm Khuyến mãi để chốt giá cuối cùng)
                    var cartItems = db.GioHangs
                        .Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                        .Where(g => g.MaTK == tk.MaTK).ToList();

                    if (cartItems.Count == 0) return Json(new { status = false, message = "Giỏ hàng trống!" });

                    // 3. Đổ dữ liệu từ Giỏ Hàng sang Chi Tiết Hóa Đơn
                    foreach (var item in cartItems)
                    {
                        var bt = item.BienThe;

                        // Tính lại giá có khuyến mãi tại thời điểm đặt hàng
                        var activeKMs = bt.Sanpham.SanPhamKhuyenMais.Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now).ToList();
                        var kmRieng = activeKMs.FirstOrDefault(k => k.MaBT == bt.MaBT);
                        var kmChung = activeKMs.FirstOrDefault(k => k.MaBT == null);
                        int phanTram = kmRieng?.PhanTramGiam ?? kmChung?.PhanTramGiam ?? 0;
                        decimal giaChot = phanTram > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTram / 100), 0) : bt.GiaBan;

                        ChiTietHoaDon cthd = new ChiTietHoaDon
                        {
                            MaHD = hd.MaHD,
                            MaBT = bt.MaBT,
                            SoLuongMua = item.SoLuong,
                            GiaMua = giaChot
                        };
                        db.ChiTietHoaDons.Add(cthd);

                        // 4. Trừ số lượng tồn kho
                        bt.SoLuongTon -= item.SoLuong;
                        if (bt.SoLuongTon < 0)
                            throw new Exception($"Sản phẩm {bt.Sanpham.TenSP} không đủ số lượng tồn kho!");

                        // 5. Xóa sản phẩm khỏi giỏ hàng
                        db.GioHangs.Remove(item);
                    }

                    db.SaveChanges();
                    transaction.Commit(); // Hoàn tất giao dịch
                    return Json(new { status = true, billid = hd.MaHD });
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Nếu có lỗi, quay lại trạng thái ban đầu, không lưu gì cả

                    // -- THÊM ĐOẠN NÀY ĐỂ BỚI MÓC LỖI THỰC SỰ ẨN BÊN TRONG --
                    string errorMsg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMsg = ex.InnerException.Message;
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMsg = ex.InnerException.InnerException.Message;
                        }
                    }
                    // --------------------------------------------------------

                    return Json(new { status = false, message = "Lỗi hệ thống: " + errorMsg });
                }
            }
        }

        // 4. HỦY ĐƠN HÀNG
        // 4. HỦY ĐƠN HÀNG (GIAO DIỆN KHÁCH)
        [HttpPost]
        public JsonResult ChangeStatus(int mahd, int stt)
        {
            try
            {
                TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];

                // Lấy hóa đơn của chính khách hàng này
                HoaDon hd = db.HoaDons.FirstOrDefault(x => x.MaHD == mahd && x.MaTK == tk.MaTK);

                if (hd == null || hd.TrangThai != 1) // Chỉ cho phép hủy khi đang chờ xác nhận (1)
                {
                    return Json(new { status = false, message = "Không thể hủy đơn hàng này!" });
                }

                // Cập nhật trạng thái
                hd.TrangThai = stt; // stt = 0 (Hủy)
                hd.NguoiSua = tk.HoTen; // Ghi nhận khách hàng là người tự hủy
                hd.NgaySua = DateTime.Now;

                // Hoàn lại số lượng tồn kho (Truy vấn trực tiếp để đảm bảo EF tracking)
                if (stt == 0)
                {
                    var chiTietHDs = db.ChiTietHoaDons.Where(c => c.MaHD == mahd).ToList();
                    foreach (var ct in chiTietHDs)
                    {
                        var bt = db.BienThes.FirstOrDefault(b => b.MaBT == ct.MaBT);
                        if (bt != null)
                        {
                            bt.SoLuongTon += ct.SoLuongMua;
                        }
                    }
                }

                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi khi hủy đơn: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SwitchToCOD(int maHD)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, message = "Lỗi đăng nhập" });

            var hd = db.HoaDons.FirstOrDefault(x => x.MaHD == maHD && x.MaTK == tk.MaTK);
            if (hd != null)
            {
                // ÉP CỨNG TRẠNG THÁI LÀ COD 
                hd.PhuongThucThanhToan = "COD";
                hd.GhiChu = hd.GhiChu + " [Khách đã báo đổi sang thanh toán COD (Tiền mặt)]";

                db.SaveChanges();
                return Json(new { status = true });
            }
            return Json(new { status = false, message = "Không tìm thấy đơn hàng" });
        }
        // 5. TRANG THANH TOÁN QR ĐỘNG
        [HttpGet]
        public ActionResult PaymentQR(int id)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            var hd = db.HoaDons
                .Include(x => x.TaiKhoanNguoiDung)
                .Include(x => x.ChiTietHoaDons.Select(c => c.BienThe.Sanpham))
                .FirstOrDefault(x => x.MaHD == id && x.MaTK == tk.MaTK);

            if (hd == null) return RedirectToAction("PageNotFound", "Error");

            // Lấy tổng tiền để render QR
            decimal tongTienDonHang = db.ChiTietHoaDons.Where(c => c.MaHD == id).Sum(c => c.GiaMua * c.SoLuongMua);
            ViewBag.TongTien = tongTienDonHang;

            return View(hd);
        }
    }
}