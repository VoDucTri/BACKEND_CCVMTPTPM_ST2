using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace nhom5_webAPI.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; } // Khóa chính
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;

        // Quan hệ
        public ICollection<ServiceImage>? Images { get; set; } // Hình ảnh
        public ICollection<Appointment>? Appointments { get; set; } // Cuộc hẹn
    }
}
