namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class ProductWithRelatedResponse
    {
        public Product Product { get; set; }
        public List<Product> SameCategories { get; set; }
        public List<Product> SamePrices { get; set; }
        public List<Product> RelatedProducts { get; set; }
                public List<Review> Reviews { get; set; }
    }
}
