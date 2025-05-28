using System;

namespace RestaurantApp.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public int DiscountPercentage { get; set; }
        public int PointsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ExpiryDate { get; set; }
        
        // User relationship (optional, a coupon might not be assigned to a user yet)
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}