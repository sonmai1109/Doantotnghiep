using Maison.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;

namespace Maison.Controllers
{
    public class DanhGiaController : Controller
    {
        private shopdb db = new shopdb();

        // 1. Partial: Danh sách đánh giá (Chỉ hiện trạng thái = 1 là đã duyệt)
        // 1. Partial: Danh sách đánh giá 
        public PartialViewResult DanhSachTheoSP(int maSP)
        {
            var ds = db.DanhGias
                       .Where(d => d.BienThe.MaSP == maSP && d.TrangThai == 1)
                       .OrderByDescending(d => d.NgayTao)
                       .Include(d => d.TaiKhoanNguoiDung)
                       // NẠP SÂU VÀO ĐỂ LẤY THÔNG SỐ (BẮT BUỘC PHẢI CÓ DÒNG NÀY)
                       .Include(d => d.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                       .ToList();
            return PartialView("_DanhSachDanhGia", ds);
        }

        // 2. POST: Thêm đánh giá (Sửa maSP thành maBT)
        [HttpPost]
        public JsonResult ThemDanhGia(int maBT, int xepHang, string binhLuan, HttpPostedFileBase imageFile)
        {
            try
            {
                var tk = (TaiKhoanNguoiDung)Session[Maison.Session.ConstaintUser.USER_SESSION];
                if (tk == null)
                {
                    return Json(new { success = false, message = "Bạn phải đăng nhập mới có thể đánh giá!" });
                }

                // Xử lý lưu ảnh nếu khách có đính kèm
                string duongDanAnh = null;
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string uploadDir = Server.MapPath("~/Content/Images/Reviews/");
                    if (!System.IO.Directory.Exists(uploadDir)) System.IO.Directory.CreateDirectory(uploadDir);

                    string fileName = DateTime.Now.Ticks.ToString() + "_" + System.IO.Path.GetFileName(imageFile.FileName);
                    string filePath = System.IO.Path.Combine(uploadDir, fileName);
                    imageFile.SaveAs(filePath);

                    duongDanAnh = "/Content/Images/Reviews/" + fileName;
                }

                DanhGia dg = new DanhGia
                {
                    MaBT = maBT, // <--- SỬA THÀNH MaBT Ở ĐÂY
                    MaTK = tk.MaTK,
                    XepHang = xepHang,
                    BinhLuan = binhLuan,
                    NgayTao = DateTime.Now,
                    TrangThai = 1,
                    HinhAnh = duongDanAnh
                };

                db.DanhGias.Add(dg);
                db.SaveChanges();

                return Json(new { success = true, message = "Cảm ơn bạn! Đánh giá đã được ghi nhận." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
