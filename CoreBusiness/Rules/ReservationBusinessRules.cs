using System;
using System.Collections.Generic;
using System.Linq;
using CoreBusiness.Enums;
using CoreBusiness.Exceptions;
using CoreBusiness.Models;

namespace CoreBusiness.Rules;

/// <summary>
/// TIÊU CHÍ K1.3: Cài đặt LUẬT 1 - RÀNG BUỘC ĐẶT BÀN TRONG COREBUSINESS
/// Luật 1.1: Số khách đặt <= Sức chứa của bàn
/// Luật 1.2: Một bàn không có 2 đặt chỗ trùng nhau trong cùng khung 2 giờ
/// </summary>
public static class ReservationBusinessRules
{
    public const double MinHoursBetweenReservations = 2.0;

    /// <summary>
    /// Kiểm tra sức chứa của bàn so với lượng khách
    /// </summary>
    public static void ValidateCapacity(int guestCount, DiningTable table)
    {
        if (guestCount <= 0)
        {
            throw new DomainException("Số lượng khách phải lớn hơn 0.");
        }

        if (table == null)
        {
            throw new DomainException("Thông tin bàn không tồn tại.");
        }

        if (guestCount > table.Capacity)
        {
            throw new CapacityExceededException(guestCount, table.Capacity, table.TableNumber);
        }
    }

    /// <summary>
    /// Kiểm tra trùng lặp lịch đặt trong khoảng 2 giờ
    /// </summary>
    public static void ValidateTimeOverlap(
        DateTime requestedTime, 
        int requestedTableId, 
        string tableNumber, 
        IEnumerable<Reservation> existingReservations,
        int? ignoreReservationId = null)
    {
        if (requestedTime < DateTime.Now.AddMinutes(-5))
        {
            throw new DomainException("Thời gian đặt bàn phải ở tương lai, không thể đặt giờ đã qua.");
        }

        // Lọc các lịch đặt bàn còn hiệu lực (không bị Hủy) của đúng bàn này
        var activeReservations = existingReservations
            .Where(r => r.DiningTableId == requestedTableId)
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .Where(r => !ignoreReservationId.HasValue || r.ReservationId != ignoreReservationId.Value);

        foreach (var existing in activeReservations)
        {
            var difference = Math.Abs((existing.ReservationTime - requestedTime).TotalHours);
            if (difference < MinHoursBetweenReservations)
            {
                throw new ReservationOverlapException(tableNumber, requestedTime, existing.ReservationTime);
            }
        }
    }
}
