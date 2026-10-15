using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class DiningTableInMemoryRepository : IDiningTableRepository
{
    private static readonly List<DiningTable> _tables = new()
    {
        new DiningTable { TableId = 1, TableNumber = "Bàn 01", Capacity = 2, Status = TableStatus.Available, Location = "Tầng 1 (Cửa sổ)", Description = "Bàn đôi lãng mạn hướng phố" },
        new DiningTable { TableId = 2, TableNumber = "Bàn 02", Capacity = 4, Status = TableStatus.Available, Location = "Tầng 1 (Sảnh chính)", Description = "Bàn 4 người tiện lợi gần quầy" },
        new DiningTable { TableId = 3, TableNumber = "Bàn 03", Capacity = 4, Status = TableStatus.Available, Location = "Tầng 1 (Sảnh chính)", Description = "Bàn 4 người ghế sofa êm ái" },
        new DiningTable { TableId = 4, TableNumber = "Bàn 04", Capacity = 6, Status = TableStatus.Available, Location = "Tầng 2 (Ban công)", Description = "Bàn 6 người ngắm view sông Hương" },
        new DiningTable { TableId = 5, TableNumber = "Bàn 05", Capacity = 6, Status = TableStatus.Available, Location = "Tầng 2 (Trong nhà)", Description = "Bàn gia đình ấm cúng" },
        new DiningTable { TableId = 6, TableNumber = "Bàn 06", Capacity = 8, Status = TableStatus.Available, Location = "Tầng 2 (Sân thượng)", Description = "Bàn tiệc nhóm lớn thoáng mát" },
        new DiningTable { TableId = 7, TableNumber = "VIP 01", Capacity = 10, Status = TableStatus.Available, Location = "Phòng VIP Hoàng Gia", Description = "Phòng riêng máy lạnh, âm thanh riêng" },
        new DiningTable { TableId = 8, TableNumber = "Bàn 08", Capacity = 4, Status = TableStatus.Available, Location = "Sân vườn", Description = "Bàn ngoài trời nhiều cây xanh" },
        new DiningTable { TableId = 9, TableNumber = "Bàn 09", Capacity = 2, Status = TableStatus.Available, Location = "Sân vườn", Description = "Bàn đôi yên tĩnh góc vườn" },
        new DiningTable { TableId = 10, TableNumber = "VIP 02", Capacity = 12, Status = TableStatus.Available, Location = "Phòng VIP Cung Đình", Description = "Phòng tiệc lớn tiếp khách quý" },
    };

    public Task<IEnumerable<DiningTable>> GetAllTablesAsync()
    {
        return Task.FromResult<IEnumerable<DiningTable>>(_tables.OrderBy(t => t.TableId));
    }

    public Task<DiningTable?> GetTableByIdAsync(int tableId)
    {
        var table = _tables.FirstOrDefault(t => t.TableId == tableId);
        return Task.FromResult(table);
    }

    public Task UpdateTableStatusAsync(int tableId, TableStatus status)
    {
        var table = _tables.FirstOrDefault(t => t.TableId == tableId);
        if (table != null)
        {
            table.Status = status;
        }
        return Task.CompletedTask;
    }

    public Task<int> AddTableAsync(DiningTable table)
    {
        int newId = _tables.Any() ? _tables.Max(t => t.TableId) + 1 : 1;
        table.TableId = newId;
        _tables.Add(table);
        return Task.FromResult(newId);
    }

    public Task UpdateTableAsync(DiningTable table)
    {
        var existing = _tables.FirstOrDefault(t => t.TableId == table.TableId);
        if (existing != null)
        {
            existing.TableNumber = table.TableNumber;
            existing.Capacity = table.Capacity;
            existing.Location = table.Location;
            existing.Status = table.Status;
            existing.Description = table.Description;
        }
        return Task.CompletedTask;
    }

    public Task DeleteTableAsync(int tableId)
    {
        var table = _tables.FirstOrDefault(t => t.TableId == tableId);
        if (table != null)
        {
            _tables.Remove(table);
        }
        return Task.CompletedTask;
    }
}
