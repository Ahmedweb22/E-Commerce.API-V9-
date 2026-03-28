namespace E_Commerce.API_V9_.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public double Discount { get; set; }
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int ? ProductId { get; set; }
        public Product? Product { get; set; }
         public int MaxUsage { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsValid => MaxUsage >= 1 && DateTime.UtcNow < ExpiredAt;
    }
}
