using Microsoft.AspNetCore.Mvc;
using Talleres360_front.Filters;
using Talleres360_front.Models.Taller;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

public class TallerController : Controller
{
    private readonly TallerService _tallerService;

    public TallerController(TallerService tallerService)
    {
        _tallerService = tallerService;
    }

    // GET /taller/setup
    [HttpGet]
    public IActionResult Setup()
    {
        string? jwt = HttpContext.Session.GetString("jwt");
        if (string.IsNullOrEmpty(jwt))
            return RedirectToAction("Login", "Auth");

        if (AuthService.EsPerfilConfigurado(HttpContext))
            return RedirectToAction("Index", "Home");

        return View(new SetupTallerForm());
    }

    // POST /taller/setup
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(SetupTallerForm model)
    {
        string? jwt = HttpContext.Session.GetString("jwt");
        if (string.IsNullOrEmpty(jwt))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        (bool success, string? error) = await _tallerService.ConfigurarPerfilAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al guardar la configuración.");
            return View(model);
        }

        AuthService.MarcarPerfilConfigurado(HttpContext);

        TempData["Success"] = "¡Taller configurado correctamente! Ya puedes empezar a usar Talleres360.";
        return RedirectToAction("Index", "Home");
    }
}
