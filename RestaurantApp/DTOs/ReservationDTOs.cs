using System;

namespace RestaurantApp.DTOs
{    public class AvailabilityCheckModel
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int NumberOfPlaces { get; set; }
    }    public class ReservationCreateModel
    {
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int NumberOfPlaces { get; set; }
    }

    public class CouponApplyModel
    {
        public required string CouponCode { get; set; }
    }

    public class TableAvailabilityResponse
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int TotalCapacity { get; set; }
        public int AvailableCapacity { get; set; }
        public int ReservedCapacity { get; set; }
        public bool IsAvailable { get; set; }
        public List<ReservationSummary> ActiveReservations { get; set; } = new List<ReservationSummary>();
    }    public class ReservationSummary
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfPlaces { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class LiveTableStatusResponse
    {
        public DateTime CurrentDateTime { get; set; }
        public int TotalTables { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public int AvailableCapacity { get; set; }
        public double OccupancyPercentage { get; set; }
        public List<TimeSlotAvailability> TimeSlots { get; set; } = new List<TimeSlotAvailability>();
        public List<ReservationSummary> TodaysReservations { get; set; } = new List<ReservationSummary>();
    }

    public class TimeSlotAvailability
    {
        public TimeSpan Time { get; set; }
        public int AvailableCapacity { get; set; }
        public int ReservedCapacity { get; set; }
        public bool IsAvailable { get; set; }
        public int NumberOfReservations { get; set; }
    }
}