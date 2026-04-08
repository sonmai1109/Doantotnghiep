using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGH { get; set; }

        public int MaTK { get; set; } // Giỏ hàng của ai?

        public int MaBT { get; set; } // Khách chọn Cấu hình (Biến thể) nào?

        public int SoLuong { get; set; } // Số lượng mua

        public DateTime NgayThem { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết
        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}