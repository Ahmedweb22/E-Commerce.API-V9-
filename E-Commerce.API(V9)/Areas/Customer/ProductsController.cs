using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.CUSTOMER_AREA)]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Review> _reviewRepository;

        public ProductsController(IRepository<Product> productRepository, IRepository<Review> reviewRepository)
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(int? categoryId)
        {
            const double discount = 50;
            var products = (await _productRepository.GetAsync(e => e.Discount > discount, includes: [e => e.Catgeory])).AsEnumerable();
            if (categoryId is not null)
            {
                products = products.Where(e => e.CatgeoryId == categoryId);
            }

            products = products
                   .Skip(0)
                .Take(8);
                 
            return Ok(products);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
         
                var product = await _productRepository.GetOneAsync(e => e.Id == id);
            if (product is null)
                return NotFound();
         
            var sameCategories = (await _productRepository.GetAsync(e => e.CatgeoryId == product.CatgeoryId && e.Id != product.Id))
                .Skip(0)
               .Take(4);

            var miniPrice = product.Price - (product.Price * 0.10);
            var maxPrice = product.Price + (product.Price * 0.10);
            var samePrice = (await _productRepository.GetAsync(e => e.Price >= miniPrice && e.Price <= maxPrice && e.Id != product.Id))
                .Skip(0)
               .Take(4);
            var relatedProducts = (await _productRepository.GetAsync(e => e.Name.Contains(product.Name) && e.Id != product.Id))
                .Skip(0)
               .Take(4);

            var reviews = await _reviewRepository.GetAsync(e => e.ProductId == product.Id);

            return Ok(new ProductWithRelatedResponse
            {
                Product = product,
                SameCategories = sameCategories.ToList(),
                SamePrices = samePrice.ToList(),
                RelatedProducts = relatedProducts.ToList(),
                Reviews = reviews
            });
        }
    }
}
