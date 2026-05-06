using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Auth;
using Talleres360_front.Models.Auth;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // GET /auth/login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequest());
    }

    // POST /auth/login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        (bool success, string? error) = await _authService.LoginAsync(HttpContext, model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al iniciar sesión.");
            return View(model);
        }

        if (!AuthService.EsPerfilConfigurado(HttpContext))
            return RedirectToAction("Setup", "Taller");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // GET /auth/register
    [HttpGet]
    public IActionResult Register()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    // POST /auth/register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        (bool success, string? error) = await _authService.RegisterAsync(model.ToRequest());

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al registrar el taller.");
            return View(model);
        }

        TempData["Success"] = "Taller registrado correctamente. Revisa tu correo para activar la cuenta.";
        return RedirectToAction(nameof(VerificacionPendiente));
    }

    // GET /auth/verificacion-pendiente
    [HttpGet]
    public IActionResult VerificacionPendiente()
    {
        return View();
    }

    // POST /auth/logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}
