using System;
using System.Linq;
using System.Web.Mvc;
using Maison.Models;
using PagedList;

namespace Maison.Controllers
{
    public class NewsController : Controller
    {
        private shopdb db = new shopdb();

        // 1. TRANG DANH SÁCH TIN TỨC (Xem tất cả)
        public ActionResult Index(int page = 1, int pageSize = 9)
        {
            // Chỉ lấy các tin tức được phép Public (TrangThai == 1)
            var tinTucs = db.TinTucs
                .Where(t => t.TrangThai == 1)
                .OrderByDescending(t => t.NgayDang)
                .ToPagedList(page, pageSize);

            return View(tinTucs);
        }

        // 2. TRANG CHI TIẾT BÀI VIẾT (Nơi bị lỗi 404)
        public ActionResult Details(int id)
        {
            var tin = db.TinTucs.FirstOrDefault(t => t.MaTin == id && t.TrangThai == 1);
            if (tin == null) return RedirectToAction("PageNotFound", "Error");

            tin.LuotXem += 1;
            db.SaveChanges();

            // ĐOẠN CODE NÀY LÀ ĐỂ LOAD CỘT BÊN PHẢI
            ViewBag.TinLienQuan = db.TinTucs
                .Where(t => t.TrangThai == 1 && t.MaTin != id)
                .OrderByDescending(t => t.NgayDang)
                .Take(5)
                .ToList();

            return View(tin);
        }
    }
}