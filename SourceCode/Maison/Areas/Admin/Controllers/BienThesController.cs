using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maison.Models;
using System.Data.Entity;
using System.IO;
using System.Web.Script.Serialization;

namespace Maison.Areas.Admin.Controllers // Đổi namespace nếu cần
{
    public class BienThesController : BaseController
    {
        shopdb db = new shopdb();

        // 1. TRANG DANH SÁCH BIẾN THỂ CỦA 1 SẢN PHẨM CỤ THỂ
        public ActionResult Index(int? maSP)
        {
            // BƯỚC BẢO VỆ: Nếu URL không có mã SP, quay về trang danh sách sản phẩm
            if (maSP == null) return RedirectToAction("Index", "Sanphams");

            ViewBag.MaSP = maSP;

            // Tìm sản phẩm gốc để lấy Tên và Danh Mục
            var sp = db.Sanphams.FirstOrDefault(s => s.MaSP == maSP);
            if (sp == null) return RedirectToAction("Index", "Sanphams");

            ViewBag.TenSP = sp.TenSP;
            ViewBag.MaDM = sp.MaDM; // Cực kỳ quan trọng để JS biết load thông số nào

            // Lấy danh sách biến thể của sản phẩm này (kèm thông số + KÈM THƯ VIỆN ẢNH)
            var bienthes = db.BienThes
                             .Include(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                             .Include(b => b.ThuVienAnhs) // Dòng phép thuật để hiện Gallery Ảnh phụ
                             .Where(bt => bt.MaSP == maSP)
                             .OrderByDescending(bt => bt.MaBT)
                             .ToList();

            return View(bienthes);
        }

        // ==========================================
        // 2. API: LẤY THUỘC TÍNH (RAM, CPU...) KÈM CÁC GIÁ TRỊ CŨ (8GB, 16GB...)
        // ==========================================
        [HttpPost]
        public JsonResult LoadThuocTinhTheoDanhMuc(int maDM)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var data = db.ThuocTinhs
                         .Where(t => t.MaDM == maDM || t.MaDM == null)
                         .Select(t => new {
                             MaTT = t.MaTT,
                             TenTT = t.TenTT,
                             GiaTris = t.GiaTriTTs.Select(g => new { g.MaGT, g.GiaTri }).ToList()
                         }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // ==========================================
        // 3. API: LOAD DỮ LIỆU ĐỂ SỬA BIẾN THỂ
        // ==========================================
        [HttpPost]
        public JsonResult Loaddata(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var bt = db.BienThes.Where(a => a.MaBT == id).Select(b => new {
                b.MaBT,
                b.GiaBan,
                b.SoLuongTon,
                // Lấy mảng các ID Giá Trị (MaGT) đang được cấu hình cho biến thể này
                CacThuocTinhDangChon = b.ChiTietBTs.Select(c => new { c.GiaTriTT.MaTT, c.MaGT }).ToList()
            }).FirstOrDefault();

            return Json(bt, JsonRequestBehavior.AllowGet);
        }

        // ==========================================
        // 4. HÀM DÙNG CHUNG: XỬ LÝ GÁN THÔNG SỐ (DROPDOWN + GÕ TAY)
        // ==========================================
        private void ProcessAttributes(int maBT, FormCollection form)
        {
            // Xóa toàn bộ cấu hình cũ của biến thể này (nếu có) để cập nhật lại từ đầu
            var oldDetails = db.ChiTietBTs.Where(c => c.MaBT == maBT).ToList();
            if (oldDetails.Count > 0)
            {
                db.ChiTietBTs.RemoveRange(oldDetails);
                db.SaveChanges();
            }

            // Quét dữ liệu gửi lên từ giao diện
            foreach (string key in form.AllKeys)
            {
                if (key.StartsWith("ThuocTinh_Select_"))
                {
                    int maTT = int.Parse(key.Replace("ThuocTinh_Select_", ""));
                    string selectValue = form[key].Trim(); // Lấy ID cũ hoặc chữ "NEW"

                    if (string.IsNullOrEmpty(selectValue)) continue;

                    int finalMaGT = 0;

                    if (selectValue == "NEW")
                    {
                        // Lấy giá trị gõ tay ở ô Input ẩn
                        string newText = form["ThuocTinh_New_" + maTT]?.Trim();
                        if (!string.IsNullOrEmpty(newText))
                        {
                            // Kiểm tra xem vô tình chữ gõ tay đã tồn tại chưa
                            var exist = db.GiaTriTTs.FirstOrDefault(g => g.MaTT == maTT && g.GiaTri.ToLower() == newText.ToLower());
                            if (exist != null)
                            {
                                finalMaGT = exist.MaGT;
                            }
                            else
                            {
                                // Chưa có thì tạo luôn vào DB
                                GiaTriTT gtMoi = new GiaTriTT { MaTT = maTT, GiaTri = newText };
                                db.GiaTriTTs.Add(gtMoi);
                                db.SaveChanges();
                                finalMaGT = gtMoi.MaGT;
                            }
                        }
                    }
                    else
                    {
                        // Nếu chọn Dropdown bình thường -> Ép kiểu sang ID
                        finalMaGT = int.Parse(selectValue);
                    }

                    // Tiến hành gán vào bảng nối ChiTietBT
                    if (finalMaGT > 0)
                    {
                        db.ChiTietBTs.Add(new ChiTietBT { MaBT = maBT, MaGT = finalMaGT });
                    }
                }
            }
            db.SaveChanges();
        }

        // ==========================================
        // 5. THÊM MỚI BIẾN THỂ
        // ==========================================
        [HttpPost]
        public JsonResult Create(BienThe bt, HttpPostedFileBase ImageFile, FormCollection form, HttpPostedFileBase[] GalleryFiles)
        {
            // Bật Transaction để đảm bảo: Nếu lỗi ở 1 bước bất kỳ, toàn bộ dữ liệu sẽ tự Rollback (thu hồi) không tạo ra rác.
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // --- BƯỚC 1: Bắt Session Audit Log ---
                    TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                    if (tk != null)
                    {
                        bt.NgayTao = DateTime.Now;
                        bt.NguoiTao = tk.HoTen;
                        bt.NgaySua = DateTime.Now;
                        bt.NguoiSua = tk.HoTen;
                    }
                    bt.TrangThai = true;

                    // --- BƯỚC 2: Xử lý ẢNH CHÍNH (Thumbnail) ---
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string dirPath = Server.MapPath("~/Content/Images/BienThes/");
                        if (!System.IO.Directory.Exists(dirPath))
                            System.IO.Directory.CreateDirectory(dirPath);

                        string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                        string path = System.IO.Path.Combine(dirPath, fileName);
                        ImageFile.SaveAs(path);
                        bt.HinhAnh = "/Content/Images/BienThes/" + fileName;
                    }

                    // --- BƯỚC 3: Lưu Biến thể vào DB (Để lấy được mã MaBT sinh tự động) ---
                    db.BienThes.Add(bt);
                    db.SaveChanges();

                    // --- BƯỚC 4: Xử lý ẢNH PHỤ (Gallery - Nhiều ảnh) ---
                    if (GalleryFiles != null && GalleryFiles.Length > 0)
                    {
                        string galleryPath = Server.MapPath("~/Content/Images/ThuVienAnhs/");
                        if (!System.IO.Directory.Exists(galleryPath))
                            System.IO.Directory.CreateDirectory(galleryPath);

                        foreach (var file in GalleryFiles)
                        {
                            // Lọc qua từng file, bỏ qua nếu file bị null
                            if (file != null && file.ContentLength > 0)
                            {
                                string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(file.FileName);
                                string path = System.IO.Path.Combine(galleryPath, fileName);
                                file.SaveAs(path);

                                // Lưu xuống bảng ThuVienAnh, móc với cái MaBT vừa tạo ở Bước 3
                                db.ThuVienAnhs.Add(new ThuVienAnh
                                {
                                    MaBT = bt.MaBT,
                                    DuongDanAnh = "/Content/Images/ThuVienAnhs/" + fileName
                                });
                            }
                        }
                        db.SaveChanges(); // Cập nhật mảng ảnh phụ xuống DB
                    }

                    // --- BƯỚC 5: Xử lý các Thuộc tính (RAM, CPU, VGA...) ---
                    ProcessAttributes(bt.MaBT, form);

                    // Hoàn thành xuất sắc, đóng chốt Database!
                    transaction.Commit();
                    return Json(new { status = true, message = "Thêm cấu hình và thư viện ảnh thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { status = false, message = "Lỗi hệ thống: " + ex.Message });
                }
            }
        }

        // ==========================================
        // 6. CẬP NHẬT BIẾN THỂ
        // ==========================================
        [HttpPost]
        // Đã thêm tham số List<int> sortedGalleryIds vào cuối
        public JsonResult Update(BienThe bt, HttpPostedFileBase ImageFile, FormCollection form, List<int> sortedGalleryIds)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var doi = db.BienThes.FirstOrDefault(a => a.MaBT == bt.MaBT);
                    if (doi == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu cấu hình!" });

                    // 1. Cập nhật thông tin cơ bản
                    doi.GiaBan = bt.GiaBan;
                    doi.SoLuongTon = bt.SoLuongTon;

                    TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                    if (tk != null)
                    {
                        doi.NgaySua = DateTime.Now;
                        doi.NguoiSua = tk.HoTen;
                    }

                    // 2. Cập nhật Ảnh chính (Nếu có)
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string dirPath = Server.MapPath("~/Content/Images/BienThes/");
                        if (!System.IO.Directory.Exists(dirPath))
                            System.IO.Directory.CreateDirectory(dirPath);

                        string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(ImageFile.FileName);
                        ImageFile.SaveAs(System.IO.Path.Combine(dirPath, fileName));
                        doi.HinhAnh = "/Content/Images/BienThes/" + fileName;
                    }

                    // 3. LƯU LẠI VỊ TRÍ ẢNH PHỤ SAU KHI KÉO THẢ
                    if (sortedGalleryIds != null && sortedGalleryIds.Count > 0)
                    {
                        for (int i = 0; i < sortedGalleryIds.Count; i++)
                        {
                            int idAnh = sortedGalleryIds[i];
                            var img = db.ThuVienAnhs.FirstOrDefault(x => x.MaAnh == idAnh);
                            if (img != null)
                            {
                                img.ThuTu = i; // Đánh lại số thứ tự (0, 1, 2...) theo đúng mảng gửi lên
                            }
                        }
                    }

                    // 4. Cập nhật lại Thuộc tính
                    ProcessAttributes(doi.MaBT, form);

                    db.Entry(doi).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { status = true, message = "Cập nhật cấu hình thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { status = false, message = "Lỗi cập nhật: " + ex.Message });
                }
            }
        }
        // API Sửa nhanh thông số trực tiếp tại trang Biến Thể
        [HttpPost]
        public JsonResult SuaGiaTriNhanh(int maGT, string giaTriMoi)
        {
            try
            {
                giaTriMoi = giaTriMoi.Trim();
                var gt = db.GiaTriTTs.FirstOrDefault(g => g.MaGT == maGT);
                if (gt == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                // Kiểm tra xem chữ mới có bị trùng không
                var check = db.GiaTriTTs.FirstOrDefault(g => g.MaTT == gt.MaTT && g.GiaTri.ToLower() == giaTriMoi.ToLower() && g.MaGT != maGT);
                if (check != null) return Json(new { status = false, message = "Lỗi: Giá trị này đã tồn tại trong danh sách chọn rồi!" });

                gt.GiaTri = giaTriMoi;
                db.Entry(gt).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message });
            }
        }
        // API Thêm nhanh thông số (Bấm nút cạnh ô nhập)
        [HttpPost]
        public JsonResult ThemGiaTriNhanh(int maTT, string giaTri)
        {
            try
            {
                giaTri = giaTri.Trim();
                // Check trùng
                var check = db.GiaTriTTs.FirstOrDefault(g => g.MaTT == maTT && g.GiaTri.ToLower() == giaTri.ToLower());
                if (check != null) return Json(new { status = false, message = "Thông số này đã tồn tại!" });

                GiaTriTT gt = new GiaTriTT { MaTT = maTT, GiaTri = giaTri };
                db.GiaTriTTs.Add(gt);
                db.SaveChanges();

                // Quan trọng: Trả về cái MaGT mới được tạo để Javascript chọn nó luôn
                return Json(new { status = true, message = "Đã lưu!", id = gt.MaGT, text = gt.GiaTri });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        // 1. API Lấy danh sách ảnh của Biến thể (Đã sắp xếp theo ThuTu)
        [HttpGet]
        public JsonResult GetGallery(int maBT)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var list = db.ThuVienAnhs.Where(x => x.MaBT == maBT)
                                     .OrderBy(x => x.ThuTu)
                                     .Select(x => new { x.MaAnh, x.DuongDanAnh })
                                     .ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // 2. API Xóa 1 ảnh phụ
        [HttpPost]
        public JsonResult DeleteGalleryImage(int maAnh)
        {
            var img = db.ThuVienAnhs.Find(maAnh);
            if (img != null)
            {
                // Tùy chọn: Xóa luôn file vật lý trên ổ cứng cho nhẹ Server
                string fullPath = Request.MapPath(img.DuongDanAnh);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                db.ThuVienAnhs.Remove(img);
                db.SaveChanges();
                return Json(new { status = true });
            }
            return Json(new { status = false });
        }
        [HttpPost]
        public JsonResult UploadGalleryAjax(int maBT, HttpPostedFileBase[] files)
        {
            try
            {
                if (files != null && files.Length > 0)
                {
                    string galleryPath = Server.MapPath("~/Content/Images/ThuVienAnhs/");
                    if (!System.IO.Directory.Exists(galleryPath))
                        System.IO.Directory.CreateDirectory(galleryPath);

                    // 1. Tìm số thứ tự lớn nhất hiện tại của biến thể này
                    // Nếu chưa có ảnh nào thì bắt đầu từ -1, ảnh đầu tiên sẽ là 0
                    int maxThuTu = db.ThuVienAnhs
                                     .Where(x => x.MaBT == maBT)
                                     .Select(x => (int?)x.ThuTu)
                                     .Max() ?? -1;

                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            maxThuTu++; // Tự động tăng số thứ tự lên 1 cho ảnh tiếp theo

                            string fileName = DateTime.Now.Ticks + "_" + System.IO.Path.GetFileName(file.FileName);
                            file.SaveAs(System.IO.Path.Combine(galleryPath, fileName));

                            db.ThuVienAnhs.Add(new ThuVienAnh
                            {
                                MaBT = maBT,
                                DuongDanAnh = "/Content/Images/ThuVienAnhs/" + fileName,
                                ThuTu = maxThuTu // Gán số thứ tự tự động tăng
                            });
                        }
                    }
                    db.SaveChanges();
                    return Json(new { status = true, message = "Đã tải ảnh lên!" });
                }
                return Json(new { status = false, message = "Không có file nào được chọn" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        // 3. API Lưu thứ tự ảnh khi Admin kéo thả
        [HttpPost]
        public JsonResult UpdateGalleryOrder(List<int> sortedIds)
        {
            if (sortedIds != null && sortedIds.Count > 0)
            {
                for (int i = 0; i < sortedIds.Count; i++)
                {
                    int id = sortedIds[i];
                    var img = db.ThuVienAnhs.Find(id);
                    if (img != null) img.ThuTu = i; // Gán thứ tự mới (0, 1, 2...)
                }
                db.SaveChanges();
            }
            return Json(new { status = true });
        }

        // ==========================================
        // 7. XÓA BIẾN THỂ
        // ==========================================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var bt = db.BienThes.FirstOrDefault(a => a.MaBT == id);
                    if (bt == null) return Json(new { status = false, message = "Không tìm thấy dữ liệu!" });

                    // Xóa cấu hình ở bảng trung gian ChiTietBT trước
                    var chitiet = db.ChiTietBTs.Where(c => c.MaBT == id).ToList();
                    if (chitiet.Count > 0)
                    {
                        db.ChiTietBTs.RemoveRange(chitiet);
                        db.SaveChanges();
                    }

                    // Xóa Biến thể
                    db.BienThes.Remove(bt);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { status = true });
                }
                catch
                {
                    transaction.Rollback();
                    return Json(new { status = false, message = "Không thể xóa vì biến thể này đã có trong Hóa Đơn!" });
                }
            }
        }
    }
}