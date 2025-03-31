using nhom5_webAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Appointment
{
    [Key]
    public int AppointmentId { get; set; } // Khóa chính

    [Required]
    public string UserId { get; set; } // Khóa ngoại đến bảng User

    [ForeignKey("UserId")]
    public User? User { get; set; } // Đánh dấu là nullable

    [Required]
    public int ServiceId { get; set; } // Khóa ngoại đến bảng Service

    [ForeignKey("ServiceId")]
    public Service? Service { get; set; } // Đánh dấu là nullable

    [Required]
    public DateTime AppointmentDate { get; set; } // Ngày hẹn

    [Required]
    public string Status { get; set; } = "Pending"; // Trạng thái
}

