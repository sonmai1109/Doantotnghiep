using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using PagedList;
using System.Data.Entity;

namespace Maison.Areas.Admin.Controllers
{
    public class TinTucsController : BaseController
    {
        private shopdb db = new shopdb();

        // 1. DANH SÁCH TIN TỨC
        public ActionResult Index(string q, int page = 1, int pageSize = 10)
        {
            var list = db.TinTucs.AsQueryable();
            if (!string.IsNullOrEmpty(q))
            {
                list = list.Where(x => x.TieuDe.Contains(q));
            }
            ViewBag.q = q;
            return View(list.OrderByDescending(x => x.NgayDang).ToPagedList(page, pageSize));
        }

        // 2. FORM THÊM MỚI (CHỈ DUY NHẤT 1 HÀM GET VÀ 1 HÀM POST Ở ĐÂY)
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TinTuc model, HttpPostedFileBase ImageUpload)
        {
            try
            {
                // XỬ LÝ UPLOAD ẢNH ĐẠI DIỆN
                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    string fileName = "news_" + DateTime.Now.Ticks.ToString() + System.IO.Path.GetExtension(ImageUpload.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Images/News/"), fileName);

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Content/Images/News/")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/Images/News/"));
                    }

                    ImageUpload.SaveAs(path);
                    model.AnhDaiDien = "/Content/Images/News/" + fileName;
                }
                else
                {
                    ModelState.AddModelError("ImageUpload", "Vui lòng chọn ảnh đại diện cho bài viết!");
                    return View(model);
                }

                // LƯU TIN TỨC VÀO DB
                var admin = (TaiKhoanQuanTri)Session[ConstaintUser.ADMIN_SESSION];
                model.NgayDang = DateTime.Now;
                model.MaTK = admin.ID;
                model.LuotXem = 0;
                model.TrangThai = 1;

                db.TinTucs.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }

        // 3. FORM CHỈNH SỬA
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tin = db.TinTucs.Find(id);
            if (tin == null) return HttpNotFound();
            return View(tin);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(TinTuc model, HttpPostedFileBase ImageUpload)
        {
            try
            {
                var tin = db.TinTucs.Find(model.MaTin);
                if (tin == null) return HttpNotFound();

                // Nếu có chọn ảnh mới thì Upload và đè lên Link cũ
                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    string fileName = "news_" + DateTime.Now.Ticks.ToString() + System.IO.Path.GetExtension(ImageUpload.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Images/News/"), fileName);

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Content/Images/News/")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/Images/News/"));
                    }

                    ImageUpload.SaveAs(path);
                    tin.AnhDaiDien = "/Content/Images/News/" + fileName;
                }

                // Cập nhật các thông tin Text
                tin.TieuDe = model.TieuDe;
                tin.TomTat = model.TomTat;
                tin.NoiDung = model.NoiDung;
                tin.TrangThai = model.TrangThai;

                db.Entry(tin).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }

        // 4. XÓA BÀI VIẾT
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var tin = db.TinTucs.Find(id);
            if (tin != null)
            {
                db.TinTucs.Remove(tin);
                db.SaveChanges();
                return Json(new { status = true });
            }
            return Json(new { status = false });
        }
    }
}