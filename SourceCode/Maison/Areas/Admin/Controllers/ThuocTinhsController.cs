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
            ViewBag.locMaDM = locMaDM;

            var thuoctinhs = db.ThuocTinhs.Include(t => t.DanhMuc).AsQueryable();

            if (!string.IsNullOrEmpty(timkiem))
            {
                thuoctinhs = thuoctinhs.Where(t => t.TenTT.Contains(timkiem));
            }

            if (locMaDM != null && locMaDM != 0)
            {
                thuoctinhs = thuoctinhs.Where(t => t.MaDM == locMaDM);
            }

            // --- VẼ CÂY DANH MỤC CHO DROPDOWN THUỘC TÍNH (Được chọn cả Cha lẫn Con) ---
            var danhMucs = db.Danhmucs.ToList();
            var selectListDM = new List<SelectListItem>();

            foreach (var cha in danhMucs.Where(d => d.MaDMCha == null))
            {
                selectListDM.Add(new SelectListItem { Value = cha.MaDM.ToString(), Text = "📁 " + cha.TenDM.ToUpper() });

                foreach (var con in danhMucs.Where(d => d.MaDMCha == cha.MaDM))
                {
                    selectListDM.Add(new SelectListItem { Value = con.MaDM.ToString(), Text = "   --- " + con.TenDM });
                }
            }
            ViewBag.MaDM = selectListDM;
            // --------------------------------------------------------------------------

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
        [HttpGet]
        public JsonResult GetThuocTinhTheoDanhMuc(int maDM)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var dmHienTai = db.Danhmucs.FirstOrDefault(x => x.MaDM == maDM);
            if (dmHienTai == null) return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            // 1. Đổi List<int> thành List<int?>
            List<int?> danhSachID = new List<int?> { dmHienTai.MaDM }; // Lấy ID của Con

            if (dmHienTai.MaDMCha != null)
            {
                // 2. Không cần dùng .Value nữa vì List giờ đã nhận int?
                danhSachID.Add(dmHienTai.MaDMCha);
            }

            // Lấy tất cả thuộc tính (chung + riêng)
            var thuocTinhs = db.ThuocTinhs
                .Where(tt => danhSachID.Contains(tt.MaDM) || tt.MaDM == null) // null là dùng chung toàn hệ thống
                .OrderByDescending(tt => tt.LaThuocTinhChinh)
                .ThenBy(tt => tt.ThuTuHienThi)
                .Select(tt => new { tt.MaTT, tt.TenTT, tt.LaThuocTinhChinh })
                .ToList();
            return Json(thuocTinhs, JsonRequestBehavior.AllowGet);
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