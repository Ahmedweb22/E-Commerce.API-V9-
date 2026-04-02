namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class PromotionsResponse
    {
        public IEnumerable<Promotion> Promotions { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
