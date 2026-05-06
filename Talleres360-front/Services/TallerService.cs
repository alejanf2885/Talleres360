using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Talleres;
using Talleres360_front.Models.Taller;

namespace Talleres360_front.Services;

public class TallerService
{
    private readonly ApiClient _api;

    public TallerService(ApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string? ErrorMessage)> ConfigurarPerfilAsync(SetupTallerForm form)
    {
        ConfigurarTallerRequest request = new()
        {
            CIF       = form.CIF.Trim(),
            Direccion = form.Direccion.Trim(),
            Localidad = form.Localidad.Trim(),
            Telefono  = form.Telefono.Trim(),
            Logo      = form.LogoBase64 ?? string.Empty
        };

        ApiResponse<bool>? result = await _api.PutAsync<bool>("api/v1/workshops/config", request);

        if (result?.Success == true)
            return (true, null);

        return (false, result?.Message ?? "Error al guardar la configuración del taller.");
    }
}
