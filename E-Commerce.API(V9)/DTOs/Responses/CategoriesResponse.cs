namespace E_Commerce.API_V9_.ViewModels
{
    public class CategoriesResponse
    {
        public IEnumerable<Catgeory> Categories { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
