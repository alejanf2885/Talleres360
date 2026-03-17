using Talleres360.Models;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Enums;

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

        public async Task<bool> ConfigurarPerfilAsync(int tallerId, string cif, string direccion, string localidad, string telefono, string? logoBase64)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);

            if (taller == null)
            {
                return false;
            }

            taller.CIF = cif;
            taller.Direccion = direccion;
            taller.Localidad = localidad;
            taller.Telefono = telefono;

            taller.PerfilConfigurado = true;
            taller.FechaActualizacion = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(logoBase64))
            {
                taller.Logo = await _imagenService.SubirImagenBase64Async(logoBase64, CarpetaDestino.Talleres);
            }

            await _tallerRepo.UpdateAsync(taller);

            return true;
        }

        public async Task<Taller> CrearTallerBaseAsync(string nombre, int planId)
        {
            Taller taller = new Taller
            {
                Nombre = nombre,
                PlanId = planId,
                Activo = true,
                PerfilConfigurado = false,
                FechaCreacion = DateTime.Now
            };

            await _tallerRepo.AddAsync(taller);
            return taller;
        }
    }
}