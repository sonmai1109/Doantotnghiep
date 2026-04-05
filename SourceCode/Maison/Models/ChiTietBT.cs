using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ChiTietBT")]
    public class ChiTietBT
    {
        [Key, Column(Order = 0)]
        public int MaBT { get; set; }

        [Key, Column(Order = 1)]
        public int MaGT { get; set; }

        [ForeignKey("MaBT")]
        public virtual BienThe BienThe { get; set; }

        [ForeignKey("MaGT")]
        public virtual GiaTriTT GiaTriTT { get; set; }
    }
}