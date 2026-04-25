using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using System.Data.Entity;
using System.IO;
using System.Web.UI;
using PagedList;

namespace Maison.Areas.Admin.Controllers // Nhớ sửa namespace
{
    public class SanphamsController : BaseController
    {
        shopdb db = new shopdb();

        public ActionResult Index(string timkiem, int? locMaDM, int? locMaBrand, int page = 1, int pagesize = 4)
        {
            ViewBag.timkiem = timkiem;
            ViewBag.locMaDM = locMaDM;
            ViewBag.locMaBrand = locMaBrand;

            var sanphams = db.Sanphams.Include(s => s.DanhMuc).Include(s => s.Brand).AsQueryable();

            // --- 1. XỬ LÝ BỘ LỌC TÌM KIẾM NÂNG CAO ---
            if (!string.IsNullOrEmpty(timkiem))
            {
                sanphams = sanphams.Where(s => s.TenSP.Contains(timkiem));
            }
            if (locMaDM.HasValue)
            {
                // Lọc: Nếu chọn danh mục Cha, lấy cả danh mục Cha lẫn các danh mục Con của nó
                var danhSachIdLoc = db.Danhmucs.Where(d => d.MaDM == locMaDM || d.MaDMCha == locMaDM).Select(d => d.MaDM).ToList();
                sanphams = sanphams.Where(s => danhSachIdLoc.Contains(s.MaDM));
            }
            if (locMaBrand.HasValue)
            {
                sanphams = sanphams.Where(s => s.MaBrand == locMaBrand);
            }

            // --- 2. TẠO CÂY DANH MỤC CHO POPUP THÊM/SỬA (KHÓA THẺ CHA) ---
            var danhMucs = db.Danhmucs.ToList();
            var selectListPopup = new List<SelectListItem>();
            var selectListFilter = new List<SelectListItem>(); // Dùng cho bộ lọc

            foreach (var cha in danhMucs.Where(x => x.MaDMCha == null))
            {
                // Thêm vào Popup (Bị khóa)
                selectListPopup.Add(new SelectListItem { Value = "", Text = "--- " + cha.TenDM.ToUpper() + " ---", Disabled = true });
                // Thêm vào Filter (Cho phép chọn để lọc tất cả con)
                selectListFilter.Add(new SelectListItem { Value = cha.MaDM.ToString(), Text = "📁 " + cha.TenDM.ToUpper() });

                foreach (var con in danhMucs.Where(x => x.MaDMCha == cha.MaDM))
                {
                    selectListPopup.Add(new SelectListItem { Value = con.MaDM.ToString(), Text = "\xA0\xA0\xA0\xA0" + con.TenDM });
                    selectListFilter.Add(new SelectListItem { Value = con.MaDM.ToString(), Text = "   --- " + con.TenDM });
                }
            }

            ViewBag.MaDMList = selectListPopup; // Dành cho Add/Edit
            ViewBag.FilterDM = selectListFilter; // Dành cho Thanh tìm kiếm
            ViewBag.MaBrand = new SelectList(db.Brands, "MaBrand", "TenBrand");

            return View(sanphams.OrderByDescending(s => s.MaSP).ToPagedList(page, pagesize));
        }

        [HttpPost]
      
        public JsonResult Create(Sanpham sp, HttpPostedFileBase ImageFile)
        {
            try
            {
                // Lấy thông tin tài khoản đang đăng nhập từ Session
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];

                if (tk != null)
                {
                    sp.NgayTao = DateTime.Now;
                    sp.NguoiTao = tk.HoTen;
                    sp.NgaySua = DateTime.Now;
                    sp.NguoiSua = tk.HoTen; // Khi mới tạo thì người tạo cũng là người sửa lần cuối
                }

                // Xử lý upload ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string dirPath = Server.MapPath("~/Content/Images/SanPhams/");
                    if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

                    string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                    string path = System.IO.Path.Combine(dirPath, fileName);
                    ImageFile.SaveAs(path);
                    sp.HinhAnh = "/Content/Images/SanPhams/" + fileName;
                }

                db.Sanphams.Add(sp);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Loaddata(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var sp = db.Sanphams.FirstOrDefault(a => a.MaSP == id);
            return Json(sp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(Sanpham sp, HttpPostedFileBase ImageFile)
        {
            try
            {
                var doi = db.Sanphams.FirstOrDefault(a => a.MaSP == sp.MaSP);
                if (doi == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                // Cập nhật thông tin cơ bản
                doi.TenSP = sp.TenSP;
                doi.MaDM = sp.MaDM;
                doi.MaBrand = sp.MaBrand;
                doi.MoTa = sp.MoTa;
                doi.ThoiHanBaoHanh = sp.ThoiHanBaoHanh;

                // Bắt Session Người sửa
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                if (tk != null)
                {
                    doi.NgaySua = DateTime.Now;
                    doi.NguoiSua = tk.HoTen;
                }

                // Xử lý đổi ảnh (nếu có up ảnh mới)
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string dirPath = Server.MapPath("~/Content/Images/SanPhams/");
                    if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

                    string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                    string path = System.IO.Path.Combine(dirPath, fileName);
                    ImageFile.SaveAs(path);
                    doi.HinhAnh = "/Content/Images/SanPhams/" + fileName;
                }

                db.Entry(doi).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi cập nhật: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var sp = db.Sanphams.FirstOrDefault(a => a.MaSP == id);
                db.Sanphams.Remove(sp);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch
            {
                return Json(new { status = false, message = "Không thể xóa vì sản phẩm này đang có biến thể hoặc hóa đơn!" });
            }
        }
    }
}