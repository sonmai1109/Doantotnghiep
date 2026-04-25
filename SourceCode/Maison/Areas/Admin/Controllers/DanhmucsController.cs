using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Maison.Models;
using PagedList;
using static System.Data.Entity.Infrastructure.Design.Executor;
namespace Maison.Areas.Admin.Controllers
{
    public class DanhmucsController : BaseController
    {
        shopdb db = new shopdb();
   public ActionResult Index(string timkiem, int page = 1, int pagesize = 7)
        {
            ViewBag.timkiem = timkiem;
            // Include DanhMucCha để hiển thị tên danh mục cha ra bảng
            var danhmucs = db.Danhmucs.Include(d => d.DanhMucCha).AsQueryable();
            
            if (!string.IsNullOrEmpty(timkiem))
            {
                danhmucs = danhmucs.Where(dm => dm.TenDM.Contains(timkiem));
            }

            // GỬI DANH SÁCH DANH MỤC GỐC RA VIEW ĐỂ LÀM DROPDOWN LIST (THÊM/SỬA)
            // Danh mục gốc là những danh mục có MaDMCha == null
            ViewBag.ListCha = db.Danhmucs.Where(d => d.MaDMCha == null).ToList();

            return View(danhmucs.OrderBy(dm => dm.MaDM).ToPagedList(page, pagesize));
        }
        [HttpPost]
        public JsonResult Create(Danhmuc dm)
        {

            try
            {
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                dm.NgayTao = DateTime.Now;
                dm.NguoiTao = tk.HoTen;
                dm.NguoiSua = tk.HoTen;
                dm.NgaySua=DateTime.Now;
                db.Danhmucs.Add(dm);
                db.SaveChanges();
                return Json(new { status = true, message = "Thêm thành công!" });


            }
            catch (Exception)
            {

                return Json(new { status = false, message = "Tên danh mục đã tồn tại!" });

            }
        }
        [HttpPost]
        public JsonResult Loaddata(int id)
        {
            Danhmuc dm = db.Danhmucs.FirstOrDefault(a => a.MaDM == id);
            // Ép kiểu sang Object ẩn danh để tránh lỗi JSON Circular Reference
            var result = new
            {
                MaDM = dm.MaDM,
                TenDM = dm.TenDM,
                MaDMCha = dm.MaDMCha
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Update(Danhmuc dm)
        {
            try
            {
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Maison.Session.ConstaintUser.ADMIN_SESSION];
                Danhmuc doi = db.Danhmucs.FirstOrDefault(a => a.MaDM == dm.MaDM);

                doi.TenDM = dm.TenDM;
                doi.MaDMCha = dm.MaDMCha; // CẬP NHẬT THÊM MÃ DANH MỤC CHA
                doi.NgaySua = DateTime.Now;
                doi.NguoiSua = tk.HoTen;

                db.Entry(doi).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, message = "Sửa thành công!" });
            }
            catch (Exception)
            {
                return Json(new { status = false, message = "Lỗi khi cập nhật!" });
            }
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Danhmuc dm = db.Danhmucs.Where(a => a.MaDM.Equals(id)).FirstOrDefault();
                db.Danhmucs.Remove(dm);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch {

                return Json(new { status = false, });
            }



        }
    }
}

