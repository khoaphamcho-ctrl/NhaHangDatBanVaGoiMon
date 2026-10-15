using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using Dapper;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.SQL;

/// <summary>
/// TIÊU CHÍ K2.2 & K2.3: CÀI ĐẶT GIAO DỊCH 1 TRANSACTION VÀ TRUY VẤN THAM SỐ HÓA
/// Cặp Header-Line (TableOrder - TableOrderLine) được ghi trong 1 transaction duy nhất.
/// </summary>
public class TableOrderSqlRepository : ITableOrderRepository
{
    private readonly SqlDataAccess _db;

    public TableOrderSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<TableOrder?> GetActiveOrderByTableIdAsync(int tableId)
    {
        using var conn = _db.CreateConnection();
        const string sqlOrder = @"
            SELECT o.TableOrderId, o.DiningTableId, o.OrderTime, o.SubTotal, o.ServiceFeeRate, 
                   o.ServiceFeeAmount, o.TotalAmount, o.Status, o.Note, o.ClosedAt,
                   t.TableNumber
            FROM dbo.TableOrders o
            INNER JOIN dbo.DiningTables t ON o.DiningTableId = t.TableId
            WHERE o.DiningTableId = @TableId AND o.Status = @OpenStatus";

        var order = await conn.QueryFirstOrDefaultAsync<TableOrder>(sqlOrder, new 
        { 
            TableId = tableId, 
            OpenStatus = (int)OrderStatus.Open 
        });

        if (order == null) return null;

        const string sqlLines = @"
            SELECT l.TableOrderLineId, l.TableOrderId, l.MenuItemId, l.UnitPrice, 
                   l.Quantity, l.LineTotal, l.SpecialInstructions, l.Status,
                   m.Name AS MenuItemName
            FROM dbo.TableOrderLines l
            INNER JOIN dbo.MenuItems m ON l.MenuItemId = m.MenuItemId
            WHERE l.TableOrderId = @OrderId";

        var lines = await conn.QueryAsync<TableOrderLine>(sqlLines, new { OrderId = order.TableOrderId });
        order.Lines = lines.ToList();

        return order;
    }

    public async Task<TableOrder?> GetOrderByIdAsync(int orderId)
    {
        using var conn = _db.CreateConnection();
        const string sqlOrder = @"
            SELECT o.TableOrderId, o.DiningTableId, o.OrderTime, o.SubTotal, o.ServiceFeeRate, 
                   o.ServiceFeeAmount, o.TotalAmount, o.Status, o.Note, o.ClosedAt,
                   t.TableNumber
            FROM dbo.TableOrders o
            INNER JOIN dbo.DiningTables t ON o.DiningTableId = t.TableId
            WHERE o.TableOrderId = @OrderId";

        var order = await conn.QueryFirstOrDefaultAsync<TableOrder>(sqlOrder, new { OrderId = orderId });
        if (order == null) return null;

        const string sqlLines = @"
            SELECT l.TableOrderLineId, l.TableOrderId, l.MenuItemId, l.UnitPrice, 
                   l.Quantity, l.LineTotal, l.SpecialInstructions, l.Status,
                   m.Name AS MenuItemName
            FROM dbo.TableOrderLines l
            INNER JOIN dbo.MenuItems m ON l.MenuItemId = m.MenuItemId
            WHERE l.TableOrderId = @OrderId";

        var lines = await conn.QueryAsync<TableOrderLine>(sqlLines, new { OrderId = order.TableOrderId });
        order.Lines = lines.ToList();

        return order;
    }

    public async Task<IEnumerable<TableOrder>> GetAllOrdersAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT o.TableOrderId, o.DiningTableId, o.OrderTime, o.SubTotal, o.ServiceFeeRate, 
                   o.ServiceFeeAmount, o.TotalAmount, o.Status, o.Note, o.ClosedAt,
                   t.TableNumber
            FROM dbo.TableOrders o
            INNER JOIN dbo.DiningTables t ON o.DiningTableId = t.TableId
            ORDER BY o.OrderTime DESC";

