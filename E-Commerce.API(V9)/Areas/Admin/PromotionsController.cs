using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN} , {SD.ROLE_EMPLOYEE}")]
    public class PromotionsController : ControllerBase
    {
        private IRepository<Promotion> _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private IRepository<Product> _productRepository;
        public PromotionsController(IRepository<Promotion> promotionRepository, UserManager<ApplicationUser> userManager, IRepository<Product> productRepository)
        {
            _promotionRepository = promotionRepository;
            _userManager = userManager;
            _productRepository = productRepository;

        }
        [HttpGet]
        public async Task<IActionResult> Get(string? code, int page = 1)
        {
            var promotions = await _promotionRepository.GetAsync(includes: [m => m.Product, m => m.ApplicationUser], tracking: false);

            var products = await _productRepository.GetAsync(tracking: false);
            var users = _userManager.Users.AsNoTracking().AsQueryable();
            if (code is not null)
                promotions = promotions.Where(e => e.Code.Contains(code)).ToList();

            if (page < 1)
                page = 1;
            int pageSize = 10;
            int currentPage = page;
            double totalCount = Math.Ceiling(promotions.Count() / (double)pageSize);
            promotions = promotions.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return Ok(new PromotionsResponse
            {
                Promotions = promotions,
                Products = products,
                Users = users,
                TotalPages = totalCount,
                CurrentPage = currentPage
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var promotion = await _promotionRepository.GetOneAsync(e => e.Id == id);
            if (promotion is null)
                return NotFound();
            return Ok(promotion);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            await _promotionRepository.CreateAsync(promotion);
            await _promotionRepository.CommitAsync();


            return Ok(new SuccessResponse()
            { 
            Msg = "Promotion created successfully"  
            });
        }
       
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.ROLE_SUPER_ADMIN},{SD.ROLE_ADMIN}")]
        public async Task<IActionResult> Update([FromRoute] int id ,Promotion promotion)
        {
            var promotionInDb = await _promotionRepository.GetOneAsync(o => o.Id == id);

            if (promotionInDb is null)
                return NotFound();
            promotionInDb.Code = promotion.Code;
            promotionInDb.ApplicationUserId = promotion.ApplicationUserId;
            promotionInDb.Discount = promotion.Discount;
            promotionInDb.ExpiredAt = promotion.ExpiredAt;
            promotionInDb.MaxUsage = promotion.MaxUsage;
            promotionInDb.ProductId = promotion.ProductId;
            promotionInDb.CreatedAt = promotion.CreatedAt;
            await _promotionRepository.CommitAsync();
            return Ok(new SuccessResponse()
            {
                Msg = "Promotion updated successfully"
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.ROLE_SUPER_ADMIN},{SD.ROLE_ADMIN}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var promotion = await _promotionRepository.GetOneAsync(e => e.Id == id);
            if (promotion is null)
                return NotFound();
            _promotionRepository.Delete(promotion);
            await _promotionRepository.CommitAsync();
            return NoContent ();
        }
    }
}
