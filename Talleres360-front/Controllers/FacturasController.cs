using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
[Route("[controller]")]
public class FacturasController : Controller
{
    private readonly FacturaService _facturaService;

    public FacturasController(FacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 15)
    {
        ApiResult<PagedResponse<FacturaDto>> resultado = await _facturaService.ListarFacturasAsync(pageNumber, pageSize);
        if (resultado.IsUnauthorized)
        {
            return RedirectToAction("Login", "Auth");
        }

        PagedResponse<FacturaDto> paginacion = resultado.Data ?? new PagedResponse<FacturaDto>();

        ViewData["PageNumber"] = paginacion.PageNumber;
        ViewData["TotalPages"] = paginacion.TotalPages;
        ViewData["TotalCount"] = paginacion.TotalCount;
        ViewData["HasPreviousPage"] = paginacion.HasPreviousPage;
        ViewData["HasNextPage"] = paginacion.HasNextPage;

        return View(paginacion);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ApiResult<FacturaDto> resultado = await _facturaService.ObtenerFacturaAsync(id);
        if (!resultado.Success)
        {
            TempData["Error"] = resultado.Error?.Mensaje ?? "Factura no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        return View(resultado.Data!);
    }

    [HttpGet("Trabajo/{trabajoId:int}")]
    public async Task<IActionResult> Trabajo(int trabajoId)
    {
        ApiResult<FacturaDto> resultado = await _facturaService.ObtenerFacturaPorTrabajoAsync(trabajoId);
        if (!resultado.Success)
        {
            TempData["Error"] = resultado.Error?.Mensaje ?? "No se pudo obtener la factura del trabajo.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Detalle), new { id = resultado.Data!.Id });
    }
}
