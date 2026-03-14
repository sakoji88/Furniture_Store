using System.Security.Claims;
using Furniture_Store.Services;
using Furniture_Store.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Furniture_Store.Controllers;

public class AccountController(IAuthService authService) : Controller
{
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.FullName = model.FullName.Trim();
        model.Email = model.Email.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var result = await authService.RegisterAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.Email = model.Email.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var result = await authService.LoginAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // Cookie-аутентификация: после входа записываем claims пользователя.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, model.Email),
            new(ClaimTypes.Role, result.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        TempData["Success"] = "Вы успешно вошли в систему.";
        return RedirectToAction("Index", "Products");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Вы вышли из аккаунта.";
        return RedirectToAction("Index", "Home");
    }
}
