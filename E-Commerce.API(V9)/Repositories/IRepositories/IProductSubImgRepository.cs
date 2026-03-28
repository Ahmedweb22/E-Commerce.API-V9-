namespace E_Commerce.API_V9_.Repositories.IRepositories
{
    public interface IProductSubImgRepository : IRepository<ProductSubImg>
    {
        void DeleteRange(List<ProductSubImg> productSubImgs);
    }
}
