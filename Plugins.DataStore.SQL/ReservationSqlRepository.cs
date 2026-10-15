using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using Dapper;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.SQL;

public class ReservationSqlRepository : IReservationRepository
{
    private readonly SqlDataAccess _db;

    public ReservationSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT r.*, t.TableNumber, t.Capacity AS TableCapacity
            FROM dbo.Reservations r
            INNER JOIN dbo.DiningTables t ON r.DiningTableId = t.TableId
            ORDER BY r.ReservationTime DESC";

        return await conn.QueryAsync<Reservation>(sql);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByTableAsync(int tableId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT r.*, t.TableNumber, t.Capacity AS TableCapacity
            FROM dbo.Reservations r
            INNER JOIN dbo.DiningTables t ON r.DiningTableId = t.TableId
            WHERE r.DiningTableId = @TableId";

        return await conn.QueryAsync<Reservation>(sql, new { TableId = tableId });
    }

    public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT r.*, t.TableNumber, t.Capacity AS TableCapacity
            FROM dbo.Reservations r
            INNER JOIN dbo.DiningTables t ON r.DiningTableId = t.TableId
            WHERE r.ReservationId = @ReservationId";

        return await conn.QueryFirstOrDefaultAsync<Reservation>(sql, new { ReservationId = reservationId });
    }

    public async Task<int> AddReservationAsync(Reservation reservation)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Reservations (DiningTableId, CustomerName, PhoneNumber, ReservationTime, GuestCount, Status, Note, CreatedAt)
            VALUES (@DiningTableId, @CustomerName, @PhoneNumber, @ReservationTime, @GuestCount, @Status, @Note, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            reservation.DiningTableId,
            reservation.CustomerName,
            reservation.PhoneNumber,
            reservation.ReservationTime,
            reservation.GuestCount,
            Status = (int)reservation.Status,
            reservation.Note,
            reservation.CreatedAt
        });
    }

    public async Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE dbo.Reservations SET Status = @Status WHERE ReservationId = @ReservationId";
        await conn.ExecuteAsync(sql, new { Status = (int)status, ReservationId = reservationId });
    }
}
