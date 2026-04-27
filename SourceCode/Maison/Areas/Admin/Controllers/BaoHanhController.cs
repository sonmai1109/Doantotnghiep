using Maison.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;

namespace Maison.Areas.Admin.Controllers
{
    public class BaoHanhController : BaseController // Nhớ đổi thành BaseController nếu có
    {
        shopdb db = new shopdb();

        // 1. DANH SÁCH PHIẾU BẢO HÀNH
        [HttpGet]
        public ActionResult Index(string searchString, int? status, int page = 1, int pageSize = 10)
        {
            // Lấy danh sách phiếu BH kèm thông tin Khách hàng, Hóa đơn và Sản phẩm (Biến thể)
            var query = db.Baohanhs
                .Include(b => b.TaiKhoanNguoiDung)
                .Include(b => b.HoaDon)
                .Include(b => b.BienThe.Sanpham)
                .AsQueryable();

            if (status != null)
            {
                query = query.Where(x => x.TrangThai == status);
                ViewBag.Status = status;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.searchString = searchString;
                // Tìm theo số điện thoại khách hoặc Mã phiếu
                query = query.Where(b => b.TaiKhoanNguoiDung.SoDienThoai.Contains(searchString) || b.MaPhieu.ToString() == searchString);
            }

            return View(query.OrderByDescending(b => b.NgayTiepNhan).ToPagedList(page, pageSize));
        }

        // 2. LẤY CHI TIẾT PHIẾU ĐỂ HIỆN MODAL
        [HttpPost]
        public JsonResult GetDetails(int id)
        {
            try
            {
                var bh = db.Baohanhs
                      .Include(b => b.TaiKhoanNguoiDung)
                      .Include(b => b.BienThe.Sanpham)
                      // Kéo sâu thêm 1 tầng xuống bảng ThuocTinh
                      .Include(b => b.BienThe.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh))
                      .FirstOrDefault(b => b.MaPhieu == id);

                if (bh == null) return Json(new { error = "Không tìm thấy phiếu bảo hành" });

                // --- TRONG BAOHANHCONTROLLER.CS ---

                string cauHinh = "Bản tiêu chuẩn";
                if (bh.BienThe.ChiTietBTs.Any())
                {
                    // Lọc: Chỉ lấy những giá trị thuộc về Thuộc Tính Chính
                    var dsChinh = bh.BienThe.ChiTietBTs
                        .Where(cb => cb.GiaTriTT != null &&
                                     cb.GiaTriTT.ThuocTinh != null &&
                                     cb.GiaTriTT.ThuocTinh.LaThuocTinhChinh == true) // LỌC Ở ĐÂY
                        .OrderBy(cb => cb.GiaTriTT.ThuocTinh.ThuTuHienThi)
                        .Select(cb => cb.GiaTriTT.GiaTri)
                        .ToList();

                    if (dsChinh.Any())
                    {
                        cauHinh = string.Join(" | ", dsChinh);
                    }
                    else
                    {
                        // Nếu không có cái nào là chính thì hiện 3 cái đầu tiên cho gọn
                        cauHinh = string.Join(" | ", bh.BienThe.ChiTietBTs.Take(3).Select(cb => cb.GiaTriTT.GiaTri));
                    }
                }

                var result = new
                {
                    MaPhieu = bh.MaPhieu,
                    MaHD = bh.MaHD,
                    TenKhach = bh.TaiKhoanNguoiDung.HoTen,
                    SDT = bh.TaiKhoanNguoiDung.SoDienThoai,
                    TenSP = bh.BienThe.Sanpham.TenSP,
                    CauHinh = cauHinh,
                    TinhTrangLoi = bh.TinhTrangLoi,
                    NoiDungSuaChua = bh.NoiDungSuaChua ?? "",
                    ChiPhiSuaChua = bh.ChiPhiSuaChua ?? 0,
                    NgayTiepNhan = bh.NgayTiepNhan?.ToString("yyyy-MM-dd"), // Format chuẩn để gán vào input type="date"
                    NgayHenTra = bh.NgayHenTra?.ToString("yyyy-MM-dd"),
                    TrangThai = bh.TrangThai
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi: " + ex.Message });
            }
        }

