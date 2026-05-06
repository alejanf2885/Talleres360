using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Models.Flota;
using Talleres360.Enums;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Presupuestos;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class PresupuestosController : Controller
{
    private readonly PresupuestoService _presupuestoService;
    private readonly VehiculoService _vehiculoService;
    private readonly TrabajoService _trabajoService;
    private readonly InventarioService _inventarioService;
    private readonly ServicioService _servicioService;

    public PresupuestosController(
        PresupuestoService presupuestoService,
        VehiculoService vehiculoService,
        TrabajoService trabajoService,
        InventarioService inventarioService,
        ServicioService servicioService)
    {
        _presupuestoService = presupuestoService;
        _vehiculoService = vehiculoService;
        _trabajoService = trabajoService;
        _inventarioService = inventarioService;
        _servicioService = servicioService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 15)
    {
        ApiResult<PagedResponse<TrabajoDto>> result = await _presupuestoService.ListarPresupuestosAsync(pageNumber, pageSize);
        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<TrabajoDto> paged = result.Data ?? new PagedResponse<TrabajoDto>();

        ViewData["PageNumber"]  = paged.PageNumber;
        ViewData["TotalPages"]  = paged.TotalPages;
        ViewData["TotalCount"]  = paged.TotalCount;
        ViewData["HasPreviousPage"] = paged.HasPreviousPage;
        ViewData["HasNextPage"]     = paged.HasNextPage;

        return View(paged);
    }

    [HttpGet("Crear")]
    public async Task<IActionResult> Crear(int? vehiculoId)
    {
        await CargarCombosVehiculosAsync();
        var model = new PresupuestoFormModel
        {
            Estado = TrabajoEstado.PRESUPUESTO,
            EstadoPago = TrabajoEstadoPago.PENDIENTE,
            VehiculoId = vehiculoId ?? 0
        };
        return View(model);
    }

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PresupuestoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombosVehiculosAsync();
            return View(model);
        }

        model.Estado = TrabajoEstado.PRESUPUESTO;
        model.EstadoPago = TrabajoEstadoPago.PENDIENTE;

        ApiResult<TrabajoDto> result = await _presupuestoService.CrearPresupuestoAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al crear el presupuesto.");
            await CargarCombosVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Presupuesto creado. Ya puedes añadirle conceptos.";
        return RedirectToAction(nameof(Detalle), new { id = result.Data!.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ApiResult<TrabajoDto> result = await _presupuestoService.ObtenerPresupuestoAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = "Presupuesto no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        // Cargar las líneas (detalles) usando el servicio de trabajos que ya llama a /api/v1/trabajos/{id}/detalles
        ApiResult<List<DetalleTrabajoDto>> lineasResult = await _trabajoService.ObtenerLineasAsync(id);

        // Cargar catálogos
        var productosResult = await _inventarioService.ListarProductosAsync(1, 200);
        var serviciosResult = await _servicioService.ListarServiciosAsync(1, 200);

        ViewData["Lineas"] = lineasResult.Data ?? new List<DetalleTrabajoDto>();
        ViewData["ProductosCatalog"] = productosResult.Data?.Data ?? new List<Talleres360.Dtos.Inventario.ProductoDto>();
        ViewData["ServiciosCatalog"] = serviciosResult.Data?.Data ?? new List<Talleres360.Dtos.Servicios.ServicioDto>();
        ViewData["RechazarForm"] = new RechazarPresupuestoFormModel();
        return View(result.Data!);
    }

    [HttpGet("{id:int}/Editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<TrabajoDto> result = await _presupuestoService.ObtenerPresupuestoAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = "Presupuesto no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (result.Data!.Estado != TrabajoEstado.PRESUPUESTO)
        {
            TempData["Error"] = "Solo se pueden editar presupuestos en estado borrador.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        await CargarCombosVehiculosAsync();
        return View(PresupuestoService.FromDto(result.Data!));
    }

    [HttpPost("{id:int}/Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PresupuestoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombosVehiculosAsync();
            return View(model);
        }

        ApiResult<TrabajoDto> result = await _presupuestoService.EditarPresupuestoAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al actualizar el presupuesto.");
            await CargarCombosVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Presupuesto actualizado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost("{id:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _presupuestoService.EliminarPresupuestoAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el presupuesto.";
        else
            TempData["Success"] = "Presupuesto eliminado.";

        return RedirectToAction(nameof(Index));
    }

    // ── Transiciones de Estado ───────────────────────────────────────────────

    [HttpPost("{id:int}/Enviar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(int id)
    {
        ApiResult<TrabajoDto> result = await _presupuestoService.EnviarPresupuestoAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "Error al enviar el presupuesto.";
        else
            TempData["Success"] = "Presupuesto marcado como enviado.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost("{id:int}/Aceptar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aceptar(int id)
    {
        ApiResult<TrabajoDto> result = await _presupuestoService.AceptarPresupuestoAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error?.Mensaje ?? "Error al aceptar el presupuesto.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        TempData["Success"] = "Presupuesto aceptado y convertido en Orden de Trabajo.";
        // Redirigir al Detalle del Trabajo abierto
        return RedirectToAction("Detalle", "Trabajos", new { id });
    }

    [HttpPost("{id:int}/Rechazar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, RechazarPresupuestoFormModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Debes indicar el motivo de rechazo.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        ApiResult<TrabajoDto> result = await _presupuestoService.RechazarPresupuestoAsync(id, form.MotivoRechazo);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "Error al rechazar el presupuesto.";
        else
            TempData["Success"] = "Presupuesto rechazado.";

        return RedirectToAction(nameof(Detalle), new { id });
    }


    // ── Líneas (Conceptos) ────────────────────────────────────────────────────

    [HttpPost("{id:int}/Lineas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarLinea(int id, Talleres360_front.Models.Trabajos.LineaTrabajoFormModel lineaForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de la línea inválidos. Revisa los campos.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        ApiResult<DetalleTrabajoDto> result = await _trabajoService.AgregarLineaAsync(id, lineaForm);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo agregar el concepto.";
        else
            TempData["Success"] = "Concepto agregado al presupuesto.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost("{id:int}/Lineas/{lineaId:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarLinea(int id, int lineaId)
    {
        ApiResult<bool> result = await _trabajoService.EliminarLineaAsync(id, lineaId);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el concepto.";
        else
            TempData["Success"] = "Concepto eliminado.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    private async Task CargarCombosVehiculosAsync()
    {
        ApiResult<PagedResponse<VehiculoDetalle>> result =
            await _vehiculoService.ListarAsync(1, 200, null);
        List<VehiculoDetalle> vehiculos = result.Data?.Data?.ToList() ?? new List<VehiculoDetalle>();

        ViewData["Vehiculos"] = new SelectList(
            vehiculos.Select(v => new { 
                Id = v.Id, 
                Display = $"{v.Matricula} - {v.MarcaNombre} {v.ModeloNombre}" 
            }),
            "Id",
            "Display"
        );
    }
}
