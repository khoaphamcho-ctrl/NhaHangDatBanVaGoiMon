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
(N'Bánh Cố Đô Huế', 1, N'Các món bánh bèo, nậm, lọc đặc trưng truyền thống xứ Huế', 'bi-disc'),
(N'Đặc Sản Huế & Miền Trung', 2, N'Bún bò Huế, Cơm hến, Nem lụi, Mì Quảng, Bánh xèo miền Trung', 'bi-fire'),
(N'Món Ngon Bắc - Nam', 3, N'Phở bò thố đá, Bún chả Hà Nội, Cơm tấm Sài Gòn, Bò lúc lắc', 'bi-stars'),
(N'Hải Sản & Món Nướng', 4, N'Tôm sú nướng muối ớt, Mực một nắng sa tế, Cá hồi áp chảo', 'bi-water'),
(N'Cơm Niêu, Lẩu & Món Gia Đình', 5, N'Cơm niêu kho quẹt, Lẩu Thái hải sản, Canh chua cá bớp', 'bi-cup-hot-fill'),
(N'Khai Vị & Gỏi / Salad', 6, N'Gỏi vả tôm thịt, Salad rong nho mè rang, Nem hải sản chiên xù', 'bi-egg-fried'),
(N'Tráng Miệng & Chè Cung Đình', 7, N'Chè hạt sen long nhãn, chè heo quay, Bánh flan caramel cốt dừa, Trái cây', 'bi-flower1'),
(N'Đồ Uống, Trà & Cà Phê', 8, N'Trà Cung Đình Huế, Cà phê muối, Trà đào cam sả, Sinh tố bơ', 'bi-cup-straw');

-- Nạp món ăn
INSERT INTO dbo.MenuItems (CategoryId, Name, Price, Description, ImageUrl, IsAvailable, Unit) VALUES
-- 1. Bánh Cố Đô Huế
(1, N'Mẹt Bánh Bèo Chén Huế (10 chén)', 65000, N'Bánh bèo bột gạo mềm mịn, tôm chấy đỏ cam thơm lừng, tóp mỡ giòn rụm và nước mắm ớt xanh ngọt cay.', 'images/dishes/banh-beo.jpg', 1, N'Mẹt'),
(1, N'Bánh Nậm Lá Dong (5 cái)', 45000, N'Bánh nậm gói lá dong truyền thống, nhân tôm thịt xay nhuyễn đậm đà, ăn kèm chả huế.', 'images/dishes/banh-nam.jpg', 1, N'Dĩa'),
(1, N'Bánh Bột Lọc Tôm Thịt Gói Lá', 50000, N'Vỏ bánh trong veo dai mềm, nhân tôm sông tươi và thịt ba chỉ rim kẹo đậm đà.', 'images/dishes/banh-bot-loc.jpg', 1, N'Dĩa'),

-- 2. Đặc Sản Huế & Miền Trung
(2, N'Bún Bò Huế Đặc Biệt (Càng Cua + Bắp Bò)', 85000, N'Nước dùng hầm xương bò 12 tiếng cùng sả và mắm ruốc Cố đô, bắp hoa bò mềm ngọt, chả cua Huế tươi ngon.', 'images/dishes/bun-bo-hue.jpg', 1, N'Tô'),
(2, N'Cơm Hến Cồn Hến Cố Đô', 55000, N'Cơm nguội kèm hến xào đậm đà, tóp mỡ chiên giòn, bắp chuối thái mỏng, ruốc thơm và ớt xào Huế.', 'images/dishes/com-hen.jpg', 1, N'Phần'),
(2, N'Mẹt Nem Lụi Nướng Than Hoa (8 cây)', 95000, N'Nem lụi quấn sả nướng vàng óng, ăn kèm rau sống vườn Huế, bánh tráng và nước chấm tương gan đậu béo ngậy.', 'images/dishes/nem-lui.jpg', 1, N'Mẹt'),
(2, N'Cá Bống Thệ Kho Tộ Niêu Đất', 120000, N'Cá bống thệ sông Hương kho tiêu cay nồng trong tộ đất, thịt chắc bùi béo ngậy.', 'images/dishes/ca-bong-kho-to.jpg', 0, N'Tộ'),
(2, N'Mì Quảng Tôm Thịt Trứng Cút Bánh Tráng', 75000, N'Sợi mì vàng nghệ dẻo dai, tôm đất rim đậm vị, thịt ba chỉ thơm nức, đậu phộng rang giòn và bánh tráng mè.', 'images/dishes/mi-quang.jpg', 1, N'Tô'),
(2, N'Bánh Xèo Miền Trung Tôm Nhảy Giòn Rụm', 68000, N'Vỏ bánh xèo giòn rụm màu vàng nghệ, nhân tôm đất nhảy tanh tách, giá đỗ tươi giòn cuộn rau cải xanh và bánh tráng.', 'images/dishes/banh-xeo.jpg', 1, N'Dĩa'),

