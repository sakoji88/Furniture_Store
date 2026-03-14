using Furniture_Store.Data;
using Furniture_Store.Models;
using Furniture_Store.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Repositories;

public class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<(List<Product> Products, int TotalCount)> GetFilteredAsync(ProductFilterViewModel filter, int pageSize)
    {
        var query = dbContext.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search)
                                     || p.Article.ToLower().Contains(search)
                                     || (p.Material != null && p.Material.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.InStockOnly)
            query = query.Where(p => p.QuantityInStock > 0);

        query = (filter.SortBy.ToLower(), filter.SortDirection.ToLower()) switch
        {
            ("price", "desc") => query.OrderByDescending(p => p.Price),
            ("price", _) => query.OrderBy(p => p.Price),
            ("category", "desc") => query.OrderByDescending(p => p.Category!.Name),
            ("category", _) => query.OrderBy(p => p.Category!.Name),
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        var products = await query.Skip((filter.Page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (products, totalCount);
    }

    public Task<Product?> GetByIdAsync(int id) => dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> ArticleExistsAsync(string article, int? excludedProductId = null)
    {
        var query = dbContext.Products.AsQueryable();
        if (excludedProductId.HasValue)
            query = query.Where(p => p.Id != excludedProductId.Value);
        return query.AnyAsync(p => p.Article == article);
    }
}
