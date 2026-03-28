namespace E_Commerce.API_V9_.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }=string.Empty;
        public string? Description { get; set; }
        public string MainImg { get; set; }=string.Empty;
        public double Price { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public double Rate { get; set; }
        public bool Status { get; set; }
        public int CatgeoryId { get; set; }
        public Catgeory Catgeory { get; set; } = default!;
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = default!;
    }
}
