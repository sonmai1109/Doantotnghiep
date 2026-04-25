using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // phải có

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

        // --- 1. THÊM CỘT MÃ DANH MỤC CHA ---
        // Dùng int? (cho phép Null) vì các danh mục gốc (Laptop, Chuột) sẽ không có cha
        public int? MaDMCha { get; set; }

        [StringLength(50)]
        public string NguoiSua { get; set; }

        [StringLength(50)]
        public string NguoiTao { get; set; }

        [Required]
        public DateTime NgayTao { get; set; }

        public DateTime? NgaySua { get; set; }

        // --- 2. THIẾT LẬP MỐI QUAN HỆ TỰ TRỎ (CHA <-> CON) ---
        // Thuộc tính này giúp bạn lấy ra thông tin Danh Mục Cha từ 1 Danh Mục Con
        [ForeignKey("MaDMCha")]
        public virtual Danhmuc DanhMucCha { get; set; }

        // Thuộc tính này giúp bạn lấy ra 1 List các Danh Mục Con từ 1 Danh Mục Cha
        public virtual ICollection<Danhmuc> DanhMucCons { get; set; }
        // -----------------------------------------------------

        public virtual ICollection<Sanpham> Sanphams { get; set; }
        public virtual ICollection<ChatbotKnowledge> ChatbotKnowledges { get; set; }
        public virtual ICollection<ThuocTinh> ThuocTinhs { get; set; }

        public Danhmuc()
        {
            Sanphams = new HashSet<Sanpham>();
            ChatbotKnowledges = new HashSet<ChatbotKnowledge>();
            ThuocTinhs = new HashSet<ThuocTinh>();

            // Khởi tạo list Danh mục con
            DanhMucCons = new HashSet<Danhmuc>();
        }
    }
}