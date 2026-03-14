using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.Models;

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