-- 3. Món Ngon Bắc - Nam
(3, N'Phở Bò Tái Lăn Thố Đá Hà Nội', 89000, N'Nước dùng hầm xương ống bò và quế hồi 24 tiếng sôi sùng sục trong thố đá, thịt bò tươi xào lăn tỏi gừng thơm lừng.', 'images/dishes/pho-bo.jpg', 1, N'Thố'),
(3, N'Bún Chả Hà Nội Nướng Than Hoa', 75000, N'Chả miếng ba chỉ và chả viên nướng xém cạnh thơm nức mùi than hoa, chấm nước mắm chua ngọt ấm nóng kèm dưa góp giòn.', 'images/dishes/bun-cha.jpg', 1, N'Phần'),
(3, N'Cơm Tấm Sườn Bì Chả Sài Gòn', 75000, N'Hạt gạo tấm thơm dẻo, sườn cốt lết ướp mật ong nướng than vàng óng, chả trứng hấp béo ngậy, bì heo giòn dai và mỡ hành.', 'images/dishes/com-tam.jpg', 1, N'Dĩa'),
(3, N'Bò Lúc Lắc Chảo Gang Khoai Tây Giòn', 135000, N'Thịt thăn bò tươi mềm mọng cắt quân cờ xào lửa lớn với ớt chuông, hành tây trên chảo gang xèo xèo kèm khoai tây chiên vàng.', 'images/dishes/bo-luc-lac.jpg', 1, N'Chảo'),

-- 4. Hải Sản & Món Nướng
(4, N'Tôm Sú Nướng Muối Ớt Xanh Nha Trang', 145000, N'Tôm sú biển tươi sống xiên que nướng than hoa thơm phức vỏ giòn rụm, chấm sốt muối ớt chanh xanh cay nồng kích thích.', 'images/dishes/tom-nuong.jpg', 1, N'Đĩa'),
(4, N'Mực Một Nắng Nướng Sa Tế Cù Lao Chàm', 165000, N'Mực một nắng dày thịt dai giòn sần sật, tẩm ướp sa tế tỏi ớt cay tê lưỡi nướng vàng xém rơm.', 'images/dishes/tom-nuong.jpg', 1, N'Đĩa'),
(4, N'Bạch Tuộc Hấp Gừng Sả Lá Chanh', 125000, N'Bạch tuộc tươi giòn ngọt tự nhiên hấp cùng gừng non, sả cây đập dập và lá chanh thơm ngát, chấm mắm gừng ớt hiểm.', 'images/dishes/tom-nuong.jpg', 1, N'Đĩa'),
(4, N'Cá Hồi Áp Chảo Sốt Chanh Dây Chua Ngọt', 185000, N'Phi lê cá hồi Na Uy áp chảo da giòn tan thịt mềm mọng nước, hòa quyện sốt chanh leo vàng óng chua ngọt thanh tao.', 'images/dishes/ca-bong-kho-to.jpg', 1, N'Phần'),

-- 5. Cơm Niêu, Lẩu & Món Gia Đình
(5, N'Cơm Niêu Cháy Giòn Kho Quẹt Tôm Thịt', 95000, N'Cơm niêu đất đáy cháy vàng giòn rụm, niêu đất kho quẹt ba chỉ tôm khô tiêu sọ đậm đà chấm cùng đĩa rau củ luộc tươi xanh.', 'images/dishes/com-nieu.jpg', 1, N'Thố'),
(5, N'Lẩu Thái Hải Sản Tomyum Chua Cay (3-4 người)', 299000, N'Nước dùng Tomyum cốt dừa chua cay chuẩn vị Thái Lan, đầy ắp tôm sú, mực tươi, ba chỉ bò Mỹ, nghêu, rau muống và nấm tuyết.', 'images/dishes/lau-thai.jpg', 1, N'Nồi'),
(5, N'Canh Chua Cá Bớp Bông Điên Điển Tây Nam Bộ', 110000, N'Cá bớp tươi béo ngậy nấu cùng me chín, bạc hà, cà chua bi, bắp chuối và hoa điên điển mùa nước nổi ngọt mát giải nhiệt.', 'images/dishes/ca-bong-kho-to.jpg', 1, N'Tô'),
(5, N'Sườn Heo Non Nướng Mật Ong Rừng', 130000, N'Sườn cây non tuyển chọn ướp mật ong hoa rừng nguyên chất và ngũ vị hương nướng than hoa vàng óng, thịt mềm róc xương.', 'images/dishes/nem-lui.jpg', 1, N'Phần'),

