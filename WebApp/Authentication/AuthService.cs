using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using UseCases.DataStorePluginInterfaces;

namespace WebApp.Authentication;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(IUserRepository userRepo, AuthenticationStateProvider authStateProvider)
    {
        _userRepo = userRepo;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
        }

        var user = await _userRepo.AuthenticateAsync(username.Trim(), password);
        if (user == null)
        {
            return (false, "Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        _authStateProvider.MarkUserAsAuthenticated(user.Username, user.FullName, user.Role);
        return (true, null);
    }

    public Task LogoutAsync()
    {
        _authStateProvider.MarkUserAsLoggedOut();
        return Task.CompletedTask;
    }
}
