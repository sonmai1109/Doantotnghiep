using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using System.Data.Entity;

namespace Maison.Controllers
{
    public class CartController : Controller
    {
        shopdb db = new shopdb();

        // 1. GIAO DIỆN GIỎ HÀNG
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [HttpGet]
        public ActionResult Orders()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            // Ép đăng nhập: Chưa đăng nhập thì đá về trang Login
            if (tk == null) return RedirectToAction("Login", "Home");

            // Lôi giỏ hàng của user này từ Database lên
            var cartItems = db.GioHangs
                .Include(g => g.BienThe)
                .Include(g => g.BienThe.Sanpham)
                .Include(g => g.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                .Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(g => g.MaTK == tk.MaTk)
                .OrderByDescending(g => g.NgayThem)
                .ToList();

            // Tính giá thực tế sau Khuyến mãi cho từng sản phẩm
            Dictionary<int, decimal> dicGia = new Dictionary<int, decimal>();
            foreach (var item in cartItems)
            {
                var bt = item.BienThe;
                var activeKMs = bt.Sanpham.SanPhamKhuyenMais.Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now).ToList();
                var kmRieng = activeKMs.FirstOrDefault(k => k.MaBT == bt.MaBT);
                var kmChung = activeKMs.FirstOrDefault(k => k.MaBT == null);

                int phanTram = kmRieng?.PhanTramGiam ?? kmChung?.PhanTramGiam ?? 0;
                decimal giaBan = phanTram > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTram / 100), 0) : bt.GiaBan;

                dicGia.Add(item.MaGH, giaBan);
            }

            ViewBag.DicGia = dicGia;
            return View(cartItems);
        }

        // 2. THÊM VÀO GIỎ HÀNG
        [HttpPost]
        public JsonResult AddToCart(int mabt, int soluongmua)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, notLoggedIn = true, message = "Vui lòng đăng nhập để mua hàng!" });

            var bt = db.BienThes.Find(mabt);
            if (bt == null) return Json(new { status = false, message = "Không tìm thấy sản phẩm." });

            // Kiểm tra trong DB xem khách đã từng thêm cấu hình này vào giỏ chưa?
            var existItem = db.GioHangs.FirstOrDefault(g => g.MaTK == tk.MaTk && g.MaBT == mabt);
            int soLuongDaCo = existItem != null ? existItem.SoLuong : 0;
            int tongYeuCau = soLuongDaCo + soluongmua;

            if (tongYeuCau > bt.SoLuongTon)
                return Json(new { status = false, message = $"Hết hàng! (Kho còn: {bt.SoLuongTon}, Bạn đã có: {soLuongDaCo})" });

            if (existItem != null)
            {
                existItem.SoLuong = tongYeuCau; // Có rồi thì cộng dồn số lượng
            }
            else
            {
                // Chưa có thì tạo mới
                db.GioHangs.Add(new GioHang { MaTK = tk.MaTk, MaBT = mabt, SoLuong = soluongmua });
            }

            db.SaveChanges();
            int cartCount = db.GioHangs.Where(g => g.MaTK == tk.MaTk).Count();

            return Json(new { status = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
        }

        // 3. ĐỔI SỐ LƯỢNG TRỰC TIẾP
        [HttpPost]
        public JsonResult UpdateFromCart(int MaBT, int SoLuongMua)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, message = "Vui lòng đăng nhập." });

            var item = db.GioHangs.Include(g => g.BienThe).FirstOrDefault(g => g.MaTK == tk.MaTk && g.MaBT == MaBT);
            if (item == null) return Json(new { status = false, message = "Sản phẩm không có trong giỏ." });

            if (SoLuongMua > item.BienThe.SoLuongTon)
                return Json(new { status = false, message = $"Chỉ còn tối đa {item.BienThe.SoLuongTon} sản phẩm." });

            item.SoLuong = SoLuongMua;
            db.SaveChanges();

            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        // 4. XÓA SẢN PHẨM KHỎI GIỎ
        [HttpPost]
        public JsonResult DeleteFromCart(int mabt)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk != null)
            {
                var item = db.GioHangs.FirstOrDefault(g => g.MaTK == tk.MaTk && g.MaBT == mabt);
                if (item != null)
                {
                    db.GioHangs.Remove(item);
                    db.SaveChanges();
                }
                int cartCount = db.GioHangs.Where(g => g.MaTK == tk.MaTk).Count();
                return Json(new { count = cartCount }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { count = 0 });
        }

        // 5. TRANG ĐẶT HÀNG
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [HttpGet]
        public ActionResult CheckOut()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            ViewBag.TaiKhoan = tk;
            // Dùng nguyên logic gọi dữ liệu như trang Orders
            var cartItems = db.GioHangs.Include(g => g.BienThe).Include(g => g.BienThe.Sanpham).Include(g => g.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)).Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai)).Where(g => g.MaTK == tk.MaTk).ToList();

            Dictionary<int, decimal> dicGia = new Dictionary<int, decimal>();
            foreach (var item in cartItems)
            {
                var bt = item.BienThe;
                var activeKMs = bt.Sanpham.SanPhamKhuyenMais.Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now).ToList();
                var kmRieng = activeKMs.FirstOrDefault(k => k.MaBT == bt.MaBT);
                var kmChung = activeKMs.FirstOrDefault(k => k.MaBT == null);
                int phanTram = kmRieng?.PhanTramGiam ?? kmChung?.PhanTramGiam ?? 0;
                dicGia.Add(item.MaGH, phanTram > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTram / 100), 0) : bt.GiaBan);
            }
            ViewBag.DicGia = dicGia;

            return View(cartItems);
        }
    }
}