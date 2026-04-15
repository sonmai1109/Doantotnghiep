using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using Maison.Session;
using System.Data.Entity;

namespace Maison.Controllers
{
    public class BaoHanhController : Controller
    {
        shopdb db = new shopdb();

        // 1. TRANG DANH SÁCH BẢO HÀNH CỦA KHÁCH
        [HttpGet]
        public ActionResult MyWarranties()
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            // Kéo dữ liệu Bảo Hành kèm theo Cấu Hình (ChiTietBTs -> GiaTriTT)
            var listBH = db.Baohanhs
                .Include(b => b.BienThe.Sanpham)
                .Include(b => b.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT))
                .Where(b => b.MaTK == tk.MaTK)
                .OrderByDescending(b => b.NgayTiepNhan)
                .ToList();

            return View(listBH);
        }

        // 2. TRANG XEM CHI TIẾT 1 PHIẾU BẢO HÀNH
        [HttpGet]
        public ActionResult Details(int id)
        {
            TaiKhoanNguoiDung tk = (TaiKhoanNguoiDung)Session[ConstaintUser.USER_SESSION];
            if (tk == null) return RedirectToAction("Login", "Home");

            var bh = db.Baohanhs
                .Include(b => b.BienThe.Sanpham)
                .Include(b => b.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT))
                .FirstOrDefault(b => b.MaPhieu == id && b.MaTK == tk.MaTK); // Bảo mật: Chỉ xem được phiếu của mình

            if (bh == null) return RedirectToAction("PageNotFound", "Error");

            return View(bh);
        }
    }
}