using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360_front.Models;
using Talleres360_front.Models.Presupuestos;

namespace Talleres360_front.Services;

public class PresupuestoService
{
    private readonly ApiClient _api;

    public PresupuestoService(ApiClient api) => _api = api;

    public async Task<ApiResult<PagedResponse<TrabajoDto>>> ListarPresupuestosAsync(int pageNumber = 1, int pageSize = 15)
    {
        string url = $"api/v1/presupuestos?pageNumber={pageNumber}&pageSize={pageSize}";
        return await _api.GetResultAsync<PagedResponse<TrabajoDto>>(url);
    }

    public async Task<ApiResult<TrabajoDto>> ObtenerPresupuestoAsync(int id) =>
        await _api.GetResultAsync<TrabajoDto>($"api/v1/presupuestos/{id}");

    public async Task<ApiResult<TrabajoDto>> CrearPresupuestoAsync(PresupuestoFormModel form) =>
        await _api.PostResultAsync<TrabajoDto>("api/v1/presupuestos", MapToCrear(form));

    public async Task<ApiResult<TrabajoDto>> EditarPresupuestoAsync(int id, PresupuestoFormModel form) =>
        await _api.PutResultAsync<TrabajoDto>($"api/v1/presupuestos/{id}", MapToActualizar(form));

    public async Task<ApiResult<bool>> EliminarPresupuestoAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/presupuestos/{id}");

    public async Task<ApiResult<TrabajoDto>> EnviarPresupuestoAsync(int id) =>
        await _api.PostResultAsync<TrabajoDto>($"api/v1/presupuestos/{id}/enviar", new { });

    public async Task<ApiResult<TrabajoDto>> AceptarPresupuestoAsync(int id, string? firmaAceptacionUrl = null) =>
        await _api.PostResultAsync<TrabajoDto>($"api/v1/presupuestos/{id}/aceptar", new AceptarPresupuestoRequest { FirmaAceptacionUrl = firmaAceptacionUrl });

    public async Task<ApiResult<TrabajoDto>> RechazarPresupuestoAsync(int id, string motivo) =>
        await _api.PostResultAsync<TrabajoDto>($"api/v1/presupuestos/{id}/rechazar", new RechazarPresupuestoRequest { MotivoRechazo = motivo });


    // ── Mappers ───────────────────────────────────────────────────────────────

    public static PresupuestoFormModel FromDto(TrabajoDto p) => new()
    {
        Id                   = p.Id,
        VehiculoId           = p.VehiculoId ?? 0,
        MecanicoAsignadoId   = p.MecanicoAsignadoId,
        CitaId               = p.CitaId,
        TituloMantenimiento  = p.TituloMantenimiento,
        TrabajoRealizado     = p.TrabajoRealizado,
        KmEntrada            = p.KmEntrada,
        Estado               = p.Estado,
        EstadoPago           = p.EstadoPago,
        FechaEntregaEstimada = p.FechaEntregaEstimada
    };

    private static CrearTrabajoRequest MapToCrear(PresupuestoFormModel f) => new()
    {
        VehiculoId           = f.VehiculoId,
        MecanicoAsignadoId   = f.MecanicoAsignadoId,
        CitaId               = f.CitaId,
        TituloMantenimiento  = f.TituloMantenimiento?.Trim(),
        TrabajoRealizado     = f.TrabajoRealizado?.Trim(),
        KmEntrada            = f.KmEntrada,
        Estado               = f.Estado,
        EstadoPago           = f.EstadoPago,
        DatosIncompletos     = false
    };

    private static ActualizarTrabajoRequest MapToActualizar(PresupuestoFormModel f) => new()
    {
        VehiculoId           = f.VehiculoId,
        MecanicoAsignadoId   = f.MecanicoAsignadoId,
        TituloMantenimiento  = f.TituloMantenimiento?.Trim(),
        TrabajoRealizado     = f.TrabajoRealizado?.Trim(),
        KmEntrada            = f.KmEntrada,
        Estado               = f.Estado,
        EstadoPago           = f.EstadoPago,
        FechaEntregaEstimada = f.FechaEntregaEstimada,
        DatosIncompletos     = false
    };
}
