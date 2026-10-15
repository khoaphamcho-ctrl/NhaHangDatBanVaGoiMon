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
        // 1. Bánh Cố Đô
        new MenuItem
        {
            MenuItemId = 1,
            CategoryId = 1,
            CategoryName = "Bánh Cố Đô Huế",
            Name = "Mẹt Bánh Bèo Chén Huế (10 chén)",
            Price = 65000,
            Description = "Bánh bèo bột gạo mềm mịn, tôm chấy đỏ cam thơm lừng, tóp mỡ giòn rụm và nước mắm ớt xanh ngọt cay.",
            ImageUrl = "https://images.unsplash.com/photo-1541544741938-0af808871cc0?w=600&auto=format&fit=crop",
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
            ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop",
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
            ImageUrl = "https://images.unsplash.com/photo-1509722747041-616f39b57569?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Dĩa"
        },

        // 2. Món Chính & Đặc Sản
        new MenuItem
        {
            MenuItemId = 4,
            CategoryId = 2,
            CategoryName = "Món Chính & Đặc Sản",
            Name = "Bún Bò Huế Đặc Biệt (Càng Cua + Bắp Bò)",
            Price = 85000,
            Description = "Nước dùng hầm xương bò 12 tiếng cùng sả và mắm ruốc Cố đô, bắp hoa bò mềm ngọt, chả cua Huế tươi ngon.",
            ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Tô"
        },
        new MenuItem
        {
            MenuItemId = 5,
            CategoryId = 2,
            CategoryName = "Món Chính & Đặc Sản",
            Name = "Cơm Hến Cồn Hến Cố Đô",
            Price = 55000,
            Description = "Cơm nguội kèm hến xào đậm đà, tóp mỡ chiên giòn, bắp chuối thái mỏng, ruốc thơm và ớt xào Huế.",
            ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Phần"
        },
        new MenuItem
        {
            MenuItemId = 6,
            CategoryId = 2,
            CategoryName = "Món Chính & Đặc Sản",
            Name = "Mẹt Nem Lụi Nướng Than Hoa (8 cây)",
            Price = 95000,
            Description = "Nem lụi quấn sả nướng vàng óng, ăn kèm rau sống vườn Huế, bánh tráng và nước chấm tương gan đậu béo ngậy.",
            ImageUrl = "https://images.unsplash.com/photo-1529042410759-befb1204b468?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Mẹt"
        },
        new MenuItem
        {
            MenuItemId = 7,
            CategoryId = 2,
            CategoryName = "Món Chính & Đặc Sản",
            Name = "Cá Bống Thệ Kho Tộ Niêu Đất",
            Price = 120000,
            Description = "Cá bống thệ sông Hương kho tiêu cay nồng trong tộ đất, thịt chắc bùi béo ngậy.",
            ImageUrl = "https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=600&auto=format&fit=crop",
            IsAvailable = false, // Phục vụ kịch bản demo món hết hàng! (Luật 2.2)
            Unit = "Tộ"
        },

        // 3. Khai vị & Gỏi
        new MenuItem
        {
            MenuItemId = 8,
            CategoryId = 3,
            CategoryName = "Khai Vị & Gỏi",
            Name = "Gỏi Vả Trộn Tôm Thịt Bánh Tráng Mè",
            Price = 75000,
            Description = "Trái vả non luộc chín thái mỏng trộn cùng tôm sông, thịt ba rọi và rau răm, xúc bánh tráng nướng giòn.",
            ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Đĩa"
        },

        // 4. Tráng Miệng & Chè Cung Đình
        new MenuItem
        {
            MenuItemId = 9,
            CategoryId = 4,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Chè Hạt Sen Long Nhãn Cung Đình",
            Price = 40000,
            Description = "Hạt sen hồ Tịnh Tâm bùi dẻo bọc trong cùi long nhãn giòn ngọt, nước đường phèn thanh mát.",
            ImageUrl = "https://images.unsplash.com/photo-1505253758473-96b3d5ebcd96?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Chén"
        },
        new MenuItem
        {
            MenuItemId = 10,
            CategoryId = 4,
            CategoryName = "Tráng Miệng & Chè Cung Đình",
            Name = "Chè Bột Lọc Heo Quay Đặc Sản",
            Price = 45000,
            Description = "Món chè độc nhất vô nhị xứ Huế, vị ngọt thanh của nước đường gừng kết hợp vị béo mặn của thịt heo quay giòn bì.",
            ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Chén"
        },

        // 5. Đồ Uống & Trà Cung Đình
        new MenuItem
        {
            MenuItemId = 11,
            CategoryId = 5,
            CategoryName = "Đồ Uống & Trà Cung Đình",
            Name = "Ấm Trà Cung Đình Nhất Dạ Đế Vương",
            Price = 60000,
            Description = "Ấm trà thảo mộc 16 vị quý (atiso, cúc hoa, đẳng sâm, cỏ ngọt, tim sen...) bồi bổ sức khỏe và thanh lọc cơ thể.",
            ImageUrl = "https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=600&auto=format&fit=crop",
            IsAvailable = true,
            Unit = "Ấm"
        },
        new MenuItem
        {
            MenuItemId = 12,
            CategoryId = 5,
            CategoryName = "Đồ Uống & Trà Cung Đình",
            Name = "Nước Mía Ép Quất Huế Ướp Lạnh",
            Price = 25000,
            Description = "Mía ngọt thanh kết hợp vị chua dịu thơm mát của tắc tươi.",
            ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=600&auto=format&fit=crop",
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
