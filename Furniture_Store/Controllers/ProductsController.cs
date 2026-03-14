using Furniture_Store.Data;
using Furniture_Store.Models;
using Furniture_Store.Repositories;
using Furniture_Store.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Controllers;

public class ProductsController(IProductRepository productRepository, ApplicationDbContext dbContext) : Controller
{
    private const int PageSize = 6;

    public async Task<IActionResult> Index([FromQuery] ProductFilterViewModel filter)
    {
        filter.Search = filter.Search?.Trim();
        filter.SortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "name" : filter.SortBy;
        filter.SortDirection = filter.SortDirection == "desc" ? "desc" : "asc";
        filter.Page = filter.Page < 1 ? 1 : filter.Page;

        var categories = await dbContext.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        filter.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

        var (products, totalCount) = await productRepository.GetFilteredAsync(filter, PageSize);
        filter.TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

        return View(new ProductListPageViewModel { Filter = filter, Products = products });
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            TempData["Error"] = "Товар не найден.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormViewModelAsync(new ProductCreateEditViewModel()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateEditViewModel model)
    {
        NormalizeProductVm(model);

        if (await productRepository.ArticleExistsAsync(model.Article))
            ModelState.AddModelError(nameof(model.Article), "Товар с таким артикулом уже существует.");

        if (!ModelState.IsValid)
            return View(await BuildFormViewModelAsync(model));

        try
        {
            await productRepository.AddAsync(new Product
            {
                Name = model.Name,
                Article = model.Article,
                Price = model.Price,
                QuantityInStock = model.QuantityInStock,
                Material = model.Material,
                Color = model.Color,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                IsArchived = model.IsArchived
            });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить товар. Проверьте поля: длину текста, обязательные значения и уникальность артикула.");
            return View(await BuildFormViewModelAsync(model));
        }

        TempData["Success"] = "Товар успешно добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            TempData["Error"] = "Товар не найден.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ProductCreateEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Article = product.Article,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock,
            Material = product.Material,
            Color = product.Color,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsArchived = product.IsArchived
        };

        return View(await BuildFormViewModelAsync(vm));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductCreateEditViewModel model)
    {
        NormalizeProductVm(model);

        if (await productRepository.ArticleExistsAsync(model.Article, model.Id))
            ModelState.AddModelError(nameof(model.Article), "Товар с таким артикулом уже существует.");

        if (!ModelState.IsValid)
            return View(await BuildFormViewModelAsync(model));

        var product = await productRepository.GetByIdAsync(model.Id);
        if (product is null)
        {
            TempData["Error"] = "Товар не найден.";
            return RedirectToAction(nameof(Index));
        }

        product.Name = model.Name;
        product.Article = model.Article;
        product.Price = model.Price;
        product.QuantityInStock = model.QuantityInStock;
        product.Material = model.Material;
        product.Color = model.Color;
        product.Description = model.Description;
        product.ImageUrl = model.ImageUrl;
        product.CategoryId = model.CategoryId;
        product.IsArchived = model.IsArchived;

        try
        {
            await productRepository.UpdateAsync(product);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Не удалось обновить товар. Проверьте корректность введённых данных.");
            return View(await BuildFormViewModelAsync(model));
        }
        TempData["Success"] = "Товар обновлён.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            TempData["Error"] = "Товар не найден.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            TempData["Error"] = "Товар уже удалён или не существует.";
            return RedirectToAction(nameof(Index));
        }

        await productRepository.DeleteAsync(product);
        TempData["Success"] = "Товар удалён.";
        return RedirectToAction(nameof(Index));
    }

    private static void NormalizeProductVm(ProductCreateEditViewModel model)
    {
        model.Name = model.Name.Trim();
        model.Article = model.Article.Trim().ToUpperInvariant();
        model.Material = model.Material?.Trim();
        model.Color = model.Color?.Trim();
        model.Description = model.Description?.Trim();
        model.ImageUrl = model.ImageUrl?.Trim();
    }

    private async Task<ProductCreateEditViewModel> BuildFormViewModelAsync(ProductCreateEditViewModel vm)
    {
        vm.Categories = await dbContext.Categories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
        return vm;
    }
}
