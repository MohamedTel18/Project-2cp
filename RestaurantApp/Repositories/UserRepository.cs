using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetUserWithDetailsAsync(string userId)
        {
            return await _dbSet
                .Include(u => u.Orders)
                .Include(u => u.Reservations)
                .Include(u => u.Coupons)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> ActivateAccountAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.IsAccountActivated = true;
            await UpdateAsync(user);
            return true;
        }

        public async Task<bool> AddPointsAsync(string userId, int points)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.Points += points;
            await UpdateAsync(user);
            return true;
        }

        public async Task<bool> UsePointsAsync(string userId, int points)
        {
            var user = await GetByIdAsync(userId);
            if (user == null || user.Points < points) return false;

            user.Points -= points;
            await UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<Coupon>> GetUserCouponsAsync(string userId)
        {
            var user = await GetUserWithDetailsAsync(userId);
            if (user == null) return new List<Coupon>();

            return user.Coupons.Where(c => c.IsActive).ToList();
        }
    }
}