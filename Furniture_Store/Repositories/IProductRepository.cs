using Furniture_Store.Models;
using Furniture_Store.ViewModels;

namespace Furniture_Store.Repositories;

public interface IProductRepository
{
    Task<(List<Product> Products, int TotalCount)> GetFilteredAsync(ProductFilterViewModel filter, int pageSize);
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ArticleExistsAsync(string article, int? excludedProductId = null);
}
