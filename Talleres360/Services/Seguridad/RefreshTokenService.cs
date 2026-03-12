using System.Security.Cryptography;
using Talleres360.Dtos.Seguridad;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios; 
using Talleres360.Models;

namespace Talleres360.Services.Seguridad
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IUsuarioRepository _usuarioRepo; 
        private readonly ITokenService _tokenService;

        public RefreshTokenService(
            IRefreshTokenRepository refreshTokenRepo,
            IUsuarioRepository usuarioRepo, 
            ITokenService tokenService)
        {
            _refreshTokenRepo = refreshTokenRepo;
            _usuarioRepo = usuarioRepo;
            _tokenService = tokenService;
        }

        public async Task<string> CrearRefreshTokenAsync(int usuarioId)
        {
            string refreshToken = GenerarTokenAleatorio();

            TokenSeguridad tokenEntity = new TokenSeguridad
            {
                UsuarioId = usuarioId,
                Token = refreshToken,
                TipoToken = "REFRESH_TOKEN",
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(7),
                Usado = false
            };

            await _refreshTokenRepo.AgregarAsync(tokenEntity);
            return refreshToken;
        }

        public async Task<TokenRefreshResult> ValidarYRenovarAsync(string refreshToken)
        {
            TokenRefreshResult result = new TokenRefreshResult();
            TokenSeguridad tokenEntity = await _refreshTokenRepo.ObtenerPorTokenAsync(refreshToken);

            if (tokenEntity == null)
            {
                result.Exito = false; result.MensajeError = "Refresh token no válido."; return result;
            }

            if (tokenEntity.Usado)
            {
                result.Exito = false; result.MensajeError = "Este token ya fue utilizado."; return result;
            }

            if (tokenEntity.FechaExpiracion < DateTime.UtcNow)
            {
                result.Exito = false; result.MensajeError = "El refresh token ha expirado."; return result;
            }

            var usuario = await _usuarioRepo.GetByIdAsync(tokenEntity.UsuarioId);

            if (usuario == null || !usuario.Activo || usuario.Eliminado)
            {
                result.Exito = false; result.MensajeError = "Usuario no válido."; return result;
            }

            tokenEntity.Usado = true;
            await _refreshTokenRepo.ActualizarAsync(tokenEntity);

            string nuevoJwt = _tokenService.GenerarJwtToken(usuario);
            string nuevoRefreshToken = await CrearRefreshTokenAsync(usuario.Id);

            result.Exito = true;
            result.NuevoJwtToken = nuevoJwt;
            result.NuevoRefreshToken = nuevoRefreshToken;

            return result;
        }

        public async Task RevocarRefreshTokenAsync(string refreshToken)
        {
            TokenSeguridad tokenEntity = await _refreshTokenRepo.ObtenerPorTokenAsync(refreshToken);

            if (tokenEntity != null && !tokenEntity.Usado)
            {
                tokenEntity.Usado = true;
                await _refreshTokenRepo.ActualizarAsync(tokenEntity);
            }
        }

        private string GenerarTokenAleatorio()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}