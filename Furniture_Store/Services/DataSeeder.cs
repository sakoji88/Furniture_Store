using Furniture_Store.Data;
using Furniture_Store.Models;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(new Role { Name = "User" }, new Role { Name = "Admin" });
            await context.SaveChangesAsync();
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

        if (!await context.Users.AnyAsync(u => u.Email == "admin@furniturestore.local"))
        {
            context.Users.Add(new User
            {
                FullName = "Главный администратор",
                Email = "admin@furniturestore.local",
                PasswordHash = hasher.HashPassword("Admin123!"),
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Диваны", Description = "Мягкая мебель для гостиной." },
                new Category { Name = "Столы", Description = "Обеденные, журнальные и рабочие столы." },
                new Category { Name = "Шкафы", Description = "Шкафы для хранения одежды и вещей." }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            var divan = await context.Categories.FirstAsync(c => c.Name == "Диваны");
            var table = await context.Categories.FirstAsync(c => c.Name == "Столы");
            var wardrobe = await context.Categories.FirstAsync(c => c.Name == "Шкафы");

            context.Products.AddRange(
                new Product
                {
                    Name = "Диван Нордик",
                    Article = "DVN-001",
                    Price = 45990,
                    QuantityInStock = 8,
                    Material = "Ткань",
                    Color = "Серый",
                    Description = "Удобный трехместный диван в скандинавском стиле.",
                    CategoryId = divan.Id
                },
                new Product
                {
                    Name = "Стол Loft Oak",
                    Article = "TBL-105",
                    Price = 19990,
                    QuantityInStock = 12,
                    Material = "Массив дуба",
                    Color = "Натуральный",
                    Description = "Обеденный стол на 6 персон.",
                    CategoryId = table.Id
                },
                new Product
                {
                    Name = "Шкаф Classic 3D",
                    Article = "WRD-333",
                    Price = 38990,
                    QuantityInStock = 5,
                    Material = "ЛДСП",
                    Color = "Белый",
                    Description = "Трехдверный шкаф с зеркалом.",
                    CategoryId = wardrobe.Id
                });

            await context.SaveChangesAsync();
        }
    }
}
