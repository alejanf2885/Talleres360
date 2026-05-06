using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Responses;
using Talleres360_front.Models;
using Talleres360_front.Models.Citas;

namespace Talleres360_front.Services;

public class CitaService
{
    private readonly ApiClient _api;

    public CitaService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResponse<CitaDto>>> ListarAsync(
        int pageNumber, int pageSize, string? estado = null)
    {
        string url = $"api/v1/citas?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(estado))
            url += $"&estado={Uri.EscapeDataString(estado)}";

        return await _api.GetResultAsync<PagedResponse<CitaDto>>(url);
    }

    public async Task<ApiResult<CitaDto>> ObtenerAsync(int id) =>
        await _api.GetResultAsync<CitaDto>($"api/v1/citas/{id}");

    public async Task<ApiResult<CitaDto>> CrearAsync(CitaFormModel form)
    {
        CrearCitaRequest request = MapToCrear(form);
        return await _api.PostResultAsync<CitaDto>("api/v1/citas", request);
    }

    public async Task<ApiResult<CitaDto>> EditarAsync(int id, CitaFormModel form)
    {
        ActualizarCitaRequest request = MapToActualizar(form);
        return await _api.PutResultAsync<CitaDto>($"api/v1/citas/{id}", request);
    }

    public async Task<ApiResult<bool>> EliminarAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/citas/{id}");

    public async Task<ApiResult<object>> ConvertirATrabajoAsync(int id) =>
        await _api.PostResultAsync<object>($"api/v1/citas/{id}/convertir-a-trabajo", new { });

    public static CitaFormModel FromDto(CitaDto c) => new()
    {
        Id                 = c.Id,
        VehiculoId         = c.VehiculoId,
        NombreClienteTemp  = c.NombreClienteTemp,
        FechaCita          = c.FechaCita,
        HoraAproximada     = c.HoraAproximada,
        Descripcion        = c.Descripcion,
        Estado             = c.Estado
    };

    private static CrearCitaRequest MapToCrear(CitaFormModel f) => new()
    {
        VehiculoId        = f.VehiculoId,
        NombreClienteTemp = f.NombreClienteTemp?.Trim(),
        FechaCita         = f.FechaCita,
        HoraAproximada    = f.HoraAproximada?.Trim(),
        Descripcion       = f.Descripcion?.Trim(),
        Estado            = f.Estado
    };

    private static ActualizarCitaRequest MapToActualizar(CitaFormModel f) => new()
    {
        VehiculoId        = f.VehiculoId,
        NombreClienteTemp = f.NombreClienteTemp?.Trim(),
        FechaCita         = f.FechaCita,
        HoraAproximada    = f.HoraAproximada?.Trim(),
        Descripcion       = f.Descripcion?.Trim(),
        Estado            = f.Estado
    };
}
