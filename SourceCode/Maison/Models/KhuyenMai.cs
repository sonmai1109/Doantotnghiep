using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("KhuyenMai")]
    public class KhuyenMai
    {
        [Key]
        public int MaKM { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình")]
        [StringLength(200)]
        public string TenKM { get; set; }

        public string MoTa { get; set; }

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public int TrangThai { get; set; }

        // --- Audit Log ---
        public DateTime? NgayTao { get; set; }
        [StringLength(50)]
        public string NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        [StringLength(50)]
        public string NguoiSua { get; set; }

        // Liên kết 1 - N (1 Khuyến mãi có nhiều SP tham gia)
        public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; }

        public KhuyenMai()
        {
            SanPhamKhuyenMais = new HashSet<SanPhamKhuyenMai>();
        }
    }
}