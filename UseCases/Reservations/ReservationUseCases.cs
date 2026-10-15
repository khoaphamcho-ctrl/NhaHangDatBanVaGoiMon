using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Exceptions;
using CoreBusiness.Models;
using CoreBusiness.Rules;
using UseCases.DataStorePluginInterfaces;

namespace UseCases.Reservations;

public interface IViewTablesUseCase
{
    Task<IEnumerable<DiningTable>> ExecuteAsync();
    Task<DiningTable?> ExecuteAsync(int tableId);
}

public class ViewTablesUseCase : IViewTablesUseCase
{
    private readonly IDiningTableRepository _tableRepository;

    public ViewTablesUseCase(IDiningTableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<IEnumerable<DiningTable>> ExecuteAsync()
    {
        return await _tableRepository.GetAllTablesAsync();
    }

    public async Task<DiningTable?> ExecuteAsync(int tableId)
    {
        return await _tableRepository.GetTableByIdAsync(tableId);
    }
}

public interface IPlaceReservationUseCase
{
    Task<int> ExecuteAsync(Reservation reservation);
}

/// <summary>
/// TIÊU CHÍ K1.3: CÀI ĐẶT LUẬT 1 VÀO USECASE
/// Luật 1.1: Số khách <= Sức chứa bàn
/// Luật 1.2: Một bàn không có 2 đặt chỗ trùng nhau trong cùng khung 2 giờ
/// </summary>
public class PlaceReservationUseCase : IPlaceReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IDiningTableRepository _tableRepository;

    public PlaceReservationUseCase(
        IReservationRepository reservationRepository,
        IDiningTableRepository tableRepository)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
    }

    public async Task<int> ExecuteAsync(Reservation reservation)
    {
        // 1. Kiểm tra tồn tại bàn
        var table = await _tableRepository.GetTableByIdAsync(reservation.DiningTableId);
        if (table == null)
        {
            throw new DomainException($"Không tìm thấy bàn có mã ID = {reservation.DiningTableId}.");
        }

        // 2. Kiểm tra LUẬT 1.1: Sức chứa của bàn
        ReservationBusinessRules.ValidateCapacity(reservation.GuestCount, table);

        // 3. Kiểm tra LUẬT 1.2: Không trùng lịch trong khung 2 giờ
        var existingReservations = await _reservationRepository.GetReservationsByTableAsync(reservation.DiningTableId);
        ReservationBusinessRules.ValidateTimeOverlap(
            reservation.ReservationTime, 
            reservation.DiningTableId, 
            table.TableNumber, 
            existingReservations);

        // 4. Nếu hợp lệ, lưu vào CSDL
        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.Now;
        var newId = await _reservationRepository.AddReservationAsync(reservation);

        // Cập nhật trạng thái bàn sang Reserved nếu đặt trong hôm nay
        if (reservation.ReservationTime.Date == DateTime.Today)
        {
            await _tableRepository.UpdateTableStatusAsync(table.TableId, TableStatus.Reserved);
        }

        return newId;
    }
}

public interface ICancelReservationUseCase
{
    Task ExecuteAsync(int reservationId);
}

public class CancelReservationUseCase : ICancelReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public CancelReservationUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task ExecuteAsync(int reservationId)
    {
        await _reservationRepository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Cancelled);
    }
}
