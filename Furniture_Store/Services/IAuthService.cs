using Furniture_Store.ViewModels;

namespace Furniture_Store.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string Message, int UserId, string Role)> LoginAsync(LoginViewModel model);
}
