using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Enums;
using CoreBusiness.Exceptions;
using CoreBusiness.Models;
using CoreBusiness.Rules;
using UseCases.Cart;
using UseCases.DataStorePluginInterfaces;

namespace UseCases.Orders;

public interface IViewTableOrderUseCase
{
    Task<TableOrder?> GetActiveOrderByTableIdAsync(int tableId);
    Task<TableOrder?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<TableOrder>> GetAllOrdersAsync();
}

public class ViewTableOrderUseCase : IViewTableOrderUseCase
{
    private readonly ITableOrderRepository _orderRepo;

    public ViewTableOrderUseCase(ITableOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<TableOrder?> GetActiveOrderByTableIdAsync(int tableId)
    {
        return await _orderRepo.GetActiveOrderByTableIdAsync(tableId);
    }

    public async Task<TableOrder?> GetOrderByIdAsync(int orderId)
    {
        return await _orderRepo.GetOrderByIdAsync(orderId);
    }

    public async Task<IEnumerable<TableOrder>> GetAllOrdersAsync()
    {
        return await _orderRepo.GetAllOrdersAsync();
    }
}

public interface ISubmitTableOrderUseCase
{
    Task<int> ExecuteAsync(int tableId, string note = "");
}

/// <summary>
/// TIÊU CHÍ K1.3: CÀI ĐẶT LUẬT 2 - RÀNG BUỘC GỌI MÓN VÀ TÍNH HÓA ĐƠN
/// Luật 2.1: Chỉ gọi món cho bàn đang mở
/// Luật 2.2: Không gọi món đã hết hàng
/// Luật 2.3: Tính hóa đơn = Tổng món + Phí phục vụ (%)
/// Luật 2.4: Bàn đã đóng / thanh toán (Paid) thì cấm sửa / gọi thêm
/// Ghi toàn bộ Header-Line trong 1 Transaction (K2.2)
/// </summary>
public class SubmitTableOrderUseCase : ISubmitTableOrderUseCase
{
    private readonly ITableOrderRepository _orderRepo;
    private readonly IDiningTableRepository _tableRepo;
    private readonly IMenuItemRepository _menuItemRepo;
    private readonly IRestaurantConfigRepository _configRepo;
    private readonly ITableCartService _cartService;

    public SubmitTableOrderUseCase(
        ITableOrderRepository orderRepo,
        IDiningTableRepository tableRepo,
        IMenuItemRepository menuItemRepo,
        IRestaurantConfigRepository configRepo,
        ITableCartService cartService)
    {
        _orderRepo = orderRepo;
        _tableRepo = tableRepo;
        _menuItemRepo = menuItemRepo;
        _configRepo = configRepo;
        _cartService = cartService;
    }

