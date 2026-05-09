using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using System.Text.RegularExpressions;
using Maison.Areas.Admin.Data;

namespace Maison.Controllers
{
    public class HotSaleDTO
    {
        public Maison.Models.BienThe BienThe { get; set; }
        public int PhanTramMax { get; set; } // Luôn giữ % to nhất bất chấp ở chương trình nào
        public List<int> ThuocCacChuyenTrinh { get; set; } // Danh sách các Mã Khuyến Mãi chứa SP này
        public decimal GiaSauGiam => (BienThe?.GiaBan ?? 0) * (1 - (decimal)PhanTramMax / 100);
        public int? SoLuongKhuyenMai { get; set; }
        public int SoLuongDaBan { get; set; }
    }
    public class HomeController : Controller
    {
        shopdb db = new shopdb();
        public ActionResult Index()
        {
            DateTime now = DateTime.Now;
            db.Configuration.ProxyCreationEnabled = false;

            // =======================================================
            // 1. XỬ LÝ HOT SALE (TÁCH RIÊNG CẤU HÌNH, SO SÁNH % CHUNG/RIÊNG)
            // =======================================================

            var activePromos = db.KhuyenMais
                .Where(k => k.TrangThai == 1 && k.NgayBatDau <= now && k.NgayKetThuc >= now)
                .OrderBy(k => k.NgayKetThuc)
                .ToList();
            ViewBag.ActivePromos = activePromos;

            var spKhuyenMais = db.SanPhamKhuyenMais
                .Where(sk => sk.KhuyenMai.TrangThai == 1 && sk.KhuyenMai.NgayBatDau <= now && sk.KhuyenMai.NgayKetThuc >= now)
                .ToList();

            // Lấy danh sách ID các sản phẩm đang có bất kỳ dòng khuyến mãi nào
            var danhSachMaSP = spKhuyenMais.Select(sk => sk.MaSP).Distinct().ToList();

            // Lấy TẤT CẢ Biến Thể của các SP đó lên
            var cacBienTheHienTai = db.BienThes
                .Include(b => b.Sanpham)
                .Include(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                .Where(b => danhSachMaSP.Contains(b.MaSP)) // Không dùng .Value
                .ToList();

            var listHotSale = new List<HotSaleDTO>();

            // VÒNG LẶP XÉT DUYỆT TỪNG CẤU HÌNH ĐỘC LẬP
            // VÒNG LẶP XÉT DUYỆT TỪNG CẤU HÌNH ĐỘC LẬP
            foreach (var bt in cacBienTheHienTai)
            {
                var kmApDungChung = spKhuyenMais.Where(sk => sk.MaSP == bt.MaSP && sk.MaBT == null).ToList();
                var kmApDungRieng = spKhuyenMais.Where(sk => sk.MaSP == bt.MaSP && sk.MaBT == bt.MaBT).ToList();

                int maxChung = kmApDungChung.Select(sk => sk.PhanTramGiam).DefaultIfEmpty(0).Max();
                int maxRieng = kmApDungRieng.Select(sk => sk.PhanTramGiam).DefaultIfEmpty(0).Max();
                int phanTramChot = Math.Max(maxChung, maxRieng);

                if (phanTramChot > 0)
                {
                    // Lọc ra ĐÚNG cái bản ghi khuyến mãi đã tạo ra cái % to nhất này
                    // Để lấy được cái Giới Hạn Flash Sale của nó
                    var recordGiamNhieuNhat = kmApDungRieng.FirstOrDefault(k => k.PhanTramGiam == phanTramChot)
                                           ?? kmApDungChung.FirstOrDefault(k => k.PhanTramGiam == phanTramChot);

                    var thuocCacCT = spKhuyenMais
                        .Where(sk => sk.MaSP == bt.MaSP && (sk.MaBT == null || sk.MaBT == bt.MaBT))
                        .Where(sk => sk.MaKM.HasValue).Select(sk => sk.MaKM.Value).Distinct().ToList();

                    listHotSale.Add(new HotSaleDTO
                    {
                        BienThe = bt,
                        PhanTramMax = phanTramChot,
                        ThuocCacChuyenTrinh = thuocCacCT,
                        // BỔ SUNG DỮ LIỆU
                        SoLuongKhuyenMai = recordGiamNhieuNhat?.SoLuongKhuyenMai,
                        SoLuongDaBan = recordGiamNhieuNhat?.SoLuongDaBan ?? 0
                    });
                }
            }

            // Gửi dữ liệu ra View (Lấy 20 cấu hình giảm sâu nhất)
            ViewBag.HotSaleItems = listHotSale.OrderByDescending(x => x.PhanTramMax).Take(20).ToList();

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
            // CHỈ LẤY DANH MỤC CẤP 1 (MaDMCha bị null)
            var danhmucs = db.Danhmucs.Where(p => p.MaDMCha == null).ToList();

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
        // THÊM: Biến string XacNhanMatKhau để hứng giá trị từ input HTML thuần ở View
        public ActionResult SignUp(TaiKhoanNguoiDung tk, string XacNhanMatKhau, string CityId, string DistrictId, string WardId, string SoNha)

        {

            ViewBag.CityId = CityId;
            ViewBag.DistrictId = DistrictId;
            ViewBag.WardId = WardId;
            ViewBag.SoNha = SoNha;
            if (!ModelState.IsValid)
            {
                return View(tk);
            }

            // 1. Kiểm tra độ mạnh của mật khẩu (Ít nhất 6 ký tự, có chữ, số, và ký tự đặc biệt)
            var regexMk = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
            if (!regexMk.IsMatch(tk.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Mật khẩu phải từ 6 ký tự, gồm chữ, số và ký tự đặc biệt (@, $, !, %, *, ?, &)");
                return View(tk);
            }

            // 2. Kiểm tra xác nhận mật khẩu
            if (tk.MatKhau != XacNhanMatKhau)
            {
                ViewBag.LoiXacNhan = "Mật khẩu nhập lại không khớp!";
                return View(tk);
            }

            // 3. Kiểm tra trùng Tên đăng nhập
            TaiKhoanNguoiDung checkTen = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.TenDangNhap.Equals(tk.TenDangNhap));
            if (checkTen != null)
            {
                ViewBag.mess = "Tên đăng nhập đã tồn tại!";
                return View(tk);
            }

            // 4. Kiểm tra trùng Email
            var checkEmail = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.Email.Equals(tk.Email));
            if (checkEmail != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(tk);
            }

            // 5. Kiểm tra trùng Số điện thoại
            var checkSDT = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.SoDienThoai.Equals(tk.SoDienThoai));
            if (checkSDT != null)
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng!");
                return View(tk);
            }

