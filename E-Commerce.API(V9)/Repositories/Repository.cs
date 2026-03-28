using System.Linq.Expressions;

namespace E_Commerce.API_V9_.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;
        private DbSet<T> _dbset;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbset = _context.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
           await _dbset.AddAsync(entity);
  
        }
        public void Update(T entity)
        {
            _dbset.Update(entity);
           
        }
        public void Delete(T entity) 
        {
        _dbset.Remove(entity);
       
        }
        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null
            , bool tracking = true)
        {
           var categories = _dbset.AsQueryable();
            if (expression != null)
            {
                categories = categories.Where(expression);
            }
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    categories = categories.Include(include);
                }
            }
            if (!tracking) {
                categories = categories.AsNoTracking();

               // categories = categories.Where(e => e.Status);
            }
            return await categories.ToListAsync();
        }
        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null
            , bool tracking = true)
        {
           return (await GetAsync(expression, includes, tracking)).FirstOrDefault();
        }
        public async Task<int> CommitAsync()
        {
            try            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               Console.WriteLine($" Error:{ex.Message}");
                return 0;
            }
        }
    }
}
