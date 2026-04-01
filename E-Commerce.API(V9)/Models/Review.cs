namespace E_Commerce.API_V9_.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public double Rate { get; set; }
        public string? Comment { get; set; }
    }
}
