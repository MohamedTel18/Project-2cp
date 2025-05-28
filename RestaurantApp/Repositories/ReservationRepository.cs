using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IUserRepository _userRepository;          // Restaurant capacity constants (could be moved to configuration)
        private const int MAX_TABLES = 20;
        private const int MAX_CAPACITY = 20; // Maximum places capacity (same as tables)
        
        public ReservationRepository(
            ApplicationDbContext context,
            ICouponRepository couponRepository,
            IUserRepository userRepository) : base(context)
        {
            _couponRepository = couponRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(r => r.ReservationDate.Date == date.Date)
                .OrderBy(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<bool> IsTableAvailableAsync(DateTime date, TimeSpan time, int numberOfPlaces)
        {
            // Get all confirmed reservations for the specified date and time (within 2 hours)
            var timeStart = time.Add(new TimeSpan(-1, 0, 0)); // 1 hour before
            var timeEnd = time.Add(new TimeSpan(1, 0, 0));    // 1 hour after
            
            var reservations = await _dbSet
                .Where(r => r.ReservationDate.Date == date.Date && 
                           r.IsConfirmed &&
                           r.ReservationTime >= timeStart && 
                           r.ReservationTime <= timeEnd)
                .ToListAsync();
              // Calculate total places already booked
            int totalPlacesBooked = reservations.Sum(r => r.NumberOfPlaces);
            
            // Check if adding the new reservation would exceed capacity
            return (totalPlacesBooked + numberOfPlaces) <= MAX_CAPACITY;
        }

        public async Task<bool> ApplyCouponAsync(int reservationId, string couponCode)
        {
            var reservation = await GetByIdAsync(reservationId);
            if (reservation == null || reservation.IsCouponApplied) return false;

            var coupon = await _couponRepository.GetCouponByCodeAsync(couponCode);
            if (coupon == null || !coupon.IsActive) return false;

            // Null check before accessing UserId
            if (string.IsNullOrEmpty(reservation.UserId)) return false;
            
            var user = await _userRepository.GetByIdAsync(reservation.UserId);
            if (user == null || user.Points < coupon.PointsRequired) return false;            // Apply discount (assuming a fixed amount per place)
            decimal baseAmount = reservation.NumberOfPlaces * 10m; // $10 per place
            decimal discountAmount = (baseAmount * coupon.DiscountPercentage) / 100;
            reservation.DiscountAmount = discountAmount;
            reservation.IsCouponApplied = true;

            // Use points
            await _userRepository.UsePointsAsync(user.Id, coupon.PointsRequired);

            // Update reservation
            await UpdateAsync(reservation);
            return true;
        }

        public async Task ConfirmReservationAsync(int reservationId)
        {
            var reservation = await GetByIdAsync(reservationId);
            if (reservation == null) return;

            reservation.IsConfirmed = true;
            await UpdateAsync(reservation);
            
            // Add points to user for confirmed reservation (only if user is not null)
            if (!string.IsNullOrEmpty(reservation.UserId))
            {
                await _userRepository.AddPointsAsync(reservation.UserId, 5); // 5 points for reservation
            }
        }        // Get total places reserved for today (all confirmed reservations)
        public async Task<int> GetTotalReservedForDateAsync(DateTime date)
        {
            var reservations = await _dbSet
                .Where(r => r.ReservationDate.Date == date.Date && r.IsConfirmed)
                .ToListAsync();
            
            return reservations.Sum(r => r.NumberOfPlaces);
        }// Get available capacity for today
        public async Task<int> GetAvailableCapacityForDateAsync(DateTime date)
        {
            // Dynamic capacity: 20 - total number of reservation records in database
            var totalReservationRecords = await _dbSet.CountAsync();
            var dynamicMaxCapacity = Math.Max(0, 20 - totalReservationRecords);
            
            var totalReserved = await GetTotalReservedForDateAsync(date);
            return Math.Max(0, dynamicMaxCapacity - totalReserved);
        }
    }
}