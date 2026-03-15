using Talleres360.Dtos.Seguridad;
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

        public async Task<(bool PuedeAcceder, string Mensaje)> ValidarAccesoPremiumAsync(int tallerId)
        {
            var taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null) return (false, "Taller no encontrado.");

            if (!taller.Activo)
                return (false, "Tu cuenta de taller está desactivada.");

            if (taller.TipoSuscripcion != "PRO" && taller.TipoSuscripcion != "PREMIUM")
                return (false, "Esta funcionalidad requiere un plan Pro o Premium.");

            return (true, string.Empty);
        }

        public async Task<EstadoSuscripcionResponse> ObtenerEstadoSuscripcionAsync(int tallerId)
        {
            var taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
                return new EstadoSuscripcionResponse { EsActivo = false, Mensaje = "Taller no encontrado." };

            var response = new EstadoSuscripcionResponse
            {
                EsActivo = taller.Activo,
                TipoSuscripcion = taller.TipoSuscripcion
            };

            if (taller.TipoSuscripcion == "TRIAL")
            {
                var fechaExpiracion = taller.FechaCreacion.AddDays(30);
                var diasRestantes = (fechaExpiracion - DateTime.UtcNow).Days;
                
                response.DiasRestantesTrial = diasRestantes > 0 ? diasRestantes : 0;
                
                if (diasRestantes <= 0)
                {
                    response.EsActivo = false;
                    response.Mensaje = "Tu periodo de prueba ha finalizado.";
                }
                else if (diasRestantes <= 5)
                {
                    response.Mensaje = $"¡Atención! Te quedan {diasRestantes} días de prueba.";
                }
            }

            return response;
        }
    }
}