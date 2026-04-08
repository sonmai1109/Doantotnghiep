using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using PagedList;
using Newtonsoft.Json;
using System.Data.Entity;

namespace Maison.Controllers
{
    public class ProductController : Controller
    {
        shopdb db = new shopdb(); // Dùng đúng DbContext của bạn

        // GET: Product/Shop
        public ActionResult Shop(string searchString, int? madm, int? maBrand, int page = 1, int pageSize = 9)
        {
            ViewBag.searchString = searchString;
            ViewBag.madm = madm;
            ViewBag.maBrand = maBrand; // LƯU LẠI MABRAND ĐỂ VIEW CÒN DÙNG

            // Lấy toàn bộ sản phẩm (kèm Biến Thể để tính giá)
            var sanphams = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Include(s => s.BienThes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                sanphams = sanphams.Where(sp => sp.TenSP.Contains(searchString));

            if (madm != null && madm != 0)
            {
                sanphams = sanphams.Where(sp => sp.MaDM == madm);
                ViewBag.DanhMuc = db.Danhmucs.FirstOrDefault(d => d.MaDM == madm);
            }

            // --- MỚI THÊM: LỌC THEO BRAND ---
            if (maBrand != null && maBrand != 0)
            {
                sanphams = sanphams.Where(sp => sp.MaBrand == maBrand);
                ViewBag.Brand = db.Brands.FirstOrDefault(b => b.MaBrand == maBrand); // Lấy tên Brand truyền sang View
            }

            var result = sanphams.OrderByDescending(sp => sp.NgayTao).ToPagedList(page, pageSize);

            ViewBag.GiaSauKhuyenMai = TinhGiaSauKhuyenMai(result.ToList());
            ViewBag.ActionName = "Shop";

            return View(result);
        }

        public ActionResult Sale(int page = 1, int pageSize = 9)
        {
            DateTime now = DateTime.Now;
            var sanphams = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(sp => sp.SanPhamKhuyenMais.Any(km =>
                    km.KhuyenMai.TrangThai == 1 &&
                    km.KhuyenMai.NgayBatDau <= now &&
                    km.KhuyenMai.NgayKetThuc >= now
                ))
                .OrderByDescending(sp => sp.NgayTao)
                .ToPagedList(page, pageSize);

            ViewBag.GiaSauKhuyenMai = TinhGiaSauKhuyenMai(sanphams.ToList());
            ViewBag.Title = "Sản phẩm khuyến mãi";
            ViewBag.ActionName = "Sale";

            return View("Shop", sanphams);
        }

        public ActionResult New(int page = 1, int pageSize = 12)
        {
            DateTime now = DateTime.Now;
            var sanphams = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(sp => !sp.SanPhamKhuyenMais.Any(k =>
                    k.KhuyenMai.TrangThai == 1 &&
                    k.KhuyenMai.NgayBatDau <= now &&
                    k.KhuyenMai.NgayKetThuc >= now))
                .OrderByDescending(sp => sp.NgayTao)
                .Take(20).ToPagedList(page, pageSize);

            ViewBag.GiaSauKhuyenMai = TinhGiaSauKhuyenMai(sanphams.ToList());
            ViewBag.Title = "Sản phẩm mới";
            ViewBag.ActionName = "New";

            return View("Shop", sanphams);
        }

        public ActionResult GiaTot(int page = 1, int pageSize = 12)
        {
            var sanphams = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Where(p => p.BienThes.Any())
                .ToList();

            var dicGiaSauKM = TinhGiaSauKhuyenMai(sanphams);

            // Sắp xếp theo giá cuối cùng thực tế (sau khi giảm)
            var spSapXep = sanphams.OrderBy(sp => dicGiaSauKM.ContainsKey(sp.MaSP) && dicGiaSauKM[sp.MaSP] != null ? dicGiaSauKM[sp.MaSP] : sp.BienThes.Min(b => b.GiaBan))
                                   .Take(20)
                                   .ToList();

            ViewBag.GiaSauKhuyenMai = dicGiaSauKM;
            ViewBag.Title = "Sản phẩm giá tốt";
            ViewBag.ActionName = "GiaTot";

            return View("Shop", spSapXep.ToPagedList(page, pageSize));
        }

        // ===============================================
        // HÀM TÍNH GIÁ KHUYẾN MÃI CHO TỪNG SẢN PHẨM Ở LƯỚI
        // ===============================================
        private Dictionary<int, decimal?> TinhGiaSauKhuyenMai(List<Sanpham> sanPhams)
        {
            DateTime now = DateTime.Now;
            var dic = new Dictionary<int, decimal?>();

            foreach (var sp in sanPhams)
            {
                // Lấy giá thấp nhất trong các cấu hình làm mốc
                decimal giaGocMin = sp.BienThes != null && sp.BienThes.Any() ? sp.BienThes.Min(b => b.GiaBan) : 0;

                var kmsp = sp.SanPhamKhuyenMais?
                    .FirstOrDefault(k => k.KhuyenMai.TrangThai == 1
                                      && k.KhuyenMai.NgayBatDau <= now
                                      && k.KhuyenMai.NgayKetThuc >= now);
                if (kmsp != null && giaGocMin > 0)
                {
                    dic[sp.MaSP] = giaGocMin * (1 - (decimal)kmsp.PhanTramGiam / 100);
                }
                else
                {
                    dic[sp.MaSP] = null;
                }
            }
            return dic;
        }
        public ActionResult ProductDetail(int id)
        {
            var sp = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.SanPhamKhuyenMais.Select(k => k.KhuyenMai))
                .Include(s => s.BienThes.Select(b => b.ThuVienAnhs))
                .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                .FirstOrDefault(s => s.MaSP == id);

            if (sp == null) return HttpNotFound();

            // 1. Lọc lấy các khuyến mãi ĐANG CHẠY của sản phẩm này
            var activeKMs = sp.SanPhamKhuyenMais
                .Where(x => x.KhuyenMai.TrangThai == 1 &&
                            x.KhuyenMai.NgayBatDau <= DateTime.Now &&
                            x.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .ToList();

            // 2. Lấy BẢNG THÔNG SỐ KỸ THUẬT
            var thongSoKyThuat = sp.BienThes
                .SelectMany(b => b.ChiTietBTs)
                .Select(c => c.GiaTriTT)
                .Where(g => g.ThuocTinh.LaThuocTinhChinh == false)
                .GroupBy(g => g.ThuocTinh)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.MaGT).Select(x => x.First()).ToList()
                );
            ViewBag.ThongSoKyThuat = thongSoKyThuat;

