using Talleres360.Dtos.Seguridad;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Enums.Errors;

namespace Talleres360.Services.Talleres
{
    public class SuscripcionGuardService : ISuscripcionGuardService
    {
        private readonly ITallerRepository _tallerRepo;
        private readonly IConfiguration _config;

        public SuscripcionGuardService(
            ITallerRepository tallerRepo,
            IConfiguration config)
        {
            _tallerRepo = tallerRepo;
            _config = config;
        }

        public async Task<AccesoResult> ValidarAccesoEscrituraAsync(int tallerId)
        {
            if (!_config.GetValue<bool>("Stripe:Enabled"))
                return AccesoResult.Permitido();

            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
                return AccesoResult.Denegado(
                    "Taller no encontrado.",
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString());

            if (taller.TipoSuscripcion == "TRIAL")
            {
                DateTime fechaExpiracion = taller.FechaCreacion.AddDays(30);
                if (DateTime.UtcNow > fechaExpiracion)
                    return AccesoResult.Denegado(
                        "Tu periodo de prueba de 30 días ha finalizado. Elige un plan para continuar.",
                        ErrorCode.SUBS_SIN_PLAN_ACTIVO.ToString());
            }

            if (!taller.Activo)
                return AccesoResult.Denegado(
                    "Tu cuenta de taller está desactivada. Contacta con soporte.",
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString());

            return AccesoResult.Permitido();
        }

        public async Task<AccesoResult> ValidarAccesoPremiumAsync(int tallerId)
        {
            if (!_config.GetValue<bool>("Stripe:Enabled"))
                return AccesoResult.Permitido();

            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
                return AccesoResult.Denegado(
                    "Taller no encontrado.",
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString());

            if (!taller.Activo)
                return AccesoResult.Denegado(
                    "Tu cuenta de taller está desactivada.",
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString());

            if (taller.TipoSuscripcion != "PRO" && taller.TipoSuscripcion != "PREMIUM")
                return AccesoResult.Denegado(
                    "Esta funcionalidad requiere un plan Pro o Premium.",
                    ErrorCode.SUBS_LIMITE_ALCANZADO.ToString());

            return AccesoResult.Permitido();
        }

        public async Task<EstadoSuscripcionResponse> ObtenerEstadoSuscripcionAsync(int tallerId)
        {
            if (!_config.GetValue<bool>("Stripe:Enabled"))
                return new EstadoSuscripcionResponse
                {
                    EsActivo = true,
                    TipoSuscripcion = "BETA",
                    Mensaje = "Acceso beta activo."
                };

            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
                return new EstadoSuscripcionResponse
                {
                    EsActivo = false,
                    Mensaje = "Taller no encontrado."
                };

            EstadoSuscripcionResponse response = new EstadoSuscripcionResponse
            {
                EsActivo = taller.Activo,
                TipoSuscripcion = taller.TipoSuscripcion
            };

            if (taller.TipoSuscripcion == "TRIAL")
            {
                DateTime fechaExpiracion = taller.FechaCreacion.AddDays(30);
                TimeSpan diferencia = fechaExpiracion - DateTime.UtcNow;
                int diasRestantes = diferencia.Days;

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