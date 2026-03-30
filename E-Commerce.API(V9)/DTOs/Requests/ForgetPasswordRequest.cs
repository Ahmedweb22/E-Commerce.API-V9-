namespace E_Commerce.API_V9_.DTOs.Requests
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string EmailOrUserName { get; set; }= string.Empty;
    }
}
