using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public UserController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileModel>> GetUserProfile()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userService.GetUserWithDetailsAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new UserProfileModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Points = user.Points
            });
        }

        [HttpGet("coupons")]
        public async Task<ActionResult<IEnumerable<Coupon>>> GetUserCoupons()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var coupons = await _userService.GetUserCouponsAsync(userId);
            return Ok(coupons);
        }

        [HttpGet("coupons/available")]
        public async Task<ActionResult<IEnumerable<Coupon>>> GetAvailableCoupons()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var coupons = await _userService.GetAvailableCouponsAsync(userId);
            return Ok(coupons);
        }

        [HttpPost("coupons/assign")]
        public async Task<ActionResult> AssignCoupon([FromBody] CouponAssignModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (await _userService.AssignCouponToUserAsync(model.CouponCode, userId))
                return NoContent();

            return BadRequest("Failed to assign coupon");
        }

        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                // In a real application, you would send a new email confirmation
                user.Email = model.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPut("password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }

    public class UserProfileModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int Points { get; set; }
    }

    public class CouponAssignModel
    {
        public string CouponCode { get; set; }
    }

    public class UpdateProfileModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}