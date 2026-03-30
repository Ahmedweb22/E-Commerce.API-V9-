namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class BrandsResponse
    {
        public IEnumerable<Brand> Brands { get; set; }
        public int CurrentPage { get; set; }
        public double TotalPages { get; set; }
    }
}
