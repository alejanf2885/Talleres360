    using Talleres360.Dtos.Seguridad;
    
    namespace Talleres360.Interfaces.Seguridad
    {
        public interface ISuscripcionGuardService
        {
        Task<(bool PuedeAcceder, string Mensaje)> ValidarAccesoEscrituraAsync(int tallerId);

        Task<(bool PuedeAcceder, string Mensaje)> ValidarAccesoPremiumAsync(int tallerId);

        Task<EstadoSuscripcionResponse> ObtenerEstadoSuscripcionAsync(int tallerId);
    }
}
