using Talleres360.Models;
using Talleres360.Dtos.Responses; 

namespace Talleres360.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        Task<ServiceResult<Usuario>> GetByEmailAsync(string email);
        Task<ServiceResult<Usuario>> GetByIdAsync(int id);
        Task<ServiceResult<bool>> ActualizarUltimoAccesoAsync(int usuarioId);
        Task<ServiceResult<bool>> ActivarUsuarioAsync(int usuarioId);

        Task<ServiceResult<Usuario>> CrearUsuarioAdminAsync(int tallerId, string nombre, string email, string password, string? rutaImagen = null);
    }
}