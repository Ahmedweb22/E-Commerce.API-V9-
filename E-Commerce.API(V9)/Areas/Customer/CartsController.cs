using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace E_Commerce.API_V9_.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Promotion> promotionRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _promotionRepository = promotionRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var cartItems = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);
            bool ProductionApplied = true;
            if (code is not null)
            {
                var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code && e.IsValid);
                if (promotion is not null)
                {
                    foreach (var item in cartItems)
                    {
                        if (promotion.ProductId is null || promotion.ProductId == item.ProductId)
                        {
                            item.Price = item.Price - (item.Price * promotion.Discount / 100);
                            await _cartRepository.CommitAsync();
                            return Ok(new SuccessResponse { Msg = "Promotion code applied successfully." });
                        }
                        else
                        {
                            return BadRequest(new ErrorResponce()
                            {
                                ErrorMsg = "This promotion code is not applicable to any of the products in your cart.",
                            });
                        }

                    }

                }
            }
            return Ok(cartItems);

        }
        [HttpGet("AddToCart")]
        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _productRepository.GetOneAsync(e => e.Id == productId);
            if (user is null || product is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);
            if (cartItem is null)
            {
                await _cartRepository.CreateAsync(new Cart
                {
                    ApplicationUserId = user.Id,
                    ProductId = productId,
                    Count = count,
                    Price = product.Price
                });
            }
            else
            {
                cartItem.Count += count;
            }
            await _cartRepository.CommitAsync();

            return Ok(new SuccessResponse
            { Msg = "Product added to cart successfully" });
        }
        [HttpPatch("{id}/increment")]
        public async Task<IActionResult> Increment([FromRoute] int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId, includes: [e => e.Product]);
            if (cartItem is null)
                return NotFound();
            if (cartItem.Count != cartItem.Product.Quantity)
            {
                cartItem.Count++;
                await _cartRepository.CommitAsync();
            }
            else
            {
                //TempData["error-notification"] = "You have reached the maximum quantity for this product.";
                return BadRequest(new ErrorResponce()
                {
                    ErrorMsg = "You have reached the maximum quantity for this product.",
                });
            }

            return NoContent();
        }
        [HttpPatch("{id}/decrement")]
        public async Task<IActionResult> Decrement([FromRoute] int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);
            if (cartItem is null)
                return NotFound();
            if (cartItem.Count > 1)
            {
                cartItem.Count--;
                await _cartRepository.CommitAsync();
            }
            return NoContent();
        }
        [HttpPatch("{id}/Remove")]
        public async Task<IActionResult> Remove([FromRoute] int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);
            if (cartItem is null)
                return NotFound();
            _cartRepository.Delete(cartItem);
            await _cartRepository.CommitAsync();
            return NoContent();
        }
        [HttpGet("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel",
            };
            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);
            foreach (var item in carts)
            {
                options.LineItems = carts.Select(cart => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },

                    },
                    Quantity = item.Count,
                }).ToList();
            }
            var services = new SessionService();
            var session = services.Create(options);
            return Ok(new SuccessResponse()
            {
                Msg = "Checkout session created successfully.",
               OptionalData = [ session.Url ]  
            });
        }
    }
}
