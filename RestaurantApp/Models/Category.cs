using System;
using System.Collections.Generic;

namespace RestaurantApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        
        // Navigation property
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}