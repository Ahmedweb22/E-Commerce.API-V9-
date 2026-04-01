namespace E_Commerce.API_V9_.DTOs.Requests
{
    public record ReviewCreateRequest(
        int ProductId,
        double Rate,
        string? Comment,
        List<IFormFile>? Imgs
    );
    
}
