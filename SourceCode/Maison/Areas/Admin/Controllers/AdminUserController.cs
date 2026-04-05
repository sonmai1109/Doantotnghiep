using Maison.Models;

using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Maison.Areas.Admin.Controllers
{
    public class AdminUserController : BaseController
    {
        shopdb db = new shopdb();
        // GET: Admin/AdminUser
        public ActionResult Index(string searchString, int page = 1, int pageSize = 5)
        {
            TaiKhoanQuanTri ses = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
            if (ses.LoaiTaiKhoan == false)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.searchString = searchString;
            var taikhoans = db.TaiKhoanQuanTris.Select(tk => tk);
            if (!String.IsNullOrEmpty(searchString))
            {
                taikhoans = taikhoans.Where(tk => tk.TenDangNhap.Contains(searchString));
            }
            return View(taikhoans.OrderBy(tk => tk.ID).ToPagedList(page, pageSize));
        }

        [HttpPost]
        public JsonResult Create(TaiKhoanQuanTri tk)
        {
            try
            {
                TaiKhoanQuanTri check = db.TaiKhoanQuanTris.Where(a => a.TenDangNhap.Equals(tk.TenDangNhap)).FirstOrDefault();
                if (check != null)
                {
                    return Json(new { status = false, message = "Tên đăng nhập đã tồn tại!" });
                }
                else
                {
                    tk.TrangThai = true;
                    db.TaiKhoanQuanTris.Add(tk);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Thêm thành công!" });
                }
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Thêm không thành công!" });
            }
        }

        [HttpPost]
        public JsonResult Index(int id)
        {
            TaiKhoanQuanTri tk = db.TaiKhoanQuanTris.Where(a => a.ID.Equals(id)).FirstOrDefault();
            return Json(tk, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(TaiKhoanQuanTri tk)
        {
            TaiKhoanQuanTri login = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
            try
            {
                TaiKhoanQuanTri update = db.TaiKhoanQuanTris.Where(a => a.ID.Equals(tk.ID)).FirstOrDefault();
                if (update.TenDangNhap.Equals(login.TenDangNhap))
                {
                    return Json(new { status = false, message = "Bạn không thể sửa tài khoản này !" });
                }
                update.LoaiTaiKhoan = tk.LoaiTaiKhoan;
                update.TrangThai = tk.TrangThai;
                db.Entry(update).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Sửa thông tin thành công" });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Có lỗi gì đó. Thử lại sau !" });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            TaiKhoanQuanTri login = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
            try
            {
                TaiKhoanQuanTri tk = db.TaiKhoanQuanTris.Where(a => a.ID.Equals(id)).FirstOrDefault();
                if (tk.TenDangNhap.Equals(login.TenDangNhap))
                {
                    return Json(new { status = false, message = "Bạn không thể xóa tài khoản này !" });
                }
                db.TaiKhoanQuanTris.Remove(tk);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception)
            {
                return Json(new { status = false });
            }
        }
    }
}