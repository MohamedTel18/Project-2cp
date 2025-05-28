using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RestaurantApp.Models
{
    public class User : IdentityUser
    {
        public required string FullName { get; set; }
        public int Points { get; set; } = 0;
        public bool IsAccountActivated { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}