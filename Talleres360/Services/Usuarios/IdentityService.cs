using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres360.Data;
using Talleres360.Enum; // Para RolesUsuario
using Talleres360.Enums; // Para PlanTipo y LoginTipo
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Password;
using Talleres360.Models;

namespace Talleres360.Services.Usuarios
{
    public class IdentityService : IIdentityService
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly ITallerRepository _tallerRepo;
        private readonly IPlanRepository _planRepo;
        private readonly IPasswordService _passwordService;
        private readonly ApplicationDbContext _context;

        public IdentityService(
            IUsuarioRepository userRepo,
            ITallerRepository tallerRepo,
            IPlanRepository planRepo,
            IPasswordService passwordService,
            ApplicationDbContext context)
        {
            _userRepo = userRepo;
            _tallerRepo = tallerRepo;
            _planRepo = planRepo;
            _passwordService = passwordService;
            _context = context;
        }

        public async Task<(bool Success, string Message)> RegistrarTallerNuevoAsync(
            string nombre, string nombreAdmin, string email, string password)
        {
            // 1. Validación previa de email
            if (await _userRepo.ExisteEmailAsync(email))
                return (false, "Ese correo ya está registrado.");

            // 2. Iniciamos transacción para asegurar que no se creen datos huérfanos
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // A. Buscamos el plan PRO usando el Enum para el Reverse Trial
                Plan? plan = await _planRepo.GetPlanPorNombreAsync(PlanTipo.PRO.ToString());
                if (plan == null)
                    return (false, "Error: El plan PRO no está configurado en la base de datos.");

                // B. Creamos el taller marcado como TRIAL
                // Generamos CIF temporal único para evitar conflictos con la restricción UNIQUE
                string cifTemporal = $"TEMP{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}".Substring(0, 20);

                Taller taller = new Taller
                {
                    Nombre = nombre,
                    PlanId = plan.Id,
                    CIF = cifTemporal,
                    TipoSuscripcion = "TRIAL", // Activamos los 30 días de prueba
                    Activo = true,
                    PerfilConfigurado = false,
                    FechaCreacion = DateTime.UtcNow // Estándar UTC
                };

                await _tallerRepo.AddAsync(taller);
                await _context.SaveChangesAsync();

                // C. Creamos el usuario Administrador vinculado al Taller
                Usuario usuario = new Usuario
                {
                    TallerId = taller.Id,
                    Nombre = nombreAdmin,
                    Email = email,
                    Rol = RolesUsuario.ADMIN,
                    FechaCreacion = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Activo = true
                };

                await _userRepo.AddAsync(usuario);
                await _context.SaveChangesAsync();

                // D. Creamos la credencial usando el Enum LoginTipo
                Credencial credencial = new Credencial
                {
                    UsuarioId = usuario.Id,
                    PasswordHash = _passwordService.HashPassword(password),
                    TipoInicioSesion = LoginTipo.LOCAL.ToString() // Evitamos strings hardcodeados
                };

                await _userRepo.AddCredencialAsync(credencial);
                await _context.SaveChangesAsync();

                // E. Todo correcto, consolidamos cambios
                await transaction.CommitAsync();

                return (true, "Taller y administrador registrados con éxito.");
            }
            catch (Exception ex)
            {
                // Si algo falla, se deshace todo (Rollback automático del taller y usuario)
                await transaction.RollbackAsync();
                return (false, "Error crítico en el registro: " + ex.Message);
            }
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