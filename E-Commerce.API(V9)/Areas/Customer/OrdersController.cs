using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Review = E_Commerce.API_V9_.Models.Review;

namespace E_Commerce.API_V9_.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<ReviewImg> _reviewImgRepository;
        public OrdersController(IRepository<Order> orderRepository , IRepository<OrderItem> orderItemRepository, IRepository<Review> reviewRepository, IRepository<ReviewImg> reviewImgRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _reviewRepository = reviewRepository;
            _reviewImgRepository = reviewImgRepository;
        }
        // Show all order
        [HttpGet]
        public async Task<IActionResult> Get(int? id , int page)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();

            var orders = await _orderRepository.GetAsync(e => e.ApplicationUserId == userId);
            //Add new filter
            if (id is not null)
                orders = orders.Where(e => e.Id == id).ToList();

            // Pagination
            if (page < 1)
                page = 1;
            int pageSize = 5;
            int currentPage = page;
            double totalCount = Math.Ceiling(orders.Count() / (double)pageSize);
            orders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new 
            {
                Orders = orders.AsEnumerable(),
                CurrentPage = currentPage,
                TotalPages = totalCount
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();
            var order = await _orderRepository.GetOneAsync(e => e.Id == id && e.ApplicationUserId == userId);
            if (order is null)
                return NotFound();
            var orderItems = await _orderItemRepository.GetAsync(e => e.OrderId == order.Id, includes: [e => e.Product]);
            return Ok(new
            {
                order,
                orderItems
            
            });
        }
        // Cancel
        [HttpGet("{id}/Cancel")]
        public async Task<IActionResult> Cancel ( int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();
            var order = await _orderRepository.GetOneAsync(e => e.Id == id && e.ApplicationUserId == userId);
            if (order is null)
                return NotFound();
            if (order.OrderStatus == OrderStatus.shipped)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorMsg = "You can't cancel this order because it's already shipped",
                });
            }
            var options = new RefundCreateOptions
            {
                PaymentIntent = order.PaymentId,
                Amount = (long)(order.TotalPrice * 100),
                Reason = RefundReasons.RequestedByCustomer,
            };
            var service = new RefundService();
            var session = service.Create(options);

            order.OrderStatus = OrderStatus.canceled;
            order.PaymentStatus = PaymentStatus.refunded;
            await _orderRepository.CommitAsync();
            return NoContent();

        }

        //Rate
        [HttpPost("Rate")]
        public async Task<IActionResult> Rate([FromForm] ReviewCreateRequest model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();

            var userOrders = (await _orderRepository.GetAsync(e => e.ApplicationUserId == userId && e.OrderStatus == OrderStatus.completed)).Select(e => e.Id);
            List<int> orderItems = [];
            foreach (var item in userOrders)
            {
                orderItems.AddRange((await _orderItemRepository.GetAsync(e => e.OrderId == item)).Select(e => e.Id));
            }
            if (orderItems.Contains(model.ProductId))
            {
                var review = new Review()
                {
                    ApplicationUserId = userId,
                    ProductId = model.ProductId,
                    Rate = model.Rate,
                    Comment = model.Comment,
                };
                await _reviewRepository.CreateAsync(review);
                await _reviewRepository.CommitAsync();
                if (model.Imgs is not null && model.Imgs.Any())
                {
                    foreach (var img in model.Imgs)
                    {
                        var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\rates", newFileName);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            img.CopyTo(stream);
                        }
                       var reviewImg = new ReviewImg()
                        {
                            ReviewId = review.Id,
                            Img = newFileName
                        };
                        await _reviewImgRepository.CreateAsync(reviewImg);
                        await _reviewImgRepository.CommitAsync();
                    }
                }
                return Ok( new SuccessResponse()
                {
                    Msg = "Review submitted successfully"
                });
            }
            else
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorMsg = "You can't rate this product because you haven't bought it yet",
                });
            }

        }
    }
}
