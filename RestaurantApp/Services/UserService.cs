using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICouponRepository _couponRepository;

        public UserService(IUserRepository userRepository, ICouponRepository couponRepository)
        {
            _userRepository = userRepository;
            _couponRepository = couponRepository;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User> GetUserWithDetailsAsync(string userId)
        {
            return await _userRepository.GetUserWithDetailsAsync(userId);
        }

        public async Task<bool> ActivateAccountAsync(string userId)
        {
            return await _userRepository.ActivateAccountAsync(userId);
        }

        public async Task<bool> AddPointsAsync(string userId, int points)
        {
            if (points <= 0) return false;
            return await _userRepository.AddPointsAsync(userId, points);
        }

        public async Task<bool> UsePointsAsync(string userId, int points)
        {
            if (points <= 0) return false;
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Points < points)
                return false;
                
            return await _userRepository.UsePointsAsync(userId, points);
        }

        public async Task<IEnumerable<Coupon>> GetUserCouponsAsync(string userId)
        {
            return await _userRepository.GetUserCouponsAsync(userId);
        }

        public async Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new List<Coupon>();
            
            return await _couponRepository.GetAvailableCouponsAsync(user.Points);
        }

        public async Task<bool> AssignCouponToUserAsync(string couponCode, string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            
            var coupon = await _couponRepository.GetCouponByCodeAsync(couponCode);
            if (coupon == null || coupon.UserId != null || user.Points < coupon.PointsRequired)
                return false;
            
            // First deduct the points
            if (!await _userRepository.UsePointsAsync(userId, coupon.PointsRequired))
                return false;
                
            // Then assign the coupon
            return await _couponRepository.AssignCouponToUserAsync(couponCode, userId);
        }
    }
}