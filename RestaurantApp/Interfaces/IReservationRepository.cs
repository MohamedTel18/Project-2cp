using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantApp.Models;

namespace RestaurantApp.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(string userId);
        Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date);
        Task<bool> IsTableAvailableAsync(DateTime date, TimeSpan time, int numberOfPlaces);
        Task<bool> ApplyCouponAsync(int reservationId, string couponCode);
        Task ConfirmReservationAsync(int reservationId);
        Task<int> GetTotalReservedForDateAsync(DateTime date);
        Task<int> GetAvailableCapacityForDateAsync(DateTime date);
    }
}