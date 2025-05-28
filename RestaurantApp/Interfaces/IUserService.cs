using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<User> GetUserWithDetailsAsync(string userId);
        Task<bool> ActivateAccountAsync(string userId);
        Task<bool> AddPointsAsync(string userId, int points);
        Task<bool> UsePointsAsync(string userId, int points);
        Task<IEnumerable<Coupon>> GetUserCouponsAsync(string userId);
        Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(string userId);
        Task<bool> AssignCouponToUserAsync(string couponCode, string userId);
    }
}