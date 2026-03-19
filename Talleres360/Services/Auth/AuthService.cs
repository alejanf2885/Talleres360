using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;

namespace Talleres360.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IPasswordService _passwordService;

        public AuthService(IUsuarioRepository userRepo, IPasswordService passwordService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
        }

        public async Task<ServiceResult<Usuario>> ValidarLoginAsync(string email, string password)
        {
            // 1. Buscar usuario
            Usuario? usuario = await _userRepo.GetByEmailAsync(email);

            if (usuario == null || usuario.Eliminado)
            {
                return ServiceResult<Usuario>.Fail(
                    ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    "El correo o la contraseña no son correctos."
                );
            }

            if (!usuario.Activo)
            {
                return ServiceResult<Usuario>.Fail(
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString(),
                    "Tu cuenta aún no está verificada. Revisa tu correo electrónico."
                );
            }

            Credencial? credencial = await _userRepo.GetCredencialLocalByUsuarioIdAsync(usuario.Id);

            if (credencial == null || !_passwordService.VerifyPassword(password, credencial.PasswordHash))
            {
                return ServiceResult<Usuario>.Fail(
                    ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    "El correo o la contraseña no son correctos."
                );
            }

            await _userRepo.ActualizarUltimoAccesoAsync(usuario.Id);

            return ServiceResult<Usuario>.Ok(usuario);
        }
    }
}