using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("GiaTriTT")]
    public class GiaTriTT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGT { get; set; }

        public int MaTT { get; set; }

        [Required]
        [StringLength(255)]
        public string GiaTri { get; set; } // Ví dụ: "Core i7-13700H", "16GB DDR5"

        [ForeignKey("MaTT")]
        public virtual ThuocTinh ThuocTinh { get; set; }

        // Liên kết n-n qua bảng ChiTietBT
        public virtual ICollection<ChiTietBT> ChiTietBTs { get; set; }

        public GiaTriTT()
        {
            ChiTietBTs = new HashSet<ChiTietBT>();
        }
    }
}