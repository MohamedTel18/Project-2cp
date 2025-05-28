using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order> GetOrderWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetAllWithItemsAsync();
        Task<bool> ApplyCouponAsync(int orderId, string couponCode);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}