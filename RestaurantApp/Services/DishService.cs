using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantApp.DTOs;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;
using RestaurantApp.Mappings;
using AutoMapper;

namespace RestaurantApp.Services
{
    public class DishService : IDishService
    {
        private readonly IMapper _mapper;
        private readonly IDishRepository _dishRepository;
        private readonly IRepository<Rating> _ratingRepository;

        public DishService(IDishRepository dishRepository, IRepository<Rating> ratingRepository,IMapper mapper)
        {
            _mapper = mapper;
            _dishRepository = dishRepository;
            _ratingRepository = ratingRepository;
        }

        public async Task<IEnumerable<Dish>> GetAllDishesAsync()
        {
            return await _dishRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Dish>> GetAllDishesAsync(int page, int pageSize)
        {
            // Calculate skip count based on page and page size
            int skipCount = (page - 1) * pageSize;
            
            // Get dishes with pagination
            var dishes = await _dishRepository.GetAllAsync();
            return dishes.Skip(skipCount).Take(pageSize);
        }
        
        public async Task<int> GetDishesCountAsync()
        {
            var dishes = await _dishRepository.GetAllAsync();
            return dishes.Count();
        }

        public async Task<Dish> GetDishByIdAsync(int id)
        {
            return await _dishRepository.GetDishWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            return await _dishRepository.GetDishesByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<Dish>> GetTopRatedDishesAsync(int count)
        {
            return await _dishRepository.GetTopRatedDishesAsync(count);
        }

        public async Task<IEnumerable<TopRatedDishDTO>> GetTopRatedDishSummaryAsync(int count = 3)
        {
            return await _dishRepository.GetTopRatedDishesWithDetailsAsync(count);
        }

        /*public async Task<bool> AddDishAsync(Dish dish)
        {
            try
            {
                await _dishRepository.AddAsync(dish);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }*/

        public async Task<bool> AddDishAsync(CreateDishDTO createDishDto)
        {
            try
            {
                var dish=_mapper.Map<Dish>(createDishDto);

                await _dishRepository.AddAsync(dish);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDishAsync(Dish dish)
        {
            try
            {
                await _dishRepository.UpdateAsync(dish);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDishAsync(int id)
        {
            try
            {
                var dish = await _dishRepository.GetByIdAsync(id);
                if (dish == null) return false;

                await _dishRepository.RemoveAsync(dish);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RateDishAsync(int dishId, string userId, int rating, string comment)
        {
            try
            {
                if (rating < 1 || rating > 5)
                    return false;

                var dish = await _dishRepository.GetByIdAsync(dishId);
                if (dish == null) return false;

                // Create a new rating
                var newRating = new Rating
                {
                    DishId = dishId,
                    UserId = userId,
                    Value = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                await _ratingRepository.AddAsync(newRating);

                // Update the dish's average rating
                await _dishRepository.UpdateRatingAsync(dishId, rating);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}