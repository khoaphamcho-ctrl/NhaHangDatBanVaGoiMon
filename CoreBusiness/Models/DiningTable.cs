using CoreBusiness.Enums;

namespace CoreBusiness.Models;

public class DiningTable
{
    public int TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4; // Sức chứa (số ghế)
    public TableStatus Status { get; set; } = TableStatus.Available;
    public string Location { get; set; } = "Tầng 1"; // Tầng 1, Tầng 2, Sân vườn, VIP
    public string Description { get; set; } = string.Empty;
}
