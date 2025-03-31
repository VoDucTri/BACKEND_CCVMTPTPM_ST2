using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace nhom5_webAPI.Models
{
    public class Pet
    {
        public int PetId { get; set; }

        [Required]
        [StringLength(100)] // Giới hạn độ dài tên thú cưng
        public string Name { get; set; }

        [Range(0, 100)] // Giới hạn tuổi hợp lệ
        public int Age { get; set; }

        [Range(0, double.MaxValue)] // Giá trị lớn hơn hoặc bằng 0
        public decimal Price { get; set; }

        [StringLength(500)] // Giới hạn độ dài mô tả
        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [JsonIgnore] // Ngăn serialize vòng lặp
        public PetCategory? Category { get; set; } // Cho phép nullable

        public ICollection<PetImages> Images { get; set; } = new List<PetImages>();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PetStatus Status { get; set; } = PetStatus.Available;
    }

    public enum PetStatus
    {
        Available,
        Sold
    }
}
