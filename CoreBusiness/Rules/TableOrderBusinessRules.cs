using System;
using System.Collections.Generic;
using CoreBusiness.Enums;
using CoreBusiness.Exceptions;
using CoreBusiness.Models;

namespace CoreBusiness.Rules;

/// <summary>
/// TIÊU CHÍ K1.3: Cài đặt LUẬT 2 - RÀNG BUỘC GỌI MÓN VÀ ĐÓNG BÀN TRONG COREBUSINESS
/// Luật 2.1: Chỉ được gọi món cho bàn đang mở (Status == Occupied hoặc có Order Open)
/// Luật 2.2: Không được gọi món đã hết (IsAvailable == false)
/// Luật 2.3: Tính hóa đơn = Tổng dòng món + Phí phục vụ (lấy từ cấu hình)
/// Luật 2.4: Bàn đã đóng / thanh toán (Paid) thì cấm sửa đổi hoặc thêm món mới
/// </summary>
public static class TableOrderBusinessRules
{
    /// <summary>
    /// Kiểm tra tính hợp lệ khi bàn mở gọi món
    /// </summary>
    public static void ValidateCanOrderForTable(DiningTable table, TableOrder? currentOrder)
    {
        if (table == null)
        {
            throw new DomainException("Bàn không tồn tại.");
        }

        if (currentOrder != null && currentOrder.Status == OrderStatus.Paid)
        {
            throw new OrderAlreadyPaidException(table.TableNumber);
        }
    }

    /// <summary>
    /// Kiểm tra món ăn còn phục vụ không
    /// </summary>
    public static void ValidateMenuItemAvailable(MenuItem item)
    {
        if (item == null)
        {
            throw new DomainException("Món ăn không tồn tại trong thực đơn.");
        }

        if (!item.IsAvailable)
        {
            throw new ItemUnavailableException(item.Name);
        }
    }

    /// <summary>
    /// Kiểm tra không được sửa đơn hàng đã đóng / thanh toán
    /// </summary>
    public static void ValidateOrderCanModify(TableOrder order)
    {
        if (order == null)
        {
            throw new DomainException("Đơn gọi món không tồn tại.");
        }

        if (order.Status == OrderStatus.Paid)
        {
            throw new OrderAlreadyPaidException(order.TableNumber);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Đơn hàng này đã bị hủy, không thể chỉnh sửa.");
        }
    }

    /// <summary>
    /// Luật tính tiền hóa đơn = Tổng dòng + Phí phục vụ (%)
    /// </summary>
    public static void CalculateOrderTotal(TableOrder order, decimal serviceFeeRatePercent)
    {
        if (order == null) return;

        order.ServiceFeeRate = serviceFeeRatePercent;
        order.SubTotal = 0;

        foreach (var line in order.Lines)
        {
            line.LineTotal = line.UnitPrice * line.Quantity;
            order.SubTotal += line.LineTotal;
        }

        order.ServiceFeeAmount = Math.Round(order.SubTotal * (order.ServiceFeeRate / 100m), 0);
        order.TotalAmount = order.SubTotal + order.ServiceFeeAmount;
    }
}
