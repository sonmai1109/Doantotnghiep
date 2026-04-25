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

        public int? MaKM { get; set; } // Phải cho phép NULL nếu bạn có dùng thao tác xóa KM

        public int? MaBT { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm!")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm phải từ 1 đến 100!")]
        public int PhanTramGiam { get; set; }

        // ==========================================
        // 2 CỘT MỚI THÊM CHO FLASH SALE
        // ==========================================

        // Số lượng suất khuyến mãi (Mặc định = null nghĩa là Bán Không Giới Hạn)
        public int? SoLuongKhuyenMai { get; set; }

        // Số suất đã bán (Khởi tạo luôn = 0)
        public int SoLuongDaBan { get; set; } = 0;

        // ==========================================

        [ForeignKey("MaSP")]
        public virtual Sanpham Sanpham { get; set; }

        [ForeignKey("MaKM")]
        public virtual KhuyenMai KhuyenMai { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}