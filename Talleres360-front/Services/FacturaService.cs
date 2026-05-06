using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360_front.Models;

namespace Talleres360_front.Services;

public class FacturaService
{
    private readonly ApiClient _api;

    public FacturaService(ApiClient api) => _api = api;

    public async Task<ApiResult<PagedResponse<FacturaDto>>> ListarFacturasAsync(int pageNumber = 1, int pageSize = 15)
    {
        string url = $"api/v1/facturas?pageNumber={pageNumber}&pageSize={pageSize}";
        return await _api.GetResultAsync<PagedResponse<FacturaDto>>(url);
    }

    public async Task<ApiResult<FacturaDto>> ObtenerFacturaAsync(int id) =>
        await _api.GetResultAsync<FacturaDto>($"api/v1/facturas/{id}");

    public async Task<ApiResult<FacturaDto>> ObtenerFacturaPorTrabajoAsync(int trabajoId) =>
        await _api.GetResultAsync<FacturaDto>($"api/v1/facturas/trabajo/{trabajoId}");

    public async Task<ApiResult<FacturaDto>> RegenerarPdfAsync(int facturaId) =>
        await _api.PostResultAsync<FacturaDto>($"api/v1/facturas/{facturaId}/regenerar-pdf", new { });
}
