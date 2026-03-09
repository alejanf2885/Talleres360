using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Services.Talleres
{
    public class SuscripcionGuardService : ISuscripcionGuardService
    {

        private readonly ITallerRepository _tallerRepo;

        public SuscripcionGuardService(ITallerRepository tallerRepo)
        {
            _tallerRepo = tallerRepo;
        }

        public async Task<(bool PuedeAcceder, string Mensaje)> ValidarAccesoEscrituraAsync(int tallerId)
        {
            var taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null) return (false, "Taller no encontrado.");

            // 1. Verificar si el Trial ha caducado
            if (taller.TipoSuscripcion == "TRIAL")
            {
                var fechaExpiracion = taller.FechaCreacion.AddDays(30);
                if (DateTime.UtcNow > fechaExpiracion)
                {
                    return (false, "Tu periodo de prueba de 30 días ha finalizado. Elige un plan para continuar.");
                }
            }

            // 2. Verificar si el taller está activo 
            if (!taller.Activo)
            {
                return (false, "Tu cuenta de taller está desactivada. Contacta con soporte.");
            }

            return (true, string.Empty);
        }
    }
}