using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Drawing2D;

namespace Maison.Models
{
    [Table("SanPham")]
    public class Sanpham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSP { get; set; }

        public int MaDM { get; set; }
        public int MaBrand { get; set; }

        [Required]
        [StringLength(150)]
        public string TenSP { get; set; }

        [Column(TypeName = "ntext")]
        public string MoTa { get; set; }

        public int ThoiHanBaoHanh { get; set; }

        public DateTime NgayTao { get; set; }
        public string NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public string NguoiSua { get; set; }

        [StringLength(255)]
        public string HinhAnh { get; set; } // Ảnh chính đại diện cho dòng máy

        [ForeignKey("MaDM")]
        public virtual Danhmuc DanhMuc { get; set; }

        [ForeignKey("MaBrand")]
        public virtual Brand Brand { get; set; }

        // Kết nối với biến thể để lấy ảnh làm Gallery
        public virtual ICollection<BienThe> BienThes { get; set; }
        //public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; }
        public Sanpham()
        {
            NgayTao = DateTime.Now;
            BienThes = new HashSet<BienThe>();
            SanPhamKhuyenMais = new HashSet<SanPhamKhuyenMai>();
            //DanhGias = new HashSet<DanhGia>();
        }
    }
}