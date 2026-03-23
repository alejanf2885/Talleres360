using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Usuarios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IPasswordService _passwordService;
        private readonly ITallerService _tallerService;

        public AuthService(
            IUsuarioRepository userRepo,
            IPasswordService passwordService,
            ITallerService tallerService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
            _tallerService = tallerService;
        }

        public async Task<ServiceResult<UsuarioLoginDto>> ValidarLoginAsync(
            string email, string password)
        {
            string emailNormalizado = email.Trim().ToLower();

            Usuario? usuario = await _userRepo.GetByEmailAsync(emailNormalizado);

            if (usuario == null || usuario.Eliminado)
            {
                return ServiceResult<UsuarioLoginDto>.Fail(
                    ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    "El correo o la contraseña no son correctos."
                );
            }

            if (!usuario.Activo)
            {
                return ServiceResult<UsuarioLoginDto>.Fail(
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString(),
                    "Tu cuenta aún no está verificada. Revisa tu correo electrónico."
                );
            }

            Credencial? credencial = await _userRepo
                .GetCredencialLocalByUsuarioIdAsync(usuario.Id);

            if (credencial == null ||
                !_passwordService.VerifyPassword(password, credencial.PasswordHash))
            {
                return ServiceResult<UsuarioLoginDto>.Fail(
                    ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    "El correo o la contraseña no son correctos."
                );
            }

            await _userRepo.ActualizarUltimoAccesoAsync(usuario.Id);

            bool perfilConfigurado = false;
            if (usuario.TallerId.HasValue)
            {
                perfilConfigurado = await _tallerService
                    .VerificarPerfilConfiguradoAsync(usuario.TallerId.Value);
            }

            UsuarioLoginDto dto = new UsuarioLoginDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                TallerId = usuario.TallerId,
                SecurityStamp = usuario.SecurityStamp, 
                PerfilConfigurado = perfilConfigurado
            };

            return ServiceResult<UsuarioLoginDto>.Ok(dto);
        }
    }
}