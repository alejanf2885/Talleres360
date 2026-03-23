using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Talleres;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Services.Talleres
{
    public class TallerService : ITallerService
    {
        private readonly ITallerRepository _tallerRepo;
        private readonly IImagenService _imagenService;

        public TallerService(ITallerRepository tallerRepo, IImagenService imagenService)
        {
            _tallerRepo = tallerRepo;
            _imagenService = imagenService;
        }

        public async Task<ServiceResult<WorkshopDto>> ObtenerTallerPorIdAsync(int tallerId)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);

            if (taller == null)
            {
                return ServiceResult<WorkshopDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "El taller solicitado no existe."
                );
            }

            WorkshopDto dto = new WorkshopDto
            {
                Id = taller.Id,
                Nombre = taller.Nombre,
                CIF = taller.Cif,
                Direccion = taller.Direccion,
                Localidad = taller.Localidad,
                Telefono = taller.Telefono,
                PerfilConfigurado = taller.PerfilConfigurado,
                TipoSuscripcion = taller.TipoSuscripcion,
                Logo = taller.Logo
            };

            return ServiceResult<WorkshopDto>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> ConfigurarPerfilAsync(int tallerId, ConfigurarTallerRequest request)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Taller no encontrado."
                );
            }

            Taller? tallerConMismoCif = await _tallerRepo.GetByCifAsync(request.CIF);
            if (tallerConMismoCif != null && tallerConMismoCif.Id != tallerId)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.REG_CIF_DUPLICADO.ToString(),
                    "El CIF introducido ya está registrado por otro taller."
                );
            }

            taller.Cif = request.CIF;
            taller.Direccion = request.Direccion;
            taller.Localidad = request.Localidad;
            taller.Telefono = request.Telefono;
            taller.PerfilConfigurado = true;
            taller.FechaActualizacion = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Logo))
            {
                string? urlLogo = await _imagenService.SubirImagenBase64Async(request.Logo, CarpetaDestino.Talleres);
                if (!string.IsNullOrEmpty(urlLogo))
                {
                    taller.Logo = urlLogo;
                }
            }

            await _tallerRepo.UpdateAsync(taller);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<Taller>> CrearTallerBaseAsync(string nombre, int planId)
        {
            Taller taller = new Taller
            {
                Nombre = nombre,
                PlanId = planId,
                Activo = true,
                PerfilConfigurado = false,
                FechaCreacion = DateTime.UtcNow
            };

            await _tallerRepo.AddAsync(taller);

            return ServiceResult<Taller>.Ok(taller);
        }

        public async Task<bool> VerificarPerfilConfiguradoAsync(int tallerId)
        {
            return await _tallerRepo.IsPerfilConfiguradoAsync(tallerId);
        }

        public async Task<bool> ExistsByCifAsync(string cif)
        {
            return await _tallerRepo.ExistsByCifAsync(cif);
        }


    }
}