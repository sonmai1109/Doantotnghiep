using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        public int MaTK { get; set; }

        public int XepHang { get; set; } // Ví dụ: 1 đến 5 sao

        public string BinhLuan { get; set; }

        public DateTime? NgayTao { get; set; }

        public int TrangThai { get; set; }

        public int MaBT { get; set; } // Khóa ngoại móc vào Biến thể

        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}