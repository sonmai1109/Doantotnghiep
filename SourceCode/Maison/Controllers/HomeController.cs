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
            DateTime now = DateTime.Now;

            // 1. Sản phẩm khuyến mãi (Giữ nguyên như cũ, đã làm xong)
            // 1. Sản phẩm khuyến mãi (ĐÃ FIX: Trả về List<Sanpham> thay vì List<SanPhamKhuyenMai>)
            var activeKMs = db.SanPhamKhuyenMais
                    .Include(k => k.KhuyenMai)
                    .Where(k => k.KhuyenMai.TrangThai == 1 && k.KhuyenMai.NgayBatDau <= now && k.KhuyenMai.NgayKetThuc >= now)
                    .ToList();

            // Nhóm KM lại để dễ tra cứu. Key là Mã Sản Phẩm.
            var kmGomNhomTheoSP = activeKMs.GroupBy(k => k.MaSP).ToDictionary(g => g.Key, g => g.ToList());

            // Bước B: Lấy toàn bộ biến thể ra để xét duyệt
            // Bước B: Lấy toàn bộ biến thể ra để xét duyệt
            var tatCaBienThe = db.BienThes
                .Include(b => b.Sanpham)
                .Include(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                .ToList();

            // ĐÃ SỬA CHỖ NÀY: Dùng Tuple<BienThe, int> thay vì dynamic
            var danhSachHotSale = new List<Tuple<BienThe, int>>();

            foreach (var bt in tatCaBienThe)
            {
                if (kmGomNhomTheoSP.ContainsKey(bt.MaSP))
                {
                    var kmCuaSPNay = kmGomNhomTheoSP[bt.MaSP];

                    int maxRieng = kmCuaSPNay.Where(k => k.MaBT == bt.MaBT).Select(k => k.PhanTramGiam).DefaultIfEmpty(0).Max();
                    int maxChung = kmCuaSPNay.Where(k => k.MaBT == null).Select(k => k.PhanTramGiam).DefaultIfEmpty(0).Max();
                    int phanTramChot = Math.Max(maxRieng, maxChung);

                    if (phanTramChot > 0)
                    {
                        // ĐÃ SỬA CHỖ NÀY: Khởi tạo Tuple (Item1 là Biến thể, Item2 là Phần trăm)
                        danhSachHotSale.Add(new Tuple<BienThe, int>(bt, phanTramChot));
                    }
                }
            }

            // ĐÃ SỬA CHỖ NÀY: Gọi x.Item2 (chính là PhanTramGiam) để sắp xếp
            ViewBag.HotSale = danhSachHotSale.OrderByDescending(x => x.Item2).Take(20).ToList();

            // 2. Sản phẩm mới
            var sanPhamMoi = db.Sanphams
                .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai)) // <--- BẮT BUỘC THÊM DÒNG NÀY
                .OrderByDescending(p => p.NgayTao)
                .Take(20) // Sửa thành 20
                .ToList();

            // 3. Giá tốt
            var giaTot = db.Sanphams
                .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai)) // <--- BẮT BUỘC THÊM DÒNG NÀY
                .Where(p => p.BienThes.Any())
                .OrderBy(p => p.BienThes.Min(b => b.GiaBan))
                .Take(20) // Sửa thành 20
                .ToList();

     
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
        [HttpGet]
        public ActionResult LiveSearch(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return Content("");

            // Thêm .Include(sp => sp.DanhMuc) để lôi tên Danh mục ra
            var sanPhams = db.Sanphams
                             .Include(sp => sp.DanhMuc) // <-- THÊM DÒNG NÀY LÀ XONG
                             .Where(sp => sp.TenSP.ToLower().Contains(keyword.ToLower()))
                             .Take(5)
                             .ToList();

            return PartialView("_LiveSearchResults", sanPhams);
        }
        [ChildActionOnly]
        public ActionResult CartCount()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[Maison.Session.ConstaintUser.USER_SESSION];
            int count = 0;

            if (tk != null)
            {
                // Nếu đã đăng nhập, đếm số lượng các cấu hình khác nhau nằm trong giỏ
                count = db.GioHangs.Where(g => g.MaTK == tk.MaTK).Count();
            }

            return PartialView(count);
        }

    }
}