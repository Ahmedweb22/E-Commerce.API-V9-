using E_Commerce.API_V9_.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN} , {SD.ROLE_EMPLOYEE}")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _repository;

        public OrdersController(IRepository<Order> repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? userId, int page = 1)
        {
            var orders = await _repository.GetAsync(tracking: false);

            // Filter
            if (userId is not null)
                orders = orders.Where(o => o.ApplicationUserId == userId).ToList();

            // Pagination
            if (page < 1)
                page = 1;

            int pageSize = 5;
            int currentPage = page;
            double totalCount = Math.Ceiling(orders.Count() / (double)pageSize);

            orders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new OrderResponse()
            {
                Orders = orders,
                TotalPages = totalCount,
                CurrentPage = currentPage
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var order = await _repository.GetOneAsync(o => o.Id == id);

            if (order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            await _repository.CreateAsync(order);
            await _repository.CommitAsync();

            return Ok(new SuccessResponse()
            {
                Msg = "Order created successfully"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Update([FromRoute] int id, Order order)
        {
            var orderInDb = await _repository.GetOneAsync(o => o.Id == id);

            if (orderInDb is null)
                return NotFound();

            orderInDb.OrderStatus = order.OrderStatus;
            orderInDb.PaymentStatus = order.PaymentStatus;
            orderInDb.PaymentType = order.PaymentType;
            orderInDb.TotalPrice = order.TotalPrice;
            orderInDb.Carrier = order.Carrier;
            orderInDb.Tracking = order.Tracking;
            orderInDb.ShippedDate = order.ShippedDate;

            await _repository.CommitAsync();

            return Ok(new SuccessResponse()
            {
                Msg = "Order updated successfully"
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var order = await _repository.GetOneAsync(o => o.Id == id);

            if (order is null)
                return NotFound();

            _repository.Delete(order);
            await _repository.CommitAsync();

            return Ok(new SuccessResponse()
            {
                Msg = "Order deleted successfully"
            });
        }
    }
}
