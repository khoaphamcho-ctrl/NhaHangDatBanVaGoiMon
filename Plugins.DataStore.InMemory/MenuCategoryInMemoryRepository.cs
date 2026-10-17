using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class MenuCategoryInMemoryRepository : IMenuCategoryRepository
{
    private static readonly List<MenuCategory> _categories = new()
    {
        new MenuCategory { CategoryId = 1, Name = "Bánh Cố Đô Huế", DisplayOrder = 1, Description = "Các món bánh bèo, nậm, lọc đặc trưng truyền thống xứ Huế", Icon = "bi-disc" },
        new MenuCategory { CategoryId = 2, Name = "Đặc Sản Huế & Miền Trung", DisplayOrder = 2, Description = "Bún bò Huế, Cơm hến, Nem lụi, Mì Quảng, Bánh xèo miền Trung", Icon = "bi-fire" },
        new MenuCategory { CategoryId = 3, Name = "Món Ngon Bắc - Nam", DisplayOrder = 3, Description = "Phở bò thố đá, Bún chả Hà Nội, Cơm tấm Sài Gòn, Bò lúc lắc", Icon = "bi-stars" },
        new MenuCategory { CategoryId = 4, Name = "Hải Sản & Món Nướng", DisplayOrder = 4, Description = "Tôm sú nướng muối ớt, Mực một nắng sa tế, Cá hồi áp chảo", Icon = "bi-water" },
        new MenuCategory { CategoryId = 5, Name = "Cơm Niêu, Lẩu & Món Gia Đình", DisplayOrder = 5, Description = "Cơm niêu kho quẹt, Lẩu Thái hải sản, Canh chua cá bớp", Icon = "bi-cup-hot-fill" },
        new MenuCategory { CategoryId = 6, Name = "Khai Vị & Gỏi / Salad", DisplayOrder = 6, Description = "Gỏi vả tôm thịt, Salad rong nho mè rang, Nem hải sản chiên xù", Icon = "bi-egg-fried" },
        new MenuCategory { CategoryId = 7, Name = "Tráng Miệng & Chè Cung Đình", DisplayOrder = 7, Description = "Chè hạt sen long nhãn, chè heo quay, Bánh flan caramel cốt dừa, Trái cây", Icon = "bi-flower1" },
        new MenuCategory { CategoryId = 8, Name = "Đồ Uống, Trà & Cà Phê", DisplayOrder = 8, Description = "Trà Cung Đình Huế, Cà phê muối, Trà đào cam sả, Sinh tố bơ", Icon = "bi-cup-straw" }
    };

    public Task<IEnumerable<MenuCategory>> GetAllCategoriesAsync()
    {
        return Task.FromResult<IEnumerable<MenuCategory>>(_categories.OrderBy(c => c.DisplayOrder));
    }

    public Task<MenuCategory?> GetCategoryByIdAsync(int categoryId)
    {
        return Task.FromResult(_categories.FirstOrDefault(c => c.CategoryId == categoryId));
    }

    public Task<int> AddCategoryAsync(MenuCategory category)
    {
        int newId = _categories.Any() ? _categories.Max(c => c.CategoryId) + 1 : 1;
        category.CategoryId = newId;
        _categories.Add(category);
        return Task.FromResult(newId);
    }

    public Task UpdateCategoryAsync(MenuCategory category)
    {
        var existing = _categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
        if (existing != null)
        {
            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.DisplayOrder = category.DisplayOrder;
            existing.Icon = category.Icon;
        }
        return Task.CompletedTask;
    }

    public Task DeleteCategoryAsync(int categoryId)
    {
        var cat = _categories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (cat != null)
        {
            _categories.Remove(cat);
        }
        return Task.CompletedTask;
    }
}
