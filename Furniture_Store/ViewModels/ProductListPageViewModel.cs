using Furniture_Store.Models;

namespace Furniture_Store.ViewModels;

public class ProductListPageViewModel
{
    public ProductFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<Product> Products { get; set; } = [];
}
