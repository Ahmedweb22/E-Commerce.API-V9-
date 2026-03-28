namespace E_Commerce.API_V9_.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; }=false;
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
        public bool IsValid => !IsUsed && DateTime.UtcNow < ExpireAt;
            public string UserId { get; set; } = string.Empty;
            public ApplicationUser User { get; set; } = null!;
    }
}