        // 3. CẬP NHẬT TRẠNG THÁI VÀ THÔNG TIN SỬA CHỮA
        [HttpPost]
        public JsonResult UpdateBaoHanh(int maPhieu, int trangThai, string noiDung, decimal chiPhi, DateTime? ngayHenTra)
        {
            try
            {
                Baohanh bh = db.Baohanhs.FirstOrDefault(x => x.MaPhieu == maPhieu);
                if (bh == null) return Json(new { status = false, message = "Không tìm thấy phiếu" });

                // Không cho phép khôi phục nếu đã Hủy (0) hoặc Hoàn thành (4)
                if ((bh.TrangThai == 0 || bh.TrangThai == 4) && trangThai != bh.TrangThai)
                {
                    return Json(new { status = false, message = "Phiếu này đã đóng, không thể đổi trạng thái!" });
                }

                bh.TrangThai = trangThai;
                bh.NoiDungSuaChua = noiDung;
                bh.ChiPhiSuaChua = chiPhi;
                if (ngayHenTra.HasValue)
                {
                    bh.NgayHenTra = ngayHenTra;
                }

                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        // 4. TRANG TẠO PHIẾU BẢO HÀNH MỚI (Dành cho Admin tra cứu SĐT khách)
        [HttpGet]
        public ActionResult Create(string sdt)
        {
            ViewBag.SDT = sdt;

            // Nếu Admin có gõ SĐT để tìm kiếm
            if (!string.IsNullOrEmpty(sdt))
            {
                // Tìm khách hàng
                var khach = db.TaiKhoanNguoiDungs.FirstOrDefault(x => x.SoDienThoai == sdt);
                if (khach != null)
                {
                    ViewBag.KhachHang = khach;

                    // Lấy ra danh sách CÁC SẢN PHẨM KHÁCH ĐÃ MUA (Hóa đơn trạng thái 3 hoặc 4)
                    var dsDaMua = db.ChiTietHoaDons
                          .Include(c => c.HoaDon)
                          .Include(c => c.BienThe.Sanpham)
                          // Kéo sâu thêm 1 tầng xuống bảng ThuocTinh
                          .Include(c => c.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT.ThuocTinh))
                          .Where(c => c.HoaDon.MaTK == khach.MaTK && c.HoaDon.TrangThai == 3)
                          .OrderByDescending(c => c.HoaDon.NgayDat)
                          .ToList();

                    return View(dsDaMua);
                }
                else
                {
                    ViewBag.Error = "Không tìm thấy khách hàng nào với Số điện thoại này!";
                }
            }
            return View(); // Trả về view trống nếu chưa tìm
        }

        // 5. XỬ LÝ LƯU PHIẾU BẢO HÀNH MỚI VÀO DATABASE
        // 5. XỬ LÝ LƯU PHIẾU BẢO HÀNH MỚI VÀO DATABASE
        [HttpPost]
        public JsonResult CreateTicket(int maHD, int maBT, int maTK, string loiKhachBao)
        {
            try
            {
                Baohanh bh = new Baohanh();
                bh.MaHD = maHD;
                bh.MaBT = maBT;
                bh.MaTK = maTK;
                bh.TinhTrangLoi = loiKhachBao;
                bh.NgayTiepNhan = DateTime.Now;
                bh.TrangThai = 1; // 1 = Chờ tiếp nhận / Đang xử lý

                // ---> BƠM THÊM GIÁ TRỊ MẶC ĐỊNH ĐỂ CHỐNG LỖI SQL SERVER BẮT BUỘC NHẬP <---
                bh.ChiPhiSuaChua = 0;
                bh.NoiDungSuaChua = "Chưa có";
                bh.NgayHenTra = DateTime.Now.AddDays(7); // Mặc định hẹn khách 7 ngày trả máy
                // --------------------------------------------------------------------------

                db.Baohanhs.Add(bh);
                db.SaveChanges();

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                // ---> ĐOẠN CODE "BỚI MÓC" LỖI THẬT SỰ NẰM SÂU BÊN TRONG <---
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg = ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMsg = ex.InnerException.InnerException.Message;
                    }
                }

                // Trả về lỗi chi tiết lên màn hình cho bạn xem
                return Json(new { status = false, message = "Lỗi SQL: " + errorMsg });
            }
        }
    }
}