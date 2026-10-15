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
        new MenuCategory { CategoryId = 1, Name = "Bánh Cố Đô Huế", DisplayOrder = 1, Description = "Các món bánh bèo, nậm, lọc đặc trưng truyền thống", Icon = "bi-disc" },
        new MenuCategory { CategoryId = 2, Name = "Món Chính & Đặc Sản", DisplayOrder = 2, Description = "Bún bò Huế, Cơm hến, Nem lụi, Vịt nướng", Icon = "bi-fire" },
        new MenuCategory { CategoryId = 3, Name = "Khai Vị & Gỏi", DisplayOrder = 3, Description = "Gỏi vả tôm thịt, Cuốn diếp, Chả tôm", Icon = "bi-egg-fried" },
        new MenuCategory { CategoryId = 4, Name = "Tráng Miệng & Chè Cung Đình", DisplayOrder = 4, Description = "Chè hạt sen long nhãn, chè bột lọc bọc heo quay", Icon = "bi-flower1" },
        new MenuCategory { CategoryId = 5, Name = "Đồ Uống & Trà Cung Đình", DisplayOrder = 5, Description = "Trà Cung Đình Huế, Nước mát, Nước ép tươi", Icon = "bi-cup-straw" }
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
