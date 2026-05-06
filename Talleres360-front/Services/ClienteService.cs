using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Responses;
using Talleres360.Models.Negocio;
using Talleres360_front.Models;
using Talleres360_front.Models.Clientes;

namespace Talleres360_front.Services;

public class ClienteService
{
    private readonly ApiClient _api;

    public ClienteService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<Talleres360.Dtos.PagedResponse<Cliente>>> ListarAsync(
        int pageNumber, int pageSize, string? buscar)
    {
        string url = $"api/v1/customers?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(buscar))
            url += $"&buscar={Uri.EscapeDataString(buscar)}";

        return await _api.GetResultAsync<PagedResponse<Cliente>>(url);
    }

    public async Task<ApiResult<Cliente>> ObtenerAsync(int id) =>
        await _api.GetResultAsync<Cliente>($"api/v1/customers/{id}");

    public async Task<ApiResult<Cliente>> CrearAsync(ClienteFormModel form)
    {
        CrearClienteRequest request = MapToCrear(form);
        return await _api.PostResultAsync<Cliente>("api/v1/customers", request);
    }

    public async Task<ApiResult<Cliente>> EditarAsync(int id, ClienteFormModel form)
    {
        ActualizarClienteRequest request = MapToActualizar(form);
        return await _api.PutResultAsync<Cliente>($"api/v1/customers/{id}", request);
    }

    public async Task<ApiResult<bool>> EliminarAsync(int id) =>
        await _api.DeleteResultAsync<bool>($"api/v1/customers/{id}");

    public static ClienteFormModel FromCliente(Cliente c) => new()
    {
        Id                   = c.Id,
        Nombre               = c.Nombre,
        Apellidos            = c.Apellidos,
        NifCif               = c.NifCif,
        EsEmpresa            = c.EsEmpresa,
        Telefono             = c.Telefono,
        Email                = c.Email ?? string.Empty,
        Direccion            = c.Direccion,
        CodigoPostal         = c.CodigoPostal,
        Localidad            = c.Localidad,
        Provincia            = c.Provincia,
        AceptaComunicaciones = c.AceptaComunicaciones
    };

    private static CrearClienteRequest MapToCrear(ClienteFormModel f) => new()
    {
        Nombre               = f.Nombre.Trim(),
        Apellidos            = f.Apellidos?.Trim(),
        NifCif               = f.NifCif?.Trim().ToUpper(),
        EsEmpresa            = f.EsEmpresa,
        Telefono             = f.Telefono.Trim(),
        Email                = f.Email.Trim().ToLower(),
        Direccion            = f.Direccion?.Trim(),
        CodigoPostal         = f.CodigoPostal?.Trim(),
        Localidad            = f.Localidad?.Trim(),
        Provincia            = f.Provincia?.Trim(),
        AceptaComunicaciones = f.AceptaComunicaciones
    };

    private static ActualizarClienteRequest MapToActualizar(ClienteFormModel f) => new()
    {
        Nombre               = f.Nombre.Trim(),
        Apellidos            = f.Apellidos?.Trim(),
        NifCif               = f.NifCif?.Trim().ToUpper(),
        EsEmpresa            = f.EsEmpresa,
        Telefono             = f.Telefono.Trim(),
        Email                = f.Email.Trim().ToLower(),
        Direccion            = f.Direccion?.Trim(),
        CodigoPostal         = f.CodigoPostal?.Trim(),
        Localidad            = f.Localidad?.Trim(),
        Provincia            = f.Provincia?.Trim(),
        AceptaComunicaciones = f.AceptaComunicaciones
    };
}
