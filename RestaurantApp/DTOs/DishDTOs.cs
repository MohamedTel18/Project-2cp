using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.DTOs
{
    public class TopRatedDishDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; } = 0;
        public int NumberOfRaters { get; set; } = 0;
    }

    public class DishDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int NumberOfRaters { get; set; }
        public int CategoryId { get; set; }
    }

    public record CreateDishDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public string? PhotoUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between 0.01 and 999.99")]
        public decimal Price { get; set; }
    }
}
