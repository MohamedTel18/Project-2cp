using System;

namespace RestaurantApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        
        // Order relationship
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        // Dish relationship
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
    }
}