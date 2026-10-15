using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Models;

namespace UseCases.DataStorePluginInterfaces;

public interface IDiningTableRepository
{
    Task<IEnumerable<DiningTable>> GetAllTablesAsync();
    Task<DiningTable?> GetTableByIdAsync(int tableId);
    Task UpdateTableStatusAsync(int tableId, TableStatus status);
    Task<int> AddTableAsync(DiningTable table);
    Task UpdateTableAsync(DiningTable table);
    Task DeleteTableAsync(int tableId);
}

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetAllReservationsAsync();
    Task<IEnumerable<Reservation>> GetReservationsByTableAsync(int tableId);
    Task<Reservation?> GetReservationByIdAsync(int reservationId);
    Task<int> AddReservationAsync(Reservation reservation);
    Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status);
}

public interface IMenuCategoryRepository
{
    Task<IEnumerable<MenuCategory>> GetAllCategoriesAsync();
    Task<MenuCategory?> GetCategoryByIdAsync(int categoryId);
    Task<int> AddCategoryAsync(MenuCategory category);
    Task UpdateCategoryAsync(MenuCategory category);
    Task DeleteCategoryAsync(int categoryId);
}

public interface IMenuItemRepository
{
    Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync(int? categoryId = null, string? search = null);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);
    Task<int> AddMenuItemAsync(MenuItem item);
    Task UpdateMenuItemAsync(MenuItem item);
    Task ToggleAvailabilityAsync(int menuItemId, bool isAvailable);
    Task DeleteMenuItemAsync(int menuItemId);
}

public interface ITableOrderRepository
{
    Task<TableOrder?> GetActiveOrderByTableIdAsync(int tableId);
    Task<TableOrder?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<TableOrder>> GetAllOrdersAsync();
    /// <summary>
    /// Ghi cặp Header-Line trong 1 Transaction duy nhất theo tiêu chí K2.2
    /// </summary>
    Task<int> CreateOrUpdateOrderWithLinesAsync(TableOrder order);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdateOrderLineStatusAsync(int orderLineId, OrderLineStatus status);
    Task CloseOrderAndTableAsync(int orderId, int tableId, decimal totalAmount);
}

public interface IRestaurantConfigRepository
{
    Task<string> GetConfigValueAsync(string key, string defaultValue);
    Task SetConfigValueAsync(string key, string value);
    Task<decimal> GetServiceFeeRateAsync();
}

public interface IUserRepository
{
    Task<UserAccount?> AuthenticateAsync(string username, string password);
    Task<UserAccount?> GetUserByUsernameAsync(string username);
}
