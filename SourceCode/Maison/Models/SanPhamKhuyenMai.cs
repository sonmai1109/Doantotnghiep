using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("SanPhamKhuyenMai")]
    public class SanPhamKhuyenMai
    {
        [Key]
        public int ID { get; set; }

        public int MaSP { get; set; }

        public int MaKM { get; set; }

        // --- CỘT QUAN TRỌNG ĐÂY ---
        public int? MaBT { get; set; } // Cho phép Null để giảm giá toàn bộ sản phẩm

        public int PhanTramGiam { get; set; }

        // Khóa ngoại nối với Sản phẩm
        [ForeignKey("MaSP")]
        public virtual Sanpham Sanpham { get; set; }

        // Khóa ngoại nối với Chương trình KM
        [ForeignKey("MaKM")]
        public virtual KhuyenMai KhuyenMai { get; set; }

        // Khóa ngoại nối với Biến thể (Mới thêm)
        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}