        var orders = await conn.QueryAsync<TableOrder>(sql);
        return orders;
    }

    /// <summary>
    /// TIÊU CHÍ K2.2: GHI CẶP HEADER-LINE TRONG 1 TRANSACTION
    /// Bắt đầu transaction, ghi Header (TableOrders), lấy ID rồi ghi từng Line (TableOrderLines)
    /// </summary>
    public async Task<int> CreateOrUpdateOrderWithLinesAsync(TableOrder order)
    {
        using var conn = _db.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var transaction = conn.BeginTransaction();
        try
        {
            if (order.TableOrderId <= 0)
            {
                // 1. Thêm mới Header TableOrders
                const string insertOrderSql = @"
                    INSERT INTO dbo.TableOrders (DiningTableId, OrderTime, SubTotal, ServiceFeeRate, ServiceFeeAmount, TotalAmount, Status, Note)
                    VALUES (@DiningTableId, @OrderTime, @SubTotal, @ServiceFeeRate, @ServiceFeeAmount, @TotalAmount, @Status, @Note);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                order.TableOrderId = await conn.ExecuteScalarAsync<int>(insertOrderSql, new
                {
                    order.DiningTableId,
                    order.OrderTime,
                    order.SubTotal,
                    order.ServiceFeeRate,
                    order.ServiceFeeAmount,
                    order.TotalAmount,
                    Status = (int)order.Status,
                    order.Note
                }, transaction);

                // 2. Thêm mới các Lines TableOrderLines trong cùng Transaction
                const string insertLineSql = @"
                    INSERT INTO dbo.TableOrderLines (TableOrderId, MenuItemId, UnitPrice, Quantity, LineTotal, SpecialInstructions, Status)
                    VALUES (@TableOrderId, @MenuItemId, @UnitPrice, @Quantity, @LineTotal, @SpecialInstructions, @Status);";

                foreach (var line in order.Lines)
                {
                    line.TableOrderId = order.TableOrderId;
                    await conn.ExecuteAsync(insertLineSql, new
                    {
                        line.TableOrderId,
                        line.MenuItemId,
                        line.UnitPrice,
                        line.Quantity,
                        line.LineTotal,
                        line.SpecialInstructions,
                        Status = (int)line.Status
                    }, transaction);
                }
            }
            else
            {
                // Cập nhật Header
                const string updateOrderSql = @"
                    UPDATE dbo.TableOrders
                    SET SubTotal = @SubTotal,
                        ServiceFeeRate = @ServiceFeeRate,
                        ServiceFeeAmount = @ServiceFeeAmount,
                        TotalAmount = @TotalAmount,
                        Note = @Note
                    WHERE TableOrderId = @TableOrderId;";

                await conn.ExecuteAsync(updateOrderSql, new
                {
                    order.SubTotal,
                    order.ServiceFeeRate,
                    order.ServiceFeeAmount,
                    order.TotalAmount,
                    order.Note,
                    order.TableOrderId
                }, transaction);

                // Xóa và cập nhật lại lines trong transaction
                const string deleteLinesSql = "DELETE FROM dbo.TableOrderLines WHERE TableOrderId = @TableOrderId;";
                await conn.ExecuteAsync(deleteLinesSql, new { order.TableOrderId }, transaction);

                const string insertLineSql = @"
                    INSERT INTO dbo.TableOrderLines (TableOrderId, MenuItemId, UnitPrice, Quantity, LineTotal, SpecialInstructions, Status)
                    VALUES (@TableOrderId, @MenuItemId, @UnitPrice, @Quantity, @LineTotal, @SpecialInstructions, @Status);";

                foreach (var line in order.Lines)
                {
                    line.TableOrderId = order.TableOrderId;
                    await conn.ExecuteAsync(insertLineSql, new
                    {
                        line.TableOrderId,
                        line.MenuItemId,
                        line.UnitPrice,
                        line.Quantity,
                        line.LineTotal,
                        line.SpecialInstructions,
                        Status = (int)line.Status
                    }, transaction);
                }
            }

            // Commit giao dịch khi thành công
            transaction.Commit();
            return order.TableOrderId;
        }
        catch
        {
            // Rollback nếu có bất kỳ lỗi nào xảy ra để bảo toàn tính toàn vẹn CSDL
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE dbo.TableOrders SET Status = @Status WHERE TableOrderId = @OrderId";
        await conn.ExecuteAsync(sql, new { Status = (int)status, OrderId = orderId });
    }

    public async Task UpdateOrderLineStatusAsync(int orderLineId, OrderLineStatus status)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE dbo.TableOrderLines SET Status = @Status WHERE TableOrderLineId = @LineId";
        await conn.ExecuteAsync(sql, new { Status = (int)status, LineId = orderLineId });
    }

    public async Task CloseOrderAndTableAsync(int orderId, int tableId, decimal totalAmount)
    {
        using var conn = _db.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var transaction = conn.BeginTransaction();
        try
        {
            const string sqlOrder = @"
                UPDATE dbo.TableOrders
                SET Status = @PaidStatus,
                    TotalAmount = @TotalAmount,
                    ClosedAt = @ClosedAt
                WHERE TableOrderId = @OrderId;";

            await conn.ExecuteAsync(sqlOrder, new
            {
                PaidStatus = (int)OrderStatus.Paid,
                TotalAmount = totalAmount,
                ClosedAt = DateTime.Now,
                OrderId = orderId
            }, transaction);

            // Cập nhật trạng thái các món đã phục vụ
            const string sqlLines = @"
                UPDATE dbo.TableOrderLines
                SET Status = @ServedStatus
                WHERE TableOrderId = @OrderId;";

            await conn.ExecuteAsync(sqlLines, new
            {
                ServedStatus = (int)OrderLineStatus.Served,
                OrderId = orderId
            }, transaction);

            // Chuyển bàn về Available (Trống)
            const string sqlTable = @"
                UPDATE dbo.DiningTables
                SET Status = @AvailableStatus
                WHERE TableId = @TableId;";

            await conn.ExecuteAsync(sqlTable, new
            {
                AvailableStatus = (int)TableStatus.Available,
                TableId = tableId
            }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
