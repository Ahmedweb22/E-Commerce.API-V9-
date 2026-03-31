using System.Security.Claims;
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
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Promotion> promotionRepository, IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? code)
        {
            //var user = await _userManager.GetUserAsync(User);
            //if (user is null)
            //    return NotFound();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();
            var cartItems = await _cartRepository.GetAsync(e => e.ApplicationUserId == userId, includes: [e => e.Product]);
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
                            return BadRequest(new ErrorResponse()
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
            //var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = await _productRepository.GetOneAsync(e => e.Id == productId);
            if (userId is null || product is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == userId && e.ProductId == productId);
            if (cartItem is null)
            {
                await _cartRepository.CreateAsync(new Cart
                {
                    ApplicationUserId = userId,
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
            //var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == userId && e.ProductId == productId, includes: [e => e.Product]);
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
                return BadRequest(new ErrorResponse()
                {
                    ErrorMsg = "You have reached the maximum quantity for this product.",
                });
            }

            return NoContent();
        }
        [HttpPatch("{id}/decrement")]
        public async Task<IActionResult> Decrement([FromRoute] int productId)
        {
            //var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return NotFound();
            var cartItem = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == userId && e.ProductId == productId);
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
            //var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
                return NotFound();

            Order order = new Order
            {
                ApplicationUserId = userId,
            };
            await _orderRepository.CreateAsync(order);
            await _orderRepository.CommitAsync();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkouts/Success/{order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkouts/Cancel",
            };
            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == userId, includes: [e => e.Product]);
            order.TotalPrice = carts.Sum(e => e.Price);   
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
            order.SessionId = session.Id;
            await _orderRepository.CommitAsync();
            return Ok(new SuccessResponse()
            {
                Msg = "Checkout session created successfully.",
               OptionalData = [ session.Url ]  
            });
        }
    }
}
