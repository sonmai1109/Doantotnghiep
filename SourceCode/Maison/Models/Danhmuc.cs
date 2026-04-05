using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;//phai có
namespace Maison.Models
{
    [Table("DanhMuc")]
    public class Danhmuc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDM { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDM { get; set; }
        [StringLength(50)]
        public string NguoiSua { get; set; }
        [StringLength(50)]
        public string NguoiTao { get; set; }
        [Required]
        public DateTime NgayTao { get; set; } // Đã xóa StringLength
        public DateTime? NgaySua { get; set; } // Dùng dấu ? để cho phép Null khi chưa sửa

        public virtual ICollection<Sanpham> Sanphams { get; set; }
        public virtual ICollection<ChatbotKnowledge> ChatbotKnowledges { get; set; }


        public Danhmuc()
        {
            Sanphams = new HashSet<Sanpham>();
            ChatbotKnowledges = new HashSet<ChatbotKnowledge>();
        }
    }
}