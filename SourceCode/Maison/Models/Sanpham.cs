using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Maison.Models
{
    [Table("SanPham")]
    public class Sanpham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSP { get; set; }

        public int MaDM { get; set; }

        [Required]
        [StringLength(150)]
        public string TenSP { get; set; }

        [Column(TypeName = "money")]
        public decimal Gia { get; set; }

        [Column(TypeName = "ntext")]
        [Required]
        public string MoTa { get; set; }

        [Required]
        [StringLength(50)]
        public string ChatLieu { get; set; }

        [Column(TypeName = "ntext")]
        [Required]
        public string HuongDan { get; set; }

        public DateTime NgayTao { get; set; }

        [Required]
        [StringLength(100)]
        public string NguoiTao { get; set; }

        public DateTime? NgaySua { get; set; } // Cho phép null

        [StringLength(100)]
        public string NguoiSua { get; set; }

        [Required]
        [StringLength(150)]
        public string HinhAnh { get; set; }

        [ForeignKey("MaDM")] // Xác định rõ MaDM là khóa ngoại
        public virtual Danhmuc DanhMuc { get; set; }
        // Trong file Sanpham.cs
        public virtual ICollection<sanphamchitiet> ChiTietSanPhams { get; set; }

        public Sanpham()
        {
            ChiTietSanPhams = new HashSet<sanphamchitiet>();
        }
    }
}