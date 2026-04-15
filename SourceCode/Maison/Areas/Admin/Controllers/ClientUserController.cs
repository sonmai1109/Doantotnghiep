using Maison.Models;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Maison.Areas.Admin.Controllers
{
    public class ClientUserController : BaseController // Nhớ đổi thành BaseController nếu bạn có dùng
    {
        shopdb db = new shopdb();
        
        // GET: Admin/ClientUser
        [HttpGet]
        public ActionResult Index(string searchString, int page = 1, int pageSize = 10)
        {
            ViewBag.searchString = searchString;
            var taikhoans = db.TaiKhoanNguoiDungs.AsQueryable();
            
            if (!String.IsNullOrEmpty(searchString))
            {
                taikhoans = taikhoans.Where(tk => tk.TenDangNhap.Contains(searchString) || tk.SoDienThoai.Contains(searchString));
            }
            // Sắp xếp tài khoản mới nhất lên đầu
            return View(taikhoans.OrderByDescending(tk => tk.MaTK).ToPagedList(page, pageSize));
        }

        // ĐỔI TRẠNG THÁI (Kích hoạt <-> Vô hiệu hóa)
        [HttpPost]
        public JsonResult Update(int Matk)
        {
            try
            {
                TaiKhoanNguoiDung update = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.MaTK == Matk);
                if (update == null) return Json(new { status = false, message = "Không tìm thấy tài khoản" });

                update.TrangThai = !update.TrangThai; // Đảo ngược trạng thái
                db.Entry(update).State = EntityState.Modified;
                db.SaveChanges();
                
                string msg = update.TrangThai ? "Đã MỞ KHÓA tài khoản!" : "Đã VÔ HIỆU tài khoản thành công!";
                return Json(new { status = true, message = msg });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi hệ thống. Thử lại sau!" });
            }
        }

        // XÓA TÀI KHOẢN (Cẩn thận lỗi khóa ngoại)
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                TaiKhoanNguoiDung tk = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.MaTK == id);
                if (tk == null) return Json(new { status = false, message = "Không tìm thấy tài khoản" });

                db.TaiKhoanNguoiDungs.Remove(tk);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception)
            {
                // Bắt lỗi nếu tài khoản này đã từng mua hàng (dính khóa ngoại Hóa Đơn)
                return Json(new { status = false, message = "Không thể xóa! Khách hàng này đã có Dữ liệu Đơn hàng/Bảo hành trên hệ thống. Vui lòng chọn 'Vô hiệu hóa' thay vì Xóa." });
            }
        }
    }
}