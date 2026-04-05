using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ThuVienAnh")]
    public class ThuVienAnh
    {
        [Key]
        public int MaAnh { get; set; }

        public int MaBT { get; set; } // ĐỔI THÀNH MaBT (Móc vào Biến thể)

        [Required]
        [StringLength(500)]
        public string DuongDanAnh { get; set; }
        public int ThuTu { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }
    }
}