namespace E_Commerce.API_V9_.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
