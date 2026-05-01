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

            // Kéo sâu xuống bảng ThuocTinh để lấy ThuTuHienThi
            // Trong BaoHanhController.cs (Front-end)
            // Không thay đổi gì nhiều ở đây, chỉ cần đảm bảo dòng Include đã kéo đến ThuocTinh
            var listBH = db.Baohanhs
                .Include(b => b.BienThe.Sanpham)
                .Include(b => b.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT.ThuocTinh))
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

            // Tương tự, kéo sâu xuống bảng ThuocTinh
            var bh = db.Baohanhs
                .Include(b => b.BienThe.Sanpham)
                .Include(b => b.BienThe.ChiTietBTs.Select(cb => cb.GiaTriTT.ThuocTinh)) // ĐÃ SỬA DÒNG NÀY
                .FirstOrDefault(b => b.MaPhieu == id && b.MaTK == tk.MaTK);

            if (bh == null) return RedirectToAction("PageNotFound", "Error");

            return View(bh);
        }
    }
}