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
                // === LỚP CẢNH VỆ 1: THUẬT TOÁN QUÉT TRÙNG LẶP THEO GIA PHẢ (CHA - CON - GLOBAL) ===
                // Mặc định luôn quét nhóm Dùng chung (null) và chính Danh mục hiện tại
                List<int?> giaPhaID = new List<int?> { null, tt.MaDM };

                if (tt.MaDM != null)
                {
                    var dmHienTai = db.Danhmucs.FirstOrDefault(d => d.MaDM == tt.MaDM);
                    if (dmHienTai != null)
                    {
                        // 1. Quét ngược lên Cha (nếu có)
                        if (dmHienTai.MaDMCha != null)
                            giaPhaID.Add(dmHienTai.MaDMCha);

                        // 2. Quét xuôi xuống tất cả các danh mục Con (nếu nó là Cha)
                        var dsCon = db.Danhmucs.Where(d => d.MaDMCha == tt.MaDM).Select(d => (int?)d.MaDM).ToList();
                        giaPhaID.AddRange(dsCon);
                    }
                }

                // Kiểm tra xem Tên Thuộc Tính đã tồn tại bất cứ đâu trong dòng họ (Gia phả) này chưa?
                var checkTrungTen = db.ThuocTinhs.FirstOrDefault(x =>
                    x.TenTT.ToLower() == tt.TenTT.ToLower() &&
                    giaPhaID.Contains(x.MaDM));

                if (checkTrungTen != null)
                {
                    string tenNoiTrung = checkTrungTen.MaDM == null ? "Dùng chung (Toàn hệ thống)" : checkTrungTen.DanhMuc.TenDM;
                    return Json(new
                    {
                        status = false,
                        message = $"Từ chối: Thuộc tính '{tt.TenTT}' đã được khai báo ở danh mục '{tenNoiTrung}'. Hệ thống sẽ tự kế thừa, bạn không được tạo trùng!"
                    });
                }
                // ============================================================================

                // Nếu không nhập thứ tự hiển thị, gán mặc định là 999 (Xếp bét)
                tt.ThuTuHienThi = tt.ThuTuHienThi ?? 999;

                // LỚP CẢNH VỆ 2: Kiểm tra trùng vị trí (Loại trừ số 999)
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

                // === LỚP CẢNH VỆ 1: QUÉT TRÙNG LẶP KHI ĐỔI TÊN / ĐỔI DANH MỤC ===
                List<int?> giaPhaID = new List<int?> { null, tt.MaDM };

                if (tt.MaDM != null)
                {
                    var dmHienTai = db.Danhmucs.FirstOrDefault(d => d.MaDM == tt.MaDM);
                    if (dmHienTai != null)
                    {
                        if (dmHienTai.MaDMCha != null) giaPhaID.Add(dmHienTai.MaDMCha);
                        var dsCon = db.Danhmucs.Where(d => d.MaDMCha == tt.MaDM).Select(d => (int?)d.MaDM).ToList();
                        giaPhaID.AddRange(dsCon);
                    }
                }

                // Lưu ý: Phải loại trừ chính cái thuộc tính đang sửa ra khỏi việc kiểm tra (x.MaTT != tt.MaTT)
                var checkTrungTen = db.ThuocTinhs.FirstOrDefault(x =>
                    x.MaTT != tt.MaTT &&
                    x.TenTT.ToLower() == tt.TenTT.ToLower() &&
                    giaPhaID.Contains(x.MaDM));

                if (checkTrungTen != null)
                {
                    string tenNoiTrung = checkTrungTen.MaDM == null ? "Dùng chung (Toàn hệ thống)" : checkTrungTen.DanhMuc.TenDM;
                    return Json(new
                    {
                        status = false,
                        message = $"Từ chối: Tên '{tt.TenTT}' đã bị trùng với danh mục '{tenNoiTrung}' trong cùng nhánh!"
                    });
                }
                // =================================================================

                // Gán mặc định nếu Admin xóa trống ô nhập
                int viTriMoi = tt.ThuTuHienThi ?? 999;

                // LỚP CẢNH VỆ 2: Check trùng Vị trí hiển thị (Loại trừ chính nó đang sửa và loại trừ số 999)
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