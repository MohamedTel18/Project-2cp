using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.DTOs;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Repositories
{    public class DishRepository : Repository<Dish>, IDishRepository
    {
        public DishRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Dish>> GetTopRatedDishesAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.NumberOfRaters)
                .Take(count)
                .Include(d => d.Category)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<TopRatedDishDTO>> GetTopRatedDishesWithDetailsAsync(int count)
        {
            var topDishes = await _dbSet
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.NumberOfRaters)
                .Take(count)
                .Select(d => new TopRatedDishDTO 
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    PhotoUrl = d.PhotoUrl,
                    AverageRating = d.AverageRating,
                    NumberOfRaters = d.NumberOfRaters
                })
                .ToListAsync();
                
            return topDishes;
        }

        public async Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(d => d.CategoryId == categoryId)
                .Include(d => d.Category)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }        public async Task<Dish> GetDishWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Category)
                .Include(d => d.Ratings)
                .FirstOrDefaultAsync(d => d.Id == id) ?? new Dish
                {
                    Id = 0,
                    Name = "Not Found",
                    Description = "Dish not found",
                    PhotoUrl = "",
                    Price = 0
                };
        }

        public async Task UpdateRatingAsync(int dishId, int rating)
        {
            var dish = await GetByIdAsync(dishId);
            if (dish == null) return;

            // Update the average rating and number of raters
            double currentTotal = dish.AverageRating * dish.NumberOfRaters;
            dish.NumberOfRaters++;
            dish.AverageRating = (currentTotal + rating) / dish.NumberOfRaters;

            await UpdateAsync(dish);
        }
    }
}