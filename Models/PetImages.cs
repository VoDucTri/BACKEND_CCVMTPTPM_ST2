using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace nhom5_webAPI.Models
{
    public class PetImages
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PetId { get; set; }

        [JsonIgnore] // Ngăn vòng lặp khi serialize
        public Pet? Pet { get; set; } // Cho phép nullable

        [Required] // Đảm bảo URL không null
        [Url] // Đảm bảo URL hợp lệ
        public string ImageUrl { get; set; }
    }
}
