namespace E_Commerce.API_V9_.Models
{
    public class ReviewImg
    {
        public int Id { get; set; }
        public int? ReviewId { get; set; }
        public Review? Review { get; set; }
        public string? Img { get; set; }
    }
}
