using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Furniture_Store.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Article { get; set; } = string.Empty;

    [Range(0.01, 10_000_000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, 100_000)]
    public int QuantityInStock { get; set; }

    [StringLength(80)]
    public string? Material { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public bool IsArchived { get; set; }
}
