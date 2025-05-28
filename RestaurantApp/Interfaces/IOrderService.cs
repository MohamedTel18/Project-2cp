using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersForDateAsync(DateTime date);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<bool> CreateOrderAsync(Order order, List<OrderItem> orderItems);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> ApplyCouponAsync(int orderId, string couponCode);
        Task<bool> ProcessPaymentAsync(int orderId, string cardNumber, string cardHolderName, string expiryDate, string cvv);
    }
}