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
                    hd.PhuongThucThanhToan = string.IsNullOrEmpty(PhuongThuc) ? "COD" : PhuongThuc;
                    hd.TrangThaiThanhToan = 0; // 0 = Chưa thanh toán
                    db.HoaDons.Add(hd);
                    db.SaveChanges(); // Lấy MaHD phát sinh tự động

                    // 2. Lấy giỏ hàng từ Database
                    var cartItems = db.GioHangs
                        .Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                        .Where(g => g.MaTK == tk.MaTK).ToList();

                    if (cartItems.Count == 0) return Json(new { status = false, message = "Giỏ hàng trống!" });

                    // 3. Đổ dữ liệu từ Giỏ Hàng sang Chi Tiết Hóa Đơn
                    foreach (var item in cartItems)
                    {
                        var bt = item.BienThe;

                        // ========================================================
                        // TÍNH TOÁN GIÁ & TRỪ SUẤT FLASH SALE (NẾU CÓ)
                        // ========================================================
                        var activeKMs = bt.Sanpham.SanPhamKhuyenMais
                            .Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now)
                            .ToList();

                        var kmApDung = activeKMs.Where(k => k.MaBT == null || k.MaBT == bt.MaBT).ToList();

                        // Lọc ra các KM Flash Sale mà vẫn CÒN SUẤT
                        // ... (code trên giữ nguyên) ...

                        // TÌM FLASH SALE BẰNG LUẬT MỚI (PHẢI ĐỦ SUẤT MỚI ĐƯỢC ÁP DỤNG)
                        var kmConSuat = kmApDung.Where(k =>
                            k.SoLuongKhuyenMai == null ||
                            (k.SoLuongKhuyenMai.Value - k.SoLuongDaBan) >= item.SoLuong
                        ).ToList();

                        int phanTram = 0;
                        SanPhamKhuyenMai flashSaleDangApDung = null;

                        if (kmConSuat.Any())
                        {
                            phanTram = kmConSuat.Max(k => k.PhanTramGiam);
                            flashSaleDangApDung = kmConSuat.FirstOrDefault(k => k.PhanTramGiam == phanTram);
                        }

                        decimal giaChot = phanTram > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTram / 100), 0) : bt.GiaBan;

                        ChiTietHoaDon cthd = new ChiTietHoaDon
                        {
                            MaHD = hd.MaHD,
                            MaBT = bt.MaBT,
                            SoLuongMua = item.SoLuong,
                            GiaMua = giaChot
                        };
                        db.ChiTietHoaDons.Add(cthd);

                        // Trừ số lượng tồn kho (Kho tổng)
                        bt.SoLuongTon -= item.SoLuong;
                        if (bt.SoLuongTon < 0)
                            throw new Exception($"Sản phẩm {bt.Sanpham.TenSP} không đủ số lượng tồn kho!");

                        // CỘNG VÀO SỐ LƯỢNG ĐÃ BÁN CỦA FLASH SALE (VỚI CHỐT CHẶN AN TOÀN)
                        if (flashSaleDangApDung != null && flashSaleDangApDung.SoLuongKhuyenMai != null)
                        {
                            // Safety Check lần cuối trước khi ghi DB
                            if (flashSaleDangApDung.SoLuongDaBan + item.SoLuong > flashSaleDangApDung.SoLuongKhuyenMai.Value)
                            {
                                throw new Exception($"Sản phẩm {bt.Sanpham.TenSP} đã hết Flash Sale. Vui lòng quay lại giỏ hàng để cập nhật!");
                            }

                            flashSaleDangApDung.SoLuongDaBan += item.SoLuong;
                            db.Entry(flashSaleDangApDung).State = EntityState.Modified;
                        }

                        db.GioHangs.Remove(item);
                    }

                    // ... (Code dưới giữ nguyên) ...

                    db.SaveChanges();
                    transaction.Commit(); // Hoàn tất giao dịch
                    return Json(new { status = true, billid = hd.MaHD });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    string errorMsg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMsg = ex.InnerException.Message;
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMsg = ex.InnerException.InnerException.Message;
                        }
                    }
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
                // Hoàn lại số lượng tồn kho và suất Flash Sale
                if (stt == 0) // Trạng thái 0 là Hủy
                {
                    var chiTietHDs = db.ChiTietHoaDons.Where(c => c.MaHD == mahd).ToList();
                    foreach (var ct in chiTietHDs)
                    {
                        // Trả lại kho tổng
                        var bt = db.BienThes.FirstOrDefault(b => b.MaBT == ct.MaBT);
                        if (bt != null)
                        {
                            bt.SoLuongTon += ct.SoLuongMua;
                        }

                        // THÊM: Trả lại suất Flash Sale (Tìm theo giá mua để biết nó đã từng hưởng KM nào)
                        var activeKMs = db.SanPhamKhuyenMais.Where(k => k.MaBT == ct.MaBT || (k.MaSP == ct.BienThe.MaSP && k.MaBT == null)).ToList();

                        // Quét các khuyến mãi có giới hạn số lượng, nếu tìm thấy thì trả lại số suất đã bán
                        foreach (var km in activeKMs.Where(x => x.SoLuongKhuyenMai != null && x.SoLuongDaBan > 0))
                        {
                            // Kiểm tra xem giá lúc mua có khớp với % giảm của KM này không
                            decimal giaSauKhiGiamCuaKM = Math.Round(ct.BienThe.GiaBan * (1 - (decimal)km.PhanTramGiam / 100), 0);
                            if (ct.GiaMua == giaSauKhiGiamCuaKM)
                            {
                                km.SoLuongDaBan -= ct.SoLuongMua; // Trả lại số lượng
                                if (km.SoLuongDaBan < 0) km.SoLuongDaBan = 0; // Chống lỗi âm
                                db.Entry(km).State = EntityState.Modified;
                            }
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