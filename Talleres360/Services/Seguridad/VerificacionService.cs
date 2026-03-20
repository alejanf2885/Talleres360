using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Talleres360.Configuration;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors; 
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
            IOptions<UrlSettings> urlSettings)
        {
            _verificacionRepo = verificacionRepo;
            _frontendBaseUrl = urlSettings.Value.FrontendUrl.TrimEnd('/');
        }

        public async Task<string> GenerarLinkVerificacionAsync(int usuarioId)
        {
            string token = await GenerarTokenRegistroAsync(usuarioId);
            return $"{_frontendBaseUrl}/auth/verify-email?token={token}";
        }

        public async Task<string> GenerarTokenRegistroAsync(int usuarioId)
        {
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

        public async Task<ServiceResult<int>> ValidarYConsumirTokenAsync(string token)
        {
            var verificacion = await _verificacionRepo.GetByTokenAsync(token);

            if (verificacion == null)
            {
                return ServiceResult<int>.Fail(
                    ErrorCode.AUTH_TOKEN_INVALIDO.ToString(),
                    "El enlace de verificación no es válido o ya ha sido usado.");
            }

            if (verificacion.FechaExpiracion < DateTime.UtcNow)
            {
                await _verificacionRepo.DeleteAsync(verificacion);

                return ServiceResult<int>.Fail(
                    ErrorCode.AUTH_TOKEN_EXPIRADO.ToString(),
                    "El enlace ha caducado. Por favor, solicita uno nuevo.");
            }

            int usuarioId = verificacion.UsuarioId;

            await _verificacionRepo.DeleteAsync(verificacion);

            return ServiceResult<int>.Ok(usuarioId);
        }
    }
}