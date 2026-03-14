using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.ViewModels;

public class ProductFilterViewModel
{
    [StringLength(120)]
    public string? Search { get; set; }

    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    [Range(0, 10_000_000)]
    [Display(Name = "Цена от")]
    public decimal? MinPrice { get; set; }

    [Range(0, 10_000_000)]
    [Display(Name = "Цена до")]
    public decimal? MaxPrice { get; set; }

    [Display(Name = "Только в наличии")]
    public bool InStockOnly { get; set; }

    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";

    [Range(1, 500)]
    public int Page { get; set; } = 1;

    public int TotalPages { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}
