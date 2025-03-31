using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom5_webAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }  // Bỏ [Required]

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }  // Thêm virtual và ? (nullable)

        [Required]
        [StringLength(50)]
        public string ProductType { get; set; } = string.Empty;

        public int? ProductId { get; set; }
        public int? PetId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
