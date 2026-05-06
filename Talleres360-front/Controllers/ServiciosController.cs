using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Servicios;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Servicios;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class ServiciosController : Controller
{
    private readonly ServicioService _servicioService;

    public ServiciosController(ServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 15, string? buscar = null)
    {
        ApiResult<PagedResponse<ServicioDto>> result =
            await _servicioService.ListarServiciosAsync(pageNumber, pageSize, buscar);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<ServicioDto> paged = result.Data ?? new PagedResponse<ServicioDto>();

        ViewData["Buscar"]      = buscar;
        ViewData["PageNumber"]  = paged.PageNumber;
        ViewData["TotalPages"]  = paged.TotalPages;
        ViewData["TotalCount"]  = paged.TotalCount;
        ViewData["HasPreviousPage"] = paged.HasPreviousPage;
        ViewData["HasNextPage"]     = paged.HasNextPage;

        return View(paged);
    }

    [HttpGet("Crear")]
    public IActionResult Crear()
    {
        return View(new ServicioFormModel());
    }

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ServicioFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApiResult<ServicioDto> result = await _servicioService.CrearServicioAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al crear el servicio.");
            return View(model);
        }

        TempData["Success"] = "Servicio creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/Editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<ServicioDto> result = await _servicioService.ObtenerServicioAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = "Servicio no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(ServicioService.FromDto(result.Data!));
    }

    [HttpPost("{id:int}/Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ServicioFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApiResult<ServicioDto> result = await _servicioService.EditarServicioAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al actualizar el servicio.");
            return View(model);
        }

        TempData["Success"] = "Servicio actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _servicioService.EliminarServicioAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el servicio.";
        else
            TempData["Success"] = "Servicio eliminado correctamente.";

        return RedirectToAction(nameof(Index));
    }
}
