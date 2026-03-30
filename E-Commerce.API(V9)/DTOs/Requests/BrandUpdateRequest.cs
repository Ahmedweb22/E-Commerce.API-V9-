namespace E_Commerce.API_V9_.DTOs.Requests
{
    public class BrandUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; } 
        public bool Status { get; set; }
    }
}
