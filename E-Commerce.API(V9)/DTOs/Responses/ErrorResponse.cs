namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class ErrorResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime UTCDate { get; set; } = DateTime.UtcNow;
        public string ErrorMsg { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }
}
