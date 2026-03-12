namespace Talleres360.Interfaces.Talleres
{
    public interface IRegistroTallerService
    {
        Task<(bool Success, string Message)> RegistrarNuevoClienteSaaSAsync(string nombreTaller, string nombreAdmin, string email, string password);
    }
}