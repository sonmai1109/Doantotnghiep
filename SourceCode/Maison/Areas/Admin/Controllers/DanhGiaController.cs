using Maison.Models;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Maison.Areas.Admin.Controllers
{
    public class DanhGiaController : BaseController
    {
        shopdb db = new shopdb();

        // 1. DANH SÁCH ĐÁNH GIÁ
        [HttpGet]
        public ActionResult Index(string searchString, int page = 1, int pageSize = 10)
        {
            ViewBag.searchString = searchString;

            // Lấy danh sách đánh giá kèm thông tin Khách hàng và Sản phẩm
            var danhGias = db.DanhGias
                .Include(d => d.TaiKhoanNguoiDung)
                .Include(d => d.BienThe.Sanpham)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm kiếm theo tên khách hàng hoặc tên sản phẩm
                danhGias = danhGias.Where(d => d.TaiKhoanNguoiDung.HoTen.Contains(searchString)
                                            || d.BienThe.Sanpham.TenSP.Contains(searchString));
            }

            return View(danhGias.OrderByDescending(d => d.NgayTao).ToPagedList(page, pageSize));
        }

        // 2. ẨN / HIỆN ĐÁNH GIÁ (KIỂM DUYỆT)
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var dg = db.DanhGias.FirstOrDefault(x => x.MaDanhGia == id);
                if (dg == null) return Json(new { status = false, message = "Không tìm thấy đánh giá!" });

                // Đảo ngược trạng thái (1: Hiện, 0: Ẩn)
                dg.TrangThai = dg.TrangThai == 1 ? 0 : 1;
                db.SaveChanges();

                string msg = dg.TrangThai == 1 ? "Đã duyệt và hiển thị đánh giá lên Web!" : "Đã ẩn đánh giá này khỏi Web!";
                return Json(new { status = true, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 3. ADMIN GỬI PHẢN HỒI
        [HttpPost]
        public JsonResult ReplyReview(int id, string replyText)
        {
            try
            {
                var dg = db.DanhGias.FirstOrDefault(x => x.MaDanhGia == id);
                if (dg == null) return Json(new { status = false, message = "Không tìm thấy đánh giá!" });

                dg.PhanHoiCuaAdmin = replyText;
                db.SaveChanges();

                return Json(new { status = true, message = "Đã gửi phản hồi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 4. XÓA ĐÁNH GIÁ
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var dg = db.DanhGias.FirstOrDefault(x => x.MaDanhGia == id);
                if (dg == null) return Json(new { status = false, message = "Không tìm thấy!" });

                db.DanhGias.Remove(dg);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi không thể xóa!" });
            }
        }
    }
}