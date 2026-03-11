using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Maison.Models
{
    public class shopdb: DbContext

    {
        public shopdb():base("name=shop") 
        
        { 
        
        }
        public virtual DbSet<Sanpham> Sanphams { get; set; }
        public virtual DbSet<sanphamchitiet> Sanphamchitiets { get; set; }
        public virtual DbSet<Danhmuc> Danhmucs { get; set; }
        public virtual DbSet<TaiKhoanNguoiDung> TaiKhoanNguoiDungs { get; set; }
        public virtual DbSet<TaiKhoanQuanTri> TaiKhoanQuanTris { get; set; }


    }
}