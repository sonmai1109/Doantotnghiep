using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Maison.Models
{
    [Table("Baohanh")]
    public class Baohanh
    {
        [Key]
        public int MaPhieu { get; set; }

        public int MaBT { get; set; }

        public int MaHD { get; set; }

        public int MaTK { get; set; }

        public DateTime? NgayTiepNhan { get; set; }

        public DateTime? NgayHenTra { get; set; }

        public string TinhTrangLoi { get; set; }

        public string NoiDungSuaChua { get; set; }

        public decimal? ChiPhiSuaChua { get; set; }

        public int? TrangThai { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }

        [ForeignKey("MaHD")]
        public virtual HoaDon HoaDon { get; set; }

        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }
    }
}