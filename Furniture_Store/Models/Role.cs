using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.Models;

public class Role
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
