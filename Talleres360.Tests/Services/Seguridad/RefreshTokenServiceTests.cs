using Moq;
using Talleres360.Dtos.Seguridad;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Services.Seguridad;
using Xunit;

namespace Talleres360.Tests.Services.Seguridad
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly RefreshTokenService _service;

        public RefreshTokenServiceTests()
        {
            _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            _usuarioRepoMock = new Mock<IUsuarioRepository>();
            _tokenServiceMock = new Mock<ITokenService>();

            _service = new RefreshTokenService(
                _refreshTokenRepoMock.Object,
                _usuarioRepoMock.Object,
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task CrearRefreshTokenAsync_DebePersistirTokenYRetornarValor()
        {
            // Arrange
            const int usuarioId = 10;

            // Act
            string token = await _service.CrearRefreshTokenAsync(usuarioId);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
            _refreshTokenRepoMock.Verify(x => x.AgregarAsync(It.Is<TokenSeguridad>(t =>
                t.UsuarioId == usuarioId &&
                t.TipoToken == "REFRESH_TOKEN" &&
                !t.Usado &&
                t.FechaExpiracion > t.FechaCreacion)), Times.Once);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenInvalido_DebeRetornarError()
        {
            // Arrange
            _refreshTokenRepoMock.Setup(x => x.ObtenerPorTokenAsync("invalido"))
                .ReturnsAsync((TokenSeguridad?)null);

            // Act
            TokenRefreshResult result = await _service.ValidarYRenovarAsync("invalido");

            // Assert
            Assert.False(result.Exito);
            Assert.Equal("Refresh token no válido.", result.MensajeError);
            _tokenServiceMock.Verify(x => x.GenerarJwtToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenValido_DebeMarcarUsadoYGenerarNuevosTokens()
        {
            // Arrange
            var tokenEntity = new TokenSeguridad
            {
                UsuarioId = 1,
                Token = "token-valido",
                TipoToken = "REFRESH_TOKEN",
                FechaCreacion = DateTime.UtcNow.AddMinutes(-1),
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
                Usado = false
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = "admin@test.com",
                Nombre = "Admin",
                Activo = true,
                Eliminado = false,
                Rol = Enum.RolesUsuario.ADMIN
            };

            _refreshTokenRepoMock.Setup(x => x.ObtenerPorTokenAsync("token-valido"))
                .ReturnsAsync(tokenEntity);

            _usuarioRepoMock.Setup(x => x.GetByIdAsync(usuario.Id))
                .ReturnsAsync(usuario);

            _tokenServiceMock.Setup(x => x.GenerarJwtToken(usuario))
                .Returns("jwt-nuevo");

            // Act
            TokenRefreshResult result = await _service.ValidarYRenovarAsync("token-valido");

            // Assert
            Assert.True(result.Exito);
            Assert.Equal("jwt-nuevo", result.NuevoJwtToken);
            Assert.False(string.IsNullOrWhiteSpace(result.NuevoRefreshToken));

            _refreshTokenRepoMock.Verify(x => x.ActualizarAsync(It.Is<TokenSeguridad>(t => t.Token == "token-valido" && t.Usado)), Times.Once);
            _refreshTokenRepoMock.Verify(x => x.AgregarAsync(It.Is<TokenSeguridad>(t =>
                t.UsuarioId == usuario.Id &&
                t.TipoToken == "REFRESH_TOKEN" &&
                !t.Usado)), Times.Once);
        }

        [Fact]
        public async Task RevocarRefreshTokenAsync_TokenExistenteNoUsado_DebeMarcarloComoUsado()
        {
            // Arrange
            var tokenEntity = new TokenSeguridad
            {
                Token = "token-revocar",
                Usado = false
            };

            _refreshTokenRepoMock.Setup(x => x.ObtenerPorTokenAsync("token-revocar"))
                .ReturnsAsync(tokenEntity);

            // Act
            await _service.RevocarRefreshTokenAsync("token-revocar");

            // Assert
            _refreshTokenRepoMock.Verify(x => x.ActualizarAsync(It.Is<TokenSeguridad>(t => t.Token == "token-revocar" && t.Usado)), Times.Once);
        }
    }
}
