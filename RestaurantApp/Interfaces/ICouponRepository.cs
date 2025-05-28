using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        Task<Coupon> GetCouponByCodeAsync(string code);
        Task<bool> AssignCouponToUserAsync(string couponCode, string userId);
        Task<bool> IsCouponValidAsync(string couponCode, string userId);
        Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(int userPoints);
    }
}