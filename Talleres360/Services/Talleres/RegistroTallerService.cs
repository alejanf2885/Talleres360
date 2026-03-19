using Microsoft.EntityFrameworkCore.Storage;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Imagenes;
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
        private readonly IImagenService _imagenService;
        private readonly ApplicationDbContext _context;

        public RegistroTallerService(
            ITallerRepository tallerRepo,
            IPlanRepository planRepo,
            IUsuarioService usuarioService,
            IImagenService imagenService,
            ApplicationDbContext context)
        {
            _tallerRepo = tallerRepo;
            _planRepo = planRepo;
            _usuarioService = usuarioService;
            _imagenService = imagenService;
            _context = context;
        }

        public async Task<ServiceResult<bool>> RegistrarNuevoClienteSaaSAsync(RegistroRequest request)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Plan? plan = await _planRepo.GetPlanPorNombreAsync(PlanTipo.PRO.ToString());
                if (plan == null)
                {
                    return ServiceResult<bool>.Fail(
                        ErrorCode.REG_PLAN_NO_ENCONTRADO.ToString(),
                        "El plan de suscripción no está configurado en el sistema."
                    );
                }

                string cifTemporal = $"TEMP{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}".Substring(0, 20);

                Taller taller = new Taller
                {
                    Nombre = request.NombreTaller,
                    PlanId = plan.Id,
                    CIF = cifTemporal,
                    TipoSuscripcion = "TRIAL",
                    Activo = true,
                    PerfilConfigurado = false,
                    FechaCreacion = DateTime.UtcNow
                };

                await _tallerRepo.AddAsync(taller);
                await _context.SaveChangesAsync();

                string? rutaImagen = null;
                if (!string.IsNullOrWhiteSpace(request.Imagen))
                {
                    string? resultImagen = await _imagenService.SubirImagenBase64Async(request.Imagen, CarpetaDestino.Usuarios);

                    if (string.IsNullOrEmpty(resultImagen))
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<bool>.Fail(
                            ErrorCode.REG_ERROR_SUBIDA_IMAGEN.ToString(),
                            "Error al procesar la imagen de perfil."
                        );
                    }
                    rutaImagen = resultImagen;
                }

                ServiceResult<Usuario> resultUsuario = await _usuarioService.CrearUsuarioAdminAsync(
                    taller.Id,
                    request.NombreAdmin,
                    request.Email,
                    request.Password,
                    rutaImagen);

                if (!resultUsuario.Success)
                {
                    await transaction.RollbackAsync();

                    return ServiceResult<bool>.Fail(
                        resultUsuario.ErrorCode ?? ErrorCode.REG_ERROR_CREACION_USUARIO.ToString(),
                        resultUsuario.Message ?? "Error al crear el usuario administrador."
                    );
                }

                await transaction.CommitAsync();

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Ocurrió un error crítico durante el registro: " + ex.Message
                );
            }
        }
    }
}