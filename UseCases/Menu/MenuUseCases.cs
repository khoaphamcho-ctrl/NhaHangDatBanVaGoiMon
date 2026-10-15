using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace UseCases.Menu;

public interface IViewMenuUseCase
{
    Task<IEnumerable<MenuCategory>> GetCategoriesAsync();
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int? categoryId = null, string? search = null);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);
}

public class ViewMenuUseCase : IViewMenuUseCase
{
    private readonly IMenuCategoryRepository _categoryRepo;
    private readonly IMenuItemRepository _menuItemRepo;

    public ViewMenuUseCase(IMenuCategoryRepository categoryRepo, IMenuItemRepository menuItemRepo)
    {
        _categoryRepo = categoryRepo;
        _menuItemRepo = menuItemRepo;
    }

    public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync()
    {
        return await _categoryRepo.GetAllCategoriesAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(int? categoryId = null, string? search = null)
    {
        return await _menuItemRepo.GetAllMenuItemsAsync(categoryId, search);
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
    {
        return await _menuItemRepo.GetMenuItemByIdAsync(menuItemId);
    }
}

public interface IManageMenuUseCase
{
    Task<int> AddMenuItemAsync(MenuItem item);
    Task UpdateMenuItemAsync(MenuItem item);
    Task ToggleAvailabilityAsync(int menuItemId, bool isAvailable);
    Task DeleteMenuItemAsync(int menuItemId);
}

public class ManageMenuUseCase : IManageMenuUseCase
{
    private readonly IMenuItemRepository _menuItemRepo;

    public ManageMenuUseCase(IMenuItemRepository menuItemRepo)
    {
        _menuItemRepo = menuItemRepo;
    }

    public async Task<int> AddMenuItemAsync(MenuItem item)
    {
        return await _menuItemRepo.AddMenuItemAsync(item);
    }

    public async Task UpdateMenuItemAsync(MenuItem item)
    {
        await _menuItemRepo.UpdateMenuItemAsync(item);
    }

    public async Task ToggleAvailabilityAsync(int menuItemId, bool isAvailable)
    {
        await _menuItemRepo.ToggleAvailabilityAsync(menuItemId, isAvailable);
    }

    public async Task DeleteMenuItemAsync(int menuItemId)
    {
        await _menuItemRepo.DeleteMenuItemAsync(menuItemId);
    }
}
