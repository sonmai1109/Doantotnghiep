using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using System.Data.Entity;

namespace Maison.Areas.Admin.Controllers // Đổi thành namespace của bạn
{
    public class ThuocTinhsController : BaseController
    {
        shopdb db = new shopdb();

        public ActionResult Index(string timkiem)
        {
            ViewBag.timkiem = timkiem;
            // Dùng Include để lấy được Tên Danh Mục thay vì chỉ lấy cái ID
            var thuoctinhs = db.ThuocTinhs.Include(t => t.DanhMuc).AsQueryable();

            if (!string.IsNullOrEmpty(timkiem))
            {
                thuoctinhs = thuoctinhs.Where(t => t.TenTT.Contains(timkiem));
            }

            // Lấy danh sách Danh mục gửi sang View để làm thẻ <select>
            ViewBag.MaDM = new SelectList(db.Danhmucs, "MaDM", "TenDM");

            return View(thuoctinhs.OrderByDescending(t => t.MaTT).ToList());
        }

        [HttpPost]
        public JsonResult Create(ThuocTinh tt)
        {
            try
            {
                // Tránh việc cùng 1 danh mục mà tạo 2 thuộc tính trùng tên (VD: Laptop có 2 cái "RAM")
                var check = db.ThuocTinhs.FirstOrDefault(x => x.TenTT.ToLower() == tt.TenTT.ToLower() && x.MaDM == tt.MaDM);
                if (check != null) return Json(new { status = false, message = "Thuộc tính này đã tồn tại trong danh mục!" });

                db.ThuocTinhs.Add(tt);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Loaddata(int id)
        {
            db.Configuration.ProxyCreationEnabled = false; // Chống lỗi vòng lặp JSON
            var tt = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == id);
            return Json(tt, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(ThuocTinh tt)
        {
            try
            {
                var doi = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == tt.MaTT);
                if (doi == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                doi.TenTT = tt.TenTT;
                doi.MaDM = tt.MaDM; // Cập nhật danh mục mới (nếu đổi)

                // --- THÊM DÒNG NÀY ĐỂ LƯU LOẠI THUỘC TÍNH CHÍNH/PHỤ ---
                doi.LaThuocTinhChinh = tt.LaThuocTinhChinh;

                db.Entry(doi).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Cập nhật thành công!" });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi cập nhật!" });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var tt = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == id);
                db.ThuocTinhs.Remove(tt);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch
            {
                return Json(new { status = false, message = "Không thể xóa do thuộc tính này đang được sử dụng!" });
            }
        }
    }
}