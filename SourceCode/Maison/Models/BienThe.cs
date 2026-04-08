using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("BienThe")]
    public class BienThe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBT { get; set; }

        public int MaSP { get; set; }

        [Column(TypeName = "money")]
        public decimal GiaBan { get; set; }

        public int SoLuongTon { get; set; }

        [StringLength(150)]
        public string HinhAnh { get; set; }

        public bool TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }

        [StringLength(50)]
        public string NguoiTao { get; set; }

        public DateTime? NgaySua { get; set; }

        [StringLength(50)]
        public string NguoiSua { get; set; }

        // Khóa ngoại đến Sản phẩm gốc
        [ForeignKey("MaSP")]
        public virtual Sanpham Sanpham { get; set; }

        // Liên kết đến các bảng khác
        public virtual ICollection<Baohanh> Baohanhs { get; set; }
        public virtual ICollection<ChiTietBT> ChiTietBTs { get; set; }
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; }
        public virtual ICollection<ThuVienAnh> ThuVienAnhs { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; }
        public BienThe()
        {
            DanhGias = new HashSet<DanhGia>();
            Baohanhs = new HashSet<Baohanh>();
            ChiTietBTs = new HashSet<ChiTietBT>();
            SanPhamKhuyenMais = new HashSet<SanPhamKhuyenMai>();
            ChiTietHoaDons = new HashSet<ChiTietHoaDon>();
            ThuVienAnhs = new HashSet<ThuVienAnh>();
            GioHangs = new HashSet<GioHang>();
        }
    }
}