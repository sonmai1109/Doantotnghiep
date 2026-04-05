using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("TinTuc")]
    public class TinTuc
    {
        [Key]
        public int MaTin { get; set; }

        [Required, StringLength(250)]
        public string TieuDe { get; set; }

        [StringLength(250)]
        public string AnhDaiDien { get; set; }

        public string TomTat { get; set; }

        public string NoiDung { get; set; }

        public DateTime? NgayDang { get; set; }

        public int? MaTK { get; set; } // Khóa ngoại nối với Tài khoản người dùng

        public int? LuotXem { get; set; }

        public int TrangThai { get; set; }

        // ĐỔI Ở ĐÂY: Nối vào TaiKhoanNguoiDung
        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }
    }
}