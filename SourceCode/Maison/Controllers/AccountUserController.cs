using System;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using System.Text.RegularExpressions;

namespace Maison.Controllers
{
    public class AccountUserController : Controller
    {
        shopdb db = new shopdb(); // Sử dụng đúng db context của bạn

        // 1. ĐỔI MẬT KHẨU
        [HttpGet]
        public ActionResult ChangePassWord()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");
            
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassWord(string oldpassword, string password)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            var currentTk = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.MaTK == tk.MaTK);

            if (currentTk == null || currentTk.MatKhau != oldpassword)
            {
                ModelState.AddModelError("ErrorUpdate", "Mật khẩu cũ không chính xác!");
            }
            else
            {
                // THÊM: Kiểm tra độ mạnh của mật khẩu mới
                var regexMk = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
                if (!regexMk.IsMatch(password))
                {
                    ModelState.AddModelError("ErrorUpdate", "Mật khẩu mới phải từ 6 ký tự, gồm chữ, số và ký tự đặc biệt (@, $, !, %, *, ?, &)");
                    return View();
                }

                currentTk.MatKhau = password;
                db.SaveChanges();

                Session[ConstaintUser.USER_SESSION] = currentTk;
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("ChangePassWord");
            }
            return View();
        }

        // 2. CHỈNH SỬA THÔNG TIN CÁ NHÂN
        // 2. CHỈNH SỬA THÔNG TIN CÁ NHÂN (GET) - KHÔNG CẦN TRUYỀN ID
        [HttpGet]
        public ActionResult UserInfor()
        {
            TaiKhoanNguoiDung session = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (session == null) return RedirectToAction("Login", "Home");

            // Tự động lấy ID từ Session đang đăng nhập để tìm trong DB
            TaiKhoanNguoiDung tk = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.MaTK == session.MaTK);

            // Lỡ có biến cố gì mà trong DB bị xóa mất tài khoản đó
            if (tk == null)
            {
                Session.Remove(ConstaintUser.USER_SESSION); // Xóa session rác
                return RedirectToAction("Login", "Home");
            }

            return View(tk);
        }

        [HttpPost]
        public ActionResult UserInfor([Bind(Include = "MaTK,HoTen,DiaChi,Email,SoDienThoai,NgaySinh,GioiTinh,TrangThai")] TaiKhoanNguoiDung tk)
        {
            TaiKhoanNguoiDung session = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (session == null || tk.MaTK != session.MaTK) return RedirectToAction("Login", "Home");

            // 1. Kiểm tra Email có bị trùng với người KHÁC không (Loại trừ MaTK của mình)
            var checkEmail = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.Email == tk.Email && a.MaTK != tk.MaTK);
            if (checkEmail != null)
            {
                ModelState.AddModelError("ErrorUpdate", "Email này đã được tài khoản khác sử dụng!");
                return View(tk);
            }

            // 2. Kiểm tra SĐT có bị trùng với người KHÁC không
            var checkSDT = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.SoDienThoai == tk.SoDienThoai && a.MaTK != tk.MaTK);
            if (checkSDT != null)
            {
                ModelState.AddModelError("ErrorUpdate", "Số điện thoại này đã được tài khoản khác sử dụng!");
                return View(tk);
            }

            TaiKhoanNguoiDung edit = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.MaTK == tk.MaTK);
            if (edit != null)
            {
                try
                {
                    edit.HoTen = tk.HoTen;
                    edit.DiaChi = tk.DiaChi;
                    edit.Email = tk.Email;
                    edit.SoDienThoai = tk.SoDienThoai;
                    edit.NgaySinh = tk.NgaySinh;
                    edit.GioiTinh = tk.GioiTinh;
                    db.SaveChanges();

                    Session[ConstaintUser.USER_SESSION] = edit;
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("ErrorUpdate", "Có lỗi xảy ra trong quá trình cập nhật. Vui lòng thử lại!");
                }
            }
            return View(edit);
        }
    }
}