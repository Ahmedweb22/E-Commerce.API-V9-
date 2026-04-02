namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class OrderResponse
    {
        public IEnumerable<Order> Orders { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
