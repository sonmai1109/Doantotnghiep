using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Maison.Models;

using PagedList;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace Maison.Areas.Admin.Controllers // Nhớ sửa lại namespace theo đúng thư mục của bạn
{
    public class BrandsController : BaseController
    {
        shopdb db = new shopdb();

        // HIỂN THỊ DANH SÁCH
        public ActionResult Index(string timkiem, int page = 1, int pagesize = 7)
        {
            ViewBag.timkiem = timkiem;
            var brands = db.Brands.Select(b => b);

            if (!string.IsNullOrEmpty(timkiem))
            {
                brands = brands.Where(b => b.TenBrand.Contains(timkiem));
            }

            return View(brands.OrderByDescending(b => b.MaBrand).ToPagedList(page, pagesize)); // Có thể dùng PagedList như danh mục
        }

        // THÊM MỚI
        [HttpPost]
        public JsonResult Create(Brand brand, HttpPostedFileBase ImageFile)
        {
            try
            {
                var check = db.Brands.FirstOrDefault(b => b.TenBrand.ToLower() == brand.TenBrand.ToLower());
                if (check != null) return Json(new { status = false, message = "Tên thương hiệu đã tồn tại!" });

                // XỬ LÝ UPLOAD ẢNH
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Tạo thư mục nếu chưa có
                    string dirPath = Server.MapPath("~/Content/Images/Brands/");
                    if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

                    // Đổi tên file thêm timestamp để tránh trùng lặp
                    string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                    string path = System.IO.Path.Combine(dirPath, fileName);

                    ImageFile.SaveAs(path);
                    brand.Logo = "/Content/Images/Brands/" + fileName; // Lưu đường dẫn vào DB
                }

                db.Brands.Add(brand);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        // LẤY DỮ LIỆU ĐỂ SỬA
        [HttpPost]
        public JsonResult Loaddata(int id)
        {
            db.Configuration.ProxyCreationEnabled = false; // Tắt proxy để tránh lỗi JSON
            Brand brand = db.Brands.FirstOrDefault(a => a.MaBrand == id);
            return Json(brand, JsonRequestBehavior.AllowGet);
        }

        // CẬP NHẬT
        [HttpPost]
        public JsonResult Update(Brand brand, HttpPostedFileBase ImageFile)
        {
            try
            {
                Brand doi = db.Brands.FirstOrDefault(a => a.MaBrand == brand.MaBrand);
                if (doi == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                doi.TenBrand = brand.TenBrand;
                doi.MoTa = brand.MoTa;

                // NẾU CÓ CHỌN ẢNH MỚI THÌ CẬP NHẬT, KHÔNG THÌ GIỮ NGUYÊN ẢNH CŨ
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string dirPath = Server.MapPath("~/Content/Images/Brands/");
                    if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

                    string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                    string path = System.IO.Path.Combine(dirPath, fileName);
                    ImageFile.SaveAs(path);
                    doi.Logo = "/Content/Images/Brands/" + fileName;
                }

                db.Entry(doi).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Sửa thành công!" });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi cập nhật!" });
            }
        }

        // XÓA
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Brand brand = db.Brands.FirstOrDefault(a => a.MaBrand == id);
                db.Brands.Remove(brand);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch
            {
                // Bắt lỗi nếu Thương hiệu này đã có Sản phẩm (Khóa ngoại)
                return Json(new { status = false, message = "Thương hiệu này đang có sản phẩm, không thể xóa!" });
            }
        }
    }
}