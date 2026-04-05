using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ChatbotLog")]
    public class ChatbotLog
    {
        [Key]
        public int Id { get; set; }

        public int? MaTK { get; set; } // Có thể null nếu khách vãng lai chat

        [Required]
        public string CauHoiKhach { get; set; }

        public string BotTraLoi { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [ForeignKey("MaTK")]
        public virtual TaiKhoanNguoiDung TaiKhoanNguoiDung { get; set; }
    }
}