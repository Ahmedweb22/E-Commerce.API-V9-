namespace E_Commerce.API_V9_.DTOs.Requests
{
    public class ResetPasswordRequest
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
