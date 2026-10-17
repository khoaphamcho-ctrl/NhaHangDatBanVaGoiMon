using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class MenuItemInMemoryRepository : IMenuItemRepository
{
    private static readonly List<MenuItem> _items = new()
    {
        // 1. BÁNH CỐ ĐÔ HUẾ (CategoryId = 1)
        new MenuItem
        {
            MenuItemId = 1,
            CategoryId = 1,
            CategoryName = "Bánh Cố Đô Huế",
            Name = "Mẹt Bánh Bèo Chén Huế (10 chén)",
            Price = 65000,
            Description = "Bánh bèo bột gạo mềm mịn, tôm chấy đỏ cam thơm lừng, tóp mỡ giòn rụm và nước mắm ớt xanh ngọt cay.",
            ImageUrl = "images/dishes/banh-beo.jpg",
            IsAvailable = true,
            Unit = "Mẹt"
        },
        new MenuItem
        {
            MenuItemId = 2,
            CategoryId = 1,
            CategoryName = "Bánh Cố Đô Huế",
            Name = "Bánh Nậm Lá Dong (5 cái)",
            Price = 45000,
            Description = "Bánh nậm gói lá dong truyền thống, nhân tôm thịt xay nhuyễn đậm đà, ăn kèm chả huế.",
            ImageUrl = "images/dishes/banh-nam.jpg",
            IsAvailable = true,
            Unit = "Dĩa"
        },
        new MenuItem
        {
            MenuItemId = 3,
            CategoryId = 1,
            CategoryName = "Bánh Cố Đô Huế",
            Name = "Bánh Bột Lọc Tôm Thịt Gói Lá",
            Price = 50000,
            Description = "Vỏ bánh trong veo dai mềm, nhân tôm sông tươi và thịt ba chỉ rim kẹo đậm đà.",
            ImageUrl = "images/dishes/banh-bot-loc.jpg",
            IsAvailable = true,
            Unit = "Dĩa"
        },

        // 2. ĐẶC SẢN HUẾ & MIỀN TRUNG (CategoryId = 2)
        new MenuItem
        {
            MenuItemId = 4,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Bún Bò Huế Đặc Biệt (Càng Cua + Bắp Bò)",
            Price = 85000,
            Description = "Nước dùng hầm xương bò 12 tiếng cùng sả và mắm ruốc Cố đô, bắp hoa bò mềm ngọt, chả cua Huế tươi ngon.",
            ImageUrl = "images/dishes/bun-bo-hue.jpg",
            IsAvailable = true,
            Unit = "Tô"
        },
        new MenuItem
        {
            MenuItemId = 5,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Cơm Hến Cồn Hến Cố Đô",
            Price = 55000,
            Description = "Cơm nguội kèm hến xào đậm đà, tóp mỡ chiên giòn, bắp chuối thái mỏng, ruốc thơm và ớt xào Huế.",
            ImageUrl = "images/dishes/com-hen.jpg",
            IsAvailable = true,
            Unit = "Phần"
        },
        new MenuItem
        {
            MenuItemId = 6,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Mẹt Nem Lụi Nướng Than Hoa (8 cây)",
            Price = 95000,
            Description = "Nem lụi quấn sả nướng vàng óng, ăn kèm rau sống vườn Huế, bánh tráng và nước chấm tương gan đậu béo ngậy.",
            ImageUrl = "images/dishes/nem-lui.jpg",
            IsAvailable = true,
            Unit = "Mẹt"
        },
        new MenuItem
        {
            MenuItemId = 7,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Cá Bống Thệ Kho Tộ Niêu Đất",
            Price = 120000,
            Description = "Cá bống thệ sông Hương kho tiêu cay nồng trong tộ đất, thịt chắc bùi béo ngậy.",
            ImageUrl = "images/dishes/ca-bong-kho-to.jpg",
            IsAvailable = false, // Phục vụ demo Luật 2.2 món hết hàng!
            Unit = "Tộ"
        },
        new MenuItem
        {
            MenuItemId = 8,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Mì Quảng Tôm Thịt Trứng Cút Bánh Tráng",
            Price = 75000,
            Description = "Sợi mì vàng nghệ dẻo dai, tôm đất rim đậm vị, thịt ba chỉ thơm nức, đậu phộng rang giòn và bánh tráng mè.",
            ImageUrl = "images/dishes/mi-quang.jpg",
            IsAvailable = true,
            Unit = "Tô"
        },
        new MenuItem
        {
            MenuItemId = 9,
            CategoryId = 2,
            CategoryName = "Đặc Sản Huế & Miền Trung",
            Name = "Bánh Xèo Miền Trung Tôm Nhảy Giòn Rụm",
            Price = 68000,
            Description = "Vỏ bánh xèo giòn rụm màu vàng nghệ, nhân tôm đất nhảy tanh tách, giá đỗ tươi giòn cuộn rau cải xanh và bánh tráng.",
            ImageUrl = "images/dishes/banh-xeo.jpg",
            IsAvailable = true,
            Unit = "Dĩa"
        },

        // 3. MÓN NGON BẮC - NAM (CategoryId = 3)
        new MenuItem
        {
            MenuItemId = 10,
            CategoryId = 3,
            CategoryName = "Món Ngon Bắc - Nam",
            Name = "Phở Bò Tái Lăn Thố Đá Hà Nội",
            Price = 89000,
            Description = "Nước dùng hầm xương ống bò và quế hồi 24 tiếng sôi sùng sục trong thố đá, thịt bò tươi xào lăn tỏi gừng thơm lừng.",
            ImageUrl = "images/dishes/pho-bo.jpg",
            IsAvailable = true,
            Unit = "Thố"
        },
        new MenuItem
        {
            MenuItemId = 11,
            CategoryId = 3,
            CategoryName = "Món Ngon Bắc - Nam",
            Name = "Bún Chả Hà Nội Nướng Than Hoa",
            Price = 75000,
            Description = "Chả miếng ba chỉ và chả viên nướng xém cạnh thơm nức mùi than hoa, chấm nước mắm chua ngọt ấm nóng kèm dưa góp giòn.",
            ImageUrl = "images/dishes/bun-cha.jpg",
            IsAvailable = true,
            Unit = "Phần"
        },
        new MenuItem
        {
            MenuItemId = 12,
            CategoryId = 3,
            CategoryName = "Món Ngon Bắc - Nam",
            Name = "Cơm Tấm Sườn Bì Chả Sài Gòn",
            Price = 75000,
            Description = "Hạt gạo tấm thơm dẻo, sườn cốt lết ướp mật ong nướng than vàng óng, chả trứng hấp béo ngậy, bì heo giòn dai và mỡ hành.",
            ImageUrl = "images/dishes/com-tam.jpg",
            IsAvailable = true,
            Unit = "Dĩa"
        },
        new MenuItem
        {
            MenuItemId = 13,
            CategoryId = 3,
            CategoryName = "Món Ngon Bắc - Nam",
            Name = "Bò Lúc Lắc Chảo Gang Khoai Tây Giòn",
            Price = 135000,
            Description = "Thịt thăn bò tươi mềm mọng cắt quân cờ xào lửa lớn với ớt chuông, hành tây trên chảo gang xèo xèo kèm khoai tây chiên vàng.",
            ImageUrl = "images/dishes/bo-luc-lac.jpg",
            IsAvailable = true,
            Unit = "Chảo"
        },

        // 4. HẢI SẢN & MÓN NƯỚNG (CategoryId = 4)
        new MenuItem
        {
            MenuItemId = 14,
            CategoryId = 4,
            CategoryName = "Hải Sản & Món Nướng",
            Name = "Tôm Sú Nướng Muối Ớt Xanh Nha Trang",
            Price = 145000,
            Description = "Tôm sú biển tươi sống xiên que nướng than hoa thơm phức vỏ giòn rụm, chấm sốt muối ớt chanh xanh cay nồng kích thích.",
            ImageUrl = "images/dishes/tom-nuong.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 15,
            CategoryId = 4,
            CategoryName = "Hải Sản & Món Nướng",
            Name = "Mực Một Nắng Nướng Sa Tế Cù Lao Chàm",
            Price = 165000,
            Description = "Mực một nắng dày thịt dai giòn sần sật, tẩm ướp sa tế tỏi ớt cay tê lưỡi nướng vàng xém rơm.",
            ImageUrl = "images/dishes/tom-nuong.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 16,
            CategoryId = 4,
            CategoryName = "Hải Sản & Món Nướng",
            Name = "Bạch Tuộc Hấp Gừng Sả Lá Chanh",
            Price = 125000,
            Description = "Bạch tuộc tươi giòn ngọt tự nhiên hấp cùng gừng non, sả cây đập dập và lá chanh thơm ngát, chấm mắm gừng ớt hiểm.",
            ImageUrl = "images/dishes/tom-nuong.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 17,
            CategoryId = 4,
            CategoryName = "Hải Sản & Món Nướng",
            Name = "Cá Hồi Áp Chảo Sốt Chanh Dây Chua Ngọt",
            Price = 185000,
            Description = "Phi lê cá hồi Na Uy áp chảo da giòn tan thịt mềm mọng nước, hòa quyện sốt chanh leo vàng óng chua ngọt thanh tao.",
            ImageUrl = "images/dishes/ca-bong-kho-to.jpg",
            IsAvailable = true,
            Unit = "Phần"
        },

        // 5. CƠM NIÊU, LẨU & MÓN GIA ĐÌNH (CategoryId = 5)
        new MenuItem
        {
            MenuItemId = 18,
            CategoryId = 5,
            CategoryName = "Cơm Niêu, Lẩu & Món Gia Đình",
            Name = "Cơm Niêu Cháy Giòn Kho Quẹt Tôm Thịt",
            Price = 95000,
            Description = "Cơm niêu đất đáy cháy vàng giòn rụm, niêu đất kho quẹt ba chỉ tôm khô tiêu sọ đậm đà chấm cùng đĩa rau củ luộc tươi xanh.",
            ImageUrl = "images/dishes/com-nieu.jpg",
            IsAvailable = true,
            Unit = "Thố"
        },
        new MenuItem
        {
            MenuItemId = 19,
            CategoryId = 5,
            CategoryName = "Cơm Niêu, Lẩu & Món Gia Đình",
            Name = "Lẩu Thái Hải Sản Tomyum Chua Cay (3-4 người)",
            Price = 299000,
            Description = "Nước dùng Tomyum cốt dừa chua cay chuẩn vị Thái Lan, đầy ắp tôm sú, mực tươi, ba chỉ bò Mỹ, nghêu, rau muống và nấm tuyết.",
            ImageUrl = "images/dishes/lau-thai.jpg",
            IsAvailable = true,
            Unit = "Nồi"
        },
        new MenuItem
        {
            MenuItemId = 20,
            CategoryId = 5,
            CategoryName = "Cơm Niêu, Lẩu & Món Gia Đình",
            Name = "Canh Chua Cá Bớp Bông Điên Điển Tây Nam Bộ",
            Price = 110000,
            Description = "Cá bớp tươi béo ngậy nấu cùng me chín, bạc hà, cà chua bi, bắp chuối và hoa điên điển mùa nước nổi ngọt mát giải nhiệt.",
            ImageUrl = "images/dishes/ca-bong-kho-to.jpg",
            IsAvailable = true,
            Unit = "Tô"
        },
        new MenuItem
        {
            MenuItemId = 21,
            CategoryId = 5,
            CategoryName = "Cơm Niêu, Lẩu & Món Gia Đình",
            Name = "Sườn Heo Non Nướng Mật Ong Rừng",
            Price = 130000,
            Description = "Sườn cây non tuyển chọn ướp mật ong hoa rừng nguyên chất và ngũ vị hương nướng than hoa vàng óng, thịt mềm róc xương.",
            ImageUrl = "images/dishes/nem-lui.jpg",
            IsAvailable = true,
            Unit = "Phần"
        },

        // 6. KHAI VỊ & GỎI / SALAD (CategoryId = 6)
        new MenuItem
        {
            MenuItemId = 22,
            CategoryId = 6,
            CategoryName = "Khai Vị & Gỏi / Salad",
            Name = "Gỏi Vả Trộn Tôm Thịt Bánh Tráng Mè",
            Price = 75000,
            Description = "Trái vả non luộc chín thái mỏng trộn cùng tôm sông, thịt ba rọi và rau răm, xúc bánh tráng mè nướng giòn rụm.",
            ImageUrl = "images/dishes/goi-va.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 23,
            CategoryId = 6,
            CategoryName = "Khai Vị & Gỏi / Salad",
            Name = "Salad Ức Gà Rong Nho Sốt Mè Rang",
            Price = 69000,
            Description = "Xà lách thuỷ canh tươi giòn, ức gà áp chảo thơm lừng, rong nho biển giòn bụp hoà quyện sốt mè rang béo bùi thơm dịu.",
            ImageUrl = "images/dishes/goi-va.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 24,
            CategoryId = 6,
            CategoryName = "Khai Vị & Gỏi / Salad",
            Name = "Nem Hải Sản Phô Mai Chiên Xù",
            Price = 85000,
            Description = "Nhân tôm biển, mực tươi bọc phô mai Mozzarella kéo sợi béo ngậy, lăn bột xù chiên vàng rụm chấm sốt mayonnaise tương ớt.",
            ImageUrl = "images/dishes/nem-lui.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 25,
            CategoryId = 6,
            CategoryName = "Khai Vị & Gỏi / Salad",
            Name = "Khoai Tây Chiên Bơ Tỏi Phô Mai Lắc",
            Price = 45000,
            Description = "Khoai tây sợi lớn chiên vàng giòn tan, lắc đều cùng bơ thơm lừng tỏi phi và bột phô mai béo ngậy đậm đà.",
            ImageUrl = "images/dishes/bo-luc-lac.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },

        // 7. TRÁNG MIỆNG & CHÈ CUNG ĐÌNH (CategoryId = 7)
        new MenuItem
        {
            MenuItemId = 26,
            CategoryId = 7,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Chè Hạt Sen Long Nhãn Cung Đình",
            Price = 40000,
            Description = "Hạt sen hồ Tịnh Tâm bùi dẻo bọc trong cùi long nhãn giòn ngọt, nước đường phèn thanh mát bổ dưỡng an thần.",
            ImageUrl = "images/dishes/che-hat-sen.jpg",
            IsAvailable = true,
            Unit = "Chén"
        },
        new MenuItem
        {
            MenuItemId = 27,
            CategoryId = 7,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Chè Bột Lọc Heo Quay Đặc Sản",
            Price = 45000,
            Description = "Món chè độc nhất vô nhị xứ Huế, vị ngọt thanh của nước đường gừng kết hợp vị béo mặn của thịt heo quay giòn bì.",
            ImageUrl = "images/dishes/che-bot-loc-heo-quay.jpg",
            IsAvailable = true,
            Unit = "Chén"
        },
        new MenuItem
        {
            MenuItemId = 28,
            CategoryId = 7,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Bánh Flan Caramel Cốt Dừa Béo Ngậy",
            Price = 35000,
            Description = "Bánh flan mềm mịn tan chảy trong miệng, đẫm sốt caramel hổ phách thơm cafe và nước cốt dừa béo bùi kèm đá bào mát lạnh.",
            ImageUrl = "images/dishes/banh-flan.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },
        new MenuItem
        {
            MenuItemId = 29,
            CategoryId = 7,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Đĩa Trái Cây Tươi Thập Cẩm Nhiệt Đới",
            Price = 55000,
            Description = "Đĩa trái cây tươi ướp lạnh cao cấp gồm dưa lưới ruột cam, xoài cát Hòa Lộc, bưởi da xanh và dâu tây Đà Lạt.",
            ImageUrl = "images/dishes/che-hat-sen.jpg",
            IsAvailable = true,
            Unit = "Đĩa"
        },

        // 8. ĐỒ UỐNG, TRÀ & CÀ PHÊ (CategoryId = 8)
        new MenuItem
        {
            MenuItemId = 30,
            CategoryId = 8,
            CategoryName = "Đồ Uống, Trà & Cà Phê",
            Name = "Ấm Trà Cung Đình Nhất Dạ Đế Vương",
            Price = 60000,
            Description = "Ấm trà thảo mộc 16 vị quý (atiso, cúc hoa, đẳng sâm, cỏ ngọt, tim sen...) bồi bổ sức khỏe và thanh lọc cơ thể.",
            ImageUrl = "images/dishes/tra-cung-dinh.jpg",
            IsAvailable = true,
            Unit = "Ấm"
        },
        new MenuItem
        {
            MenuItemId = 31,
            CategoryId = 8,
            CategoryName = "Đồ Uống, Trà & Cà Phê",
            Name = "Nước Mía Ép Quất Huế Ướp Lạnh",
            Price = 25000,
            Description = "Mía ngọt thanh kết hợp vị chua dịu thơm mát của tắc tươi ép nguyên chất mát lạnh sảng khoái.",
            ImageUrl = "images/dishes/nuoc-mia-quat.jpg",
            IsAvailable = true,
            Unit = "Ly"
        },
        new MenuItem
        {
            MenuItemId = 32,
            CategoryId = 8,
            CategoryName = "Đồ Uống, Trà & Cà Phê",
            Name = "Trà Đào Cam Sả Tươi Mát Lạnh",
            Price = 38000,
            Description = "Hồng trà thượng hạng ủ lạnh thơm nức mùi sả cây tươi, cam vàng mọng nước và từng miếng đào ngâm giòn ngọt thanh khiết.",
            ImageUrl = "images/dishes/tra-dao.jpg",
            IsAvailable = true,
            Unit = "Ly"
        },
        new MenuItem
        {
            MenuItemId = 33,
            CategoryId = 8,
            CategoryName = "Đồ Uống, Trà & Cà Phê",
            Name = "Cà Phê Muối Xứ Huế Truyền Thống",
            Price = 35000,
            Description = "Cà phê phin nguyên chất Robusta đậm đặc kết hợp lớp kem sữa muối béo mặn sánh mịn đặc trưng vùng đất Cố Đô.",
            ImageUrl = "images/dishes/ca-phe-muoi.jpg",
            IsAvailable = true,
            Unit = "Ly"
        },
        new MenuItem
        {
            MenuItemId = 34,
            CategoryId = 8,
            CategoryName = "Đồ Uống, Trà & Cà Phê",
            Name = "Sinh Tố Bơ Sáp Đắk Lắk Cốt Dừa",
            Price = 42000,
            Description = "Bơ sáp dẻo bùi béo tự nhiên xay nhuyễn cùng sữa đặc, sữa tươi không đường và nước cốt dừa ngậy béo thơm lừng.",
            ImageUrl = "images/dishes/nuoc-mia-quat.jpg",
            IsAvailable = true,
            Unit = "Ly"
        }
    };

    public Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync(int? categoryId = null, string? search = null)
    {
        var query = _items.AsEnumerable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     i.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<MenuItem>>(query.ToList());
    }

    public Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
    {
        return Task.FromResult(_items.FirstOrDefault(i => i.MenuItemId == menuItemId));
    }

    public Task<int> AddMenuItemAsync(MenuItem item)
    {
        int newId = _items.Any() ? _items.Max(i => i.MenuItemId) + 1 : 1;
        item.MenuItemId = newId;
        _items.Add(item);
        return Task.FromResult(newId);
    }

    public Task UpdateMenuItemAsync(MenuItem item)
    {
        var existing = _items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
        if (existing != null)
        {
            existing.Name = item.Name;
            existing.CategoryId = item.CategoryId;
            existing.CategoryName = item.CategoryName;
            existing.Price = item.Price;
            existing.Description = item.Description;
            existing.ImageUrl = item.ImageUrl;
            existing.IsAvailable = item.IsAvailable;
            existing.Unit = item.Unit;
        }
        return Task.CompletedTask;
    }

    public Task ToggleAvailabilityAsync(int menuItemId, bool isAvailable)
    {
        var existing = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existing != null)
        {
            existing.IsAvailable = isAvailable;
        }
        return Task.CompletedTask;
    }

    public Task DeleteMenuItemAsync(int menuItemId)
    {
        var existing = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existing != null)
        {
            _items.Remove(existing);
        }
        return Task.CompletedTask;
    }
}
