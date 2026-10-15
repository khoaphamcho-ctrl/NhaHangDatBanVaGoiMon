using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class ReservationInMemoryRepository : IReservationRepository
{
    private static readonly List<Reservation> _reservations = new()
    {
        // Dữ liệu mẫu phục vụ kiểm thử ca vi phạm Luật 1 (Trùng khung giờ 2h)
        new Reservation
        {
            ReservationId = 1,
            DiningTableId = 1, // Bàn 01
            TableNumber = "Bàn 01",
            TableCapacity = 2,
            CustomerName = "Nguyễn Văn An",
            PhoneNumber = "0905123456",
            ReservationTime = DateTime.Today.AddHours(18), // 18:00 hôm nay
            GuestCount = 2,
            Status = ReservationStatus.Confirmed,
            Note = "Kỷ niệm ngày cưới, chuẩn bị nến thơm",
            CreatedAt = DateTime.Now.AddDays(-1)
        },
        new Reservation
        {
            ReservationId = 2,
            DiningTableId = 4, // Bàn 04
            TableNumber = "Bàn 04",
            TableCapacity = 6,
            CustomerName = "Trần Thị Mai",
            PhoneNumber = "0914987654",
            ReservationTime = DateTime.Today.AddHours(19).AddMinutes(30), // 19:30 hôm nay
            GuestCount = 5,
            Status = ReservationStatus.Confirmed,
            Note = "Gia đình có trẻ em",
            CreatedAt = DateTime.Now.AddHours(-3)
        }
    };

    public Task<IEnumerable<Reservation>> GetAllReservationsAsync()
    {
        return Task.FromResult<IEnumerable<Reservation>>(_reservations.OrderByDescending(r => r.ReservationTime));
    }

    public Task<IEnumerable<Reservation>> GetReservationsByTableAsync(int tableId)
    {
        var list = _reservations.Where(r => r.DiningTableId == tableId).ToList();
        return Task.FromResult<IEnumerable<Reservation>>(list);
    }

    public Task<Reservation?> GetReservationByIdAsync(int reservationId)
    {
        var res = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
        return Task.FromResult(res);
    }

    public Task<int> AddReservationAsync(Reservation reservation)
    {
        int newId = _reservations.Any() ? _reservations.Max(r => r.ReservationId) + 1 : 1;
        reservation.ReservationId = newId;
        _reservations.Add(reservation);
        return Task.FromResult(newId);
    }

    public Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status)
    {
        var res = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
        if (res != null)
        {
            res.Status = status;
        }
        return Task.CompletedTask;
    }
}
