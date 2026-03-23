using Moq;
using Xunit;
using Talleres360.Dtos.Usuarios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Services.Seguridad;
using Talleres360.Enum;

namespace Talleres360.Tests.Services
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ITallerService> _tallerServiceMock;
        private readonly RefreshTokenService _sut;

        public RefreshTokenServiceTests()
        {
            _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            _usuarioRepoMock = new Mock<IUsuarioRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _tallerServiceMock = new Mock<ITallerService>();

            _sut = new RefreshTokenService(
                _refreshTokenRepoMock.Object,
                _usuarioRepoMock.Object,
                _tokenServiceMock.Object,
                _tallerServiceMock.Object
            );
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenNoExiste_DebeRetornarFail()
        {
            // Arrange
            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("noexiste"))
                .ReturnsAsync((TokenSeguridad)null!);

            // Act
            var result = await _sut.ValidarYRenovarAsync("noexiste");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenYaUsado_DebeRetornarFail()
        {
            // Arrange
            var token = new TokenSeguridad
            {
                Token = "usado",
                Usado = true,
                FechaExpiracion = DateTime.UtcNow.AddDays(1)
            };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("usado"))
                .ReturnsAsync(token);

            // Act
            var result = await _sut.ValidarYRenovarAsync("usado");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenExpirado_DebeRetornarFail()
        {
            // Arrange
            var token = new TokenSeguridad
            {
                Token = "expirado",
                Usado = false,
                FechaExpiracion = DateTime.UtcNow.AddDays(-1)
            };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("expirado"))
                .ReturnsAsync(token);

            // Act
            var result = await _sut.ValidarYRenovarAsync("expirado");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_EXPIRADO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_UsuarioIdNull_DebeRetornarFail()
        {
            // Arrange
            var token = new TokenSeguridad
            {
                Token = "valido",
                Usado = false,
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
                UsuarioId = null
            };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("valido"))
                .ReturnsAsync(token);

            // Act
            var result = await _sut.ValidarYRenovarAsync("valido");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_UsuarioInactivo_DebeRetornarFail()
        {
            // Arrange
            var token = new TokenSeguridad
            {
                Token = "valido",
                Usado = false,
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
                UsuarioId = 1
            };

            var usuario = new Usuario { Id = 1, Activo = false };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("valido"))
                .ReturnsAsync(token);

            _usuarioRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(usuario);

            // Act
            var result = await _sut.ValidarYRenovarAsync("valido");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarYRenovarAsync_TokenValido_DebeRetornarOkConSecurityStamp()
        {
            // Arrange
            string requestToken = "valido";

            var token = new TokenSeguridad
            {
                Token = requestToken,
                Usado = false,
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
                UsuarioId = 1
            };

            var usuario = new Usuario
            {
                Id = 1,
                Activo = true,
                Eliminado = false,
                Rol = RolesUsuario.ADMIN,
                TallerId = 10,
                Email = "test@test.com",
                Nombre = "Paco",
                SecurityStamp = "STAMP123"
            };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync(requestToken))
                .ReturnsAsync(token);

            _usuarioRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(usuario);

            _tallerServiceMock
                .Setup(x => x.VerificarPerfilConfiguradoAsync(10))
                .ReturnsAsync(true);

            _tokenServiceMock
                .Setup(x => x.GenerarJwtToken(It.IsAny<UsuarioLoginDto>()))
                .Returns("NUEVO_JWT");

            // Act
            var result = await _sut.ValidarYRenovarAsync(requestToken);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("NUEVO_JWT", result.Data.Token);
            Assert.True(token.Usado);

            _tokenServiceMock.Verify(x => x.GenerarJwtToken(
                It.Is<UsuarioLoginDto>(u => u.SecurityStamp == "STAMP123")), Times.Once);

            _refreshTokenRepoMock.Verify(x => x.ActualizarAsync(token), Times.Once);
        }

        [Fact]
        public async Task RevocarRefreshTokenAsync_TokenValido_LoMarcaComoUsado()
        {
            // Arrange
            string requestToken = "valido";
            var token = new TokenSeguridad { Token = requestToken, Usado = false };

            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync(requestToken))
                .ReturnsAsync(token);

            // Act
            await _sut.RevocarRefreshTokenAsync(requestToken);

            // Assert
            Assert.True(token.Usado);
            _refreshTokenRepoMock.Verify(x => x.ActualizarAsync(token), Times.Once);
        }

        [Fact]
        public async Task RevocarRefreshTokenAsync_TokenNoExiste_OkSinError()
        {
            // Arrange
            _refreshTokenRepoMock
                .Setup(x => x.ObtenerPorTokenAsync("noexiste"))
                .ReturnsAsync((TokenSeguridad)null!);

            // Act
            await _sut.RevocarRefreshTokenAsync("noexiste");

            // Assert
            _refreshTokenRepoMock.Verify(
                x => x.ActualizarAsync(It.IsAny<TokenSeguridad>()), Times.Never);
        }
    }
}