using System;

namespace RestaurantApp.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Value { get; set; } // Rating value (1-5)
        public required string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // User relationship
        public required string UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Dish relationship
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
    }
}