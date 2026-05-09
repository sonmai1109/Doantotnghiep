using Maison.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Maison.Areas.Admin.Controllers
{
    public class ThongKeController : BaseController // Nhớ kế thừa BaseController để bảo mật
    {
        shopdb db = new shopdb();

        // 1. TRANG HIỂN THỊ GIAO DIỆN
        [HttpGet]
        public ActionResult Index()
        {
            // Mặc định load ra dữ liệu của tháng hiện tại (Từ ngày 1 đến ngày hiện tại)
            ViewBag.TuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            ViewBag.DenNgay = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // 2. API TRẢ VỀ DỮ LIỆU JSON ĐỂ VẼ BIỂU ĐỒ (CHART.JS)
        [HttpPost]
        public JsonResult GetDoanhThuTongHop(string tuNgay, string denNgay, string loai)
        {
            try
            {
                DateTime start = DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null);
                DateTime end = DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).AddHours(23).AddMinutes(59).AddSeconds(59);

                // 1. Lấy dữ liệu thực tế từ Database (Chỉ lấy đơn Hoàn Thành)
                var data = db.HoaDons
                    .Include(h => h.ChiTietHoaDons.Select(c => c.BienThe))
                    .Where(h => h.TrangThai == 3 && h.NgayDat >= start && h.NgayDat <= end)
                    .ToList();

                // 2. TẠO TRỤC THỜI GIAN CHUẨN (KHÔNG BỊ LỦNG LỖ)
                var allLabels = new List<string>();
                if (loai == "nam")
                {
                    for (int y = start.Year; y <= end.Year; y++)
                        allLabels.Add(y.ToString("0000"));
                }
                else if (loai == "thang")
                {
                    var current = new DateTime(start.Year, start.Month, 1);
                    while (current <= end)
                    {
                        allLabels.Add(current.ToString("MM/yyyy"));
                        current = current.AddMonths(1);
                    }
                }
                else // Mặc định là Từng ngày
                {
                    for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
                        allLabels.Add(d.ToString("dd/MM/yyyy"));
                }

                // 3. Gom nhóm dữ liệu thực tế theo nhãn thời gian
                var groupedData = data.GroupBy(h => {
                    if (loai == "thang") return h.NgayDat.ToString("MM/yyyy");
                    if (loai == "nam") return h.NgayDat.ToString("yyyy");
                    return h.NgayDat.ToString("dd/MM/yyyy");
                })
                .ToDictionary(g => g.Key, g => new
                {
                    DoanhThu = g.Sum(h => h.ChiTietHoaDons.Sum(c => c.GiaMua * c.SoLuongMua)),
                    Von = g.Sum(h => h.ChiTietHoaDons.Sum(c => (c.BienThe.GiaNhap ?? 0) * c.SoLuongMua)),
                    SoDon = g.Count()
                });

                // 4. ĐẮP DỮ LIỆU VÀO TRỤC THỜI GIAN CHUẨN (Nếu không có thì gán = 0)
                var finalResult = allLabels.Select(label => {
                    if (groupedData.ContainsKey(label))
                    {
                        var item = groupedData[label];
                        return new { Label = label, DoanhThu = item.DoanhThu, LoiNhuan = item.DoanhThu - item.Von, SoDon = item.SoDon };
                    }
                    else
                    {
                        // Ngày nào không có đơn sẽ được gán số 0, tránh bị nhảy cóc
                        return new { Label = label, DoanhThu = 0m, LoiNhuan = 0m, SoDon = 0 };
                    }
                }).ToList();

                return Json(new
                {
                    status = true,
                    labels = finalResult.Select(x => x.Label).ToArray(),
                    doanhThu = finalResult.Select(x => x.DoanhThu).ToArray(),
                    loiNhuan = finalResult.Select(x => x.LoiNhuan).ToArray(),
                    soDon = finalResult.Select(x => x.SoDon).ToArray(),
                    tongDoanhThu = finalResult.Sum(x => x.DoanhThu),
                    tongLoiNhuan = finalResult.Sum(x => x.LoiNhuan),
                    tongSoDon = finalResult.Sum(x => x.SoDon)
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi tính toán: " + ex.Message });
            }
        }
        // 3. API LẤY TOP SẢN PHẨM BÁN CHẠY & BÁN CHẬM
        // 3. API LẤY TOP SẢN PHẨM BÁN CHẠY & BÁN CHẬM
        [HttpPost]
        public JsonResult GetTopSanpham(string tuNgay, string denNgay)
        {
            try
            {
                DateTime start = DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null);
                DateTime end = DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).AddHours(23).AddMinutes(59).AddSeconds(59);

                // 1. Lấy danh sách tất cả các Biến thể đang kinh doanh (TrangThai == true)
                var tatCaBienThe = db.BienThes
                    .Include(b => b.Sanpham)
                    .Include(b => b.ChiTietBTs.Select(ct => ct.GiaTriTT.ThuocTinh))
                    .Where(b => b.TrangThai == true) // Chỉ tính hàng đang mở bán
                    .ToList();

                // 2. Lấy chi tiết các hóa đơn hợp lệ (TrangThai != 0) trong khoảng thời gian
                var chiTietHoaDon = db.ChiTietHoaDons
                    .Where(c => c.HoaDon.NgayDat >= start && c.HoaDon.NgayDat <= end && c.HoaDon.TrangThai != 0)
                    .ToList();

                // 3. Tính toán số lượng bán cho từng biến thể
                var thongKeBienThe = tatCaBienThe.Select(bt => {
                    // Tính tổng số lượng đã bán của biến thể này trong kỳ
                    var banTrongKy = chiTietHoaDon.Where(c => c.MaBT == bt.MaBT);
                    int tongSl = banTrongKy.Any() ? banTrongKy.Sum(x => x.SoLuongMua) : 0;
                    decimal tongDt = banTrongKy.Any() ? banTrongKy.Sum(x => x.SoLuongMua * x.GiaMua) : 0m;

                    // Nối chuỗi cấu hình (Chỉ lấy thuộc tính chính + Sắp xếp thứ tự)
                    var cauHinh = string.Join(" / ", bt.ChiTietBTs
                        .Where(ct => ct.GiaTriTT.ThuocTinh.LaThuocTinhChinh == true)
                        .OrderBy(ct => ct.GiaTriTT.ThuocTinh.ThuTuHienThi)
                        .Select(ct => ct.GiaTriTT.GiaTri));

                    return new
                    {
                        MaBT = bt.MaBT,
                        TenSP = bt.Sanpham.TenSP,
                        CauHinh = cauHinh,
                        HinhAnh = bt.HinhAnh ?? bt.Sanpham.HinhAnh,
                        SoLuongBan = tongSl,
                        DoanhThuMangLai = tongDt
                    };
                }).ToList();

                // 4. Phân loại theo yêu cầu của bạn
                // Bán chạy: Sắp xếp giảm dần, lấy những cái có bán được hàng (Sl > 0)
                var topBanChay = thongKeBienThe
                    .Where(x => x.SoLuongBan > 0)
                    .OrderByDescending(x => x.SoLuongBan)
                    .Take(5).ToList();

                // Bán chậm: Sắp xếp tăng dần (Những cái bằng 0 sẽ tự động nhảy lên đầu)
                var topBanCham = thongKeBienThe
                    .OrderBy(x => x.SoLuongBan)
                    .Take(5).ToList();

                return Json(new { status = true, topBanChay = topBanChay, topBanCham = topBanCham });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi logic: " + ex.Message });
            }
        }
    }
}