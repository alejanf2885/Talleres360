using Talleres360.Enum;
using Talleres360.Enums;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IVerificacionService _verificacionService;

        public UsuarioService(
            IUsuarioRepository userRepo,
            IPasswordService passwordService,
            IEmailService emailService,
            IVerificacionService verificacionService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
            _emailService = emailService;
            _verificacionService = verificacionService;
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
     int tallerId, string nombre, string email, string password, string? rutaImagen = null)
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
                Activo = false,
                Imagen = rutaImagen
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

            string tokenReal = await _verificacionService.GenerarTokenRegistroAsync(usuario.Id);

            string link = $"https://localhost:4200/auth/verify-email?token={tokenReal}";

            string filePath = Path.Combine(AppContext.BaseDirectory, "Templates", "EmailBienvenida.html");

            if (!File.Exists(filePath))
            {
                return (true, "Usuario creado, pero no se pudo enviar el correo (plantilla no encontrada).", usuario);
            }

            string template = await File.ReadAllTextAsync(filePath);

            string cuerpoHtml = template
                .Replace("{{Nombre}}", nombre)
                .Replace("{{Link}}", link);

            await _emailService.EnviarEmailAsync(email, "¡Bienvenido a Talleres360!", cuerpoHtml);

            return (true, "¡Todo listo! 🎉 Tu cuenta ha sido creada. Por seguridad, te hemos enviado un enlace de activación a tu email. ¡Nos vemos dentro!", usuario);
        }

        public async Task ActivarUsuarioAsync(int usuarioId)
        {
            await _userRepo.ActivarUsuarioAsync(usuarioId);
        }
    }
}