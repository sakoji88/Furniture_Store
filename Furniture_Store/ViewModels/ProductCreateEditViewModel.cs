using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.ViewModels;

public class ProductCreateEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Article { get; set; } = string.Empty;

    [Range(0.01, 10_000_000)]
    public decimal Price { get; set; }

    [Range(0, 100_000)]
    [Display(Name = "Количество на складе")]
    public int QuantityInStock { get; set; }

    [StringLength(80)]
    public string? Material { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Категория")]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Display(Name = "Архивный товар")]
    public bool IsArchived { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}
