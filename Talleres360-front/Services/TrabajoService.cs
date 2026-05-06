using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360_front.Models;
using Talleres360_front.Models.Trabajos;

namespace Talleres360_front.Services;

public class TrabajoService
{
    private readonly ApiClient _api;

    public TrabajoService(ApiClient api) => _api = api;

    // ── Cabecera ──────────────────────────────────────────────────────────────

    public async Task<ApiResult<PagedResponse<TrabajoDto>>> ListarAsync(
        int pageNumber, int pageSize,
        string? estado = null, string? estadoPago = null)
    {
        string url = $"api/v1/trabajos?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(estado))
            url += $"&estado={Uri.EscapeDataString(estado)}";
        if (!string.IsNullOrWhiteSpace(estadoPago))
            url += $"&estadoPago={Uri.EscapeDataString(estadoPago)}";

        return await _api.GetResultAsync<PagedResponse<TrabajoDto>>(url);
    }

    public async Task<ApiResult<TrabajoDto>> ObtenerAsync(int id) =>
        await _api.GetResultAsync<TrabajoDto>($"api/v1/trabajos/{id}");

    public async Task<ApiResult<TrabajoDto>> CrearAsync(TrabajoFormModel form) =>
        await _api.PostResultAsync<TrabajoDto>("api/v1/trabajos", MapToCrear(form));

    public async Task<ApiResult<TrabajoDto>> EditarAsync(int id, TrabajoFormModel form) =>
        await _api.PutResultAsync<TrabajoDto>($"api/v1/trabajos/{id}", MapToActualizar(form));

    public async Task<ApiResult<bool>> EliminarAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/trabajos/{id}");

    public async Task<ApiResult<TrabajoDto>> FacturarAsync(int id) =>
        await _api.PostResultAsync<TrabajoDto>($"api/v1/trabajos/{id}/facturar", new { });

    // ── Líneas (DetallesTrabajo) ───────────────────────────────────────────────

    public async Task<ApiResult<List<DetalleTrabajoDto>>> ObtenerLineasAsync(int trabajoId) =>
        await _api.GetResultAsync<List<DetalleTrabajoDto>>($"api/v1/trabajos/{trabajoId}/detalles");

    public async Task<ApiResult<DetalleTrabajoDto>> AgregarLineaAsync(
        int trabajoId, LineaTrabajoFormModel form) =>
        await _api.PostResultAsync<DetalleTrabajoDto>(
            $"api/v1/trabajos/{trabajoId}/detalles", MapToLinea(form));

    public async Task<ApiResult<bool>> EliminarLineaAsync(int trabajoId, int lineaId) =>
        await _api.DeleteResultAsync<bool>(
            $"api/v1/trabajos/{trabajoId}/detalles/{lineaId}");

    // ── Cobros ────────────────────────────────────────────────────────────────

    public async Task<ApiResult<PagedResponse<CobroTrabajoDto>>> ObtenerCobrosAsync(int trabajoId) =>
        await _api.GetResultAsync<PagedResponse<CobroTrabajoDto>>($"api/v1/trabajos/{trabajoId}/cobros?pageSize=100");

    public async Task<ApiResult<CobroTrabajoDto>> RegistrarCobroAsync(
        int trabajoId, CobroFormModel form) =>
        await _api.PostResultAsync<CobroTrabajoDto>(
            $"api/v1/trabajos/{trabajoId}/cobros", MapToCobro(form));

    public async Task<ApiResult<bool>> EliminarCobroAsync(int trabajoId, int cobroId) =>
        await _api.DeleteResultAsync<bool>(
            $"api/v1/trabajos/{trabajoId}/cobros/{cobroId}");

    // ── Mappers ───────────────────────────────────────────────────────────────

    public static TrabajoFormModel FromDto(TrabajoDto t) => new()
    {
        Id                   = t.Id,
        VehiculoId           = t.VehiculoId ?? 0,
        MecanicoAsignadoId   = t.MecanicoAsignadoId,
        CitaId               = t.CitaId,
        TituloMantenimiento  = t.TituloMantenimiento,
        TrabajoRealizado     = t.TrabajoRealizado,
        KmEntrada            = t.KmEntrada,
        Estado               = t.Estado,
        EstadoPago           = t.EstadoPago,
        FechaEntregaEstimada = t.FechaEntregaEstimada,
        KmSalida             = t.KmSalida,
        ObservacionesEntrega = t.ObservacionesEntrega
    };

    private static CrearTrabajoRequest MapToCrear(TrabajoFormModel f) => new()
    {
        VehiculoId          = f.VehiculoId,
        MecanicoAsignadoId  = f.MecanicoAsignadoId,
        CitaId              = f.CitaId,
        TituloMantenimiento = f.TituloMantenimiento?.Trim(),
        TrabajoRealizado    = f.TrabajoRealizado?.Trim(),
        KmEntrada           = f.KmEntrada,
        Estado              = f.Estado,
        EstadoPago          = f.EstadoPago
    };

    private static ActualizarTrabajoRequest MapToActualizar(TrabajoFormModel f) => new()
    {
        VehiculoId           = f.VehiculoId,
        MecanicoAsignadoId   = f.MecanicoAsignadoId,
        TituloMantenimiento  = f.TituloMantenimiento?.Trim(),
        TrabajoRealizado     = f.TrabajoRealizado?.Trim(),
        KmEntrada            = f.KmEntrada,
        Estado               = f.Estado,
        EstadoPago           = f.EstadoPago,
        FechaEntregaEstimada = f.FechaEntregaEstimada,
        KmSalida             = f.KmSalida,
        ObservacionesEntrega = f.ObservacionesEntrega?.Trim()
    };

    private static CrearDetalleTrabajoRequest MapToLinea(LineaTrabajoFormModel f) => new()
    {
        ProductoId          = f.ProductoId,
        Concepto            = f.Concepto.Trim(),
        Cantidad            = f.Cantidad,
        PrecioUnitario      = f.PrecioUnitario,
        DescuentoPorcentaje = f.DescuentoPorcentaje,
        ImpuestoPorcentaje  = f.ImpuestoPorcentaje,
        EsManoObra          = f.EsManoObra,
        EstadoMaterial      = f.EstadoMaterial,
        HorasAplicadas      = f.HorasAplicadas,
        PrecioHoraAplicado  = f.PrecioHoraAplicado
    };

    private static CrearCobroTrabajoRequest MapToCobro(CobroFormModel f) => new()
    {
        Importe    = f.Importe,
        MetodoPago = f.MetodoPago,
        Referencia = f.Referencia?.Trim(),
        Notas      = f.Notas?.Trim(),
        FechaCobro = f.FechaCobro
    };
}