    public async Task<int> ExecuteAsync(int tableId, string note = "")
    {
        // 1. Kiểm tra bàn tồn tại
        var table = await _tableRepo.GetTableByIdAsync(tableId);
        if (table == null)
        {
            throw new DomainException($"Không tìm thấy bàn ID = {tableId}.");
        }

        // Lấy đơn hàng hiện tại của bàn (nếu đã có đơn đang mở)
        var existingOrder = await _orderRepo.GetActiveOrderByTableIdAsync(tableId);

        // 2. LUẬT 2.1 & 2.4: Kiểm tra trạng thái bàn và không cho gọi thêm nếu đã thanh toán
        TableOrderBusinessRules.ValidateCanOrderForTable(table, existingOrder);

        // Lấy các món trong giỏ tạm Scoped của bàn
        var cartItems = _cartService.GetCartItems(tableId);
        if (cartItems.Count == 0 && existingOrder == null)
        {
            throw new DomainException("Giỏ món của bạn đang trống. Vui lòng chọn ít nhất 1 món ăn.");
        }

        // 3. LUẬT 2.2: Kiểm tra từng món trong giỏ xem còn phục vụ không
        foreach (var cartItem in cartItems)
        {
            var dbItem = await _menuItemRepo.GetMenuItemByIdAsync(cartItem.Item.MenuItemId);
            if (dbItem == null)
            {
                throw new DomainException($"Món '{cartItem.Item.Name}' không tồn tại trong hệ thống.");
            }
            TableOrderBusinessRules.ValidateMenuItemAvailable(dbItem);
        }

        // Chuẩn bị đơn hàng (Nếu chưa có thì tạo mới, có rồi thì gộp dòng món mới)
        TableOrder order = existingOrder ?? new TableOrder
        {
            DiningTableId = tableId,
            TableNumber = table.TableNumber,
            OrderTime = DateTime.Now,
            Status = OrderStatus.Open,
            Note = note
        };

        // Đảm bảo không sửa đơn đã đóng
        TableOrderBusinessRules.ValidateOrderCanModify(order);

        // Gộp các dòng món từ giỏ tạm vào đơn hàng
        foreach (var cartItem in cartItems)
        {
            var existingLine = order.Lines.FirstOrDefault(l => l.MenuItemId == cartItem.Item.MenuItemId);
            if (existingLine != null)
            {
                existingLine.Quantity += cartItem.Quantity;
                existingLine.LineTotal = existingLine.UnitPrice * existingLine.Quantity;
                if (!string.IsNullOrWhiteSpace(cartItem.SpecialInstructions))
                {
                    existingLine.SpecialInstructions += $"; {cartItem.SpecialInstructions}";
                }
            }
            else
            {
                order.Lines.Add(new TableOrderLine
                {
                    MenuItemId = cartItem.Item.MenuItemId,
                    MenuItemName = cartItem.Item.Name,
                    UnitPrice = cartItem.Item.Price,
                    Quantity = cartItem.Quantity,
                    LineTotal = cartItem.Item.Price * cartItem.Quantity,
                    SpecialInstructions = cartItem.SpecialInstructions,
                    Status = OrderLineStatus.SentToKitchen
                });
            }
        }

        // 4. LUẬT 2.3: Tính hóa đơn = Tổng dòng + Phí phục vụ (%)
        var serviceFeeRate = await _configRepo.GetServiceFeeRateAsync();
        TableOrderBusinessRules.CalculateOrderTotal(order, serviceFeeRate);

        // 5. TIÊU CHÍ K2.2: Ghi cặp Header-Line trong 1 Transaction
        var savedOrderId = await _orderRepo.CreateOrUpdateOrderWithLinesAsync(order);

        // Cập nhật trạng thái bàn sang Occupied (Đang phục vụ)
        if (table.Status != TableStatus.Occupied)
        {
            await _tableRepo.UpdateTableStatusAsync(tableId, TableStatus.Occupied);
        }

        // Xóa sạch giỏ tạm sau khi đã gửi gọi món thành công
        _cartService.ClearCart(tableId);

        return savedOrderId;
    }
}

public interface ICloseTableOrderUseCase
{
    Task ExecuteAsync(int orderId, string paymentMethod = "Tiền mặt", decimal? finalTotal = null);
}

/// <summary>
/// Thu ngân hoặc Khách hàng thanh toán, tính tiền hóa đơn và đóng bàn (Luật 2)
/// </summary>
public class CloseTableOrderUseCase : ICloseTableOrderUseCase
{
    private readonly ITableOrderRepository _orderRepo;
    private readonly IDiningTableRepository _tableRepo;
    private readonly IRestaurantConfigRepository _configRepo;

    public CloseTableOrderUseCase(
        ITableOrderRepository orderRepo,
        IDiningTableRepository tableRepo,
        IRestaurantConfigRepository configRepo)
    {
        _orderRepo = orderRepo;
        _tableRepo = tableRepo;
        _configRepo = configRepo;
    }

    public async Task ExecuteAsync(int orderId, string paymentMethod = "Tiền mặt", decimal? finalTotal = null)
    {
        var order = await _orderRepo.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            throw new DomainException($"Không tìm thấy đơn hàng ID = {orderId}.");
        }

        if (order.Status == OrderStatus.Paid)
        {
            throw new OrderAlreadyPaidException(order.TableNumber);
        }

        // Tính lại hóa đơn cuối cùng
        var serviceFeeRate = await _configRepo.GetServiceFeeRateAsync();
        TableOrderBusinessRules.CalculateOrderTotal(order, serviceFeeRate);

        if (finalTotal.HasValue && finalTotal.Value > 0)
        {
            order.TotalAmount = finalTotal.Value;
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            order.Note = string.IsNullOrWhiteSpace(order.Note) 
                ? $"Thanh toán: {paymentMethod}" 
                : $"{order.Note} | Thanh toán: {paymentMethod}";
        }

        // Đóng đơn hàng và giải phóng bàn về trạng thái Available
        await _orderRepo.CloseOrderAndTableAsync(orderId, order.DiningTableId, order.TotalAmount);
    }
}

public interface IUpdateOrderLineStatusUseCase
{
    Task ExecuteAsync(int orderLineId, OrderLineStatus newStatus);
}

public class UpdateOrderLineStatusUseCase : IUpdateOrderLineStatusUseCase
{
    private readonly ITableOrderRepository _orderRepo;

    public UpdateOrderLineStatusUseCase(ITableOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task ExecuteAsync(int orderLineId, OrderLineStatus newStatus)
    {
        await _orderRepo.UpdateOrderLineStatusAsync(orderLineId, newStatus);
    }
}
