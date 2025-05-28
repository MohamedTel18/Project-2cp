using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.DTOs;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetAllDishesAsync();
        Task<IEnumerable<Dish>> GetAllDishesAsync(int page, int pageSize);
        Task<int> GetDishesCountAsync();
        Task<Dish> GetDishByIdAsync(int id);
        Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId);
        Task<IEnumerable<Dish>> GetTopRatedDishesAsync(int count);
        Task<IEnumerable<TopRatedDishDTO>> GetTopRatedDishSummaryAsync(int count = 3);
        //Task<bool> AddDishAsync(Dish dish);
        Task<bool> AddDishAsync(CreateDishDTO createDishDto);
        Task<bool> UpdateDishAsync(Dish dish);
        Task<bool> DeleteDishAsync(int id);
        Task<bool> RateDishAsync(int dishId, string userId, int rating, string comment);
    }
}