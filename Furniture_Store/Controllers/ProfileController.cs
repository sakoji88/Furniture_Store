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

        var orders = await TryLoadOrdersWithAutoFixAsync(userId);

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Orders = orders
        });
    }

    private async Task<List<Models.Order>> TryLoadOrdersWithAutoFixAsync(int userId)
    {
        try
        {
            return await LoadOrdersAsync(userId);
        }
        catch (InvalidCastException)
        {
            await dbContext.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
                   AND EXISTS (
                        SELECT 1
                        FROM sys.columns c
                        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                        WHERE c.object_id = OBJECT_ID(N'[Orders]')
                          AND c.name = 'Status'
                          AND t.name IN ('varchar', 'nvarchar', 'char', 'nchar')
                   )
                BEGIN
                    IF COL_LENGTH('Orders', 'StatusIntTemp') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [StatusIntTemp] int NOT NULL CONSTRAINT [DF_Orders_StatusIntTemp_Profile] DEFAULT(1);
                    END

                    EXEC(N'UPDATE [Orders]
                        SET [StatusIntTemp] = COALESCE(
                            CASE UPPER(CAST([Status] AS nvarchar(50)))
                                WHEN ''PENDING'' THEN 1
                                WHEN ''PROCESSING'' THEN 2
                                WHEN ''SHIPPED'' THEN 3
                                WHEN ''COMPLETED'' THEN 4
                                WHEN ''CANCELLED'' THEN 5
                                ELSE NULL
                            END,
                            TRY_CAST([Status] AS int),
                            1
                        );');

                    DECLARE @dfName nvarchar(128);
                    SELECT @dfName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.object_id = dc.parent_object_id
                       AND c.column_id = dc.parent_column_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'[Orders]')
                      AND c.name = 'Status';

                    IF @dfName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @dfName + '];');
                    END

                    EXEC(N'ALTER TABLE [Orders] DROP COLUMN [Status];');
                    EXEC sp_rename 'Orders.StatusIntTemp', 'Status', 'COLUMN';
                END
                """);

            return await LoadOrdersAsync(userId);
        }
    }

    private Task<List<Models.Order>> LoadOrdersAsync(int userId)
    {
        return dbContext.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
