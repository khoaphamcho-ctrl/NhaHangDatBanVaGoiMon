# HỆ THỐNG NHÀ HÀNG CỐ ĐÔ HUẾ - ĐẶT BÀN & GỌI MÓN TRỰC TUYẾN
## ĐỒ ÁN CAPSTONE - HỌC PHẦN ECO2415 (ĐỀ TÀI SỐ 11)

Ứng dụng web xây dựng trên nền tảng **ASP.NET Core (.NET 9)** và **Blazor Web App (Interactive Server)** theo chuẩn kiến trúc **4 tầng Clean Architecture** đã được học qua chuỗi **35 video bài giảng**.

---

## 1. Cấu Trúc Dự Án (Clean Architecture 4 Tầng)

```
DeTai11_NhaHang/
├── CoreBusiness/                      # Tầng 1: Độc lập 100%, chứa Entity & Domain Rules
│   ├── Enums/                         # TableStatus, ReservationStatus, OrderStatus...
│   ├── Exceptions/                    # DomainExceptions, CapacityExceeded, ReservationOverlap...
│   ├── Models/                        # DiningTable, Reservation, MenuItem, TableOrder...
│   └── Rules/                         # 2 Luật nghiệp vụ cốt lõi (Reservation & TableOrder Rules)
│
├── UseCases/                          # Tầng 2: Chỉ tham chiếu CoreBusiness
│   ├── DataStorePluginInterfaces/     # IRepositories: IDiningTableRepo, ITableOrderRepo...
│   ├── Cart/                          # Scoped Cart Service (ITableCartService)
│   ├── Reservations/                  # ViewTablesUseCase, PlaceReservationUseCase...
│   ├── Menu/                          # ViewMenuUseCase, ManageMenuUseCase...
│   ├── Orders/                        # ViewTableOrderUseCase, SubmitTableOrderUseCase...
│   └── Ai/                            # GetAiFoodAdviceUseCase (Trợ lý AI gợi ý thực đơn)
│
├── Plugins.DataStore.InMemory/        # Tầng 3 (Plugin): Dữ liệu RAM cho Sprint 1
│   └── ...                            # Cài đặt InMemoryRepositories chạy ngay lập tức
│
├── Plugins.DataStore.SQL/             # Tầng 3 (Plugin): Dapper ORM & SQL Server cho Sprint 2
│   ├── TableOrderSqlRepository.cs     # Ghi cặp Header-Line trong 1 Database Transaction
│   └── ...                            # Truy vấn tham số hóa 100% (@param)
│
├── WebApp/                            # Tầng 4: Composition Root & Giao diện người dùng
│   ├── Authentication/                # CustomAuthStateProvider & AuthService
│   ├── Components/
│   │   ├── Pages/                     # Home, DatBan, Menu, GoiMon, AiAssistant, Login
│   │   ├── Pages/Admin/               # Floor Plan Sơ đồ bàn, Bếp & Thu ngân, Lịch đặt, Bill
│   │   └── Reusable/                  # 3 Component tái sử dụng: TableCard, MenuItemCard, OrderSummary
│   └── Program.cs                     # Composition Root đăng ký DI & hoán đổi Plugin
│
├── docs/UML/                          # Hồ sơ thiết kế UML (PlantUML)
│   ├── UseCaseDiagram.puml
│   ├── ClassDiagram.puml
│   └── SequenceDiagram.puml
│
├── Restaurant.SchemaAndData.sql       # Script CSDL 8 bảng chuẩn 3NF và dữ liệu khởi tạo
├── GRADING.md                         # Bảng đối soát minh chứng chấm điểm (K1.1 - K5.3)
└── README.md                          # Hướng dẫn chạy & thông tin đăng nhập
```

---

## 2. Hướng Dẫn Chạy Ứng Dụng

### Bước 1: Mở terminal tại thư mục dự án
```bash
cd "c:\Users\ANH KHOA\Downloads\slide dự án"
```

### Bước 2: Chạy ứng dụng
```bash
dotnet run --project WebApp/WebApp.csproj
```
Sau đó mở trình duyệt tại địa chỉ hiển thị trong terminal (ví dụ: `https://localhost:5001` hoặc `http://localhost:5000`).

---

## 3. Tài Khoản Đăng Nhập Hệ Thống

| Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `admin123` | Toàn quyền quản trị sơ đồ bàn, duyệt đơn, menu, thu ngân |
| **Nhân viên (Staff)** | `staff` | `staff123` | Quản lý sơ đồ bàn, điều phối bếp, thu ngân đóng bàn |
| **Khách hàng (Customer)** | `khach` | `khach123` | Xem menu, đặt bàn trước, gọi món tại bàn, AI gợi ý |

---

## 4. Hướng Dẫn Vấn Đáp & Demo 2 Luật Nghiệp Vụ Cốt Lõi

### Luật 1: Ràng Buộc Đặt Bàn (Truy cập `/dat-ban`)
1. **Ca 1 (Vượt sức chứa)**: Bấm nút *"Demo Vi Phạm: Vượt sức chứa bàn"*. Chọn đặt 6 người vào Bàn 01 (chỉ có 2 ghế) $\rightarrow$ Hệ thống lập tức chặn và báo lỗi thân thiện: *"Bàn 01 chỉ có sức chứa tối đa 2 khách... Vui lòng chọn bàn lớn hơn."*
2. **Ca 2 (Trùng khung giờ 2 tiếng)**: Bấm nút *"Demo Vi Phạm: Trùng lịch 2 giờ"*. Chọn giờ 19:00 cho Bàn 01 (đã có khách lúc 18:00) $\rightarrow$ Hệ thống chặn và cảnh báo: *"Bàn 01 đã có khách đặt trước lúc 18:00... vi phạm quy định cách nhau tối thiểu 2 giờ."*

### Luật 2: Ràng Buộc Gọi Món & Đóng Bàn (Truy cập `/goi-mon/1`)
1. **Ca 1 (Món đã hết hàng)**: Bấm nút *"Demo Vi Phạm: Cố tình gọi món đã hết hàng"*. Thử chọn món *"Cá Bống Thệ Kho Tộ"* (đang gắn cờ `IsAvailable = false`) $\rightarrow$ Bị chặn ngay lập tức.
2. **Ca 2 (Tính hóa đơn & Đóng bàn)**: Hóa đơn tự động tính: `SubTotal = Tổng món`, `Phí phục vụ = 5%`, `Tổng cộng = SubTotal + Phí phục vụ`. Sau khi thu ngân bấm *"Thanh Toán & Đóng Bàn"*, đơn chuyển sang `Paid`. Mọi hành động thêm/sửa món sau đó đều bị chặn với thông báo: *"Bàn đã hoàn tất thanh toán và đóng bàn. Không thể gọi thêm hoặc sửa món."*

---

## 5. Chuyển Đổi Giữa In-Memory và SQL Server

Trong file `WebApp/appsettings.json`:
* Để chạy nhanh **In-Memory (Sprint 1)**: Đặt `"DataStore": { "UseSql": false }`
* Để chạy với **SQL Server & Dapper (Sprint 2)**: 
  1. Chạy file `Restaurant.SchemaAndData.sql` trong SQL Server Management Studio (SSMS).
  2. Đặt `"DataStore": { "UseSql": true }`.
