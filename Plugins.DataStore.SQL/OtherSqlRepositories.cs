using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBusiness.Models;
using Dapper;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.SQL;

public class MenuCategorySqlRepository : IMenuCategoryRepository
{
    private readonly SqlDataAccess _db;

    public MenuCategorySqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MenuCategory>> GetAllCategoriesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT * FROM dbo.MenuCategories ORDER BY DisplayOrder, CategoryId";
        return await conn.QueryAsync<MenuCategory>(sql);
    }

    public async Task<MenuCategory?> GetCategoryByIdAsync(int categoryId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT * FROM dbo.MenuCategories WHERE CategoryId = @CategoryId";
        return await conn.QueryFirstOrDefaultAsync<MenuCategory>(sql, new { CategoryId = categoryId });
    }

    public async Task<int> AddCategoryAsync(MenuCategory category)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.MenuCategories (Name, Description, DisplayOrder, Icon)
            VALUES (@Name, @Description, @DisplayOrder, @Icon);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.Icon
        });
    }

    public async Task UpdateCategoryAsync(MenuCategory category)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE dbo.MenuCategories
            SET Name = @Name,
                Description = @Description,
                DisplayOrder = @DisplayOrder,
                Icon = @Icon
            WHERE CategoryId = @CategoryId";

        await conn.ExecuteAsync(sql, new
        {
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.Icon,
            category.CategoryId
        });
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "DELETE FROM dbo.MenuCategories WHERE CategoryId = @CategoryId";
        await conn.ExecuteAsync(sql, new { CategoryId = categoryId });
    }
}

public class MenuItemSqlRepository : IMenuItemRepository
{
    private readonly SqlDataAccess _db;

    public MenuItemSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync(int? categoryId = null, string? search = null)
    {
        using var conn = _db.CreateConnection();
        // Parameterized dynamic query without SQL concatenation
        string sql = @"
            SELECT m.*, c.Name AS CategoryName
            FROM dbo.MenuItems m
            INNER JOIN dbo.MenuCategories c ON m.CategoryId = c.CategoryId
            WHERE (@CategoryId IS NULL OR m.CategoryId = @CategoryId)
              AND (@SearchPattern IS NULL OR m.Name LIKE @SearchPattern OR m.Description LIKE @SearchPattern)
            ORDER BY m.CategoryId, m.Name";

        string? searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

        return await conn.QueryAsync<MenuItem>(sql, new
        {
            CategoryId = (categoryId.HasValue && categoryId.Value > 0) ? categoryId : null,
            SearchPattern = searchPattern
        });
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT m.*, c.Name AS CategoryName
            FROM dbo.MenuItems m
            INNER JOIN dbo.MenuCategories c ON m.CategoryId = c.CategoryId
            WHERE m.MenuItemId = @MenuItemId";

        return await conn.QueryFirstOrDefaultAsync<MenuItem>(sql, new { MenuItemId = menuItemId });
    }

    public async Task<int> AddMenuItemAsync(MenuItem item)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.MenuItems (CategoryId, Name, Price, Description, ImageUrl, IsAvailable, Unit)
            VALUES (@CategoryId, @Name, @Price, @Description, @ImageUrl, @IsAvailable, @Unit);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            item.CategoryId,
            item.Name,
            item.Price,
            item.Description,
            item.ImageUrl,
            item.IsAvailable,
            item.Unit
        });
    }

    public async Task UpdateMenuItemAsync(MenuItem item)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE dbo.MenuItems
            SET CategoryId = @CategoryId,
                Name = @Name,
                Price = @Price,
                Description = @Description,
                ImageUrl = @ImageUrl,
                IsAvailable = @IsAvailable,
                Unit = @Unit
            WHERE MenuItemId = @MenuItemId";

        await conn.ExecuteAsync(sql, new
        {
            item.CategoryId,
            item.Name,
            item.Price,
            item.Description,
            item.ImageUrl,
            item.IsAvailable,
            item.Unit,
            item.MenuItemId
        });
    }

    public async Task ToggleAvailabilityAsync(int menuItemId, bool isAvailable)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE dbo.MenuItems SET IsAvailable = @IsAvailable WHERE MenuItemId = @MenuItemId";
        await conn.ExecuteAsync(sql, new { IsAvailable = isAvailable, MenuItemId = menuItemId });
    }

    public async Task DeleteMenuItemAsync(int menuItemId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "DELETE FROM dbo.MenuItems WHERE MenuItemId = @MenuItemId";
        await conn.ExecuteAsync(sql, new { MenuItemId = menuItemId });
    }
}

public class RestaurantConfigSqlRepository : IRestaurantConfigRepository
{
    private readonly SqlDataAccess _db;

    public RestaurantConfigSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<string> GetConfigValueAsync(string key, string defaultValue)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT ConfigValue FROM dbo.RestaurantConfigs WHERE ConfigKey = @Key";
        var val = await conn.QueryFirstOrDefaultAsync<string>(sql, new { Key = key });
        return val ?? defaultValue;
    }

    public async Task SetConfigValueAsync(string key, string value)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.RestaurantConfigs WHERE ConfigKey = @Key)
                UPDATE dbo.RestaurantConfigs SET ConfigValue = @Value WHERE ConfigKey = @Key
            ELSE
                INSERT INTO dbo.RestaurantConfigs (ConfigKey, ConfigValue) VALUES (@Key, @Value)";

        await conn.ExecuteAsync(sql, new { Key = key, Value = value });
    }

    public async Task<decimal> GetServiceFeeRateAsync()
    {
        var val = await GetConfigValueAsync("ServiceFeePercent", "5");
        if (decimal.TryParse(val, out var rate))
        {
            return rate;
        }
        return 5.0m;
    }
}

public class UserSqlRepository : IUserRepository
{
    private readonly SqlDataAccess _db;

    public UserSqlRepository(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task<UserAccount?> AuthenticateAsync(string username, string password)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT * FROM dbo.UserAccounts 
            WHERE Username = @Username AND PasswordHash = @Password";

        return await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new
        {
            Username = username,
            Password = password
        });
    }

    public async Task<UserAccount?> GetUserByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT * FROM dbo.UserAccounts WHERE Username = @Username";
        return await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { Username = username });
    }
}
