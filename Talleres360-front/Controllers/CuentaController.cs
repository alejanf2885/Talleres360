using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Talleres;
using Talleres360_front.Filters;
using Talleres360_front.Models.Cuenta;
using Talleres360_front.Models.Taller;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class CuentaController : Controller
{
    private readonly TallerService _tallerService;

    public CuentaController(TallerService tallerService)
    {
        _tallerService = tallerService;
    }

    [HttpGet("Perfil")]
    public async Task<IActionResult> Perfil()
    {
        WorkshopDto? taller = await _tallerService.ObtenerPerfilAsync();
        if (taller == null)
        {
            TempData["Error"] = "No se pudo cargar el perfil del taller.";
            return RedirectToAction("Index", "Home");
        }

        PerfilTallerForm form = new()
        {
            NombreTaller     = taller.Nombre,
            CIF              = taller.CIF ?? string.Empty,
            Direccion        = taller.Direccion ?? string.Empty,
            Localidad        = taller.Localidad ?? string.Empty,
            Telefono         = taller.Telefono ?? string.Empty,
            LogoActual       = taller.Logo,
            LogoBase64       = taller.Logo,
            TipoSuscripcion  = taller.TipoSuscripcion
        };

        return View(form);
    }

    [HttpPost("Perfil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilTallerForm model)
    {
        if (string.IsNullOrEmpty(model.LogoBase64) && !string.IsNullOrEmpty(model.LogoActual))
            model.LogoBase64 = model.LogoActual;

        if (!ModelState.IsValid)
            return View(model);

        SetupTallerForm form = new()
        {
            CIF        = model.CIF,
            Direccion  = model.Direccion,
            Localidad  = model.Localidad,
            Telefono   = model.Telefono,
            LogoBase64 = model.LogoBase64
        };

        (bool success, string? error) = await _tallerService.ConfigurarPerfilAsync(form);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al guardar el perfil.");
            return View(model);
        }

        TempData["Success"] = "Perfil actualizado correctamente.";
        return RedirectToAction("Perfil");
    }
}
