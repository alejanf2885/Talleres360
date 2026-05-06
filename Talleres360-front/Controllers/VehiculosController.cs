using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models.Flota;
using Talleres360.Models.Negocio;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Vehiculos;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class VehiculosController : Controller
{
    private readonly VehiculoService _vehiculoService;
    private readonly ClienteService  _clienteService;

    public VehiculosController(VehiculoService vehiculoService, ClienteService clienteService)
    {
        _vehiculoService = vehiculoService;
        _clienteService  = clienteService;
    }

    // GET /vehiculos
    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 15, string? buscar = null)
    {
        ApiResult<PagedResponse<VehiculoDetalle>> result =
            await _vehiculoService.ListarAsync(pageNumber, pageSize, buscar);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<VehiculoDetalle> paged =
            result.Data ?? new PagedResponse<VehiculoDetalle>();

        ViewData["Buscar"]          = buscar;
        ViewData["PageNumber"]      = paged.PageNumber;
        ViewData["TotalPages"]      = paged.TotalPages;
        ViewData["TotalCount"]      = paged.TotalCount;
        ViewData["HasPreviousPage"] = paged.HasPreviousPage;
        ViewData["HasNextPage"]     = paged.HasNextPage;

        return View(paged);
    }

    // GET /vehiculos/crear
    [HttpGet("Crear")]
    public async Task<IActionResult> Crear()
    {
        await CargarCatalogosAsync();
        return View(new VehiculoFormModel());
    }

    // POST /vehiculos/crear
    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(VehiculoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync();
            return View(model);
        }

        ApiResult<VehiculoDetalle> result = await _vehiculoService.CrearAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al registrar el vehículo.");
            await CargarCatalogosAsync();
            return View(model);
        }

        TempData["Success"] = $"Vehículo {result.Data!.Matricula} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET /vehiculos/{id}/editar
    [HttpGet("{id:int}/editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<VehiculoDetalle> result = await _vehiculoService.ObtenerAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = "Vehículo no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        await CargarCatalogosAsync(result.Data!.MarcaId);
        return View(VehiculoService.FromDetalle(result.Data!));
    }

    // POST /vehiculos/{id}/editar
    [HttpPost("{id:int}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, VehiculoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(model.MarcaId);
            return View(model);
        }

        ApiResult<VehiculoDetalle> result = await _vehiculoService.EditarAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al actualizar el vehículo.");
            await CargarCatalogosAsync(model.MarcaId);
            return View(model);
        }

        TempData["Success"] = "Vehículo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST /vehiculos/{id}/eliminar
    [HttpPost("{id:int}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _vehiculoService.EliminarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el vehículo.";
        else
            TempData["Success"] = "Vehículo eliminado.";

        return RedirectToAction(nameof(Index));
    }

    // GET /vehiculos/modelos/{marcaId} — endpoint AJAX para select dinámico
    [HttpGet("Modelos/{marcaId:int}")]
    public async Task<IActionResult> Modelos(int marcaId)
    {
        ApiResult<List<ModeloVehiculoDto>> result =
            await _vehiculoService.ObtenerModelosAsync(marcaId);

        if (!result.Success) return Json(new List<ModeloVehiculoDto>());

        return Json(result.Data);
    }

    private async Task CargarCatalogosAsync(int marcaIdSeleccionada = 0)
    {
        ApiResult<List<VehiculoTipoDto>> tipos    = await _vehiculoService.ObtenerTiposAsync();
        ApiResult<List<MarcaVehiculoDto>> marcas  = await _vehiculoService.ObtenerMarcasAsync();
        ApiResult<PagedResponse<Cliente>> clientes = await _clienteService.ListarAsync(1, 200, null);

        ViewData["Tipos"]    = tipos.Data    ?? new List<VehiculoTipoDto>();
        ViewData["Marcas"]   = marcas.Data   ?? new List<MarcaVehiculoDto>();
        ViewData["Clientes"] = clientes.Data?.Data ?? new List<Cliente>();

        if (marcaIdSeleccionada > 0)
        {
            ApiResult<List<ModeloVehiculoDto>> modelos =
                await _vehiculoService.ObtenerModelosAsync(marcaIdSeleccionada);
            ViewData["Modelos"] = modelos.Data ?? new List<ModeloVehiculoDto>();
        }
        else
        {
            ViewData["Modelos"] = new List<ModeloVehiculoDto>();
        }
    }
}
