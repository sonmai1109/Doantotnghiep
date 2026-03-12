using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
namespace Maison.Controllers
{
    public class HomeController : Controller
    {
        shopdb db = new shopdb();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult dropdanhmuc()
        {
            IEnumerable<Danhmuc> danhmucs = db.Danhmucs.Select(p => p);
            return PartialView(danhmucs);
        }
        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(TaiKhoanNguoiDung tk)// gọi 1 tài khoản người dùng sẽ nhận thông tin từ view 
        {
            // kiểm tra validation trước
            if (!ModelState.IsValid)
            {
                return View(tk);
            }
            //check xem nó đã có trong database chưa 
            TaiKhoanNguoiDung check = db.TaiKhoanNguoiDungs.Where(a => a.TenDangNhap.Equals(tk.TenDangNhap)).FirstOrDefault();
            if (check != null)
            {
                //nếu chưa báo lỗi 
                ModelState.AddModelError("ERRORSIGNUP", "Tên đăng nhập đã tồn tại !");
                ViewBag.mess= "Tên đăng nhập đã tồn tại";
                 return View(tk);
            }
            else
            {
                try
                {
                    tk.TrangThai = true;
                    db.TaiKhoanNguoiDungs.Add(tk);
                    db.SaveChanges();
                    TaiKhoanNguoiDung session = db.TaiKhoanNguoiDungs.Where(a => a.TenDangNhap.Equals(tk.TenDangNhap)).FirstOrDefault();
                    Session[Maison.Session.SSUser.SS_USER] = session;
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("ErrorSignUp", "Đăng ký không thành công. Thử lại sau !");
                }
            }
            return View(tk);
        }
        [HttpGet]
        public ActionResult test()
        {
            var a = db.TaiKhoanNguoiDungs.Select(tk => tk);
            return View(a);


        }
    }
}