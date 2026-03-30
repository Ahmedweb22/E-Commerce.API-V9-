namespace E_Commerce.API_V9_.DTOs.Requests
{
    public class BrandCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile Logo { get; set; } = null!;
        public bool Status { get; set; }
    }
}
