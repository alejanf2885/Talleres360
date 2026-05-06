using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Models.Flota;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Citas;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class CitasController : Controller
{
    private readonly CitaService    _citaService;
    private readonly VehiculoService _vehiculoService;

    public CitasController(CitaService citaService, VehiculoService vehiculoService)
    {
        _citaService     = citaService;
        _vehiculoService = vehiculoService;
    }

    // GET /citas
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int pageNumber = 1, int pageSize = 15, string? estado = null)
    {
        ApiResult<PagedResponse<CitaDto>> result =
            await _citaService.ListarAsync(pageNumber, pageSize, estado);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<CitaDto> paged = result.Data ?? new PagedResponse<CitaDto>();

        ViewData["EstadoFiltro"]   = estado;
        ViewData["PageNumber"]     = paged.PageNumber;
        ViewData["TotalPages"]     = paged.TotalPages;
        ViewData["TotalCount"]     = paged.TotalCount;
        ViewData["HasPreviousPage"]= paged.HasPreviousPage;
        ViewData["HasNextPage"]    = paged.HasNextPage;

        return View(paged);
    }

    // GET /citas/crear
    [HttpGet("Crear")]
    public async Task<IActionResult> Crear()
    {
        await CargarVehiculosAsync();
        return View(new CitaFormModel());
    }

    // POST /citas/crear
    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CitaFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarVehiculosAsync();
            return View(model);
        }

        ApiResult<CitaDto> result = await _citaService.CrearAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al crear la cita.");
            await CargarVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Cita creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET /citas/{id}/editar
    [HttpGet("{id:int}/editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<CitaDto> result = await _citaService.ObtenerAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = "Cita no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        await CargarVehiculosAsync();
        return View(CitaService.FromDto(result.Data!));
    }

    // POST /citas/{id}/editar
    [HttpPost("{id:int}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CitaFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarVehiculosAsync();
            return View(model);
        }

        ApiResult<CitaDto> result = await _citaService.EditarAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al actualizar la cita.");
            await CargarVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Cita actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST /citas/{id}/eliminar
    [HttpPost("{id:int}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _citaService.EliminarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar la cita.";
        else
            TempData["Success"] = "Cita eliminada.";

        return RedirectToAction(nameof(Index));
    }

    // POST /citas/{id}/convertir
    [HttpPost("{id:int}/convertir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convertir(int id)
    {
        ApiResult<object> result = await _citaService.ConvertirATrabajoAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo convertir la cita a trabajo.";
        else
            TempData["Success"] = "Cita convertida a orden de trabajo.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarVehiculosAsync()
    {
        ApiResult<PagedResponse<VehiculoDetalle>> result =
            await _vehiculoService.ListarAsync(1, 500, null);
        ViewData["Vehiculos"] = result.Data?.Data ?? new List<VehiculoDetalle>();
    }
}
