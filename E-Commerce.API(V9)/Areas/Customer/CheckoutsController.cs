using System.Security.Claims;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace E_Commerce.API_V9_.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class CheckoutsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutsController> _logger;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        public CheckoutsController(ApplicationDbContext context, ILogger<CheckoutsController> logger ,IRepository<Cart> cartRepository, IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository)  
        {
            _context = context;
            _logger = logger;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
        }
        [HttpGet("{id}/Success")]
        public async Task<IActionResult> Success(int id)
        {
            //var user = await _userManager.GetUserAsync(User);
            //if (user is null)
            //    return NotFound();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                //1.Update Order Properties 
                var order = await _orderRepository.GetOneAsync(e => e.Id == id && e.ApplicationUserId == userId);
                if (order == null)
                    return NotFound();
                var service = new SessionService();
                var session = service.Get(order.SessionId);
                order.PaymentId = session.PaymentIntentId;
                order.OrderStatus = OrderStatus.inprocessing;
                order.PaymentStatus = PaymentStatus.completed;

                await _orderRepository.CommitAsync();
                //2. Move Order Items from Cart to OrderItems
                var cartItems = await _cartRepository.GetAsync(e => e.ApplicationUserId == userId, includes: [e => e.Product]);
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Count = item.Count,
                        Price = item.Price
                    };
                    await _orderItemRepository.CreateAsync(orderItem);
                }
                 await _orderItemRepository.CommitAsync();
                //3. Decrease Product Quantity
                foreach (var item in cartItems)
                {
                    item.Product.Quantity -= item.Count;
                }

                //4. Clear Cart
                foreach (var item in cartItems)
                {
                    _cartRepository.Delete(item);
                }
                await _cartRepository.CommitAsync();
                transaction.Commit();
                return Ok( new SuccessResponse()
                {
                    Msg = "Order completed successfully." 
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError($"Error:{ex.Message}");
                return BadRequest(new ErrorResponse()
                {
                    ErrorMsg = $"An error occurred while processing your order: {ex.Message}"
                });
            }
        }
        [HttpGet("{id}/Cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();
            var order = await _orderRepository.GetOneAsync(e => e.Id == id && e.ApplicationUserId == userId);
            if (order == null)
                return NotFound();

            order.OrderStatus = OrderStatus.inprocessing;
            order.PaymentStatus = PaymentStatus.completed;

            await _orderRepository.CommitAsync();
            return Ok(new ErrorResponse()
            {
                ErrorMsg = "Error occurred while canceling the order, please try again."
            });
        }
    }
}
