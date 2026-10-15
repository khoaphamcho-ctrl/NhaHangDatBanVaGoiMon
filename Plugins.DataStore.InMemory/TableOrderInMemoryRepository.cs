using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class TableOrderInMemoryRepository : ITableOrderRepository
{
    private static readonly List<TableOrder> _orders = new();
    private static int _nextOrderId = 1;
    private static int _nextOrderLineId = 1;

    private readonly IDiningTableRepository _tableRepo;

    public TableOrderInMemoryRepository(IDiningTableRepository tableRepo)
    {
        _tableRepo = tableRepo;
    }

    public Task<TableOrder?> GetActiveOrderByTableIdAsync(int tableId)
    {
        var order = _orders.FirstOrDefault(o => o.DiningTableId == tableId && o.Status == OrderStatus.Open);
        return Task.FromResult(order);
    }

    public Task<TableOrder?> GetOrderByIdAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.TableOrderId == orderId);
        return Task.FromResult(order);
    }

    public Task<IEnumerable<TableOrder>> GetAllOrdersAsync()
    {
        return Task.FromResult<IEnumerable<TableOrder>>(_orders.OrderByDescending(o => o.OrderTime));
    }

    public Task<int> CreateOrUpdateOrderWithLinesAsync(TableOrder order)
    {
        if (order.TableOrderId <= 0)
        {
            order.TableOrderId = _nextOrderId++;
            foreach (var line in order.Lines)
            {
                if (line.TableOrderLineId <= 0)
                {
                    line.TableOrderLineId = _nextOrderLineId++;
                    line.TableOrderId = order.TableOrderId;
                }
            }
            _orders.Add(order);
        }
        else
        {
            var existing = _orders.FirstOrDefault(o => o.TableOrderId == order.TableOrderId);
            if (existing != null)
            {
                existing.SubTotal = order.SubTotal;
                existing.ServiceFeeRate = order.ServiceFeeRate;
                existing.ServiceFeeAmount = order.ServiceFeeAmount;
                existing.TotalAmount = order.TotalAmount;
                existing.Note = order.Note;

                foreach (var line in order.Lines)
                {
                    var existingLine = existing.Lines.FirstOrDefault(l => l.MenuItemId == line.MenuItemId);
                    if (existingLine != null)
                    {
                        existingLine.Quantity = line.Quantity;
                        existingLine.LineTotal = line.LineTotal;
                        existingLine.SpecialInstructions = line.SpecialInstructions;
                    }
                    else
                    {
                        if (line.TableOrderLineId <= 0)
                        {
                            line.TableOrderLineId = _nextOrderLineId++;
                            line.TableOrderId = existing.TableOrderId;
                        }
                        existing.Lines.Add(line);
                    }
                }
            }
        }

        return Task.FromResult(order.TableOrderId);
    }

    public Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = _orders.FirstOrDefault(o => o.TableOrderId == orderId);
        if (order != null)
        {
            order.Status = status;
        }
        return Task.CompletedTask;
    }

    public Task UpdateOrderLineStatusAsync(int orderLineId, OrderLineStatus status)
    {
        foreach (var order in _orders)
        {
            var line = order.Lines.FirstOrDefault(l => l.TableOrderLineId == orderLineId);
            if (line != null)
            {
                line.Status = status;
                break;
            }
        }
        return Task.CompletedTask;
    }

    public async Task CloseOrderAndTableAsync(int orderId, int tableId, decimal totalAmount)
    {
        var order = _orders.FirstOrDefault(o => o.TableOrderId == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Paid;
            order.TotalAmount = totalAmount;
            order.ClosedAt = DateTime.Now;

            foreach (var line in order.Lines)
            {
                line.Status = OrderLineStatus.Served;
            }
        }

        // Đưa bàn về Available sau khi khách thanh toán
        await _tableRepo.UpdateTableStatusAsync(tableId, TableStatus.Available);
    }
}
