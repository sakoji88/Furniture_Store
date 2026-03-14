using System.Security.Claims;
using Furniture_Store.Data;
using Furniture_Store.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Controllers;

[Authorize]
public class ProfileController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            TempData["Error"] = "Пользователь не найден.";
            return RedirectToAction("Login", "Account");
        }

        var orders = await dbContext.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Orders = orders
        });
    }
}
