using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.DTOs;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishController : ControllerBase
    {
        private readonly IDishService _dishService;

        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dish>>> GetAllDishes()
        {
            var dishes = await _dishService.GetAllDishesAsync();
            return Ok(dishes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Dish>> GetDish(int id)
        {
            var dish = await _dishService.GetDishByIdAsync(id);
            if (dish == null)
                return NotFound();

            return Ok(dish);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Dish>>> GetDishesByCategory(int categoryId)
        {
            var dishes = await _dishService.GetDishesByCategoryAsync(categoryId);
            return Ok(dishes);
        }

        [HttpGet("top/{count}")]
        public async Task<ActionResult<IEnumerable<Dish>>> GetTopDishes(int count)
        {
            
            var dishes = await _dishService.GetTopRatedDishesAsync(count);
            return Ok(dishes);
        }
          [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<TopRatedDishDTO>>> GetTopRatedDishes()
        {
            int count = 3;
            var dishes = await _dishService.GetTopRatedDishSummaryAsync(count);
            return Ok(dishes);
        }
        /*
                [HttpPost]
                [Authorize(Roles = "Admin")]
                [ProducesResponseType(200)]
                [ProducesResponseType(400)]
                public async Task<ActionResult> AddDish([FromBody] CreateDishDTO createDishDto)
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    if (await _dishService.AddDishAsync(createDishDto))
                        return Ok(new { message = "Dish created successfully" });

                    return BadRequest(new { message = "Failed to add dish" });
                }*/

        [HttpPost("create")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDish(CreateDishDTO createDishDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _dishService.AddDishAsync(createDishDto))
                return Ok(new { message = "Dish created successfully" });

            return BadRequest(new { message = "Failed to add dish" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDish(int id, Dish dish)
        {
            if (id != dish.Id)
                return BadRequest();

            if (await _dishService.UpdateDishAsync(dish))
                return NoContent();

            return BadRequest("Failed to update dish");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDish(int id)
        {
            if (await _dishService.DeleteDishAsync(id))
                return NoContent();

            return NotFound();
        }

        [HttpPost("{id}/rate")]
        [Authorize]
        public async Task<ActionResult> RateDish(int id, [FromBody] RatingModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (await _dishService.RateDishAsync(id, userId, model.Value, model.Comment))
                return Ok();

            return BadRequest("Failed to rate dish");
        }
    }

    public class RatingModel
    {
        public int Value { get; set; }
        public required string Comment { get; set; }
    }
}