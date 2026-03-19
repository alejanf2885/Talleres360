using System.Security.Cryptography;
using Talleres360.Dtos.Seguridad;
using Talleres360.Dtos.Responses;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Enums.Errors;

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

        public async Task<ServiceResult<TokenResponseDto>> ValidarYRenovarAsync(string refreshToken)
        {
            TokenSeguridad tokenEntity = await _refreshTokenRepo.ObtenerPorTokenAsync(refreshToken);

            if (tokenEntity == null)
            {
                return ServiceResult<TokenResponseDto>.Fail(
                    ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), "El token de refresco no es válido.");
            }

            if (tokenEntity.Usado)
            {
                return ServiceResult<TokenResponseDto>.Fail(
                    ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), "Este token ya ha sido utilizado.");
            }

            if (tokenEntity.FechaExpiracion < DateTime.UtcNow)
            {
                return ServiceResult<TokenResponseDto>.Fail(
                    ErrorCode.AUTH_REFRESH_TOKEN_EXPIRADO.ToString(), "La sesión ha expirado.");
            }

            Usuario usuario = await _usuarioRepo.GetByIdAsync(tokenEntity.UsuarioId);
            if (usuario == null || !usuario.Activo)
            {
                return ServiceResult<TokenResponseDto>.Fail(
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), "El usuario ya no está activo o no existe.");
            }

            tokenEntity.Usado = true;
            await _refreshTokenRepo.ActualizarAsync(tokenEntity);

            string nuevoJwt = _tokenService.GenerarJwtToken(usuario);
            string nuevoRefreshToken = await CrearRefreshTokenAsync(usuario.Id);

            return ServiceResult<TokenResponseDto>.Ok(new TokenResponseDto
            {
                Token = nuevoJwt,
                RefreshToken = nuevoRefreshToken
            });
        }

        public async Task<ServiceResult<bool>> RevocarRefreshTokenAsync(string refreshToken)
        {
            try
            {
                TokenSeguridad tokenEntity = await _refreshTokenRepo.ObtenerPorTokenAsync(refreshToken);

                if (tokenEntity != null && !tokenEntity.Usado)
                {
                    tokenEntity.Usado = true;
                    await _refreshTokenRepo.ActualizarAsync(tokenEntity);
                }
                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.AUTH_LOGOUT_FALLIDO.ToString(), "No se pudo invalidar la sesión en el servidor.");
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