using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models.Flota;
using Talleres360_front.Models;
using Talleres360_front.Models.Vehiculos;

namespace Talleres360_front.Services;

public class VehiculoService
{
    private readonly ApiClient _api;

    public VehiculoService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResponse<VehiculoDetalle>>> ListarAsync(
        int pageNumber, int pageSize, string? matricula)
    {
        string url = $"api/v1/vehiculos?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(matricula))
            url += $"&matricula={Uri.EscapeDataString(matricula)}";

        return await _api.GetResultAsync<PagedResponse<VehiculoDetalle>>(url);
    }

    public async Task<ApiResult<VehiculoDetalle>> ObtenerAsync(int id) =>
        await _api.GetResultAsync<VehiculoDetalle>($"api/v1/vehiculos/{id}");

    public async Task<ApiResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync() =>
        await _api.GetResultAsync<List<MarcaVehiculoDto>>("api/v1/vehiculos/marcas");

    public async Task<ApiResult<List<ModeloVehiculoDto>>> ObtenerModelosAsync(int marcaId) =>
        await _api.GetResultAsync<List<ModeloVehiculoDto>>($"api/v1/vehiculos/modelos/{marcaId}");

    public async Task<ApiResult<List<VehiculoTipoDto>>> ObtenerTiposAsync() =>
        await _api.GetResultAsync<List<VehiculoTipoDto>>("api/v1/vehiculos/tipos");

    public async Task<ApiResult<VehiculoDetalle>> CrearAsync(VehiculoFormModel form)
    {
        CrearVehiculoRequest request = MapToCrear(form);
        return await _api.PostResultAsync<VehiculoDetalle>("api/v1/vehiculos", request);
    }

    public async Task<ApiResult<VehiculoDetalle>> EditarAsync(int id, VehiculoFormModel form)
    {
        ActualizarVehiculoRequest request = MapToActualizar(form);
        return await _api.PutResultAsync<VehiculoDetalle>($"api/v1/vehiculos/{id}", request);
    }

    public async Task<ApiResult<bool>> EliminarAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/vehiculos/{id}");

    public static VehiculoFormModel FromDetalle(VehiculoDetalle v) => new()
    {
        Id             = v.Id,
        ClienteId      = v.ClienteId,
        VehiculoTipoId = v.VehiculoTipoId,
        MarcaId        = v.MarcaId,
        ModeloId       = v.ModeloId,
        Matricula      = v.Matricula,
        Anio           = v.Anio,
        KmActuales     = v.KmActuales
    };

    private static CrearVehiculoRequest MapToCrear(VehiculoFormModel f) => new()
    {
        ClienteId      = f.ClienteId,
        VehiculoTipoId = f.VehiculoTipoId,
        MarcaId        = f.MarcaId,
        ModeloId       = f.ModeloId,
        Matricula      = f.Matricula.Trim().ToUpper(),
        Anio           = f.Anio,
        KmActuales     = f.KmActuales
    };

    private static ActualizarVehiculoRequest MapToActualizar(VehiculoFormModel f) => new()
    {
        ClienteId      = f.ClienteId,
        VehiculoTipoId = f.VehiculoTipoId,
        MarcaId        = f.MarcaId,
        ModeloId       = f.ModeloId,
        Matricula      = f.Matricula.Trim().ToUpper(),
        Anio           = f.Anio,
        KmActuales     = f.KmActuales
    };
}
