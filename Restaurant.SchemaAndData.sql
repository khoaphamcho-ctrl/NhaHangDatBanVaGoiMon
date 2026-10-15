-- =======================================================================================
-- ĐỒ ÁN CAPSTONE - ECO2415
-- ĐỀ TÀI 11: NHÀ HÀNG - ĐẶT BÀN & GỌI MÓN TRỰC TUYẾN
-- DATABASE SCHEMA VÀ DỮ LIỆU KHỞI TẠO CHUẨN 3NF (TIÊU CHÍ K2.1)
-- =======================================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'RestaurantDb')
BEGIN
    CREATE DATABASE RestaurantDb;
END
GO

USE RestaurantDb;
GO

-- Xóa bảng theo thứ tự khóa ngoại nếu đã tồn tại
IF OBJECT_ID('dbo.TableOrderLines', 'U') IS NOT NULL DROP TABLE dbo.TableOrderLines;
IF OBJECT_ID('dbo.TableOrders', 'U') IS NOT NULL DROP TABLE dbo.TableOrders;
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.DiningTables', 'U') IS NOT NULL DROP TABLE dbo.DiningTables;
IF OBJECT_ID('dbo.MenuItems', 'U') IS NOT NULL DROP TABLE dbo.MenuItems;
IF OBJECT_ID('dbo.MenuCategories', 'U') IS NOT NULL DROP TABLE dbo.MenuCategories;
IF OBJECT_ID('dbo.RestaurantConfigs', 'U') IS NOT NULL DROP TABLE dbo.RestaurantConfigs;
IF OBJECT_ID('dbo.UserAccounts', 'U') IS NOT NULL DROP TABLE dbo.UserAccounts;
GO

-- 1. BẢNG BÀN ĂN (DiningTables)
CREATE TABLE dbo.DiningTables (
    TableId INT IDENTITY(1,1) PRIMARY KEY,
    TableNumber NVARCHAR(50) NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0: Available, 1: Occupied, 2: Reserved
    Location NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);
GO

-- 2. BẢNG ĐẶT BÀN (Reservations)
CREATE TABLE dbo.Reservations (
    ReservationId INT IDENTITY(1,1) PRIMARY KEY,
    DiningTableId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    ReservationTime DATETIME2 NOT NULL,
    GuestCount INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0: Confirmed, 1: Completed, 2: Cancelled
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Reservations_DiningTables FOREIGN KEY (DiningTableId) 
        REFERENCES dbo.DiningTables (TableId)
);
GO

-- 3. BẢNG DANH MỤC MÓN (MenuCategories)
CREATE TABLE dbo.MenuCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    Icon NVARCHAR(50) NOT NULL DEFAULT 'bi-cup-hot'
);
GO

-- 4. BẢNG MÓN ĂN THỰC ĐƠN (MenuItems)
CREATE TABLE dbo.MenuItems (
    MenuItemId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    Unit NVARCHAR(50) NOT NULL DEFAULT N'Phần',
    CONSTRAINT FK_MenuItems_MenuCategories FOREIGN KEY (CategoryId) 
        REFERENCES dbo.MenuCategories (CategoryId)
);
GO

-- 5. BẢNG ĐƠN GỌI MÓN (TableOrders - HEADER)
CREATE TABLE dbo.TableOrders (
    TableOrderId INT IDENTITY(1,1) PRIMARY KEY,
    DiningTableId INT NOT NULL,
    OrderTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    ServiceFeeRate DECIMAL(5,2) NOT NULL DEFAULT 5.00,
    ServiceFeeAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0, -- 0: Open, 1: Paid, 2: Cancelled
    Note NVARCHAR(500) NULL,
    ClosedAt DATETIME2 NULL,
    CONSTRAINT FK_TableOrders_DiningTables FOREIGN KEY (DiningTableId) 
        REFERENCES dbo.DiningTables (TableId)
);
GO

-- 6. BẢNG DÒNG MÓN GỌI (TableOrderLines - LINE)
CREATE TABLE dbo.TableOrderLines (
    TableOrderLineId INT IDENTITY(1,1) PRIMARY KEY,
    TableOrderId INT NOT NULL,
    MenuItemId INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    LineTotal DECIMAL(18,2) NOT NULL,
    SpecialInstructions NVARCHAR(255) NULL,
    Status INT NOT NULL DEFAULT 0, -- 0: SentToKitchen, 1: Cooking, 2: Served
    CONSTRAINT FK_TableOrderLines_TableOrders FOREIGN KEY (TableOrderId) 
        REFERENCES dbo.TableOrders (TableOrderId) ON DELETE CASCADE,
    CONSTRAINT FK_TableOrderLines_MenuItems FOREIGN KEY (MenuItemId) 
        REFERENCES dbo.MenuItems (MenuItemId)
);
GO

