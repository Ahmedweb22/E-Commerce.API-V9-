namespace E_Commerce.API_V9_.Models
{
    public class Catgeory
    {
        public int Id { get; set; }
        [Required]
        [Length(3,255)]
        public string Name { get; set; }=string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public List<Product> Products { get; set; }
    }
}