            // 3. ĐÓNG GÓI JSON BIẾN THỂ (Gắn % Khuyến mãi vào TỪNG cấu hình)
            var listBT = sp.BienThes.Select(b => {

                // Thuật toán lấy % giảm: Ưu tiên KM gán riêng cho cấu hình này (MaBT). 
                // Nếu không có thì kiểm tra xem có KM gán chung cho cả Sản phẩm (MaBT == null) không.
                var kmRieng = activeKMs.FirstOrDefault(k => k.MaBT == b.MaBT);
                var kmChung = activeKMs.FirstOrDefault(k => k.MaBT == null);

                int phanTram = 0;
                if (kmRieng != null) phanTram = kmRieng.PhanTramGiam;
                else if (kmChung != null) phanTram = kmChung.PhanTramGiam;

                return new
                {
                    MaBT = b.MaBT,
                    GiaBan = b.GiaBan,
                    SoLuongTon = b.SoLuongTon,
                    HinhAnh = b.HinhAnh,
                    PhanTramGiam = phanTram, // <--- THÊM CHÌA KHÓA NÀY VÀO JSON
                    ThuVienAnhs = b.ThuVienAnhs.OrderBy(a => a.ThuTu).Select(a => a.DuongDanAnh).ToList(),
                    ChiTiets = b.ChiTietBTs.Select(c => c.MaGT).ToList()
                };
            });

            ViewBag.BienThes_Json = JsonConvert.SerializeObject(listBT);
            return View(sp);
        }
    }
}