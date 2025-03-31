namespace nhom5_webAPI.Models
{
    public class ResetPasswordRequest
    {
        public string Username { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
    }
}
