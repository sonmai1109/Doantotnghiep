using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using Maison.Areas.Admin.Data;
namespace Maison.Controllers
{
    public class HomeController : Controller
    {
        shopdb db = new shopdb();
        public ActionResult Index()
        {
            // 1. Sản phẩm khuyến mãi
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais
                .Include(s => s.Sanpham)
                // Lệnh Include cực kỳ quan trọng để View móc được tên RAM, CPU ra nối chuỗi:
                .Include(s => s.Sanpham.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .Include(s => s.KhuyenMai)
                .Where(s => s.KhuyenMai.TrangThai == 1
                            && s.KhuyenMai.NgayBatDau <= DateTime.Now
                            && s.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .ToList();

            // 2. Sản phẩm mới
            var sanPhamMoi = db.Sanphams
                .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .OrderByDescending(p => p.NgayTao)
                .Take(10)
                .ToList();

            // 3. Giá tốt
            var giaTot = db.Sanphams
                .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .Where(p => p.BienThes.Any())
                .OrderBy(p => p.BienThes.Min(b => b.GiaBan))
                .Take(10)
                .ToList();

            ViewBag.SanPhamKhuyenMai = sanPhamKhuyenMai;
            ViewBag.SanPhamMoi = sanPhamMoi;
            ViewBag.GiaTot = giaTot;

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
            TaiKhoanNguoiDung session = (TaiKhoanNguoiDung)Session[Maison.Session.ConstaintUser.USER_SESSION];
            if (session != null)
            {
                return RedirectToAction("error", "Error");

            }
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
                //ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại !");
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
                    Session[Maison.Session.ConstaintUser.USER_SESSION] = session;
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
        public ActionResult Login()
        {
            TaiKhoanNguoiDung session = (TaiKhoanNguoiDung)Session[Maison.Session.ConstaintUser.USER_SESSION];
            if (session != null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginAccount loginAccount)
        {
            if (ModelState.IsValid)
            {
                TaiKhoanNguoiDung tk = db.TaiKhoanNguoiDungs.Where
                (a => a.TenDangNhap.Equals(loginAccount.username) && a.MatKhau.Equals(loginAccount.password)).FirstOrDefault();
                if (tk != null)
                {
                    if (tk.TrangThai == false)
                    {
                        ModelState.AddModelError("ErrorLogin", "Tài khoản của bạn đã bị vô hiệu hóa !");
                    }
                    else
                    {
                        Session.Add(ConstaintUser.USER_SESSION, tk);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("ErrorLogin", "Tài khoản hoặc mật khẩu không đúng!");
                }
            }
            return View(loginAccount);
        }
        [HttpGet]
        public ActionResult Logout()
        {

            Session.Remove(ConstaintUser.USER_SESSION);
            return RedirectToAction("Index", "Home");

        }
        public ActionResult test()
        {

            var sp = db.Danhmucs.ToList();
            return PartialView(sp);


        }
        [ChildActionOnly]
        public ActionResult CartCount()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[Maison.Session.ConstaintUser.USER_SESSION];
            int count = 0;

            if (tk != null)
            {
                // Nếu đã đăng nhập, đếm số lượng các cấu hình khác nhau nằm trong giỏ
                count = db.GioHangs.Where(g => g.MaTK == tk.MaTk).Count();
            }

            return PartialView(count);
        }

    }
}