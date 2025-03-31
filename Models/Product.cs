using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nhom5_webAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; } // Khóa chính

        [Required]
        [MaxLength(100)] // Tên sản phẩm có độ dài tối đa 100 ký tự
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)] // Giá sản phẩm lớn hơn 0
        public decimal Price { get; set; }

        [MaxLength(500)] // Mô tả sản phẩm tối đa 500 ký tự
        public string Description { get; set; } = string.Empty;

        // Cột số lượng sản phẩm
        [Range(0, int.MaxValue)] // Số lượng không âm
        public int Quantity { get; set; } = 0;

        [Required]
        public int SupplyCategoryId { get; set; } // FK tới danh mục sản phẩm
        public PetSupplyCategory? SupplyCategory { get; set; }

        public ICollection<ProductImages>? Images { get; set; } = new List<ProductImages>();
    }
}
