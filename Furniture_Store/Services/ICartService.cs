using Furniture_Store.Models;

namespace Furniture_Store.Services;

public interface ICartService
{
    Task AddToCartAsync(int userId, int productId, int quantity);
    Task<IReadOnlyList<CartItem>> GetUserCartAsync(int userId);
    Task UpdateQuantityAsync(int userId, int productId, int quantity);
    Task RemoveFromCartAsync(int userId, int productId);
    Task<Order?> CheckoutAsync(int userId, string recipientName, string phone, string address);
}