            // 6. Lưu vào Database nếu qua hết các bài kiểm tra
            try
            {
                tk.TrangThai = true;
                db.TaiKhoanNguoiDungs.Add(tk);
                db.SaveChanges();
                TaiKhoanNguoiDung session = db.TaiKhoanNguoiDungs.FirstOrDefault(a => a.TenDangNhap.Equals(tk.TenDangNhap));
                Session[Maison.Session.ConstaintUser.USER_SESSION] = session;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError("ErrorSignUp", "Đăng ký không thành công. Thử lại sau !");
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
        // =======================================================
        // COMPONENT: TIN TỨC (HIỂN THỊ GẦN FOOTER)
        // =======================================================
        [ChildActionOnly]
        public ActionResult NewsPartial()
        {
            // Lấy 4 bài viết mới nhất có trạng thái hiển thị (1)
            var tinTucList = db.TinTucs
                .Where(t => t.TrangThai == 1)
                .OrderByDescending(t => t.NgayDang)
                .Take(4)
                .ToList();

            return PartialView("_NewsPartial", tinTucList);
        }
        [HttpGet]
        public JsonResult LiveSearch(string keyword) // Đổi ActionResult thành JsonResult
        {
            if (string.IsNullOrEmpty(keyword)) return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            db.Configuration.ProxyCreationEnabled = false;
            DateTime now = DateTime.Now;

            // 1. Lấy dữ liệu thô lên RAM
            var sanPhams = db.Sanphams
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.BienThes)
                .Include(sp => sp.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(sp => sp.TenSP.ToLower().Contains(keyword.ToLower()) && sp.BienThes.Any())
                .Take(5)
                .ToList();

            // 2. Tính toán và ném thẳng ra dạng JSON (Không sợ lỗi bảo mật Anonymous Type nữa)
            var results = sanPhams.Select(sp => new
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                HinhAnh = sp.HinhAnh,
                TenDM = sp.DanhMuc != null ? sp.DanhMuc.TenDM : "Đang cập nhật",
                GiaGoc = sp.BienThes.Min(b => b.GiaBan), // Lấy Min giá
                PhanTramGiam = sp.SanPhamKhuyenMais
                    .Where(k => k.KhuyenMai.TrangThai == 1 && k.KhuyenMai.NgayBatDau <= now && k.KhuyenMai.NgayKetThuc >= now)
                    .Select(k => (int?)k.PhanTramGiam)
                    .Max() ?? 0 // Lấy Max Khuyến mãi
            }).ToList();

            // Trả về JSON cho Javascript tự xử
            return Json(results, JsonRequestBehavior.AllowGet);
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