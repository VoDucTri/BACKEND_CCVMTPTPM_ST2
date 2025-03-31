using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace nhom5_webAPI.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; } 

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } 
    }
}