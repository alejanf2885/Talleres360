namespace Talleres360.Interfaces.Seguridad
{
    public interface IUserContextService
    {
        int? GetTallerId();
        int? GetUsuarioId();
        string? GetEmail();
        string? GetRol();
    }
}