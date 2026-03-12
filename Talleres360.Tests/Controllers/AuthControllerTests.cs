using Microsoft.AspNetCore.Mvc;
using Moq;
using Talleres360.API.Controllers;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Seguridad;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Xunit;

namespace Talleres360.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRegistroTallerService> _registroTallerServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _registroTallerServiceMock = new Mock<IRegistroTallerService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

            _controller = new AuthController(
                _authServiceMock.Object,
                _registroTallerServiceMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenServiceMock.Object
            );
        }

        [Fact]
        public async Task Login_CredencialesValidas_DebeRetornarOkConTokens()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "Password123!" };
            var usuario = new Usuario { Id = 1, Nombre = "Admin", Email = "test@test.com", Rol = Enum.RolesUsuario.ADMIN, TallerId = 1 };
            
            _authServiceMock.Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(usuario);

            _tokenServiceMock.Setup(x => x.GenerarJwtToken(usuario))
                .Returns("jwt-token-valido");

            _refreshTokenServiceMock.Setup(x => x.CrearRefreshTokenAsync(usuario.Id))
                .ReturnsAsync("refresh-token-generado");

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            
            // Usando reflection o dinámico para obtener las propiedades del objeto anónimo
            var tokenProperty = value.GetType().GetProperty("Token");
            var refreshTokenProperty = value.GetType().GetProperty("RefreshToken");

            Assert.NotNull(tokenProperty);
            Assert.NotNull(refreshTokenProperty);

            Assert.Equal("jwt-token-valido", tokenProperty.GetValue(value, null));
            Assert.Equal("refresh-token-generado", refreshTokenProperty.GetValue(value, null));
        }

        [Fact]
        public async Task Login_CredencialesInvalidas_DebeRetornarUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "MalPassword!" };
            
            _authServiceMock.Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;
            var errorProp = value.GetType().GetProperty("Error");
            Assert.Equal("Credenciales incorrectas o cuenta inactiva.", errorProp.GetValue(value, null));
        }

        [Fact]
        public async Task Refresh_TokenValido_DebeRetornarNuevosTokens()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "token-valido" };
            var refreshResult = new TokenRefreshResult
            {
                Exito = true,
                NuevoJwtToken = "nuevo-jwt",
                NuevoRefreshToken = "nuevo-refresh"
            };

            _refreshTokenServiceMock.Setup(x => x.ValidarYRenovarAsync(request.RefreshToken))
                .ReturnsAsync(refreshResult);

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var tokenProperty = value.GetType().GetProperty("Token");
            var refreshTokenProperty = value.GetType().GetProperty("RefreshToken");

            Assert.Equal("nuevo-jwt", tokenProperty.GetValue(value, null));
            Assert.Equal("nuevo-refresh", refreshTokenProperty.GetValue(value, null));
        }

        [Fact]
        public async Task Refresh_TokenInvalido_DebeRetornarUnauthorized()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "token-invalido" };
            var refreshResult = new TokenRefreshResult
            {
                Exito = false,
                MensajeError = "Refresh token no válido."
            };

            _refreshTokenServiceMock.Setup(x => x.ValidarYRenovarAsync(request.RefreshToken))
                .ReturnsAsync(refreshResult);

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;
            var errorProp = value.GetType().GetProperty("Error");
            
            Assert.Equal("Refresh token no válido.", errorProp.GetValue(value, null));
        }

        [Fact]
        public async Task Refresh_SinToken_DebeRetornarBadRequest()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "" };

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;
            var errorProp = value.GetType().GetProperty("Error");

            Assert.Equal("El Refresh Token es obligatorio.", errorProp.GetValue(value, null));
            _refreshTokenServiceMock.Verify(x => x.ValidarYRenovarAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_ConToken_DebeLlamarRevocarYRetornarOk()
        {
            // Arrange
            var request = new LogoutRequest { RefreshToken = "token-a-revocar" };

            // Act
            var result = await _controller.Logout(request);

            // Assert
            _refreshTokenServiceMock.Verify(x => x.RevocarRefreshTokenAsync("token-a-revocar"), Times.Once);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var mensajeProp = value.GetType().GetProperty("Mensaje");
            Assert.Equal("Sesión cerrada correctamente.", mensajeProp.GetValue(value, null));
        }

        [Fact]
        public async Task Logout_SinToken_NoDebeLlamarRevocarYRetornarOk()
        {
            // Arrange
            var request = new LogoutRequest { RefreshToken = "" };

            // Act
            var result = await _controller.Logout(request);

            // Assert
            _refreshTokenServiceMock.Verify(x => x.RevocarRefreshTokenAsync(It.IsAny<string>()), Times.Never);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var mensajeProp = value.GetType().GetProperty("Mensaje");
            Assert.Equal("Sesión cerrada correctamente.", mensajeProp.GetValue(value, null));
        }
    }
}
