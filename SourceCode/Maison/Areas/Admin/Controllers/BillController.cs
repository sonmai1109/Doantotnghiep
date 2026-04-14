using Maison.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;

namespace Maison.Areas.Admin.Controllers
{
    public class BillController : BaseController // Đổi thành BaseController nếu bạn đang dùng bảo mật đăng nhập
    {
        shopdb db = new shopdb();

        [HttpGet]
        public ActionResult Index(DateTime? searchString, int? status, int page = 1, int pageSize = 10)
        {
            var query = db.HoaDons.Include(h => h.TaiKhoanNguoiDung).AsQueryable();

            if (status != null)
            {
                query = query.Where(x => x.TrangThai == status);
                ViewBag.Status = status;
            }
            if (searchString != null)
            {
                ViewBag.searchString = searchString.Value.ToString("yyyy-MM-dd");
                string search = searchString.Value.ToString("dd/MM/yyyy");
                query = query.Where(hd => hd.NgayDat.ToString().Contains(search));
            }

            return View(query.OrderByDescending(hd => hd.NgayDat).ToPagedList(page, pageSize));
        }

        [HttpPost]
        public JsonResult GetDetails(int id)
        {
            try
            {
                var hoaDon = db.HoaDons
                    .Include(h => h.TaiKhoanNguoiDung)
                    .FirstOrDefault(h => h.MaHD == id);

                if (hoaDon == null) return Json(new { error = "Không tìm thấy hóa đơn" });

                var chiTietHD = db.ChiTietHoaDons
                    .Include(c => c.BienThe)
                    .Include(c => c.BienThe.Sanpham)
                    .Include(c => c.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT.ThuocTinh))
                    .Where(c => c.MaHD == id)
                    .ToList();

                var result = new
                {
                    hoadon = new
                    {
                        hoaDon.MaHD,
                        hoaDon.HoTenNguoiNhan,
                        hoaDon.SoDienThoaiNhan,
                        hoaDon.DiaChiNhan,
                        hoaDon.TrangThai,
                        // Convert thẳng sang String đẹp từ Backend, JS không cần cắt Regex nữa
                        NgayDat = hoaDon.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                        NgaySua = hoaDon.NgaySua?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa có",
                        NguoiSua = hoaDon.NguoiSua ?? "Hệ thống",
                        GhiChu = string.IsNullOrEmpty(hoaDon.GhiChu) ? "Không có" : hoaDon.GhiChu,
                        TaiKhoanNguoiDung = new { hoaDon.TaiKhoanNguoiDung.HoTen }
                    },
                    cthd = chiTietHD.Select(c => {
                        var bt = c.BienThe;
                        string cauHinh = bt.ChiTietBTs.Any() ? string.Join(" | ", bt.ChiTietBTs.Select(cb => cb.GiaTriTT.GiaTri)) : "Bản tiêu chuẩn";
                        string anhSP = string.IsNullOrEmpty(bt.HinhAnh) ? bt.Sanpham.HinhAnh : bt.HinhAnh;

                        return new
                        {
                            TenSP = bt.Sanpham.TenSP,
                            HinhAnh = anhSP,
                            CauHinh = cauHinh,
                            GiaMua = c.GiaMua,
                            SoLuongMua = c.SoLuongMua,
                            ThanhTien = c.GiaMua * c.SoLuongMua
                        };
                    })
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ChangeStatus(int mahd, int stt)
        {
            try
            {
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                HoaDon hd = db.HoaDons.FirstOrDefault(x => x.MaHD == mahd);
                if (hd == null) return Json(new { status = false, message = "Không tìm thấy hóa đơn" });

                int trangThaiCu = hd.TrangThai;

                // 1. CHẶN ĐỨNG VIỆC KHÔI PHỤC ĐƠN ĐÃ HỦY
                if (trangThaiCu == 0)
                {
                    return Json(new { status = false, message = "Đơn hàng đã hủy thì không thể khôi phục. Vui lòng yêu cầu khách đặt đơn mới!" });
                }

                // 2. NẾU ADMIN CHỌN HỦY ĐƠN (0) -> CỘNG LẠI TỒN KHO
                if (stt == 0)
                {
                    var chiTietHDs = db.ChiTietHoaDons.Where(x => x.MaHD == mahd).ToList();
                    foreach (var cthd in chiTietHDs)
                    {
                        var bt = db.BienThes.Find(cthd.MaBT);
                        if (bt != null) bt.SoLuongTon += cthd.SoLuongMua;
                    }
                }

                // 3. Cập nhật trạng thái mới
                hd.TrangThai = stt;
                hd.NguoiSua = tk.HoTen; // Sửa thành Session Admin nếu có
                hd.NgaySua = DateTime.Now;

                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
}