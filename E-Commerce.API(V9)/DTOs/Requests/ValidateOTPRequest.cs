namespace E_Commerce.API_V9_.DTOs.Requests
{
    public class ValidateOTPRequest
    {
        public string OTP { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
