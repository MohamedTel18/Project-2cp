using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserWithDetailsAsync(string userId);
        Task<bool> ActivateAccountAsync(string userId);
        Task<bool> AddPointsAsync(string userId, int points);
        Task<bool> UsePointsAsync(string userId, int points);
        Task<IEnumerable<Coupon>> GetUserCouponsAsync(string userId);
    }
}