-- 7. BẢNG CẤU HÌNH NHÀ HÀNG (RestaurantConfigs)
CREATE TABLE dbo.RestaurantConfigs (
    ConfigKey NVARCHAR(50) PRIMARY KEY,
    ConfigValue NVARCHAR(255) NOT NULL,
    Description NVARCHAR(255) NULL
);
GO

-- 8. BẢNG TÀI KHOẢN NGƯỜI DÙNG & PHÂN QUYỀN (UserAccounts)
CREATE TABLE dbo.UserAccounts (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL -- Admin, Staff, Customer
);
GO

-- =======================================================================================
-- NẠP DỮ LIỆU KHỞI TẠO MẪU (SEED DATA)
-- =======================================================================================

-- Nạp cấu hình
INSERT INTO dbo.RestaurantConfigs (ConfigKey, ConfigValue, Description) VALUES
('ServiceFeePercent', '5', N'Tỷ lệ phần trăm phí phục vụ (%)'),
('RestaurantName', N'Nhà Hàng Cố Đô Huế - Hoàng Cung', N'Tên thương hiệu'),
('Hotline', '0905.888.999', N'Số hotline đặt bàn'),
('Address', N'68 Lê Lợi, TP. Huế', N'Địa chỉ nhà hàng');

-- Nạp tài khoản
INSERT INTO dbo.UserAccounts (Username, PasswordHash, FullName, Role) VALUES
('admin', 'admin123', N'Quản Lý Nhà Hàng', 'Admin'),
('staff', 'staff123', N'Nhân Viên Thu Ngân & Phục Vụ', 'Staff'),
('khach', 'khach123', N'Khách Hàng Thân Thiết', 'Customer');

-- Nạp danh sách bàn
INSERT INTO dbo.DiningTables (TableNumber, Capacity, Status, Location, Description) VALUES
(N'Bàn 01', 2, 0, N'Tầng 1 (Cửa sổ)', N'Bàn đôi lãng mạn hướng phố'),
(N'Bàn 02', 4, 0, N'Tầng 1 (Sảnh chính)', N'Bàn 4 người tiện lợi gần quầy'),
(N'Bàn 03', 4, 0, N'Tầng 1 (Sảnh chính)', N'Bàn 4 người ghế sofa êm ái'),
(N'Bàn 04', 6, 0, N'Tầng 2 (Ban công)', N'Bàn 6 người ngắm view sông Hương'),
(N'Bàn 05', 6, 0, N'Tầng 2 (Trong nhà)', N'Bàn gia đình ấm cúng'),
(N'Bàn 06', 8, 0, N'Tầng 2 (Sân thượng)', N'Bàn tiệc nhóm lớn thoáng mát'),
(N'VIP 01', 10, 0, N'Phòng VIP Hoàng Gia', N'Phòng riêng máy lạnh, âm thanh riêng'),
(N'Bàn 08', 4, 0, N'Sân vườn', N'Bàn ngoài trời nhiều cây xanh'),
(N'Bàn 09', 2, 0, N'Sân vườn', N'Bàn đôi yên tĩnh góc vườn'),
(N'VIP 02', 12, 0, N'Phòng VIP Cung Đình', N'Phòng tiệc lớn tiếp khách quý');

-- Nạp danh mục món
INSERT INTO dbo.MenuCategories (Name, DisplayOrder, Description, Icon) VALUES
(N'Bánh Cố Đô Huế', 1, N'Các món bánh bèo, nậm, lọc đặc trưng truyền thống', 'bi-disc'),
(N'Món Chính & Đặc Sản', 2, N'Bún bò Huế, Cơm hến, Nem lụi, Vịt nướng', 'bi-fire'),
(N'Khai Vị & Gỏi', 3, N'Gỏi vả tôm thịt, Cuốn diếp, Chả tôm', 'bi-egg-fried'),
(N'Tráng Miệng & Chè Cung Đình', 4, N'Chè hạt sen long nhãn, chè bột lọc bọc heo quay', 'bi-flower1'),
(N'Đồ Uống & Trà Cung Đình', 5, N'Trà Cung Đình Huế, Nước mát, Nước ép tươi', 'bi-cup-straw');

