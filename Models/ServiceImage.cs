using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace nhom5_webAPI.Models
{
    public class ServiceImage
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        [Required]
        [Url] // Đảm bảo URL hợp lệ
        public string Url { get; set; } = string.Empty; // Đường dẫn URL của hình ảnh

        [Required]
        public int ServiceId { get; set; } // FK liên kết với bảng Service

        [ForeignKey("ServiceId")]
        [JsonIgnore] // Ngăn vòng lặp khi serialize
        public Service? Service { get; set; } // Navigation property (nullable để tránh lỗi vòng lặp)
    }
}

