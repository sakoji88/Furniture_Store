using System.Security.Claims;
using Furniture_Store.Services;
using Furniture_Store.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Furniture_Store.Controllers;

[Authorize]
public class CartController(ICartService cartService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var items = await cartService.GetUserCartAsync(userId);

        var model = new CartPageViewModel
        {
            Items = items.Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Товар",
                UnitPrice = i.Product?.Price ?? 0,
                Quantity = i.Quantity,
                ImageUrl = i.Product?.ImageUrl
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        await cartService.AddToCartAsync(GetUserId(), productId, quantity);
        TempData["Success"] = "Товар добавлен в корзину.";
        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await cartService.UpdateQuantityAsync(GetUserId(), productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await cartService.RemoveFromCartAsync(GetUserId(), productId);
        TempData["Success"] = "Товар удалён из корзины.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CartPageViewModel model)
    {
        model.RecipientName = model.RecipientName?.Trim() ?? string.Empty;
        model.Phone = model.Phone?.Trim() ?? string.Empty;
        model.DeliveryAddress = model.DeliveryAddress?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            var items = await cartService.GetUserCartAsync(GetUserId());
            model.Items = items.Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Товар",
                UnitPrice = i.Product?.Price ?? 0,
                Quantity = i.Quantity,
                ImageUrl = i.Product?.ImageUrl
            }).ToList();
            return View("Index", model);
        }

        var order = await cartService.CheckoutAsync(GetUserId(), model.RecipientName, model.Phone, model.DeliveryAddress);
        if (order is null)
        {
            TempData["Error"] = "Не удалось оформить заказ. Проверьте корзину и наличие товара.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Заказ №{order.Id} успешно оформлен.";
        return RedirectToAction("Index", "Profile");
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
