using Furniture_Store.Models;

namespace Furniture_Store.ViewModels;

public class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<Order> Orders { get; set; } = [];
}
