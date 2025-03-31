using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace nhom5_webAPI.Models
{
    public class ProductImages
    {
        [Key]
        public int ProductImageId { get; set; } // ID ảnh sản phẩm

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; } // FK tới Product

        [JsonIgnore] // Ngăn vòng lặp khi serialize
        public Product? Product { get; set; } // Navigation property

        [Required]
        [Url(ErrorMessage = "The ImageUrl must be a valid URL.")] // Đảm bảo URL hợp lệ
        [StringLength(500, ErrorMessage = "The ImageUrl cannot exceed 500 characters.")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
