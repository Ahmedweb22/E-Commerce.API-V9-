namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class ProductGetOneResponse
    {
        public Product Product { get; set; } = null!;
        public IEnumerable<ProductSubImg> SubImg { get; set; } = null!;
        public IEnumerable<Catgeory> Categories { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
    }
}
