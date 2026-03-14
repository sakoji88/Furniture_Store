using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
