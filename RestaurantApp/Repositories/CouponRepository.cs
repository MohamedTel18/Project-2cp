using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Repositories
{
    public class CouponRepository : Repository<Coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Coupon> GetCouponByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
        }

        public async Task<bool> AssignCouponToUserAsync(string couponCode, string userId)
        {
            var coupon = await GetCouponByCodeAsync(couponCode);
            if (coupon == null || coupon.UserId != null) return false;

            coupon.UserId = userId;
            await UpdateAsync(coupon);
            return true;
        }

        public async Task<bool> IsCouponValidAsync(string couponCode, string userId)
        {
            var coupon = await _dbSet
                .FirstOrDefaultAsync(c => c.Code == couponCode && 
                                          c.IsActive &&
                                          c.UserId == userId &&
                                          c.ExpiryDate > System.DateTime.Now);
            
            return coupon != null;
        }

        public async Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(int userPoints)
        {
            return await _dbSet
                .Where(c => c.IsActive &&
                            c.UserId == null &&
                            c.PointsRequired <= userPoints &&
                            c.ExpiryDate > System.DateTime.Now)
                .OrderBy(c => c.PointsRequired)
                .ToListAsync();
        }
    }
}