using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using Dapper;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.SQL;

public class DiningTableSqlRepository : IDiningTableRepository
{
    private readonly SqlDataAccess _db;

    public DiningTableSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DiningTable>> GetAllTablesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT * FROM dbo.DiningTables ORDER BY TableId";
        return await conn.QueryAsync<DiningTable>(sql);
    }

    public async Task<DiningTable?> GetTableByIdAsync(int tableId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT * FROM dbo.DiningTables WHERE TableId = @TableId";
        return await conn.QueryFirstOrDefaultAsync<DiningTable>(sql, new { TableId = tableId });
    }

    public async Task UpdateTableStatusAsync(int tableId, TableStatus status)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE dbo.DiningTables SET Status = @Status WHERE TableId = @TableId";
        await conn.ExecuteAsync(sql, new { Status = (int)status, TableId = tableId });
    }

    public async Task<int> AddTableAsync(DiningTable table)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.DiningTables (TableNumber, Capacity, Status, Location, Description)
            VALUES (@TableNumber, @Capacity, @Status, @Location, @Description);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            table.TableNumber,
            table.Capacity,
            Status = (int)table.Status,
            table.Location,
            table.Description
        });
    }

    public async Task UpdateTableAsync(DiningTable table)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE dbo.DiningTables
            SET TableNumber = @TableNumber,
                Capacity = @Capacity,
                Status = @Status,
                Location = @Location,
                Description = @Description
            WHERE TableId = @TableId";

        await conn.ExecuteAsync(sql, new
        {
            table.TableNumber,
            table.Capacity,
            Status = (int)table.Status,
            table.Location,
            table.Description,
            table.TableId
        });
    }

    public async Task DeleteTableAsync(int tableId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "DELETE FROM dbo.DiningTables WHERE TableId = @TableId";
        await conn.ExecuteAsync(sql, new { TableId = tableId });
    }
}
