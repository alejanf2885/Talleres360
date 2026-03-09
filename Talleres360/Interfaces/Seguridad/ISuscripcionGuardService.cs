namespace Talleres360.Interfaces.Seguridad
{
    public interface ISuscripcionGuardService
    {
        Task<(bool PuedeAcceder, string Mensaje)> ValidarAccesoEscrituraAsync(int tallerId);
    }
}
