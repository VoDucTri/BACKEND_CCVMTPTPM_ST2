using Microsoft.AspNetCore.Identity;

namespace nhom5_webAPI.Models
{
    public class User : IdentityUser
    {
        public string? Address { get; set; } // Địa chỉ người dùng
        public string? FullName { get; set; } // Tên đầy đủ người dùng

        // Quan hệ
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public string? PasswordResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; } = null; //ResetCodeExpiration được sử dụng để lưu trữ thời điểm hết hạn của mã OTP (Password Reset Code).
        public string? PreviousPasswordResetCode { get; set; }
    }
}
