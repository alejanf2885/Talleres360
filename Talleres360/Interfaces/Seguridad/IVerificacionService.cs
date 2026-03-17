namespace Talleres360.Interfaces.Seguridad
{
    public interface IVerificacionService
    {
        Task<string> GenerarTokenRegistroAsync(int usuarioId);
        Task<(bool Exito, string Mensaje, int? UsuarioId)> ValidarYConsumirTokenAsync(string token);
        Task<string> GenerarLinkVerificacionAsync(int usuarioId);
    }
}
