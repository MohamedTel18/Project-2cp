using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.DTOs;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IDishRepository : IRepository<Dish>
    {
        Task<IEnumerable<Dish>> GetTopRatedDishesAsync(int count);
        Task<IEnumerable<TopRatedDishDTO>> GetTopRatedDishesWithDetailsAsync(int count);
        Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId);
        Task<Dish> GetDishWithDetailsAsync(int id);
        Task UpdateRatingAsync(int dishId, int rating);
    }
}