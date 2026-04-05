using Maison.Models;
using Maison.Session;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Maison.Areas.Admin.Controllers
{
    public class KhuyenMaisController : BaseController // Nhớ kế thừa BaseController nếu bạn có dùng để check đăng nhập
    {
        private shopdb db = new shopdb(); // Đổi lại đúng tên DbContext của bạn

        // 1. LIỆT KÊ (Có phân trang)
        public ActionResult Index(string q, int page = 1, int pageSize = 7)
        {
            ViewBag.q = q;
            var list = db.KhuyenMais.AsQueryable();
            if (!string.IsNullOrEmpty(q)) list = list.Where(k => k.TenKM.Contains(q));
            return View(list.OrderByDescending(k => k.MaKM).ToPagedList(page, pageSize));
        }

        // 2. LẤY DỮ LIỆU SỬA
        [HttpPost]
        public JsonResult Get(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return Json(new { status = false });

            return Json(new
            {
                status = true,
                km.MaKM,
                km.TenKM,
                km.MoTa,
                NgayBatDau = km.NgayBatDau?.ToString("yyyy-MM-dd"),
                NgayKetThuc = km.NgayKetThuc?.ToString("yyyy-MM-dd")
            }, JsonRequestBehavior.AllowGet);
        }

        // 3. THÊM MỚI (Tích hợp Session)
        [HttpPost]
        public JsonResult Create(KhuyenMai model)
        {
            try
            {
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[ConstaintUser.ADMIN_SESSION];
                if (tk != null)
                {
                    model.NgayTao = DateTime.Now;
                    model.NguoiTao = tk.HoTen;
                    model.NgaySua = DateTime.Now;
                    model.NguoiSua = tk.HoTen;
                }

                model.TrangThai = 1; // Mặc định hiển thị
                db.KhuyenMais.Add(model);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm khuyến mãi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 4. CHỈNH SỬA
        [HttpPost]
        public JsonResult Edit(KhuyenMai model)
        {
            try
            {
                var km = db.KhuyenMais.Find(model.MaKM);
                if (km == null) return Json(new { status = false, message = "Không tìm thấy chương trình!" });

                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[ConstaintUser.ADMIN_SESSION];
                if (tk != null)
                {
                    km.NgaySua = DateTime.Now;
                    km.NguoiSua = tk.HoTen;
                }

                km.TenKM = model.TenKM;
                km.MoTa = model.MoTa;
                km.NgayBatDau = model.NgayBatDau;
                km.NgayKetThuc = model.NgayKetThuc;

                db.Entry(km).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 5. BẬT / TẮT & XÓA
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return Json(new { status = false });
            km.TrangThai = (km.TrangThai == 1) ? 0 : 1;
            db.SaveChanges();
            return Json(new { status = true });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return Json(new { status = false });

            // Xóa sạch các sản phẩm đang được gán khuyến mãi này trước
            var links = db.SanPhamKhuyenMais.Where(s => s.MaKM == id);
            db.SanPhamKhuyenMais.RemoveRange(links);
            db.KhuyenMais.Remove(km);
            db.SaveChanges();
            return Json(new { status = true });
        }

        // ========================================================
        // KHU VỰC GÁN SẢN PHẨM VÀO KHUYẾN MÃI
        // ========================================================

        public ActionResult ManageProducts(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound();

            // SỬA LỖI 1: Include sâu vào Biến Thể và Chi Tiết để View hiển thị được tên cấu hình (Ram, CPU...)
            var assigned = db.SanPhamKhuyenMais
                .Include(s => s.Sanpham)
                .Include(s => s.Sanpham.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT)))
                .Where(s => s.MaKM == id)
                .ToList();

            ViewBag.KhuyenMai = km;
            return View(assigned);
        }

        // API Tìm kiếm Sản phẩm (Dùng cho ô Search gõ tên)
        [HttpGet]
        public JsonResult SearchSanPham(string q)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = db.Sanphams.Where(s => s.TenSP.Contains(q))
                .Select(s => new {
                    s.MaSP,
                    s.TenSP,
                    s.HinhAnh,
                    // Lôi thêm danh sách Biến thể ra để JavaScript vẽ Dropdown
                    BienThes = s.BienThes.Select(bt => new {
                        bt.MaBT,
                        // Lấy các thông số (Đen, 8GB...) ghép lại
                        ThuocTinhs = bt.ChiTietBTs.Select(ct => ct.GiaTriTT.GiaTri).ToList()
                    }).ToList()
                })
                .Take(10).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AssignProduct(int maKM, int maSP, int? maBT, int phanTramGiam)
        {
            // SỬA LỖI 2 & 3: Xử lý triệt để logic Xung đột giữa "Toàn bộ" và "Từng cấu hình"

            if (maBT == null)
            {
                // Kịch bản A: Chọn giảm giá "TOÀN BỘ CẤU HÌNH"
                // -> Xóa sạch mọi cấu hình lẻ đang được giảm của sản phẩm này đi (để tránh đè/trùng lặp)
                var oldLinks = db.SanPhamKhuyenMais.Where(x => x.MaKM == maKM && x.MaSP == maSP).ToList();
                if (oldLinks.Any())
                {
                    db.SanPhamKhuyenMais.RemoveRange(oldLinks);
                }

                // Thêm 1 dòng duy nhất đại diện cho TOÀN BỘ
                db.SanPhamKhuyenMais.Add(new SanPhamKhuyenMai
                {
                    MaKM = maKM,
                    MaSP = maSP,
                    MaBT = null,
                    PhanTramGiam = phanTramGiam
                });
            }
            else
            {
                // Kịch bản B: Chọn giảm giá "1 CẤU HÌNH CỤ THỂ"

                // Bước 1: Nếu trước đó sản phẩm đang được gán "Toàn bộ cấu hình" (MaBT = null)
                // -> Phải xóa cái "Toàn bộ" đó đi, vì giờ Admin muốn set giá giảm riêng cho từng thằng.
                var allLink = db.SanPhamKhuyenMais.FirstOrDefault(x => x.MaKM == maKM && x.MaSP == maSP && x.MaBT == null);
                if (allLink != null)
                {
                    db.SanPhamKhuyenMais.Remove(allLink);
                }

                // Bước 2: Kiểm tra xem cấu hình cụ thể này đã được gán chưa?
                var exist = db.SanPhamKhuyenMais.FirstOrDefault(x => x.MaKM == maKM && x.MaSP == maSP && x.MaBT == maBT);
                if (exist != null)
                {
                    // Nếu gán rồi thì chỉ cập nhật %
                    exist.PhanTramGiam = phanTramGiam;
                }
                else
                {
                    // Nếu chưa thì thêm mới 1 dòng cho cấu hình này
                    db.SanPhamKhuyenMais.Add(new SanPhamKhuyenMai
                    {
                        MaKM = maKM,
                        MaSP = maSP,
                        MaBT = maBT,
                        PhanTramGiam = phanTramGiam
                    });
                }
            }

            db.SaveChanges();
            return Json(new { status = true });
        }
        // Hàm cập nhật % giảm giá khi bấm nút Sửa trong bảng
        [HttpPost]
        public JsonResult UpdateDiscount(int id, int phanTramGiam)
        {
            try
            {
                // Tìm bản ghi liên kết Sản phẩm - Khuyến mãi theo ID
                var spkm = db.SanPhamKhuyenMais.Find(id);
                if (spkm == null)
                    return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                // Cập nhật lại phần trăm
                spkm.PhanTramGiam = phanTramGiam;
                db.SaveChanges();

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult RemoveProduct(int id)
        {
            var link = db.SanPhamKhuyenMais.Find(id);
            if (link != null)
            {
                db.SanPhamKhuyenMais.Remove(link);
                db.SaveChanges();
            }
            return Json(new { status = true });
        }
    }
}