using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

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

        public async Task<Usuario?> ValidarLoginAsync(string email, string password)
        {
            Usuario? usuario = await _userRepo.GetByEmailAsync(email);

            if (usuario == null || !usuario.Activo || usuario.Eliminado)
                return null;

            // Buscamos la credencial local
            Credencial? credencial = await _userRepo.GetCredencialLocalByUsuarioIdAsync(usuario.Id);

            if (credencial != null && _passwordService.VerifyPassword(password, credencial.PasswordHash))
            {
                await _userRepo.ActualizarUltimoAccesoAsync(usuario.Id);
                return usuario;
            }

            return null;
        }
    }
}