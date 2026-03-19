using Talleres360.Dtos.Seguridad;

namespace Talleres360.Interfaces.Seguridad
{
    public interface ISuscripcionGuardService
    {
        Task<AccesoResult> ValidarAccesoEscrituraAsync(int tallerId);
        Task<AccesoResult> ValidarAccesoPremiumAsync(int tallerId);
        Task<EstadoSuscripcionResponse> ObtenerEstadoSuscripcionAsync(int tallerId);
    }
}