namespace E_Commerce.API_V9_.Models
{
   public enum OrderStatus
    {
        pending,
        inprocessing,
        shipped,
        onTheWay,
        completed,
        canceled
    }
    public enum PaymentStatus
    {
        pending,
        completed,
        canceled,
        refunded
    }
    public enum PaymentType
    {
      Visa,
      Cash
    }
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.pending;
        public double TotalPrice { get; set; }

        public string SessionId { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.pending;
        public PaymentType PaymentType { get; set; } = PaymentType.Visa;

        public DateTime? ShippedDate { get; set; }
        public string? Carrier { get; set; }
        public string? Tracking { get; set; }
 


           
    }
}
