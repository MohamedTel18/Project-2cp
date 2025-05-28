using System;

namespace RestaurantApp.Models
{    public class Reservation
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Nullable for guest reservations
        public string CustomerName { get; set; } = null!; // Required for all reservations
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfPlaces { get; set; }
        public bool IsConfirmed { get; set; }
        public string? CouponCode { get; set; }
        public bool IsCouponApplied { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public User? User { get; set; } // Nullable for guest reservations
    }
}