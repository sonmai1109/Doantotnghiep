using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using System.Data.Entity;
using PagedList;

namespace Maison.Areas.Admin.Controllers
{
    public class ThuocTinhsController : BaseController
    {
        shopdb db = new shopdb();

        // THÊM THAM SỐ locMaDM VÀO HÀM INDEX ĐỂ LÀM BỘ LỌC
        public ActionResult Index(string timkiem, int? locMaDM, int page = 1, int pagesize = 7)
        {
            ViewBag.timkiem = timkiem;
            ViewBag.locMaDM = locMaDM; // Giữ lại giá trị lọc để gán vào Dropdown

            var thuoctinhs = db.ThuocTinhs.Include(t => t.DanhMuc).AsQueryable();

            // 1. Lọc theo Tên (Tìm kiếm)
            if (!string.IsNullOrEmpty(timkiem))
            {
                thuoctinhs = thuoctinhs.Where(t => t.TenTT.Contains(timkiem));
            }

            // 2. Lọc theo Danh mục (Dropdown)
            if (locMaDM != null && locMaDM != 0)
            {
                thuoctinhs = thuoctinhs.Where(t => t.MaDM == locMaDM);
            }

            ViewBag.MaDM = new SelectList(db.Danhmucs, "MaDM", "TenDM");

            // Sắp xếp ưu tiên theo Danh mục -> Thứ tự hiển thị -> ID
            return View(thuoctinhs.OrderBy(t => t.MaDM).ThenBy(t => t.ThuTuHienThi).ThenByDescending(t => t.MaTT).ToPagedList(page, pagesize));
        }

        [HttpPost]
        public JsonResult Create(ThuocTinh tt)
        {
            try
            {
                var check = db.ThuocTinhs.FirstOrDefault(x => x.TenTT.ToLower() == tt.TenTT.ToLower() && x.MaDM == tt.MaDM);
                if (check != null) return Json(new { status = false, message = "Thuộc tính này đã tồn tại trong danh mục!" });

                // Nếu không nhập thứ tự hiển thị, gán mặc định là 999 (Xếp bét)
                tt.ThuTuHienThi = tt.ThuTuHienThi ?? 999;

                // LỚP CẢNH VỆ: Kiểm tra trùng vị trí (Loại trừ số 999)
                if (tt.ThuTuHienThi != 999)
                {
                    var checkViTri = db.ThuocTinhs.FirstOrDefault(x => x.ThuTuHienThi == tt.ThuTuHienThi && x.MaDM == tt.MaDM);
                    if (checkViTri != null)
                        return Json(new { status = false, message = $"Vị trí số {tt.ThuTuHienThi} đã được sử dụng. Vui lòng chọn số khác!" });
                }

                db.ThuocTinhs.Add(tt);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm thành công!" });
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
            var tt = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == id);
            return Json(tt, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(ThuocTinh tt)
        {
            try
            {
                var doi = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == tt.MaTT);
                if (doi == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                // Gán mặc định nếu Admin xóa trống ô nhập
                int viTriMoi = tt.ThuTuHienThi ?? 999;

                // LỚP CẢNH VỆ: Check trùng Vị trí hiển thị (Loại trừ chính nó đang sửa và loại trừ số 999)
                if (viTriMoi != 999 && viTriMoi != doi.ThuTuHienThi)
                {
                    var checkViTri = db.ThuocTinhs.FirstOrDefault(x => x.ThuTuHienThi == viTriMoi && x.MaDM == tt.MaDM && x.MaTT != tt.MaTT);
                    if (checkViTri != null)
                        return Json(new { status = false, message = $"Vị trí số {viTriMoi} đã được sử dụng. Vui lòng chọn số khác!" });
                }

                // Cập nhật thông tin
                doi.TenTT = tt.TenTT;
                doi.MaDM = tt.MaDM;
                doi.LaThuocTinhChinh = tt.LaThuocTinhChinh;
                doi.ThuTuHienThi = viTriMoi;

                db.Entry(doi).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Cập nhật thành công!" });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi cập nhật!" });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var tt = db.ThuocTinhs.FirstOrDefault(a => a.MaTT == id);
                db.ThuocTinhs.Remove(tt);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch
            {
                return Json(new { status = false, message = "Không thể xóa do thuộc tính này đang được sử dụng!" });
            }
        }
    }
}