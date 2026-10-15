using System;
using System.Collections.Generic;
using CoreBusiness.Enums;

namespace CoreBusiness.Models;

/// <summary>
/// Header entity trong cặp Header-Line theo yêu cầu đề tài
/// </summary>
public class TableOrder
{
    public int TableOrderId { get; set; }
    public int DiningTableId { get; set; }
    public DateTime OrderTime { get; set; } = DateTime.Now;
    public decimal SubTotal { get; set; }
    public decimal ServiceFeeRate { get; set; } = 5; // Mặc định 5% phí phục vụ
    public decimal ServiceFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public string Note { get; set; } = string.Empty;
    public DateTime? ClosedAt { get; set; }

    // Navigation / Display
    public string TableNumber { get; set; } = string.Empty;
    public List<TableOrderLine> Lines { get; set; } = new();

    public void RecalculateTotals()
    {
        SubTotal = 0;
        foreach (var line in Lines)
        {
            line.LineTotal = line.UnitPrice * line.Quantity;
            SubTotal += line.LineTotal;
        }
        ServiceFeeAmount = Math.Round(SubTotal * (ServiceFeeRate / 100m), 0);
        TotalAmount = SubTotal + ServiceFeeAmount;
    }
}
