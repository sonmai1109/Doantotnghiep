using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Maison.Models
{
    [Table("TaiKhoanNguoiDung")]
    public class TaiKhoanNguoiDung
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTk { get; set; }
        [Required]
        [StringLength(100)]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(50)]
        public string MatKhau { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required]
        [StringLength(11)]
        public string SoDienThoai { get; set; }

        [Required]
        [StringLength(100)]
        public string DiaChi { get; set; }

        public DateTime NgaySinh { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public bool GioiTinh { get; set; }

        public bool TrangThai { get; set; }



    }
}