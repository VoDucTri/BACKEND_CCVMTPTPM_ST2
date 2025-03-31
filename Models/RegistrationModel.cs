namespace nhom5_webAPI.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace nhom5_webAPI.Models
    {
        public class RegistrationModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;

            public string? Address { get; set; } // Địa chỉ (không bắt buộc)
            public string? PhoneNumber { get; set; } // Số điện thoại (không bắt buộc)
            public string? FullName { get; set; } // Tên đầy đủ (không bắt buộc)
        }
    }

}
