using Furniture_Store.Models;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Data;

// Главный DbContext: описывает таблицы и ограничения БД для учебного проекта.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.Article).IsUnique();

        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(150);
        modelBuilder.Entity<User>().Property(u => u.FullName).HasMaxLength(100);

        modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
        modelBuilder.Entity<Category>().Property(c => c.Description).HasMaxLength(300);

        modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(120);
        modelBuilder.Entity<Product>().Property(p => p.Article).HasMaxLength(50);
        modelBuilder.Entity<Product>().Property(p => p.Material).HasMaxLength(80);
        modelBuilder.Entity<Product>().Property(p => p.Color).HasMaxLength(50);
        modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(1000);
        modelBuilder.Entity<Product>().Property(p => p.ImageUrl).HasMaxLength(250);

        modelBuilder.Entity<CartItem>().HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();

        modelBuilder.Entity<Order>().Property(o => o.DeliveryAddress).HasMaxLength(200);
        modelBuilder.Entity<Order>().Property(o => o.RecipientName).HasMaxLength(100);
        modelBuilder.Entity<Order>().Property(o => o.Phone).HasMaxLength(20);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
