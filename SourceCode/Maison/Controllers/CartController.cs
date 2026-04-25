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
            if (tk == null) return RedirectToAction("Login", "Home");

            var cartItems = db.GioHangs
                .Include(g => g.BienThe)
                .Include(g => g.BienThe.Sanpham)
                .Include(g => g.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                .Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(g => g.MaTK == tk.MaTK)
                .OrderByDescending(g => g.NgayThem)
                .ToList();

            // Tính giá thực tế sau Khuyến mãi (CÓ CHECK FLASH SALE)
            Dictionary<int, decimal> dicGia = new Dictionary<int, decimal>();
            foreach (var item in cartItems)
            {
                var bt = item.BienThe;
                var activeKMs = bt.Sanpham.SanPhamKhuyenMais
                    .Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now)
                    .ToList();

                // Lấy tất cả KM áp dụng cho cấu hình này (Chung + Riêng)
                var kmApDung = activeKMs.Where(k => k.MaBT == null || k.MaBT == bt.MaBT).ToList();

                int phanTramChot = 0;

                // Lọc bỏ những chương trình Flash Sale đã BÁN HẾT SUẤT
                // ÉP LUẬT MỚI: Số suất CÒN LẠI phải LỚN HƠN HOẶC BẰNG Số lượng khách mua (item.SoLuong)
                var kmConSuat = kmApDung.Where(k =>
                    k.SoLuongKhuyenMai == null ||
                    (k.SoLuongKhuyenMai.Value - k.SoLuongDaBan) >= item.SoLuong // <--- CHÌA KHÓA NẰM Ở ĐÂY
                ).ToList();

                if (kmConSuat.Any())
                {
                    phanTramChot = kmConSuat.Max(k => k.PhanTramGiam);
                }

                decimal giaBan = phanTramChot > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTramChot / 100), 0) : bt.GiaBan;
                dicGia.Add(item.MaGH, giaBan);
            }

            ViewBag.DicGia = dicGia;
            return View(cartItems);
        }

        // 2. THÊM VÀO GIỎ HÀNG (Giữ nguyên)
        [HttpPost]
        public JsonResult AddToCart(int mabt, int soluongmua)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, notLoggedIn = true, message = "Vui lòng đăng nhập để mua hàng!" });

            var bt = db.BienThes.Find(mabt);
            if (bt == null) return Json(new { status = false, message = "Không tìm thấy sản phẩm." });

            var existItem = db.GioHangs.FirstOrDefault(g => g.MaTK == tk.MaTK && g.MaBT == mabt);
            int soLuongDaCo = existItem != null ? existItem.SoLuong : 0;
            int tongYeuCau = soLuongDaCo + soluongmua;

            if (tongYeuCau > bt.SoLuongTon)
                return Json(new { status = false, message = $"Hết hàng! (Kho còn: {bt.SoLuongTon}, Bạn đã có: {soLuongDaCo})" });

            if (existItem != null)
            {
                existItem.SoLuong = tongYeuCau;
            }
            else
            {
                db.GioHangs.Add(new GioHang { MaTK = tk.MaTK, MaBT = mabt, SoLuong = soluongmua });
            }

            db.SaveChanges();
            int cartCount = db.GioHangs.Where(g => g.MaTK == tk.MaTK).Count();

            return Json(new { status = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
        }

        // 3. ĐỔI SỐ LƯỢNG TRỰC TIẾP (Giữ nguyên)
        // 3. ĐỔI SỐ LƯỢNG TRỰC TIẾP TRONG GIỎ HÀNG
        [HttpPost]
        public JsonResult UpdateFromCart(int MaBT, int SoLuongMua)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return Json(new { status = false, message = "Vui lòng đăng nhập." });

            var item = db.GioHangs.Include(g => g.BienThe).FirstOrDefault(g => g.MaTK == tk.MaTK && g.MaBT == MaBT);
            if (item == null) return Json(new { status = false, message = "Sản phẩm không có trong giỏ." });

            if (SoLuongMua > item.BienThe.SoLuongTon)
                return Json(new { status = false, message = $"Chỉ còn tối đa {item.BienThe.SoLuongTon} sản phẩm trong kho." });

            // KIỂM TRA FLASH SALE CÒN SUẤT KHÔNG
            var activeKMs = db.SanPhamKhuyenMais
                .Where(x => x.MaSP == item.BienThe.MaSP && (x.MaBT == null || x.MaBT == MaBT))
                .Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .ToList();

            int phanTramChot = 0;
            var kmConSuat = activeKMs.Where(k => k.SoLuongKhuyenMai == null || k.SoLuongDaBan < k.SoLuongKhuyenMai).ToList();

            if (kmConSuat.Any())
            {
                phanTramChot = kmConSuat.Max(k => k.PhanTramGiam);
                var flashSaleDangApDung = kmConSuat.FirstOrDefault(k => k.PhanTramGiam == phanTramChot);

                // Nếu KM này có giới hạn số lượng
                if (flashSaleDangApDung != null && flashSaleDangApDung.SoLuongKhuyenMai.HasValue)
                {
                    int soSuatConLai = flashSaleDangApDung.SoLuongKhuyenMai.Value - flashSaleDangApDung.SoLuongDaBan;

                    // CHẶN NGAY NẾU MUA LỐ SUẤT
                    if (SoLuongMua > soSuatConLai)
                    {
                        return Json(new { status = false, isFlashSaleLimit = true, message = $"Sản phẩm này chỉ còn {soSuatConLai} suất giảm giá. Vui lòng giảm số lượng để tiếp tục." });
                    }
                }
            }

            item.SoLuong = SoLuongMua;
            db.SaveChanges();

            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }
        // 4. XÓA SẢN PHẨM KHỎI GIỎ (Giữ nguyên)
        [HttpPost]
        public JsonResult DeleteFromCart(int mabt)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk != null)
            {
                var item = db.GioHangs.FirstOrDefault(g => g.MaTK == tk.MaTK && g.MaBT == mabt);
                if (item != null)
                {
                    db.GioHangs.Remove(item);
                    db.SaveChanges();
                }
                int cartCount = db.GioHangs.Where(g => g.MaTK == tk.MaTK).Count();
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

            var cartItems = db.GioHangs
                .Include(g => g.BienThe)
                .Include(g => g.BienThe.Sanpham)
                .Include(g => g.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                .Include(g => g.BienThe.Sanpham.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(g => g.MaTK == tk.MaTK)
                .ToList();

            // Tính giá thực tế (CÓ CHECK FLASH SALE) y chang hàm Orders
            Dictionary<int, decimal> dicGia = new Dictionary<int, decimal>();
            foreach (var item in cartItems)
            {
                var bt = item.BienThe;
                var activeKMs = bt.Sanpham.SanPhamKhuyenMais
                    .Where(x => x.KhuyenMai.TrangThai == 1 && x.KhuyenMai.NgayBatDau <= DateTime.Now && x.KhuyenMai.NgayKetThuc >= DateTime.Now)
                    .ToList();

                var kmApDung = activeKMs.Where(k => k.MaBT == null || k.MaBT == bt.MaBT).ToList();

                int phanTramChot = 0;
                // ÉP LUẬT MỚI: Số suất CÒN LẠI phải LỚN HƠN HOẶC BẰNG Số lượng khách mua (item.SoLuong)
                var kmConSuat = kmApDung.Where(k =>
                    k.SoLuongKhuyenMai == null ||
                    (k.SoLuongKhuyenMai.Value - k.SoLuongDaBan) >= item.SoLuong // <--- CHÌA KHÓA NẰM Ở ĐÂY
                ).ToList();

                if (kmConSuat.Any())
                {
                    phanTramChot = kmConSuat.Max(k => k.PhanTramGiam);
                }

                decimal giaBan = phanTramChot > 0 ? Math.Round(bt.GiaBan * (1 - (decimal)phanTramChot / 100), 0) : bt.GiaBan;
                dicGia.Add(item.MaGH, giaBan);
            }
            ViewBag.DicGia = dicGia;

            return View(cartItems);
        }
    }
}