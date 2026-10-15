using System;
using System.Collections.Generic;
using CoreBusiness.Models;

namespace UseCases.Cart;

public class CartItem
{
    public MenuItem Item { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public string SpecialInstructions { get; set; } = string.Empty;
    public decimal LineTotal => Item.Price * Quantity;
}

/// <summary>
/// TIÊU CHÍ K1.4: State management giỏ món tại bàn dùng Scoped service
/// Tương tự IShoppingCart trong Video Part 3
/// </summary>
public interface ITableCartService
{
    event Action? OnChange;
    IReadOnlyList<CartItem> GetCartItems(int tableId);
    void AddItem(int tableId, MenuItem item, int quantity = 1, string instructions = "");
    void UpdateQuantity(int tableId, int menuItemId, int quantity);
    void RemoveItem(int tableId, int menuItemId);
    void ClearCart(int tableId);
    int GetTotalItemCount(int tableId);
    decimal GetSubTotal(int tableId);
}
