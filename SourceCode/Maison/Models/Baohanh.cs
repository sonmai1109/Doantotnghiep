//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//namespace Maison.Models
//{
//    public class Baohanh
//    {
//        [Key]
//        public int MaPhieu { get; set; }

//        public int MaBT { get; set; }

//        public int MaHD { get; set; }

//        public int MaTK { get; set; }

//        public DateTime? NgayTiepNhan { get; set; }

//        public DateTime? NgayHenTra { get; set; }

//        public string TinhTrangLoi { get; set; }

//        public string NoiDungSuaChua { get; set; }

//        public decimal? ChiPhiSuaChua { get; set; }

//        public int? TrangThai { get; set; }

//        public virtual BienThe BienThe { get; set; }

//        public virtual HoaDon HoaDon { get; set; }

//        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }
//    }
//}