using Talleres360.Dtos;
using Talleres360.Dtos.Servicios;
using Talleres360_front.Models.Servicios;
using Talleres360_front.Models;

namespace Talleres360_front.Services;

public class ServicioService
{
    private readonly ApiClient _api;

    public ServicioService(ApiClient api) => _api = api;

    public async Task<ApiResult<PagedResponse<ServicioDto>>> ListarServiciosAsync(int pageNumber = 1, int pageSize = 100, string? buscar = null)
    {
        // El endpoint /api/v1/servicios del backend devuelve una PagedResponse (o List, verificar backend).
        // En base a la Fase 8, `GET /api/v1/servicios` suele devolver PagedResponse si hereda de CRUD estándar.
        // Asumo que usa pageNumber y pageSize.
        string url = $"api/v1/servicios?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(buscar))
            url += $"&buscar={Uri.EscapeDataString(buscar)}";

        return await _api.GetResultAsync<PagedResponse<ServicioDto>>(url);
    }

    public async Task<ApiResult<ServicioDto>> ObtenerServicioAsync(int id) =>
        await _api.GetResultAsync<ServicioDto>($"api/v1/servicios/{id}");

    public async Task<ApiResult<ServicioDto>> CrearServicioAsync(ServicioFormModel form) =>
        await _api.PostResultAsync<ServicioDto>("api/v1/servicios", MapToCrear(form));

    public async Task<ApiResult<ServicioDto>> EditarServicioAsync(int id, ServicioFormModel form) =>
        await _api.PutResultAsync<ServicioDto>($"api/v1/servicios/{id}", MapToActualizar(form));

    public async Task<ApiResult<bool>> EliminarServicioAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/servicios/{id}");

    // ── Mappers ───────────────────────────────────────────────────────────────

    public static ServicioFormModel FromDto(ServicioDto s) => new()
    {
        Id                 = s.Id,
        Nombre             = s.Nombre,
        Descripcion        = s.Descripcion,
        PrecioBase         = s.PrecioBase,
        ImpuestoPorcentaje = s.ImpuestoPorcentaje,
        Activo             = s.Activo
    };

    private static CrearServicioRequest MapToCrear(ServicioFormModel f) => new()
    {
        Nombre             = f.Nombre.Trim(),
        Descripcion        = f.Descripcion?.Trim(),
        PrecioBase         = f.PrecioBase,
        ImpuestoPorcentaje = f.ImpuestoPorcentaje,
        Activo             = f.Activo
    };

    private static ActualizarServicioRequest MapToActualizar(ServicioFormModel f) => new()
    {
        Nombre             = f.Nombre.Trim(),
        Descripcion        = f.Descripcion?.Trim(),
        PrecioBase         = f.PrecioBase,
        ImpuestoPorcentaje = f.ImpuestoPorcentaje,
        Activo             = f.Activo
    };
}
