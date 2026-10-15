using System;
using CoreBusiness.Enums;

namespace CoreBusiness.Models;

public class Reservation
{
    public int ReservationId { get; set; }
    public int DiningTableId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime ReservationTime { get; set; } = DateTime.Now.AddHours(2);
    public int GuestCount { get; set; } = 2;
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation / Display property
    public string TableNumber { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
}
