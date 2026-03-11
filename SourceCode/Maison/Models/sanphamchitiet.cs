using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
namespace Maison.Models
{
    [Table("sanphamchitiet")]
    public class sanphamchitiet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IDCTSP { get; set; }

        public int MaSP { get; set; }

        public int MaKichCo { get; set; }

        public int SoLuong { get; set; }

        public int? MaMau { get; set; }

        public decimal? Gia { get; set; }
        [ForeignKey("MaSP")]
        public virtual Sanpham SanPham { get; set; }

    }
}