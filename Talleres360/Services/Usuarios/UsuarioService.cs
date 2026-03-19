using Talleres360.Dtos.Responses;
using Talleres360.Enum;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
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
        private readonly INotificacionService _notificacionService; 
        private readonly IVerificacionService _verificacionService;

        public UsuarioService(
            IUsuarioRepository userRepo,
            IPasswordService passwordService,
            INotificacionService notificacionService,
            IVerificacionService verificacionService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
            _notificacionService = notificacionService;
            _verificacionService = verificacionService;
        }

        public async Task<ServiceResult<Usuario>> GetByEmailAsync(string email)
        {
            Usuario? usuario = await _userRepo.GetByEmailAsync(email);
            if (usuario == null)
                return ServiceResult<Usuario>.Fail(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Usuario no encontrado.");

            return ServiceResult<Usuario>.Ok(usuario);
        }

        public async Task<ServiceResult<Usuario>> GetByIdAsync(int id)
        {
            Usuario? usuario = await _userRepo.GetByIdAsync(id);
            if (usuario == null)
                return ServiceResult<Usuario>.Fail(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Usuario no encontrado.");

            return ServiceResult<Usuario>.Ok(usuario);
        }

        public async Task<ServiceResult<bool>> ActualizarUltimoAccesoAsync(int usuarioId)
        {
            await _userRepo.ActualizarUltimoAccesoAsync(usuarioId);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<Usuario>> CrearUsuarioAdminAsync(
      int tallerId, string nombre, string email, string password, string? rutaImagen = null)
        {
            if (await _userRepo.ExisteEmailAsync(email))
            {
                return ServiceResult<Usuario>.Fail(ErrorCode.REG_EMAIL_YA_REGISTRADO.ToString(), "El email ya existe.");
            }

            try
            {
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
                int filasAfectadasUser = await _userRepo.SaveChangesAsync();

                if (filasAfectadasUser == 0 || usuario.Id == 0)
                {
                    return ServiceResult<Usuario>.Fail(ErrorCode.REG_ERROR_CREACION_USUARIO.ToString(), "No se pudo guardar el perfil de usuario.");
                }

                Credencial credencial = new Credencial
                {
                    UsuarioId = usuario.Id,
                    PasswordHash = _passwordService.HashPassword(password),
                    TipoInicioSesion = LoginTipo.LOCAL.ToString()
                };

                await _userRepo.AddCredencialAsync(credencial);
                int filasAfectadasCred = await _userRepo.SaveChangesAsync();

                if (filasAfectadasCred == 0)
                {
                    return ServiceResult<Usuario>.Fail(ErrorCode.REG_ERROR_CREACION_USUARIO.ToString(), "Error al generar las credenciales.");
                }

                try
                {
                    string token = await _verificacionService.GenerarTokenRegistroAsync(usuario.Id);
                    await _notificacionService.EnviarBienvenidaAsync(usuario, token);
                }
                catch (Exception ex)
                {
                }

                return ServiceResult<Usuario>.Ok(usuario);
            }
            catch (Exception ex)
            {
                return ServiceResult<Usuario>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Fallo crítico en la base de datos.");
            }
        }

        public async Task<ServiceResult<bool>> ActivarUsuarioAsync(int usuarioId)
        {
            await _userRepo.ActivarUsuarioAsync(usuarioId);
            return ServiceResult<bool>.Ok(true);
        }
    }
}