using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;
using RestaurantApp.DTOs;

namespace RestaurantApp.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(string userId);
        Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date);
        Task<bool> IsTableAvailableAsync(DateTime date, TimeSpan time, int numberOfPlaces);
        Task<bool> CreateReservationAsync(Reservation reservation);
        Task<bool> ConfirmReservationAsync(int reservationId);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<bool> ApplyCouponAsync(int reservationId, string couponCode);
        
        // New live table availability methods
        Task<TableAvailabilityResponse> GetTableAvailabilityAsync(DateTime date, TimeSpan time);
        Task<LiveTableStatusResponse> GetLiveTableStatusAsync(DateTime? date = null);
    }
}