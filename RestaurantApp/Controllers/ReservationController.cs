using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.DTOs;
using RestaurantApp.Interfaces;
using RestaurantApp.Models;

namespace RestaurantApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            // Check if the reservation belongs to the current user or if the user is an admin
            // Allow access to guest reservations (where UserId is null)
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(reservation.UserId) && 
                reservation.UserId != userId && 
                !User.IsInRole("Admin"))
                return Forbid();

            return Ok(reservation);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUserReservations()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reservations = await _reservationService.GetReservationsByUserIdAsync(userId);
            return Ok(reservations);
        }

        [HttpGet("date/{date}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservationsByDate(DateTime date)
        {
            var reservations = await _reservationService.GetReservationsByDateAsync(date);
            return Ok(reservations);
        }        [HttpGet("availability")]
        public async Task<ActionResult<bool>> CheckAvailability([FromQuery] AvailabilityCheckModel model)
        {
            var isAvailable = await _reservationService.IsTableAvailableAsync(
                model.Date, 
                model.Time, 
                model.NumberOfPlaces);
            
            return Ok(isAvailable);
        }[HttpPost]
        [AllowAnonymous] // Allow guest reservations
        public async Task<ActionResult> CreateReservation([FromBody] ReservationCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }            // Check table availability first
            var isAvailable = await _reservationService.IsTableAvailableAsync(
                model.Date, 
                model.Time, 
                model.NumberOfPlaces);

            if (!isAvailable)
            {
                return BadRequest(new { message = "Reservation impossible - no tables available for the requested date and time." });
            }

            // Get user ID if authenticated, otherwise null for guest reservations
            var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst("sub")?.Value : null;            var reservation = new Reservation
            {
                UserId = userId,
                CustomerName = model.Name,
                ReservationDate = model.Date,
                ReservationTime = model.Time,
                NumberOfPlaces = model.NumberOfPlaces,
                IsConfirmed = false, // Requires confirmation
                CreatedAt = DateTime.Now
            };

            if (await _reservationService.CreateReservationAsync(reservation))
            {                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, new 
                {
                    id = reservation.Id,
                    customerName = reservation.CustomerName,
                    reservationDate = reservation.ReservationDate,
                    reservationTime = reservation.ReservationTime,
                    numberOfPlaces = reservation.NumberOfPlaces,
                    isConfirmed = reservation.IsConfirmed,
                    message = "Reservation created successfully. Please wait for confirmation."
                });
            }

            return BadRequest(new { message = "Failed to create reservation due to a server error." });
        }

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ConfirmReservation(int id)
        {
            if (await _reservationService.ConfirmReservationAsync(id))
                return NoContent();

            return BadRequest("Failed to confirm reservation");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelReservation(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (await _reservationService.CancelReservationAsync(id))
                return NoContent();

            return BadRequest("Failed to cancel reservation");
        }

        [HttpPost("{id}/coupon")]
        public async Task<ActionResult> ApplyCoupon(int id, [FromBody] CouponApplyModel model)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            if (reservation.UserId != userId)
                return Forbid();

            if (await _reservationService.ApplyCouponAsync(id, model.CouponCode))
                return NoContent();

            return BadRequest("Failed to apply coupon");
        }

        [HttpGet("live-status")]
        [AllowAnonymous] // Allow public access to live table status
        public async Task<ActionResult<LiveTableStatusResponse>> GetLiveTableStatus([FromQuery] DateTime? date = null)
        {
            try
            {
                var liveStatus = await _reservationService.GetLiveTableStatusAsync(date);
                return Ok(liveStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving live table status", error = ex.Message });
            }
        }

        [HttpGet("table-availability")]
        [AllowAnonymous] // Allow public access to table availability
        public async Task<ActionResult<TableAvailabilityResponse>> GetTableAvailability(
            [FromQuery] DateTime date, 
            [FromQuery] TimeSpan time)
        {
            try
            {
                var availability = await _reservationService.GetTableAvailabilityAsync(date, time);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving table availability", error = ex.Message });
            }
        }

        [HttpGet("live-dashboard")]
        [Authorize(Roles = "Admin")] // Admin-only dashboard
        public async Task<ActionResult<object>> GetLiveDashboard()
        {
            try
            {
                var today = DateTime.Now.Date;
                var liveStatus = await _reservationService.GetLiveTableStatusAsync(today);
                var todayReservations = await _reservationService.GetReservationsByDateAsync(today);

                var dashboard = new
                {
                    LiveStatus = liveStatus,                    Statistics = new
                    {
                        TotalReservationsToday = todayReservations.Count(),
                        ConfirmedReservationsToday = todayReservations.Count(r => r.IsConfirmed),
                        PendingReservationsToday = todayReservations.Count(r => !r.IsConfirmed),
                        TotalGuestsToday = todayReservations.Where(r => r.IsConfirmed).Sum(r => r.NumberOfPlaces),
                        AveragePartySize = todayReservations.Any() ? 
                            Math.Round(todayReservations.Average(r => r.NumberOfPlaces), 1) : 0
                    }
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard data", error = ex.Message });
            }
        }
    }
}