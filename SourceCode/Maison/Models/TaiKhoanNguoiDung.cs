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
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(100)]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(50)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(11)]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(100)]
        public string DiaChi { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public int? GioiTinh { get; set; }

        public bool TrangThai { get; set; }
        public virtual ICollection<TinTuc> TinTucs { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<Baohanh> Baohanhs { get; set; }
        public virtual ICollection<ChatbotLog> ChatbotLogs { get; set; }
        public TaiKhoanNguoiDung()
        {
            TinTucs = new HashSet<TinTuc>();
            HoaDons = new HashSet<HoaDon>();
            DanhGias = new HashSet<DanhGia>();
            Baohanhs = new HashSet<Baohanh>();
            ChatbotLogs = new HashSet<ChatbotLog>();
            // ... các HashSet khác như DanhGias, HoaDons ...
        }
    }
}