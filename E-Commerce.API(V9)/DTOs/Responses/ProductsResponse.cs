namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class ProductsResponse
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Catgeory> Categories { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public ProductFilterResponse Filter { get; set; }
    }
}
