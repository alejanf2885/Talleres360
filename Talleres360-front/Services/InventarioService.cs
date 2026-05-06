using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360_front.Models.Inventario;
using Talleres360_front.Models;

namespace Talleres360_front.Services;

public class InventarioService
{
    private readonly ApiClient _api;

    public InventarioService(ApiClient api) => _api = api;

    // ── Categorías ────────────────────────────────────────────────────────────

    public async Task<ApiResult<List<CategoriaProductoDto>>> ListarCategoriasAsync() =>
        await _api.GetResultAsync<List<CategoriaProductoDto>>("api/v1/inventario/categorias");

    public async Task<ApiResult<CategoriaProductoDto>> CrearCategoriaAsync(CategoriaFormModel form) =>
        await _api.PostResultAsync<CategoriaProductoDto>("api/v1/inventario/categorias", new CrearCategoriaProductoRequest
        {
            Nombre = form.Nombre.Trim()
        });

    public async Task<ApiResult<bool>> EliminarCategoriaAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/inventario/categorias/{id}");

    // ── Productos ─────────────────────────────────────────────────────────────

    public async Task<ApiResult<PagedResponse<ProductoDto>>> ListarProductosAsync(
        int pageNumber, int pageSize, string? buscar = null, int? categoriaId = null)
    {
        string url = $"api/v1/inventario/productos?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(buscar))
            url += $"&buscar={Uri.EscapeDataString(buscar)}";
        if (categoriaId.HasValue)
            url += $"&categoriaId={categoriaId.Value}";

        return await _api.GetResultAsync<PagedResponse<ProductoDto>>(url);
    }

    public async Task<ApiResult<ProductoDto>> ObtenerProductoAsync(int id) =>
        await _api.GetResultAsync<ProductoDto>($"api/v1/inventario/productos/{id}");

    public async Task<ApiResult<ProductoDto>> CrearProductoAsync(ProductoFormModel form) =>
        await _api.PostResultAsync<ProductoDto>("api/v1/inventario/productos", MapToCrear(form));

    public async Task<ApiResult<ProductoDto>> EditarProductoAsync(int id, ProductoFormModel form) =>
        await _api.PutResultAsync<ProductoDto>($"api/v1/inventario/productos/{id}", MapToActualizar(form));

    public async Task<ApiResult<bool>> EliminarProductoAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/inventario/productos/{id}");

    // ── Mappers ───────────────────────────────────────────────────────────────

    public static ProductoFormModel FromDto(ProductoDto p) => new()
    {
        Id             = p.Id,
        CategoriaId    = p.CategoriaId,
        Referencia     = p.Referencia,
        Nombre         = p.Nombre,
        PrecioCompra   = p.PrecioCompra,
        PrecioVenta    = p.PrecioVenta,
        StockActual    = p.StockActual,
        ControlarStock = p.ControlarStock
    };

    private static CrearProductoRequest MapToCrear(ProductoFormModel f) => new()
    {
        CategoriaId    = f.CategoriaId,
        Referencia     = f.Referencia?.Trim(),
        Nombre         = f.Nombre.Trim(),
        PrecioCompra   = f.PrecioCompra,
        PrecioVenta    = f.PrecioVenta,
        StockActual    = f.StockActual,
        ControlarStock = f.ControlarStock
    };

    private static ActualizarProductoRequest MapToActualizar(ProductoFormModel f) => new()
    {
        CategoriaId    = f.CategoriaId,
        Referencia     = f.Referencia?.Trim(),
        Nombre         = f.Nombre.Trim(),
        PrecioCompra   = f.PrecioCompra,
        PrecioVenta    = f.PrecioVenta,
        StockActual    = f.StockActual,
        ControlarStock = f.ControlarStock
    };
}
