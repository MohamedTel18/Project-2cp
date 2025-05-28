using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;
using RestaurantApp.DTOs;

namespace RestaurantApp.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserService _userService;

        public ReservationService(IReservationRepository reservationRepository, IUserService userService)
        {
            _reservationRepository = reservationRepository;
            _userService = userService;
        }

        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _reservationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(string userId)
        {
            return await _reservationRepository.GetReservationsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date)
        {
            return await _reservationRepository.GetReservationsByDateAsync(date);
        }        public async Task<bool> IsTableAvailableAsync(DateTime date, TimeSpan time, int numberOfPlaces)
        {
            return await _reservationRepository.IsTableAvailableAsync(date, time, numberOfPlaces);
        }        public async Task<bool> CreateReservationAsync(Reservation reservation)
        {
            try
            {                // Check if table is available
                if (!await IsTableAvailableAsync(
                    reservation.ReservationDate, 
                    reservation.ReservationTime, 
                    reservation.NumberOfPlaces))
                {
                    return false;
                }

                // Create the reservation and automatically confirm it
                reservation.IsConfirmed = true; // Auto-confirm for immediate availability update
                reservation.CreatedAt = DateTime.UtcNow;
                await _reservationRepository.AddAsync(reservation);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConfirmReservationAsync(int reservationId)
        {
            try
            {
                await _reservationRepository.ConfirmReservationAsync(reservationId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null) return false;

                await _reservationRepository.RemoveAsync(reservation);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ApplyCouponAsync(int reservationId, string couponCode)
        {
            try
            {
                // Get the reservation to access user ID
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null) return false;

                // Apply the coupon to the reservation
                bool couponApplied = await _reservationRepository.ApplyCouponAsync(reservationId, couponCode);
                  if (couponApplied && !string.IsNullOrEmpty(reservation.UserId))
                {
                    // Add 10 points to the user's account for using a coupon
                    await _userService.AddPointsAsync(reservation.UserId, 10);
                }

                return couponApplied;
            }
            catch (Exception)
            {
                return false;
            }
        }        public async Task<TableAvailabilityResponse> GetTableAvailabilityAsync(DateTime date, TimeSpan time)
        {
            // Dynamic capacity: 20 - total number of reservation records in database
            var totalReservationRecords = await _reservationRepository.GetAllAsync();
            var dynamicMaxCapacity = Math.Max(0, 20 - totalReservationRecords.Count());

            // Get reservations within 2-hour window around the requested time
            var timeStart = time.Add(new TimeSpan(-1, 0, 0)); // 1 hour before
            var timeEnd = time.Add(new TimeSpan(1, 0, 0));    // 1 hour after

            var reservationsInWindow = await _reservationRepository.GetReservationsByDateAsync(date);
            var activeReservations = reservationsInWindow
                .Where(r => r.IsConfirmed && 
                           r.ReservationTime >= timeStart && 
                           r.ReservationTime <= timeEnd)
                .ToList();

            int reservedCapacity = activeReservations.Sum(r => r.NumberOfPlaces);
            int availableCapacity = dynamicMaxCapacity - reservedCapacity;            return new TableAvailabilityResponse
            {
                Date = date,
                Time = time,
                TotalCapacity = dynamicMaxCapacity,
                AvailableCapacity = Math.Max(0, availableCapacity),
                ReservedCapacity = reservedCapacity,
                IsAvailable = availableCapacity > 0,
                ActiveReservations = activeReservations.Select(r => new ReservationSummary                {
                    Id = r.Id,
                    CustomerName = r.CustomerName ?? "Guest",
                    ReservationTime = r.ReservationTime,
                    NumberOfPlaces = r.NumberOfPlaces,
                    IsConfirmed = r.IsConfirmed
                }).ToList()
            };
        }        public async Task<LiveTableStatusResponse> GetLiveTableStatusAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Now.Date;
            
            // Dynamic capacity: 20 - total number of reservation records in database
            var totalReservationRecords = await _reservationRepository.GetAllAsync();
            var dynamicMaxCapacity = Math.Max(0, 20 - totalReservationRecords.Count());
            var dynamicMaxTables = dynamicMaxCapacity; // For this app, 1 table = 1 person capacity

            // Get total reserved and available capacity for the day
            var totalReserved = await _reservationRepository.GetTotalReservedForDateAsync(targetDate);
            var availableCapacity = await _reservationRepository.GetAvailableCapacityForDateAsync(targetDate);

            // Get all reservations for the target date
            var allReservations = await _reservationRepository.GetReservationsByDateAsync(targetDate);
            var confirmedReservations = allReservations.Where(r => r.IsConfirmed).ToList();

            // Generate time slots for the day (every 30 minutes from 5:30 PM to 9:30 PM)
            var timeSlots = new List<TimeSlotAvailability>();
            var startTime = new TimeSpan(17, 30, 0); // 5:30 PM
            var endTime = new TimeSpan(21, 30, 0);   // 9:30 PM
            
            for (var time = startTime; time <= endTime; time = time.Add(new TimeSpan(0, 30, 0)))
            {
                // Calculate reservations within 1 hour window of this time slot
                var slotStart = time.Add(new TimeSpan(-1, 0, 0));
                var slotEnd = time.Add(new TimeSpan(1, 0, 0));
                
                var slotReservations = confirmedReservations
                    .Where(r => r.ReservationTime >= slotStart && r.ReservationTime <= slotEnd)
                    .ToList();

                var slotReservedCapacity = slotReservations.Sum(r => r.NumberOfPlaces);
                var slotAvailableCapacity = dynamicMaxCapacity - slotReservedCapacity;

                timeSlots.Add(new TimeSlotAvailability
                {
                    Time = time,
                    AvailableCapacity = Math.Max(0, slotAvailableCapacity),
                    ReservedCapacity = slotReservedCapacity,
                    IsAvailable = slotAvailableCapacity > 0,
                    NumberOfReservations = slotReservations.Count
                });
            }

            return new LiveTableStatusResponse
            {
                CurrentDateTime = DateTime.Now,
                TotalTables = dynamicMaxTables,
                TotalCapacity = dynamicMaxCapacity,
                CurrentOccupancy = totalReserved, // Total reserved for the day
                AvailableCapacity = availableCapacity, // Available capacity for the day
                OccupancyPercentage = dynamicMaxCapacity > 0 ? Math.Round((double)totalReserved / dynamicMaxCapacity * 100, 2) : 0,
                TimeSlots = timeSlots,                TodaysReservations = confirmedReservations.Select(r => new ReservationSummary
                {
                    Id = r.Id,
                    CustomerName = r.CustomerName ?? "Guest",
                    ReservationTime = r.ReservationTime,
                    NumberOfPlaces = r.NumberOfPlaces,
                    IsConfirmed = r.IsConfirmed
                }).OrderBy(r => r.ReservationTime).ToList()
            };
        }
    }
}