using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("Brand")]
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBrand { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(100)]
        public string TenBrand { get; set; } // Ví dụ: Dell, HP, ASUS, MSI

        [StringLength(255)]
        public string Logo { get; set; } // Đường dẫn ảnh logo thương hiệu

        [StringLength(500)]
        public string MoTa { get; set; } // Giới thiệu ngắn về hãng

        // Một thương hiệu có thể có nhiều sản phẩm
        public virtual ICollection<Sanpham> Sanphams { get; set; }

        public Brand()
        {
            Sanphams = new HashSet<Sanpham>();
        }
    }
}