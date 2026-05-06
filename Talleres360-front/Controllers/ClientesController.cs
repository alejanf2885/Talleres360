using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Models.Negocio;
using Talleres360.Dtos.Responses;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Clientes;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
public class ClientesController : Controller
{
    private readonly ClienteService _clienteService;

    public ClientesController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    // GET /clientes
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 15, string? buscar = null)
    {
        ApiResult<Talleres360.Dtos.PagedResponse<Cliente>> result =
            await _clienteService.ListarAsync(pageNumber, pageSize, buscar);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        Talleres360.Dtos.PagedResponse<Cliente> paged =
            result.Data ?? new Talleres360.Dtos.PagedResponse<Cliente>();

        ViewData["Buscar"]          = buscar;
        ViewData["PageNumber"]      = paged.PageNumber;
        ViewData["TotalPages"]      = paged.TotalPages;
        ViewData["TotalCount"]      = paged.TotalCount;
        ViewData["HasPreviousPage"] = paged.HasPreviousPage;
        ViewData["HasNextPage"]     = paged.HasNextPage;

        return View(paged);
    }

    // GET /clientes/{id}/panel  — devuelve HTML parcial para el panel lateral
    [HttpGet]
    public async Task<IActionResult> Panel(int id)
    {
        ApiResult<Cliente> result = await _clienteService.ObtenerAsync(id);

        if (!result.Success)
            return PartialView("_PanelError",
                result.Error?.Mensaje ?? "Cliente no encontrado.");

        return PartialView("_Panel", result.Data!);
    }

    // GET /clientes/crear
    public IActionResult Crear() => View(new ClienteFormModel());

    // POST /clientes/crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApiResult<Cliente> result = await _clienteService.CrearAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al crear el cliente.");
            return View(model);
        }

        TempData["Success"] = $"Cliente «{result.Data!.Nombre}» creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET /clientes/{id}/editar
    [HttpGet("{id:int}/editar")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<Cliente> result = await _clienteService.ObtenerAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = "Cliente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(ClienteService.FromCliente(result.Data!));
    }

    // POST /clientes/{id}/editar
    [HttpPost("{id:int}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ClienteFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApiResult<Cliente> result = await _clienteService.EditarAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error?.Mensaje ?? "Error al actualizar el cliente.");
            return View(model);
        }

        TempData["Success"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST /clientes/{id}/eliminar
    [HttpPost("{id:int}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _clienteService.EliminarAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el cliente.";
        else
            TempData["Success"] = "Cliente eliminado.";

        return RedirectToAction(nameof(Index));
    }
}
