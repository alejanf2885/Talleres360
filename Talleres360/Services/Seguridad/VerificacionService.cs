using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Talleres360.Configuration;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Seguridad
{
    public class VerificacionService : IVerificacionService
    {
        private readonly IVerificacionRepository _verificacionRepo;
        private readonly string _frontendBaseUrl;

        public VerificacionService(
            IVerificacionRepository verificacionRepo,
            IOptions<UrlSettings> urlSettings) // ✅ Inyectamos las opciones de configuración
        {
            _verificacionRepo = verificacionRepo;
            // Limpiamos la URL por si acaso alguien puso una '/' al final en el JSON
            _frontendBaseUrl = urlSettings.Value.FrontendUrl.TrimEnd('/');
        }

        /// <summary>
        /// Genera el token y construye la URL completa para el frontend
        /// </summary>
        public async Task<string> GenerarLinkVerificacionAsync(int usuarioId)
        {
            // 1. Reutilizamos la lógica de generar el token y guardarlo
            string token = await GenerarTokenRegistroAsync(usuarioId);

            // 2. Construimos el link final
            // Estructura: http://localhost:4200/auth/verify-email?token=...
            return $"{_frontendBaseUrl}/auth/verify-email?token={token}";
        }

        public async Task<string> GenerarTokenRegistroAsync(int usuarioId)
        {
            // Generación de token criptográficamente seguro
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");

            UsuarioVerificacion verificacion = new UsuarioVerificacion
            {
                UsuarioId = usuarioId,
                Token = token,
                Tipo = "REGISTRO",
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(24)
            };

            await _verificacionRepo.AddAsync(verificacion);

            return token;
        }

        public async Task<(bool Exito, string Mensaje, int? UsuarioId)> ValidarYConsumirTokenAsync(string token)
        {
            var verificacion = await _verificacionRepo.GetByTokenAsync(token);

            if (verificacion == null)
                return (false, "El enlace de verificación no es válido o ya ha sido usado.", null);

            if (verificacion.FechaExpiracion < DateTime.UtcNow)
            {
                await _verificacionRepo.DeleteAsync(verificacion);
                return (false, "El enlace ha caducado. Por favor, solicita uno nuevo.", null);
            }

            int usuarioId = verificacion.UsuarioId;

            await _verificacionRepo.DeleteAsync(verificacion);
            return (true, "Token válido", usuarioId);
        }
    }
}