-- Nạp món ăn
INSERT INTO dbo.MenuItems (CategoryId, Name, Price, Description, ImageUrl, IsAvailable, Unit) VALUES
(1, N'Mẹt Bánh Bèo Chén Huế (10 chén)', 65000, N'Bánh bèo bột gạo mềm mịn, tôm chấy đỏ cam thơm lừng, tóp mỡ giòn rụm và nước mắm ớt xanh ngọt cay.', 'https://images.unsplash.com/photo-1541544741938-0af808871cc0?w=600&auto=format&fit=crop', 1, N'Mẹt'),
(1, N'Bánh Nậm Lá Dong (5 cái)', 45000, N'Bánh nậm gói lá dong truyền thống, nhân tôm thịt xay nhuyễn đậm đà, ăn kèm chả huế.', 'https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop', 1, N'Dĩa'),
(1, N'Bánh Bột Lọc Tôm Thịt Gói Lá', 50000, N'Vỏ bánh trong veo dai mềm, nhân tôm sông tươi và thịt ba chỉ rim kẹo đậm đà.', 'https://images.unsplash.com/photo-1509722747041-616f39b57569?w=600&auto=format&fit=crop', 1, N'Dĩa'),
(2, N'Bún Bò Huế Đặc Biệt (Càng Cua + Bắp Bò)', 85000, N'Nước dùng hầm xương bò 12 tiếng cùng sả và mắm ruốc Cố đô, bắp hoa bò mềm ngọt, chả cua Huế tươi ngon.', 'https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=600&auto=format&fit=crop', 1, N'Tô'),
(2, N'Cơm Hến Cồn Hến Cố Đô', 55000, N'Cơm nguội kèm hến xào đậm đà, tóp mỡ chiên giòn, bắp chuối thái mỏng, ruốc thơm và ớt xào Huế.', 'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=600&auto=format&fit=crop', 1, N'Phần'),
(2, N'Mẹt Nem Lụi Nướng Than Hoa (8 cây)', 95000, N'Nem lụi quấn sả nướng vàng óng, ăn kèm rau sống vườn Huế, bánh tráng và nước chấm tương gan đậu béo ngậy.', 'https://images.unsplash.com/photo-1529042410759-befb1204b468?w=600&auto=format&fit=crop', 1, N'Mẹt'),
(2, N'Cá Bống Thệ Kho Tộ Niêu Đất', 120000, N'Cá bống thệ sông Hương kho tiêu cay nồng trong tộ đất, thịt chắc bùi béo ngậy.', 'https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=600&auto=format&fit=crop', 0, N'Tộ'), -- Món đã hết hàng
(3, N'Gỏi Vả Trộn Tôm Thịt Bánh Tráng Mè', 75000, N'Trái vả non luộc chín thái mỏng trộn cùng tôm sông, thịt ba rọi và rau răm, xúc bánh tráng nướng giòn.', 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop', 1, N'Đĩa'),
(4, N'Chè Hạt Sen Long Nhãn Cung Đình', 40000, N'Hạt sen hồ Tịnh Tâm bùi dẻo bọc trong cùi long nhãn giòn ngọt, nước đường phèn thanh mát.', 'https://images.unsplash.com/photo-1505253758473-96b3d5ebcd96?w=600&auto=format&fit=crop', 1, N'Chén'),
(4, N'Chè Bột Lọc Heo Quay Đặc Sản', 45000, N'Món chè độc nhất vô nhị xứ Huế, vị ngọt thanh của nước đường gừng kết hợp vị béo mặn của thịt heo quay giòn bì.', 'https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop', 1, N'Chén'),
(5, N'Ấm Trà Cung Đình Nhất Dạ Đế Vương', 60000, N'Ấm trà thảo mộc 16 vị quý (atiso, cúc hoa, đẳng sâm, cỏ ngọt, tim sen...) bồi bổ sức khỏe và thanh lọc cơ thể.', 'https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=600&auto=format&fit=crop', 1, N'Ấm'),
(5, N'Nước Mía Ép Quất Huế Ướp Lạnh', 25000, N'Mía ngọt thanh kết hợp vị chua dịu thơm mát của tắc tươi.', 'https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=600&auto=format&fit=crop', 1, N'Ly');

-- Nạp đặt bàn mẫu
INSERT INTO dbo.Reservations (DiningTableId, CustomerName, PhoneNumber, ReservationTime, GuestCount, Status, Note) VALUES
(1, N'Nguyễn Văn An', '0905123456', DATEADD(HOUR, 18, CAST(CAST(GETDATE() AS DATE) AS DATETIME2)), 2, 0, N'Kỷ niệm ngày cưới, chuẩn bị nến thơm'),
(4, N'Trần Thị Mai', '0914987654', DATEADD(MINUTE, 30, DATEADD(HOUR, 19, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 5, 0, N'Gia đình có trẻ em');
GO

PRINT N'Khởi tạo Cơ sở Dữ liệu RestaurantDb thành công!';
GO
