using System;
using System.Collections.Generic;

namespace RestaurantApp.Models
{
    public enum PaymentMethod
    {
        Cash,
        Online
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        InProgress,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsCouponApplied { get; set; } = false;
        
        // User relationship
        public required string UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Payment information (if online payment)
        public string? PaymentCardNumber { get; set; }
        public string? PaymentCardHolderName { get; set; }
        public string? PaymentCardExpiryDate { get; set; }
        public string? PaymentCardCVV { get; set; }
        
        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}