using Furniture_Store.Data;
using Furniture_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Furniture_Store.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index() => View(await dbContext.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        model.Name = model.Name.Trim();
        model.Description = model.Description?.Trim();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            dbContext.Categories.Add(model);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить категорию. Проверьте корректность данных и длину текста.");
            return View(model);
        }

        TempData["Success"] = "Категория добавлена.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        model.Name = model.Name.Trim();
        model.Description = model.Description?.Trim();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            dbContext.Categories.Update(model);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Не удалось обновить категорию. Проверьте корректность введённых данных.");
            return View(model);
        }

        TempData["Success"] = "Категория обновлена.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category is not null)
        {
            try
            {
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
                TempData["Success"] = "Категория удалена.";
            }
            catch
            {
                TempData["Error"] = "Категорию нельзя удалить, пока к ней привязаны товары.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
