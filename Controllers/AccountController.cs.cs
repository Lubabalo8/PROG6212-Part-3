using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceBay.MVC.Models.Dto;
using RaceBay.MVC.Models.ViewModels;
using RaceBay.MVC.Services;
using RaceBay.MVC.Services.Interfaces;

namespace RaceBay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _api;

    public AccountController(IApiService api)
    {
        _api = api;
    }

    // ─────────── LOGIN ───────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToRoleHome();

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var auth = await _api.LoginAsync(new LoginRequestDto
        {
            Email = model.Email,
            Password = model.Password
        });

        if (auth == null || string.IsNullOrEmpty(auth.Token))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Sign in
        await SignInUserAsync(auth);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToRoleHome();
    }

    // ─────────── REGISTER ───────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToRoleHome();

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var auth = await _api.RegisterAsync(new RegisterRequestDto
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role
            });

            if (auth == null)
            {
                ModelState.AddModelError("", "Registration failed. The API returned an empty response.");
                return View(model);
            }

            // If the API returns a token on register, log the user in directly.
            if (!string.IsNullOrEmpty(auth.Token))
            {
                await SignInUserAsync(auth);
                return RedirectToRoleHome();
            }

            // Otherwise send them to login
            TempData["Success"] = "Account created successfully. Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            // THIS WILL SHOW THE EXACT ERROR FROM THE API ON THE SCREEN
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // ─────────── LOGOUT ───────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _api.SetAuthToken(null);
        return RedirectToAction(nameof(Login));
    }

    // ─────────── HELPERS ───────────

    private async Task SignInUserAsync(AuthResponseDto auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Email),
            new(ClaimTypes.Email, auth.Email),
            new(ClaimTypes.Role, auth.Role),          // "Organiser" / "Participant"
            new(ClaimsHelper.Role, auth.Role),
            new(ClaimsHelper.Token, auth.Token),
            new(ClaimsHelper.Email, auth.Email),
            new(ClaimsHelper.UserId, auth.UserId ?? "")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Also set the token on ApiService for the current request context
        _api.SetAuthToken(auth.Token);
    }

    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToRoleHome()
    {
        if (User.IsInRole("Organiser"))
            return RedirectToAction("Index", "Organiser");
        if (User.IsInRole("Participant"))
            return RedirectToAction("Index", "Participant");

        return RedirectToAction("Index", "Home");
    }
}