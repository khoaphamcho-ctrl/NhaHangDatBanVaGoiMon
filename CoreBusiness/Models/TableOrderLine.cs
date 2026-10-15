using CoreBusiness.Enums;

namespace CoreBusiness.Models;

/// <summary>
/// Line entity trong cặp Header-Line theo yêu cầu đề tài
/// </summary>
public class TableOrderLine
{
    public int TableOrderLineId { get; set; }
    public int TableOrderId { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal LineTotal { get; set; }
    public string SpecialInstructions { get; set; } = string.Empty; // Ghi chú: ít đường, không cay, v.v.
    public OrderLineStatus Status { get; set; } = OrderLineStatus.SentToKitchen;
}
