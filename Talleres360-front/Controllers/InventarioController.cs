using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360_front.Filters;
using Talleres360_front.Models;
using Talleres360_front.Models.Inventario;
using Talleres360_front.Services;

namespace Talleres360_front.Controllers;

[AuthRequired]
public class InventarioController : Controller
{
    private readonly InventarioService _inventarioService;

    public InventarioController(InventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    // ── Productos ─────────────────────────────────────────────────────────────

    // GET: /inventario/productos
    [HttpGet("Inventario")]
    [HttpGet("Inventario/Productos")]
    public async Task<IActionResult> Productos(
        int pageNumber = 1, int pageSize = 15,
        string? buscar = null, int? categoriaId = null)
    {
        ApiResult<PagedResponse<ProductoDto>> result =
            await _inventarioService.ListarProductosAsync(pageNumber, pageSize, buscar, categoriaId);

        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        PagedResponse<ProductoDto> paged = result.Data ?? new PagedResponse<ProductoDto>();

        ViewData["Buscar"]      = buscar;
        ViewData["CategoriaId"] = categoriaId;
        ViewData["PageNumber"]  = paged.PageNumber;
        ViewData["TotalPages"]  = paged.TotalPages;
        ViewData["TotalCount"]  = paged.TotalCount;
        ViewData["HasPreviousPage"] = paged.HasPreviousPage;
        ViewData["HasNextPage"]     = paged.HasNextPage;

        await CargarCategoriasViewDataAsync();

        return View(paged);
    }

    // GET: /inventario/crear
    [HttpGet("Inventario/Crear")]
    public async Task<IActionResult> Crear()
    {
        await CargarCategoriasViewDataAsync();
        return View(new ProductoFormModel());
    }

    // POST: /inventario/crear
    [HttpPost("Inventario/Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasViewDataAsync();
            return View(model);
        }

        ApiResult<ProductoDto> result = await _inventarioService.CrearProductoAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al crear el producto.");
            await CargarCategoriasViewDataAsync();
            return View(model);
        }

        TempData["Success"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Productos));
    }

    // GET: /inventario/editar/{id}
    [HttpGet("Inventario/Editar/{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        ApiResult<ProductoDto> result = await _inventarioService.ObtenerProductoAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = "Producto no encontrado.";
            return RedirectToAction(nameof(Productos));
        }

        await CargarCategoriasViewDataAsync();
        return View(InventarioService.FromDto(result.Data!));
    }

    // POST: /inventario/editar/{id}
    [HttpPost("Inventario/Editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasViewDataAsync();
            return View(model);
        }

        ApiResult<ProductoDto> result = await _inventarioService.EditarProductoAsync(id, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Mensaje ?? "Error al actualizar el producto.");
            await CargarCategoriasViewDataAsync();
            return View(model);
        }

        TempData["Success"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Productos));
    }

    // POST: /inventario/eliminar/{id}
    [HttpPost("Inventario/Eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        ApiResult<bool> result = await _inventarioService.EliminarProductoAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar el producto.";
        else
            TempData["Success"] = "Producto eliminado correctamente.";

        return RedirectToAction(nameof(Productos));
    }

    // ── Categorías ────────────────────────────────────────────────────────────

    // GET: /inventario/categorias
    [HttpGet("Inventario/Categorias")]
    public async Task<IActionResult> Categorias()
    {
        ApiResult<List<CategoriaProductoDto>> result = await _inventarioService.ListarCategoriasAsync();
        if (result.IsUnauthorized) return RedirectToAction("Login", "Auth");

        ViewData["CategoriaForm"] = new CategoriaFormModel();
        return View(result.Data ?? new List<CategoriaProductoDto>());
    }

    // POST: /inventario/categorias/crear
    [HttpPost("Inventario/Categorias/Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CategoriaFormModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de categoría inválidos.";
            return RedirectToAction(nameof(Categorias));
        }

        ApiResult<CategoriaProductoDto> result = await _inventarioService.CrearCategoriaAsync(form);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "Error al crear la categoría.";
        else
            TempData["Success"] = "Categoría creada correctamente.";

        return RedirectToAction(nameof(Categorias));
    }

    // POST: /inventario/categorias/eliminar/{id}
    [HttpPost("Inventario/Categorias/Eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        ApiResult<bool> result = await _inventarioService.EliminarCategoriaAsync(id);

        if (!result.Success)
            TempData["Error"] = result.Error?.Mensaje ?? "No se pudo eliminar la categoría. Asegúrate de que no tenga productos asociados.";
        else
            TempData["Success"] = "Categoría eliminada.";

        return RedirectToAction(nameof(Categorias));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task CargarCategoriasViewDataAsync()
    {
        ApiResult<List<CategoriaProductoDto>> catResult = await _inventarioService.ListarCategoriasAsync();
        ViewData["Categorias"] = catResult.Data ?? new List<CategoriaProductoDto>();
    }
}
