using Furniture_Store.Data;
using Furniture_Store.Models;
using Microsoft.Data.SqlClient;
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
        await EnsureBackwardCompatibleSchemaAsync(context);

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
                    ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=900&q=80",
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
                    ImageUrl = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=900&q=80",
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
                    ImageUrl = "https://images.unsplash.com/photo-1595526114035-0d45ed16cfbf?auto=format&fit=crop&w=900&q=80",
                    Description = "Трехдверный шкаф с зеркалом.",
                    CategoryId = wardrobe.Id
                });

            await context.SaveChangesAsync();
        }
    }

    // Небольшой защитный слой для учебного проекта: если БД была создана в старой версии,
    // добавляем недостающие колонки, чтобы приложение не падало с ошибкой "Invalid column name".
    private static async Task EnsureBackwardCompatibleSchemaAsync(ApplicationDbContext context)
    {
        if (!await context.Database.CanConnectAsync())
        {
            return;
        }

        const string addMissingColumnsSql = """
            -- Users
            IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
               AND COL_LENGTH('Users', 'IsBanned') IS NULL
            BEGIN
                ALTER TABLE [Users] ADD [IsBanned] bit NOT NULL CONSTRAINT [DF_Users_IsBanned] DEFAULT(0);
            END

            -- Categories
            IF OBJECT_ID(N'[Categories]', N'U') IS NOT NULL
               AND COL_LENGTH('Categories', 'Description') IS NULL
            BEGIN
                ALTER TABLE [Categories] ADD [Description] nvarchar(300) NULL;
            END

            -- Products (для старых схем)
            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'Article') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [Article] nvarchar(50) NOT NULL CONSTRAINT [DF_Products_Article] DEFAULT(N'');
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'QuantityInStock') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [QuantityInStock] int NOT NULL CONSTRAINT [DF_Products_QuantityInStock] DEFAULT(0);
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'Material') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [Material] nvarchar(80) NULL;
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'Color') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [Color] nvarchar(50) NULL;
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'Description') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [Description] nvarchar(1000) NULL;
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'IsArchived') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [IsArchived] bit NOT NULL CONSTRAINT [DF_Products_IsArchived] DEFAULT(0);
            END

            IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
               AND COL_LENGTH('Products', 'ImageUrl') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [ImageUrl] nvarchar(250) NULL;
            END

            IF OBJECT_ID(N'[CartItems]', N'U') IS NULL
            BEGIN
                CREATE TABLE [CartItems](
                    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserId] int NOT NULL,
                    [ProductId] int NOT NULL,
                    [Quantity] int NOT NULL CONSTRAINT [DF_CartItems_Quantity] DEFAULT(1),
                    [AddedAt] datetime2 NOT NULL CONSTRAINT [DF_CartItems_AddedAt] DEFAULT (SYSUTCDATETIME())
                );

                CREATE UNIQUE INDEX [IX_CartItems_UserId_ProductId] ON [CartItems]([UserId],[ProductId]);
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Orders](
                    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserId] int NOT NULL,
                    [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (SYSUTCDATETIME()),
                    [DeliveryAddress] nvarchar(200) NOT NULL CONSTRAINT [DF_Orders_DeliveryAddress] DEFAULT(N''),
                    [RecipientName] nvarchar(100) NOT NULL CONSTRAINT [DF_Orders_RecipientName] DEFAULT(N''),
                    [Phone] nvarchar(20) NOT NULL CONSTRAINT [DF_Orders_Phone] DEFAULT(N''),
                    [TotalAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_TotalAmount] DEFAULT(0),
                    [Status] int NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT(1)
                );
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'CreatedAt') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Orders_CreatedAt_Legacy] DEFAULT (SYSUTCDATETIME());
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'RecipientName') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [RecipientName] nvarchar(100) NOT NULL CONSTRAINT [DF_Orders_RecipientName_Legacy] DEFAULT(N'');
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'Phone') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [Phone] nvarchar(20) NOT NULL CONSTRAINT [DF_Orders_Phone_Legacy] DEFAULT(N'');
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'DeliveryAddress') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [DeliveryAddress] nvarchar(200) NOT NULL CONSTRAINT [DF_Orders_DeliveryAddress_Legacy] DEFAULT(N'');
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'TotalAmount') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [TotalAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_TotalAmount_Legacy] DEFAULT(0);
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND COL_LENGTH('Orders', 'Status') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [Status] int NOT NULL CONSTRAINT [DF_Orders_Status_Legacy] DEFAULT(1);
            END

            -- Если в legacy-схеме Status был строкой, приводим его к int для enum OrderStatus.
            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[Orders]')
                      AND c.name = 'Status'
                      AND t.name IN ('varchar', 'nvarchar', 'char', 'nchar')
               )
               AND COL_LENGTH('Orders', 'StatusIntTemp') IS NULL
            BEGIN
                ALTER TABLE [Orders] ADD [StatusIntTemp] int NOT NULL CONSTRAINT [DF_Orders_StatusIntTemp] DEFAULT(1);
            END

            IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
               AND EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[Orders]')
                      AND c.name = 'Status'
                      AND t.name IN ('varchar', 'nvarchar', 'char', 'nchar')
               )
               AND COL_LENGTH('Orders', 'StatusIntTemp') IS NOT NULL
            BEGIN
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

                EXEC(N'ALTER TABLE [Orders] DROP COLUMN [Status];');
                EXEC sp_rename 'Orders.StatusIntTemp', 'Status', 'COLUMN';
            END

            IF OBJECT_ID(N'[OrderItems]', N'U') IS NULL
            BEGIN
                CREATE TABLE [OrderItems](
                    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [OrderId] int NOT NULL,
                    [ProductId] int NOT NULL,
                    [Quantity] int NOT NULL CONSTRAINT [DF_OrderItems_Quantity] DEFAULT(1),
                    [UnitPrice] decimal(18,2) NOT NULL CONSTRAINT [DF_OrderItems_UnitPrice] DEFAULT(0)
                );
            END
            """;

        try
        {
            await context.Database.ExecuteSqlRawAsync(addMissingColumnsSql);
        }
        catch (SqlException)
        {
            // Если таблица отличается от ожидаемой, оставляем обработку на уровне приложения/логов.
            // Это лучше, чем аварийное завершение старта для учебного демо-проекта.
        }
    }
}
