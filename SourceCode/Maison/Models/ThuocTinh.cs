using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ThuocTinh")]
    public class ThuocTinh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTT { get; set; }

        public int? MaDM { get; set; } // Cho phép Null nếu thuộc tính dùng chung cho tất cả danh mục

        [Required]
        [StringLength(100)]
        public string TenTT { get; set; } // Ví dụ: "CPU", "Dung lượng RAM", "Card đồ họa"

        [ForeignKey("MaDM")]
        public virtual Danhmuc DanhMuc { get; set; }
        public bool LaThuocTinhChinh { get; set; } = true;
        public virtual ICollection<GiaTriTT> GiaTriTTs { get; set; }

        public ThuocTinh()
        {
            GiaTriTTs = new HashSet<GiaTriTT>();
        }
    }
}