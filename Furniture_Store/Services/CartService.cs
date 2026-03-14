using Furniture_Store.Data;
using Furniture_Store.Models;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Services;

public class CartService(ApplicationDbContext dbContext) : ICartService
{
    public async Task AddToCartAsync(int userId, int productId, int quantity)
    {
        quantity = Math.Clamp(quantity, 1, 100);
        var existing = await dbContext.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

        if (existing is null)
        {
            dbContext.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
        }
        else
        {
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, 100);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CartItem>> GetUserCartAsync(int userId)
    {
        return await dbContext.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.AddedAt)
            .ToListAsync();
    }

    public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var item = await dbContext.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (item is null) return;

        item.Quantity = Math.Clamp(quantity, 1, 100);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(int userId, int productId)
    {
        var item = await dbContext.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (item is null) return;

        dbContext.CartItems.Remove(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Order?> CheckoutAsync(int userId, string recipientName, string phone, string address)
    {
        var cartItems = await dbContext.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            RecipientName = recipientName.Trim(),
            Phone = phone.Trim(),
            DeliveryAddress = address.Trim(),
            Status = OrderStatus.Pending
        };

        foreach (var item in cartItems)
        {
            if (item.Product is null || item.Product.QuantityInStock < item.Quantity)
            {
                continue;
            }

            item.Product.QuantityInStock -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            });
        }

        if (!order.Items.Any())
        {
            return null;
        }

        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        dbContext.Orders.Add(order);
        dbContext.CartItems.RemoveRange(cartItems);
        await dbContext.SaveChangesAsync();

        return order;
    }
}
