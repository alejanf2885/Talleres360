using Talleres360.Enum;
using Talleres360.Enums;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IPasswordService _passwordService;

        public UsuarioService(IUsuarioRepository userRepo, IPasswordService passwordService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task ActualizarUltimoAccesoAsync(int usuarioId)
        {
            await _userRepo.ActualizarUltimoAccesoAsync(usuarioId);
        }

        public async Task<(bool Success, string Message, Usuario? Usuario)> CrearUsuarioAdminAsync(
            int tallerId, string nombre, string email, string password)
        {
            if (await _userRepo.ExisteEmailAsync(email))
                return (false, "Ese correo ya está registrado.", null);

            Usuario usuario = new Usuario
            {
                TallerId = tallerId,
                Nombre = nombre,
                Email = email,
                Rol = RolesUsuario.ADMIN,
                FechaCreacion = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                Activo = true
            };

            await _userRepo.AddAsync(usuario);
            await _userRepo.SaveChangesAsync();

            Credencial credencial = new Credencial
            {
                UsuarioId = usuario.Id,
                PasswordHash = _passwordService.HashPassword(password),
                TipoInicioSesion = LoginTipo.LOCAL.ToString()
            };

            await _userRepo.AddCredencialAsync(credencial);
            await _userRepo.SaveChangesAsync();

            return (true, "Usuario administrador creado correctamente.", usuario);
        }
    }
}
