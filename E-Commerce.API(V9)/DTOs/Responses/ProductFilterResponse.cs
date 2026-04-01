namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class ProductFilterResponse
    {
        public string? Name { get; set; }
        public long? MinPrice { get; set; }
        public long? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool LessQuantity { get; set; }
    }
}
