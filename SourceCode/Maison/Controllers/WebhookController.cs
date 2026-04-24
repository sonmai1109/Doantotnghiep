using System;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Maison.Models;
using System.Text.RegularExpressions;

namespace Maison.Controllers
{
    public class WebhookController : Controller
    {
        shopdb db = new shopdb();

        [HttpPost]
        public ActionResult SePayListener()
        {
            try
            {
                // Đọc JSON từ SePay
                Stream req = Request.InputStream;
                req.Seek(0, System.IO.SeekOrigin.Begin);
                string json = new StreamReader(req).ReadToEnd();

                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string noiDungCK = Convert.ToString(data.content);
                decimal soTien = Convert.ToDecimal(data.transferAmount);

                if (!string.IsNullOrEmpty(noiDungCK))
                {
                    // Lọc mã hóa đơn
                    Match match = Regex.Match(noiDungCK.ToUpper(), @"DH(\d+)TS");

                    if (match.Success)
                    {
                        int maHD = int.Parse(match.Groups[1].Value);
                        var hd = db.HoaDons.FirstOrDefault(x => x.MaHD == maHD);

                        // Cập nhật Database
                        if (hd != null && hd.TrangThaiThanhToan == 0)
                        {
                            hd.TrangThaiThanhToan = 1; // Đã thanh toán
                            hd.TrangThai = 1; // Đang giao
                            hd.GhiChu += $" [Đã thanh toán tự động qua SePay: {soTien:N0}đ]";
                            db.SaveChanges();
                        }
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}