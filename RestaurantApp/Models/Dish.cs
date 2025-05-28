using System;
using System.Collections.Generic;

namespace RestaurantApp.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string PhotoUrl { get; set; }
        public decimal Price { get; set; }
        public double AverageRating { get; set; } = 0;
        public int NumberOfRaters { get; set; } = 0;
        
        // Category relationship
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        
        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}