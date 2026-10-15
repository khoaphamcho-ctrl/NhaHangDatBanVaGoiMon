using System;

namespace CoreBusiness.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// Ca vi phạm Luật 1: Số khách đặt vượt quá sức chứa của bàn
/// </summary>
public class CapacityExceededException : DomainException
{
    public CapacityExceededException(int guestCount, int capacity, string tableNumber)
        : base($"Bàn {tableNumber} chỉ có sức chứa tối đa {capacity} khách (bạn đang chọn {guestCount} người). Vui lòng chọn bàn lớn hơn.") { }
}

/// <summary>
/// Ca vi phạm Luật 1: Bàn đã có khách đặt trước trong khung 2 giờ
/// </summary>
public class ReservationOverlapException : DomainException
{
    public ReservationOverlapException(string tableNumber, DateTime requestedTime, DateTime existingTime)
        : base($"Bàn {tableNumber} đã có khách đặt trước lúc {existingTime:HH:mm dd/MM/yyyy}. Khung giờ bạn chọn ({requestedTime:HH:mm}) vi phạm quy định cách nhau tối thiểu 2 giờ.") { }
}

/// <summary>
/// Ca vi phạm Luật 2: Bàn chưa mở hoặc không ở trạng thái phục vụ
/// </summary>
public class TableNotOpenException : DomainException
{
    public TableNotOpenException(string tableNumber)
        : base($"Bàn {tableNumber} hiện không mở phục vụ gọi món trực tiếp. Vui lòng liên hệ nhân viên phục vụ.") { }
}

/// <summary>
/// Ca vi phạm Luật 2: Bàn đã thanh toán / đóng bàn, cấm gọi thêm hoặc chỉnh sửa
/// </summary>
public class OrderAlreadyPaidException : DomainException
{
    public OrderAlreadyPaidException(string tableNumber)
        : base($"Bàn {tableNumber} đã hoàn tất thanh toán và đóng bàn. Không thể gọi thêm hoặc sửa món.") { }
}

/// <summary>
/// Ca vi phạm Luật 2: Món ăn đã hết hàng
/// </summary>
public class ItemUnavailableException : DomainException
{
    public ItemUnavailableException(string itemName)
        : base($"Món '{itemName}' hiện đã tạm hết hàng trong bếp. Vui lòng chọn món khác.") { }
}
