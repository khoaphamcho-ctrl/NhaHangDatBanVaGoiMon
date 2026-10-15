using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace Plugins.DataStore.InMemory;

public class RestaurantConfigInMemoryRepository : IRestaurantConfigRepository
{
    private static readonly Dictionary<string, string> _configs = new()
    {
        { "ServiceFeePercent", "5" }, // 5% phí phục vụ
        { "RestaurantName", "Nhà Hàng Cố Đô Huế - Hoàng Cung" },
        { "Hotline", "0905.888.999" },
        { "Address", "68 Lê Lợi, TP. Huế" },
        { "OpeningHours", "09:00 - 22:30" }
    };

    public Task<string> GetConfigValueAsync(string key, string defaultValue)
    {
        if (_configs.TryGetValue(key, out var val))
        {
            return Task.FromResult(val);
        }
        return Task.FromResult(defaultValue);
    }

    public Task SetConfigValueAsync(string key, string value)
    {
        _configs[key] = value;
        return Task.CompletedTask;
    }

    public Task<decimal> GetServiceFeeRateAsync()
    {
        if (_configs.TryGetValue("ServiceFeePercent", out var val) && decimal.TryParse(val, out var rate))
        {
            return Task.FromResult(rate);
        }
        return Task.FromResult(5.0m);
    }
}

public class UserInMemoryRepository : IUserRepository
{
    private static readonly List<UserAccount> _users = new()
    {
        new UserAccount { UserId = 1, Username = "admin", PasswordHash = "admin123", FullName = "Quản Lý Nhà Hàng", Role = "Admin" },
        new UserAccount { UserId = 2, Username = "staff", PasswordHash = "staff123", FullName = "Nhân Viên Thu Ngân & Phục Vụ", Role = "Staff" },
        new UserAccount { UserId = 3, Username = "khach", PasswordHash = "khach123", FullName = "Khách Hàng Thân Thiết", Role = "Customer" }
    };

    public Task<UserAccount?> AuthenticateAsync(string username, string password)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            u.PasswordHash == password);

        return Task.FromResult(user);
    }

    public Task<UserAccount?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }
}
