using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDishService _dishService;
        private readonly IOrderService _orderService;
        private readonly IReservationService _reservationService;
        private readonly IUserService _userService;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IDishService dishService,
            IOrderService orderService,
            IReservationService reservationService,
            IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dishService = dishService;
            _orderService = orderService;
            _reservationService = reservationService;
            _userService = userService;
        }

        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                // Get today's date
                var today = DateTime.Today;
                
                // Get count of today's orders
                var todayOrders = (await _orderService.GetOrdersForDateAsync(today)).ToList();
                
                // Get count of today's reservations
                var todayReservations = (await _reservationService.GetReservationsByDateAsync(today)).ToList();
                
                // Get count of registered users
                var userCount = _userManager.Users.Count();
                
                // Get total revenue for today
                var todayRevenue = todayOrders.Sum(o => o.TotalAmount);
                
                return Ok(new
                {
                    TodayOrdersCount = todayOrders.Count,
                    TodayReservationsCount = todayReservations.Count,
                    TotalUsersCount = userCount,
                    TodayRevenue = todayRevenue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard data." });
            }
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = _userManager.Users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.FullName,
                        u.Points,
                        u.IsAccountActivated
                    })
                    .ToList();

                var totalUsers = _userManager.Users.Count();

                return Ok(new
                {
                    Users = users,
                    TotalCount = totalUsers,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users." });
            }
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                var userRoles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Points,
                    user.IsAccountActivated,
                    Roles = userRoles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user details." });
            }
        }

        [HttpPost("user/{id}/role")]
        public async Task<IActionResult> AssignRoleToUser(string id, [FromBody] RoleAssignmentModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                    return BadRequest(new { message = "Role does not exist." });

                // Check if user already has this role
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(model.Role))
                    return BadRequest(new { message = $"User already has the role '{model.Role}'." });

                // Add role to user
                var result = await _userManager.AddToRoleAsync(user, model.Role);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to assign role to user.", errors = result.Errors });

                return Ok(new { message = $"Role '{model.Role}' assigned to user successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning role to user." });
            }
        }

        [HttpDelete("user/{id}/role")]
        public async Task<IActionResult> RemoveRoleFromUser(string id, [FromBody] RoleAssignmentModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                    return BadRequest(new { message = "Role does not exist." });

                // Check if user has this role
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(model.Role))
                    return BadRequest(new { message = $"User does not have the role '{model.Role}'." });

                // Remove role from user
                var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to remove role from user.", errors = result.Errors });

                return Ok(new { message = $"Role '{model.Role}' removed from user successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing role from user." });
            }
        }

        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            try
            {
                var roles = _roleManager.Roles
                    .Select(r => new
                    {
                        r.Id,
                        r.Name
                    })
                    .ToList();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving roles." });
            }
        }

        [HttpPost("role")]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreationModel model)
        {
            try
            {
                // Check if role already exists
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                    return BadRequest(new { message = $"Role '{model.RoleName}' already exists." });

                // Create new role
                var role = new IdentityRole(model.RoleName);
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to create role.", errors = result.Errors });

                return Ok(new { message = $"Role '{model.RoleName}' created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating role." });
            }
        }

        [HttpDelete("role/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                // Check if role exists
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return NotFound(new { message = $"Role '{roleName}' not found." });

                // Check if role is in use
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                if (usersInRole.Any())
                    return BadRequest(new { message = $"Cannot delete role '{roleName}' because it is assigned to {usersInRole.Count} user(s)." });

                // Delete role
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to delete role.", errors = result.Errors });

                return Ok(new { message = $"Role '{roleName}' deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting role." });
            }
        }

        [HttpGet("dishes")]
        public async Task<IActionResult> GetAllDishes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get all dishes with paging
                var dishes = await _dishService.GetAllDishesAsync(page, pageSize);
                var totalDishes = await _dishService.GetDishesCountAsync();

                return Ok(new
                {
                    Dishes = dishes,
                    TotalCount = totalDishes,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalDishes / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving dishes." });
            }
        }

        [HttpGet("analytics/sales")]
        public async Task<IActionResult> GetSalesAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Default to last 30 days if no dates provided
                startDate ??= DateTime.Today.AddDays(-30);
                endDate ??= DateTime.Today;

                var orders = await _orderService.GetOrdersByDateRangeAsync(startDate.Value, endDate.Value);
                var ordersData = orders.ToList();

                // Group by date
                var salesByDate = ordersData
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        TotalSales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // Top selling dishes
                var topDishes = ordersData
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.DishId)
                    .Select(g => new
                    {
                        DishId = g.Key,
                        DishName = g.First().Dish.Name,
                        TotalQuantity = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.Subtotal)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToList();

                // Sales by payment method
                var salesByPaymentMethod = ordersData
                    .GroupBy(o => o.PaymentMethod)
                    .Select(g => new
                    {
                        PaymentMethod = g.Key.ToString(),
                        TotalSales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .ToList();

                return Ok(new
                {
                    SalesByDate = salesByDate,
                    TopSellingDishes = topDishes,
                    SalesByPaymentMethod = salesByPaymentMethod,
                    TotalSales = ordersData.Sum(o => o.TotalAmount),
                    TotalOrders = ordersData.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving sales analytics." });
            }
        }

        [HttpGet("analytics/customers")]
        public async Task<IActionResult> GetCustomerAnalytics()
        {
            try
            {
                // Get all users
                var users = _userManager.Users.ToList();
                
                // Calculate basic customer statistics
                var totalCustomers = users.Count;
                var newCustomersLast30Days = users.Count(u => u.CreatedAt >= DateTime.Today.AddDays(-30));
                
                // Top customers by orders
                var orders = await _orderService.GetAllOrdersAsync();
                var ordersData = orders.ToList();
                
                var topCustomersByOrders = ordersData
                    .GroupBy(o => o.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UserName = users.FirstOrDefault(u => u.Id == g.Key)?.FullName ?? "Unknown",
                        OrderCount = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(10)
                    .ToList();
                
                // Top customers by spending
                var topCustomersBySpending = ordersData
                    .GroupBy(o => o.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UserName = users.FirstOrDefault(u => u.Id == g.Key)?.FullName ?? "Unknown",
                        OrderCount = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalSpent)
                    .Take(10)
                    .ToList();

                return Ok(new
                {
                    TotalCustomers = totalCustomers,
                    NewCustomersLast30Days = newCustomersLast30Days,
                    TopCustomersByOrders = topCustomersByOrders,
                    TopCustomersBySpending = topCustomersBySpending
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customer analytics." });
            }
        }
    }

    public class RoleAssignmentModel
    {
        public string Role { get; set; }
    }

    public class RoleCreationModel
    {
        public string RoleName { get; set; }
    }
}