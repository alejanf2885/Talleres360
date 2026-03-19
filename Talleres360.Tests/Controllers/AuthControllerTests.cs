using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Talleres360.Controllers;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Seguridad;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
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

            var httpContext = new DefaultHttpContext();
            _controller = new AuthController(
                _authServiceMock.Object,
                _registroTallerServiceMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private void SetRefreshTokenCookie(string? value)
        {
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] =
                value == null ? string.Empty : $"refreshToken={value}";
        }

        [Fact]
        public async Task Login_CredencialesValidas_DebeRetornarOkConTokens()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "Password123!" };
            var usuario = new Usuario { Id = 1, Nombre = "Admin", Email = "test@test.com", Rol = Enum.RolesUsuario.ADMIN, TallerId = 1 };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(ServiceResult<Usuario>.Ok(usuario));

            _tokenServiceMock
                .Setup(x => x.GenerarJwtToken(usuario))
                .Returns("jwt-token-valido");

            _refreshTokenServiceMock
                .Setup(x => x.CrearRefreshTokenAsync(usuario.Id))
                .ReturnsAsync("refresh-token-generado");

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var tokenProperty = value!.GetType().GetProperty("Token");
            Assert.NotNull(tokenProperty);
            Assert.Equal("jwt-token-valido", tokenProperty!.GetValue(value, null));
        }

        [Fact]
        public async Task Login_CredencialesInvalidas_DebeRetornarUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "MalPassword!" };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(ServiceResult<Usuario>.Fail(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), "Credenciales incorrectas."));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);

            var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
            Assert.Equal(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), error.Codigo);
            Assert.Equal("Credenciales incorrectas.", error.Mensaje);
        }

        [Fact]
        public async Task Login_CuentaInactiva_DebeRetornarForbidden()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "Password123!" };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(ServiceResult<Usuario>.Fail(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), "Cuenta inactiva."));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);

            var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
            Assert.Equal(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), error.Codigo);
            Assert.Equal("Cuenta inactiva.", error.Mensaje);
        }

        [Fact]
        public async Task Login_ServiceDevuelveSuccessFalseSinErrorCode_UsaErrorGenerico()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "x" };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(ServiceResult<Usuario>.Fail(null, null));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);

            var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
            Assert.Equal(ErrorCode.SYS_ERROR_GENERICO.ToString(), error.Codigo);
            Assert.Equal("Error de autenticación", error.Mensaje);
        }

        [Fact]
        public async Task Refresh_TokenValido_DebeRetornarNuevosTokens()
        {
            // Arrange
            var refreshTokenCookie = "token-valido";
            SetRefreshTokenCookie(refreshTokenCookie);

            var refreshResult = new TokenRefreshResult
            {
                Exito = true,
                NuevoJwtToken = "nuevo-jwt",
                NuevoRefreshToken = "nuevo-refresh"
            };

            _refreshTokenServiceMock
                .Setup(x => x.ValidarYRenovarAsync(refreshTokenCookie))
                .ReturnsAsync(refreshResult);

            // Act
            var result = await _controller.Refresh();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var tokenProperty = value!.GetType().GetProperty("Token");
            Assert.NotNull(tokenProperty);
            Assert.Equal("nuevo-jwt", tokenProperty!.GetValue(value, null));
        }

        [Fact]
        public async Task Refresh_TokenInvalido_DebeRetornarUnauthorizedConCodigoCorrecto()
        {
            // Arrange
            var refreshTokenCookie = "token-invalido";
            SetRefreshTokenCookie(refreshTokenCookie);

            var refreshResult = new TokenRefreshResult
            {
                Exito = false,
                MensajeError = "Refresh token no válido."
            };

            _refreshTokenServiceMock
                .Setup(x => x.ValidarYRenovarAsync(refreshTokenCookie))
                .ReturnsAsync(refreshResult);

            // Act
            var result = await _controller.Refresh();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);

            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_EXPIRADO.ToString(), error.Codigo);
            Assert.Equal("Refresh token no válido.", error.Mensaje);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Refresh_TokenVacioODeblanco_DebeRetornarUnauthorizedSinLlamarServicio(string token)
        {
            // Arrange
            SetRefreshTokenCookie(token);

            // Act
            var result = await _controller.Refresh();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);

            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(), error.Codigo);
            Assert.Equal("No hay token de refresco.", error.Mensaje);
            _refreshTokenServiceMock.Verify(x => x.ValidarYRenovarAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Refresh_SinCookie_DebeRetornarUnauthorized()
        {
            // Arrange
            SetRefreshTokenCookie(null);

            // Act
            var result = await _controller.Refresh();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);

            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(), error.Codigo);
            Assert.Equal("No hay token de refresco.", error.Mensaje);
            _refreshTokenServiceMock.Verify(x => x.ValidarYRenovarAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_ConToken_DebeLlamarRevocarYRetornarOk()
        {
            // Arrange
            var refreshTokenCookie = "token-a-revocar";
            SetRefreshTokenCookie(refreshTokenCookie);

            // Act
            var result = await _controller.Logout();

            // Assert
            _refreshTokenServiceMock.Verify(x => x.RevocarRefreshTokenAsync(refreshTokenCookie), Times.Once);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var mensajeProp = value!.GetType().GetProperty("Mensaje");
            Assert.Equal("Sesión cerrada correctamente.", mensajeProp!.GetValue(value, null));
        }

        [Fact]
        public async Task Logout_SinToken_NoDebeLlamarRevocarYRetornarOk()
        {
            // Arrange
            SetRefreshTokenCookie(null);

            // Act
            var result = await _controller.Logout();

            // Assert
            _refreshTokenServiceMock.Verify(x => x.RevocarRefreshTokenAsync(It.IsAny<string>()), Times.Never);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var mensajeProp = value!.GetType().GetProperty("Mensaje");
            Assert.Equal("Sesión cerrada correctamente.", mensajeProp!.GetValue(value, null));
        }
    }
}
