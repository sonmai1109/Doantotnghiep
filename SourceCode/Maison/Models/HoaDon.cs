using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHD { get; set; }

        public int MaTK { get; set; } // ID người mua

        public DateTime NgayDat { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        public int TrangThai { get; set; } // 1: Chờ duyệt, 2: Đang giao, 3: Thành công...

        [Required]
        [StringLength(100)]
        public string HoTenNguoiNhan { get; set; }

        [Required]
        [StringLength(250)]
        public string DiaChiNhan { get; set; }

        [Required]
        [StringLength(11)]
        public string SoDienThoaiNhan { get; set; }

        public DateTime? NgaySua { get; set; }
        public string NguoiSua { get; set; }

        // Khóa ngoại đến tài khoản người mua
        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }

        // Liên kết đến chi tiết đơn hàng và bảo hành
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual ICollection<Baohanh> Baohanhs { get; set; }

        public HoaDon()
        {
            NgayDat = DateTime.Now;
            ChiTietHoaDons = new HashSet<ChiTietHoaDon>();
            Baohanhs = new HashSet<Baohanh>();
        }
    }
}