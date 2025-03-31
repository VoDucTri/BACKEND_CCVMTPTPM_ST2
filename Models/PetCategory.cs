using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace nhom5_webAPI.Models
{
    public class PetCategory
    {
        [Key] // Đánh dấu CategoryId là khóa chính
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)] // Giới hạn độ dài tên danh mục
        public string Name { get; set; }

        [JsonIgnore] // Ngăn vòng lặp khi serialize
        public ICollection<Pet>? Pets { get; set; } = new List<Pet>(); // Cho phép nullable và khởi tạo mặc định
    }
}
