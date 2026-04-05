using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maison.Models
{
    [Table("ChatbotKnowledge")]
    public class ChatbotKnowledge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string CauHoi { get; set; } // Câu hỏi mẫu hoặc Intent

        [Required]
        public string CauTraLoi { get; set; } // Câu trả lời của Bot

        [StringLength(200)]
        public string TuKhoa { get; set; } // Từ khóa để Bot dễ nhận diện (vd: "laptop, gaming, nitro")

        public int? MaDM { get; set; } // Có thể map với Danh mục nếu câu trả lời chuyên biệt cho 1 dòng SP

        [ForeignKey("MaDM")]
        public virtual Danhmuc Danhmuc { get; set; }
    }
}