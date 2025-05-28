using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IUserRepository _userRepository;
        
        public OrderRepository(
            ApplicationDbContext context,
            ICouponRepository couponRepository,
            IUserRepository userRepository) : base(context)
        {
            _couponRepository = couponRepository;
            _userRepository = userRepository;
        }

        public async Task<Order> GetOrderWithItemsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> ApplyCouponAsync(int orderId, string couponCode)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null || order.IsCouponApplied) return false;

            var coupon = await _couponRepository.GetCouponByCodeAsync(couponCode);
            if (coupon == null || !coupon.IsActive) return false;

            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user == null || user.Points < coupon.PointsRequired) return false;

            // Apply discount
            decimal discountAmount = (order.TotalAmount * coupon.DiscountPercentage) / 100;
            order.DiscountAmount = discountAmount;
            order.IsCouponApplied = true;

            // Use points
            await _userRepository.UsePointsAsync(user.Id, coupon.PointsRequired);

            // Update order
            await UpdateAsync(order);
            return true;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return;

            order.Status = status;
            await UpdateAsync(order);

            // Add points to user when order is delivered
            if (status == OrderStatus.Delivered)
            {
                int pointsToAdd = (int)(order.TotalAmount / 10); // 1 point for every $10 spent
                await _userRepository.AddPointsAsync(order.UserId, pointsToAdd);
            }
        }

        public async Task<IEnumerable<Order>> GetAllWithItemsAsync()
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}