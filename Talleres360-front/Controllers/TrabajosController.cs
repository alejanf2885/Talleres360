using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Models.Flota;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Trabajos;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class TrabajosController : Controller
{
    private readonly TrabajoService  _trabajoService;
    private readonly VehiculoService _vehiculoService;
    private readonly InventarioService _inventarioService;
    private readonly ServicioService _servicioService;

    public TrabajosController(
        TrabajoService trabajoService, 
        VehiculoService vehiculoService,
        InventarioService inventarioService,
        ServicioService servicioService)
    {
        _trabajoService  = trabajoService;
        _vehiculoService = vehiculoService;
        _inventarioService = inventarioService;
        _servicioService = servicioService;
    }

    // ── Index ─────────────────────────────────────────────────────────────────

    // GET /trabajos
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int pageNumber = 1, int pageSize = 15,
        string? estado = null, string? estadoPago = null)
    {
        ApiResult<PagedResponse<TrabajoDto>> result =
            await _trabajoService.ListarAsync(pageNumber, pageSize, estado, estadoPago);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<TrabajoDto> paged = result.Data ?? new PagedResponse<TrabajoDto>();

        ViewData["EstadoFiltro"]     = estado;
        ViewData["EstadoPagoFiltro"] = estadoPago;
        ViewData["PageNumber"]       = paged.PageNumber;
        ViewData["TotalPages"]       = paged.TotalPages;
        ViewData["TotalCount"]       = paged.TotalCount;
        ViewData["HasPreviousPage"]  = paged.HasPreviousPage;
        ViewData["HasNextPage"]      = paged.HasNextPage;

        return View(paged);
    }

    // ── Detalle ───────────────────────────────────────────────────────────────

    // GET /trabajos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ApiResult<TrabajoDto> trabajo = await _trabajoService.ObtenerAsync(id);
        if (!trabajo.Success)
        {
            TempData["Error"] = trabajo.Error?.Mensaje ?? "Trabajo no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        ApiResult<List<DetalleTrabajoDto>> lineas = await _trabajoService.ObtenerLineasAsync(id);
        ApiResult<PagedResponse<CobroTrabajoDto>> cobros = await _trabajoService.ObtenerCobrosAsync(id);

        var productosResult = await _inventarioService.ListarProductosAsync(1, 200);
        var serviciosResult = await _servicioService.ListarServiciosAsync(1, 200);

        ViewData["Lineas"]     = lineas.Data ?? new List<DetalleTrabajoDto>();
        ViewData["Cobros"]     = cobros.Data?.Data?.ToList() ?? new List<CobroTrabajoDto>();
        ViewData["ProductosCatalog"] = productosResult.Data?.Data ?? new List<Talleres360.Dtos.Inventario.ProductoDto>();
        ViewData["ServiciosCatalog"] = serviciosResult.Data?.Data ?? new List<Talleres360.Dtos.Servicios.ServicioDto>();
        ViewData["LineaForm"]  = new LineaTrabajoFormModel();
        ViewData["CobroForm"]  = new CobroFormModel();

        return View(trabajo.Data);
    }

    [HttpPost("{id:int}/Facturar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Facturar(int id)
    {
        ApiResult<TrabajoDto> result = await _trabajoService.FacturarAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo facturar el trabajo.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        TempData["Success"] = "Trabajo facturado correctamente.";
        return RedirectToAction("Trabajo", "Facturas", new { trabajoId = id });
    }

    // ── Crear ─────────────────────────────────────────────────────────────────

    // GET /trabajos/crear
    [HttpGet("Crear")]
    public async Task<IActionResult> Crear(int? vehiculoId = null, int? citaId = null)
    {
        await CargarVehiculosAsync();
        return View(new TrabajoFormModel
        {
            VehiculoId = vehiculoId ?? 0,
            CitaId     = citaId
        });
    }

    // POST /trabajos/crear
    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(TrabajoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarVehiculosAsync();
            return View(model);
        }

        ApiResult<TrabajoDto> result = await _trabajoService.CrearAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al crear el trabajo.");
            await CargarVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Orden de trabajo creada correctamente.";
        // Redirigir al Detalle para que el usuario pueda añadir líneas inmediatamente
        return RedirectToAction(nameof(Detalle), new { id = result.Data!.Id });
    }

    // ── Editar ────────────────────────────────────────────────────────────────

    // GET /trabajos/{id}/editar
    [HttpGet("{id:int}/Editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<TrabajoDto> result = await _trabajoService.ObtenerAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = "Trabajo no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        await CargarVehiculosAsync();
        return View(TrabajoService.FromDto(result.Data!));
    }

    // POST /trabajos/{id}/editar
    [HttpPost("{id:int}/Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, TrabajoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarVehiculosAsync();
            return View(model);
        }

        ApiResult<TrabajoDto> result = await _trabajoService.EditarAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al actualizar el trabajo.");
            await CargarVehiculosAsync();
            return View(model);
        }

        TempData["Success"] = "Trabajo actualizado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // ── Eliminar ──────────────────────────────────────────────────────────────

    // POST /trabajos/{id}/eliminar
    [HttpPost("{id:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _trabajoService.EliminarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el trabajo.";
        else
            TempData["Success"] = "Trabajo eliminado.";

        return RedirectToAction(nameof(Index));
    }

    // ── Líneas ────────────────────────────────────────────────────────────────

    // POST /trabajos/{id}/lineas
    [HttpPost("{id:int}/Lineas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarLinea(int id, LineaTrabajoFormModel lineaForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de la línea inválidos. Revisa los campos.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        ApiResult<DetalleTrabajoDto> result = await _trabajoService.AgregarLineaAsync(id, lineaForm);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo agregar la línea.";
        else
            TempData["Success"] = "Línea agregada correctamente.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    // POST /trabajos/{id}/lineas/{lineaId}/eliminar
    [HttpPost("{id:int}/Lineas/{lineaId:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarLinea(int id, int lineaId)
    {
        ApiResult<bool> result = await _trabajoService.EliminarLineaAsync(id, lineaId);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar la línea.";
        else
            TempData["Success"] = "Línea eliminada.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    // ── Cobros ────────────────────────────────────────────────────────────────

    // POST /trabajos/{id}/cobros
    [HttpPost("{id:int}/Cobros")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarCobro(int id, CobroFormModel cobroForm)
    {
        ApiResult<TrabajoDto> trabajo = await _trabajoService.ObtenerAsync(id);
        ApiResult<List<DetalleTrabajoDto>> lineas = await _trabajoService.ObtenerLineasAsync(id);
        ApiResult<PagedResponse<CobroTrabajoDto>> cobros = await _trabajoService.ObtenerCobrosAsync(id);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = "Error de validación: " + string.Join(" | ", errors);
            return await ReloadDetalleView(id, trabajo, lineas, cobros, cobroForm);
        }

        // Validación extra: no cobrar más de lo pendiente
        if (trabajo.Success && lineas.Success && cobros.Success)
        {
            decimal totalLineas = lineas.Data!.Sum(l => l.Cantidad * l.PrecioUnitario * (1 - l.DescuentoPorcentaje / 100) * (1 + l.ImpuestoPorcentaje / 100));
            List<CobroTrabajoDto> listaCobroActual = cobros.Data?.Data?.ToList() ?? new List<CobroTrabajoDto>();
            decimal totalCobrado = listaCobroActual.Sum(c => c.Importe);
            decimal pendiente = totalLineas - totalCobrado;

            if (cobroForm.Importe > pendiente)
            {
                TempData["Error"] = $"No puedes cobrar {cobroForm.Importe:N2} € porque el importe pendiente es de {pendiente:N2} €.";
                return await ReloadDetalleView(id, trabajo, lineas, cobros, cobroForm);
            }
        }

        ApiResult<CobroTrabajoDto> result = await _trabajoService.RegistrarCobroAsync(id, cobroForm);

        if (!result.Success)
        {
            TempData["Error"] = result.Error?.Mensaje ?? "Error al registrar el cobro en el servidor.";
            return await ReloadDetalleView(id, trabajo, lineas, cobros, cobroForm);
        }

        TempData["Success"] = "Cobro registrado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private async Task<IActionResult> ReloadDetalleView(
        int id,
        ApiResult<TrabajoDto> trabajo,
        ApiResult<List<DetalleTrabajoDto>> lineas,
        ApiResult<PagedResponse<CobroTrabajoDto>> cobros,
        CobroFormModel cobroForm)
    {
        var productosResult = await _inventarioService.ListarProductosAsync(1, 200);
        var serviciosResult = await _servicioService.ListarServiciosAsync(1, 200);

        ViewData["Lineas"]     = lineas.Data ?? new List<DetalleTrabajoDto>();
        ViewData["Cobros"]     = cobros.Data?.Data?.ToList() ?? new List<CobroTrabajoDto>();
        ViewData["ProductosCatalog"] = productosResult.Data?.Data ?? new List<Talleres360.Dtos.Inventario.ProductoDto>();
        ViewData["ServiciosCatalog"] = serviciosResult.Data?.Data ?? new List<Talleres360.Dtos.Servicios.ServicioDto>();
        ViewData["LineaForm"]  = new LineaTrabajoFormModel();
        ViewData["CobroForm"]  = cobroForm;

        return View("Detalle", trabajo.Data);
    }

    // POST /trabajos/{id}/cobros/{cobroId}/eliminar
    [HttpPost("{id:int}/Cobros/{cobroId:int}/Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCobro(int id, int cobroId)
    {
        ApiResult<bool> result = await _trabajoService.EliminarCobroAsync(id, cobroId);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el cobro.";
        else
            TempData["Success"] = "Cobro eliminado.";

        return RedirectToAction(nameof(Detalle), new { id });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task CargarVehiculosAsync()
    {
        ApiResult<PagedResponse<VehiculoDetalle>> result =
            await _vehiculoService.ListarAsync(1, 200, null);
        ViewData["Vehiculos"] = result.Data?.Data ?? new List<VehiculoDetalle>();
    }
}
