using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            // Check if the order belongs to the current user or if the user is an admin
            var userId = User.FindFirst("sub")?.Value;
            if (order.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(order);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderCreateModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = new Order
            {
                UserId = userId,
                PaymentMethod = model.PaymentMethod,
                Status = OrderStatus.Pending
            };

            if (await _orderService.CreateOrderAsync(order, model.OrderItems))
            {
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }

            return BadRequest("Failed to create order");
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateModel model)
        {
            if (await _orderService.UpdateOrderStatusAsync(id, model.Status))
                return NoContent();

            return BadRequest("Failed to update order status");
        }

        [HttpPost("{id}/coupon")]
        public async Task<ActionResult> ApplyCoupon(int id, [FromBody] CouponApplyModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            if (order.UserId != userId)
                return Forbid();

            if (await _orderService.ApplyCouponAsync(id, model.CouponCode))
                return NoContent();

            return BadRequest("Failed to apply coupon");
        }

        [HttpPost("{id}/payment")]
        public async Task<ActionResult> ProcessPayment(int id, [FromBody] PaymentProcessModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            if (order.UserId != userId)
                return Forbid();

            if (await _orderService.ProcessPaymentAsync(
                id, model.CardNumber, model.CardHolderName, model.ExpiryDate, model.CVV))
                return NoContent();

            return BadRequest("Failed to process payment");
        }
    }

    public class OrderCreateModel
    {
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderStatusUpdateModel
    {
        public OrderStatus Status { get; set; }
    }

    public class CouponApplyModel
    {
        public string CouponCode { get; set; }
    }

    public class PaymentProcessModel
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}