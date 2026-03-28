namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class SuccessResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime UTCDate { get; set; } = DateTime.UtcNow;
        public string Msg { get; set; } = string.Empty;
    }
}
