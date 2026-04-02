namespace E_Commerce.API_V9_.DTOs.Responses
{
    public class UsersResponse
    {
        public IEnumerable<UserWithRoleResponse> Users { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
