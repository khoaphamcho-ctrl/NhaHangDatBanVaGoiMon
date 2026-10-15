using System;
using System.Collections.Generic;
using System.Linq;
using CoreBusiness.Models;
using CoreBusiness.Rules;

namespace UseCases.Cart;

public class TableCartService : ITableCartService
{
    // Lưu các mục chọn món theo từng bàn (tableId -> list cart items)
    private readonly Dictionary<int, List<CartItem>> _tableCarts = new();

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public IReadOnlyList<CartItem> GetCartItems(int tableId)
    {
        if (!_tableCarts.ContainsKey(tableId))
        {
            _tableCarts[tableId] = new List<CartItem>();
        }
        return _tableCarts[tableId].AsReadOnly();
    }

    public void AddItem(int tableId, MenuItem item, int quantity = 1, string instructions = "")
    {
        // Kiểm tra món còn khả dụng không
        TableOrderBusinessRules.ValidateMenuItemAvailable(item);

        if (!_tableCarts.ContainsKey(tableId))
        {
            _tableCarts[tableId] = new List<CartItem>();
        }

        var cart = _tableCarts[tableId];
        var existing = cart.FirstOrDefault(x => x.Item.MenuItemId == item.MenuItemId);
        if (existing != null)
        {
            existing.Quantity += quantity;
            if (!string.IsNullOrWhiteSpace(instructions))
            {
                existing.SpecialInstructions = instructions;
            }
        }
        else
        {
            cart.Add(new CartItem
            {
                Item = item,
                Quantity = quantity,
                SpecialInstructions = instructions
            });
        }

        NotifyStateChanged();
    }

    public void UpdateQuantity(int tableId, int menuItemId, int quantity)
    {
        if (!_tableCarts.ContainsKey(tableId)) return;

        var cart = _tableCarts[tableId];
        var item = cart.FirstOrDefault(x => x.Item.MenuItemId == menuItemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            NotifyStateChanged();
        }
    }

    public void RemoveItem(int tableId, int menuItemId)
    {
        if (!_tableCarts.ContainsKey(tableId)) return;

        var cart = _tableCarts[tableId];
        var item = cart.FirstOrDefault(x => x.Item.MenuItemId == menuItemId);
        if (item != null)
        {
            cart.Remove(item);
            NotifyStateChanged();
        }
    }

    public void ClearCart(int tableId)
    {
        if (_tableCarts.ContainsKey(tableId))
        {
            _tableCarts[tableId].Clear();
            NotifyStateChanged();
        }
    }

    public int GetTotalItemCount(int tableId)
    {
        if (!_tableCarts.ContainsKey(tableId)) return 0;
        return _tableCarts[tableId].Sum(x => x.Quantity);
    }

    public decimal GetSubTotal(int tableId)
    {
        if (!_tableCarts.ContainsKey(tableId)) return 0;
        return _tableCarts[tableId].Sum(x => x.LineTotal);
    }
}
