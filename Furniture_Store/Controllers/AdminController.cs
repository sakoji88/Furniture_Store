using Furniture_Store.Data;
using Furniture_Store.Models;
using Furniture_Store.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.UsersCount = await dbContext.Users.CountAsync();
        ViewBag.ProductsCount = await dbContext.Products.CountAsync();
        ViewBag.CategoriesCount = await dbContext.Categories.CountAsync();
        ViewBag.BannedUsersCount = await dbContext.Users.CountAsync(u => u.IsBanned);
        return View();
    }

    public async Task<IActionResult> Users(string? search)
    {
        var query = dbContext.Users.Include(u => u.Role).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(normalized) || u.Email.ToLower().Contains(normalized));
        }

        var users = await query.OrderBy(u => u.FullName)
            .Select(u => new UserManagementViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role!.Name,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            TempData["Error"] = "Пользователь не найден.";
            return RedirectToAction(nameof(Users));
        }

        user.IsBanned = true;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Пользователь заблокирован.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unban(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            TempData["Error"] = "Пользователь не найден.";
            return RedirectToAction(nameof(Users));
        }

        user.IsBanned = false;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Пользователь разблокирован.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        if (user is null || adminRole is null)
        {
            TempData["Error"] = "Не удалось изменить роль пользователя.";
            return RedirectToAction(nameof(Users));
        }

        user.RoleId = adminRole.Id;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Пользователь назначен администратором.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Tables()
    {
        ViewBag.RecentProducts = await dbContext.Products.Include(p => p.Category).OrderByDescending(p => p.Id).Take(10).ToListAsync();
        ViewBag.Categories = await dbContext.Categories.OrderBy(c => c.Name).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Orders()
    {
        var orders = await dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var order = await dbContext.Orders.FindAsync(orderId);
        if (order is null)
        {
            TempData["Error"] = "Заказ не найден.";
            return RedirectToAction(nameof(Orders));
        }

        order.Status = status;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Статус заказа обновлён.";
        return RedirectToAction(nameof(Orders));
    }
}