-- 6. Khai Vị & Gỏi / Salad
(6, N'Gỏi Vả Trộn Tôm Thịt Bánh Tráng Mè', 75000, N'Trái vả non luộc chín thái mỏng trộn cùng tôm sông, thịt ba rọi và rau răm, xúc bánh tráng mè nướng giòn rụm.', 'images/dishes/goi-va.jpg', 1, N'Đĩa'),
(6, N'Salad Ức Gà Rong Nho Sốt Mè Rang', 69000, N'Xà lách thuỷ canh tươi giòn, ức gà áp chảo thơm lừng, rong nho biển giòn bụp hoà quyện sốt mè rang béo bùi thơm dịu.', 'images/dishes/goi-va.jpg', 1, N'Đĩa'),
(6, N'Nem Hải Sản Phô Mai Chiên Xù', 85000, N'Nhân tôm biển, mực tươi bọc phô mai Mozzarella kéo sợi béo ngậy, lăn bột xù chiên vàng rụm chấm sốt mayonnaise tương ớt.', 'images/dishes/nem-lui.jpg', 1, N'Đĩa'),
(6, N'Khoai Tây Chiên Bơ Tỏi Phô Mai Lắc', 45000, N'Khoai tây sợi lớn chiên vàng giòn tan, lắc đều cùng bơ thơm lừng tỏi phi và bột phô mai béo ngậy đậm đà.', 'images/dishes/bo-luc-lac.jpg', 1, N'Đĩa'),

-- 7. Tráng Miệng & Chè Cung Đình
(7, N'Chè Hạt Sen Long Nhãn Cung Đình', 40000, N'Hạt sen hồ Tịnh Tâm bùi dẻo bọc trong cùi long nhãn giòn ngọt, nước đường phèn thanh mát bổ dưỡng an thần.', 'images/dishes/che-hat-sen.jpg', 1, N'Chén'),
(7, N'Chè Bột Lọc Heo Quay Đặc Sản', 45000, N'Món chè độc nhất vô nhị xứ Huế, vị ngọt thanh của nước đường gừng kết hợp vị béo mặn của thịt heo quay giòn bì.', 'images/dishes/che-bot-loc-heo-quay.jpg', 1, N'Chén'),
(7, N'Bánh Flan Caramel Cốt Dừa Béo Ngậy', 35000, N'Bánh flan mềm mịn tan chảy trong miệng, đẫm sốt caramel hổ phách thơm cafe và nước cốt dừa béo bùi kèm đá bào mát lạnh.', 'images/dishes/banh-flan.jpg', 1, N'Đĩa'),
(7, N'Đĩa Trái Cây Tươi Thập Cẩm Nhiệt Đới', 55000, N'Đĩa trái cây tươi ướp lạnh cao cấp gồm dưa lưới ruột cam, xoài cát Hòa Lộc, bưởi da xanh và dâu tây Đà Lạt.', 'images/dishes/che-hat-sen.jpg', 1, N'Đĩa'),

-- 8. Đồ Uống, Trà & Cà Phê
(8, N'Ấm Trà Cung Đình Nhất Dạ Đế Vương', 60000, N'Ấm trà thảo mộc 16 vị quý (atiso, cúc hoa, đẳng sâm, cỏ ngọt, tim sen...) bồi bổ sức khỏe và thanh lọc cơ thể.', 'images/dishes/tra-cung-dinh.jpg', 1, N'Ấm'),
(8, N'Nước Mía Ép Quất Huế Ướp Lạnh', 25000, N'Mía ngọt thanh kết hợp vị chua dịu thơm mát của tắc tươi ép nguyên chất mát lạnh sảng khoái.', 'images/dishes/nuoc-mia-quat.jpg', 1, N'Ly'),
(8, N'Trà Đào Cam Sả Tươi Mát Lạnh', 38000, N'Hồng trà thượng hạng ủ lạnh thơm nức mùi sả cây tươi, cam vàng mọng nước và từng miếng đào ngâm giòn ngọt thanh khiết.', 'images/dishes/tra-dao.jpg', 1, N'Ly'),
(8, N'Cà Phê Muối Xứ Huế Truyền Thống', 35000, N'Cà phê phin nguyên chất Robusta đậm đặc kết hợp lớp kem sữa muối béo mặn sánh mịn đặc trưng vùng đất Cố Đô.', 'images/dishes/ca-phe-muoi.jpg', 1, N'Ly'),
(8, N'Sinh Tố Bơ Sáp Đắk Lắk Cốt Dừa', 42000, N'Bơ sáp dẻo bùi béo tự nhiên xay nhuyễn cùng sữa đặc, sữa tươi không đường và nước cốt dừa ngậy béo thơm lừng.', 'images/dishes/nuoc-mia-quat.jpg', 1, N'Ly');

-- Nạp đặt bàn mẫu
INSERT INTO dbo.Reservations (DiningTableId, CustomerName, PhoneNumber, ReservationTime, GuestCount, Status, Note) VALUES
(1, N'Nguyễn Văn An', '0905123456', DATEADD(HOUR, 18, CAST(CAST(GETDATE() AS DATE) AS DATETIME2)), 2, 0, N'Kỷ niệm ngày cưới, chuẩn bị nến thơm'),
(4, N'Trần Thị Mai', '0914987654', DATEADD(MINUTE, 30, DATEADD(HOUR, 19, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 5, 0, N'Gia đình có trẻ em');
GO

PRINT N'Khởi tạo Cơ sở Dữ liệu RestaurantDb thành công!';
GO
