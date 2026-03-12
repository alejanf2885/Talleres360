using Microsoft.EntityFrameworkCore.Storage;
using Talleres360.Data;
using Talleres360.Enums;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Talleres
{
    public class RegistroTallerService : IRegistroTallerService
    {
        private readonly ITallerRepository _tallerRepo;
        private readonly IPlanRepository _planRepo;
        private readonly IUsuarioService _usuarioService;
        private readonly ApplicationDbContext _context;

        public RegistroTallerService(
            ITallerRepository tallerRepo,
            IPlanRepository planRepo,
            IUsuarioService usuarioService,
            ApplicationDbContext context)
        {
            _tallerRepo = tallerRepo;
            _planRepo = planRepo;
            _usuarioService = usuarioService;
            _context = context;
        }

        public async Task<(bool Success, string Message)> RegistrarNuevoClienteSaaSAsync(
            string nombreTaller, string nombreAdmin, string email, string password)
        {
            // Iniciamos transacción para asegurar que no se creen datos huérfanos
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
                    Nombre = nombreTaller,
                    PlanId = plan.Id,
                    CIF = cifTemporal,
                    TipoSuscripcion = "TRIAL", // Activamos los 30 días de prueba
                    Activo = true,
                    PerfilConfigurado = false,
                    FechaCreacion = DateTime.UtcNow // Estándar UTC
                };

                await _tallerRepo.AddAsync(taller);
                await _context.SaveChangesAsync();

                // C. Delegamos en UsuarioService la creación del administrador
                var resultUsuario = await _usuarioService.CrearUsuarioAdminAsync(
                    taller.Id, nombreAdmin, email, password);

                if (!resultUsuario.Success)
                {
                    await transaction.RollbackAsync();
                    return (false, resultUsuario.Message);
                }

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
    }
}