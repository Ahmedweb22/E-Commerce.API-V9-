namespace E_Commerce.API_V9_.Repositories
{
    public class ProductSubImgRepository : Repository<ProductSubImg> , IProductSubImgRepository
    {
        private ApplicationDbContext _context ;

        public ProductSubImgRepository(ApplicationDbContext context): base(context) 
        {
            _context = context;
        }

        public void DeleteRange(List<ProductSubImg> productSubImgs) 
        {
        _context.ProductSubImgs.RemoveRange(productSubImgs);
        }
    }
}
