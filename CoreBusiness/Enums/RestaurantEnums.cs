namespace CoreBusiness.Enums;

public enum TableStatus
{
    Available = 0, // Bàn trống (sẵn sàng đón khách)
    Occupied = 1,  // Đang có khách ngồi / đang phục vụ
    Reserved = 2   // Đã có khách đặt trước
}

public enum ReservationStatus
{
    Confirmed = 0, // Đã xác nhận đặt bàn
    Completed = 1, // Khách đã đến và mở bàn
    Cancelled = 2  // Đã hủy lịch đặt
}

public enum OrderStatus
{
    Open = 0,      // Bàn đang mở gọi món
    Paid = 1,      // Thu ngân đã thanh toán và đóng bàn
    Cancelled = 2  // Đơn bị hủy
}

public enum OrderLineStatus
{
    SentToKitchen = 0, // Mới gửi yêu cầu vào bếp
    Cooking = 1,       // Bếp đang chế biến
    Served = 2         // Đã phục vụ lên bàn cho khách
}
