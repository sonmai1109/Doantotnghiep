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
                // 1. Ép kiểu ngày tháng an toàn
                DateTime start = DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null);
                DateTime end = DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).AddHours(23).AddMinutes(59).AddSeconds(59);

                // 2. Truy vấn dữ liệu: Chỉ lấy đơn Hoàn Thành (3), Kéo tới tận Biến Thể để lấy Giá Nhập
                var data = db.HoaDons
                    .Include(h => h.ChiTietHoaDons.Select(c => c.BienThe))
                    .Where(h => h.TrangThai == 3 && h.NgayDat >= start && h.NgayDat <= end)
                    .ToList();

                // 3. Nhóm dữ liệu linh hoạt theo Ngày/Tháng/Năm
                var grouped = data.GroupBy(h => {
                    if (loai == "thang") return h.NgayDat.ToString("MM/yyyy");
                    if (loai == "nam") return h.NgayDat.ToString("yyyy");
                    return h.NgayDat.ToString("dd/MM/yyyy");
                })
                .Select(g => {
                    // Tổng doanh thu của nhóm
                    decimal doanhThu = g.Sum(h => h.ChiTietHoaDons.Sum(c => c.GiaMua * c.SoLuongMua));

                    // Tổng vốn của nhóm (Giá nhập * Số lượng). Nếu Giá nhập Null thì coi như = 0
                    decimal von = g.Sum(h => h.ChiTietHoaDons.Sum(c => (c.BienThe.GiaNhap ?? 0) * c.SoLuongMua));

                    return new
                    {
                        Label = g.Key,
                        DoanhThu = doanhThu,
                        LoiNhuan = doanhThu - von,
                        SoDon = g.Count()
                    };
                })
                // Sắp xếp lại danh sách theo thời gian để biểu đồ chạy đúng từ trái sang phải
                .OrderBy(x => loai == "nam" ? DateTime.ParseExact(x.Label, "yyyy", null) :
                              loai == "thang" ? DateTime.ParseExact(x.Label, "MM/yyyy", null) :
                              DateTime.ParseExact(x.Label, "dd/MM/yyyy", null))
                .ToList();

                return Json(new
                {
                    status = true,
                    labels = grouped.Select(x => x.Label).ToArray(),
                    doanhThu = grouped.Select(x => x.DoanhThu).ToArray(),
                    loiNhuan = grouped.Select(x => x.LoiNhuan).ToArray(),
                    soDon = grouped.Select(x => x.SoDon).ToArray(),
                    tongDoanhThu = grouped.Sum(x => x.DoanhThu),
                    tongLoiNhuan = grouped.Sum(x => x.LoiNhuan),
                    tongSoDon = grouped.Sum(x => x.SoDon)
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi tính toán: " + ex.Message });
            }
        }
    }
}