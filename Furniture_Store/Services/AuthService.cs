using Furniture_Store.Data;
using Furniture_Store.Models;
using Furniture_Store.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Services;

// Сервис регистрации/логина: контроллер оставляем простым, бизнес-логику держим здесь.
public class AuthService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
    {
        var fullName = model.FullName.Trim();
        var email = model.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            return (false, "Поля не могут быть пустыми.");
        }

        if (await dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return (false, "Пользователь с таким email уже существует.");
        }

        var userRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole is null)
        {
            return (false, "Роль User не найдена. Проверьте сидирование БД.");
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHasher.HashPassword(model.Password),
            RoleId = userRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return (true, "Регистрация прошла успешно.");
    }

    public async Task<(bool Success, string Message, int UserId, string Role)> LoginAsync(LoginViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            return (false, "Неверный email или пароль.", 0, string.Empty);
        }

        if (user.IsBanned)
        {
            return (false, "Ваш аккаунт заблокирован. Обратитесь к администратору.", 0, string.Empty);
        }

        return (true, "Вход выполнен.", user.Id, user.Role?.Name ?? "User");
    }
}
