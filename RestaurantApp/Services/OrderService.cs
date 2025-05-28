using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IDishRepository _dishRepository;
        private readonly IUserService _userService;

        public OrderService(
            IOrderRepository orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IDishRepository dishRepository,
            IUserService userService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _dishRepository = dishRepository;
            _userService = userService;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderWithItemsAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<bool> CreateOrderAsync(Order order, List<OrderItem> orderItems)
        {
            try
            {
                // Calculate order total
                decimal totalAmount = 0;
                foreach (var item in orderItems)
                {
                    var dish = await _dishRepository.GetByIdAsync(item.DishId);
                    if (dish == null) return false;

                    item.UnitPrice = dish.Price;
                    item.Subtotal = dish.Price * item.Quantity;
                    totalAmount += item.Subtotal;
                }

                // Set the total amount
                order.TotalAmount = totalAmount;
                
                // Create the order
                await _orderRepository.AddAsync(order);

                // Add order items
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    await _orderItemRepository.AddAsync(item);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                await _orderRepository.UpdateOrderStatusAsync(orderId, status);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ApplyCouponAsync(int orderId, string couponCode)
        {
            try
            {
                // Get the order to access user ID
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return false;

                // Apply the coupon to the order
                bool couponApplied = await _orderRepository.ApplyCouponAsync(orderId, couponCode);
                
                if (couponApplied)
                {
                    // Add 15 points to the user's account for using a coupon on an order
                    await _userService.AddPointsAsync(order.UserId, 15);
                }

                return couponApplied;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string cardNumber, string cardHolderName, string expiryDate, string cvv)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.PaymentMethod != PaymentMethod.Online) return false;

                // In a real application, you would integrate with a payment gateway
                // For this demo, we'll just store the payment details

                order.PaymentCardNumber = cardNumber;
                order.PaymentCardHolderName = cardHolderName;
                order.PaymentCardExpiryDate = expiryDate;
                order.PaymentCardCVV = cvv;
                order.Status = OrderStatus.Confirmed;

                await _orderRepository.UpdateAsync(order);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersForDateAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);
                return await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _orderRepository.GetAllWithItemsAsync();
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }
    }
}