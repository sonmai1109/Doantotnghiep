using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        [Key, Column(Order = 0)]
        public int MaHD { get; set; }

        [Key, Column(Order = 1)]
        public int MaBT { get; set; }

        public int SoLuongMua { get; set; }

        [Column(TypeName = "money")]
        public decimal GiaMua { get; set; }

        // Cột cực kỳ quan trọng cho hệ thống máy tính của bạn
        public DateTime? NgayHetHanBH { get; set; }

        [ForeignKey("MaHD")]
        public virtual HoaDon HoaDon { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}