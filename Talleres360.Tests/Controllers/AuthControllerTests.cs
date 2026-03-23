using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Talleres360.Controllers;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Seguridad;
using Talleres360.Dtos.Usuarios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IRegistroTallerService> _registroMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _registroMock = new Mock<IRegistroTallerService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _refreshTokenMock = new Mock<IRefreshTokenService>();
            _userContextMock = new Mock<IUserContextService>();

            var httpContext = new DefaultHttpContext();
            _controller = new AuthController(
                _authServiceMock.Object,
                _registroMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenMock.Object,
                _userContextMock.Object)
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
        public async Task Login_CredencialesValidas_DebeRetornar200ConToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Password123!"
            };

            var usuarioDto = new UsuarioLoginDto
            {
                Id = 1,
                Email = "test@test.com",
                Nombre = "Juan",
                Rol = "ADMIN",
                TallerId = 1,
                SecurityStamp = "STAMP123"
            };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(request.Email, request.Password))
                .ReturnsAsync(ServiceResult<UsuarioLoginDto>.Ok(usuarioDto));

            _tokenServiceMock
                .Setup(x => x.GenerarJwtToken(usuarioDto))
                .Returns("jwt-token-valido");

            _refreshTokenMock
                .Setup(x => x.CrearRefreshTokenAsync(usuarioDto.Id))
                .ReturnsAsync("refresh-token");

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _tokenServiceMock.Verify(x => x.GenerarJwtToken(usuarioDto), Times.Once);
            _refreshTokenMock.Verify(x => x.CrearRefreshTokenAsync(usuarioDto.Id), Times.Once);
        }

        [Fact]
        public async Task Login_CuentaInactiva_DebeRetornar403()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", Password = "Password123!" };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(request.Email, request.Password))
                .ReturnsAsync(ServiceResult<UsuarioLoginDto>.Fail(
                    ErrorCode.AUTH_CUENTA_INACTIVA.ToString(),
                    "Cuenta inactiva"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(objectResult.Value);
            Assert.Equal(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), error.Codigo);
            _tokenServiceMock.Verify(x => x.GenerarJwtToken(It.IsAny<UsuarioLoginDto>()), Times.Never);
            _refreshTokenMock.Verify(x => x.CrearRefreshTokenAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Login_CredencialesIncorrectas_DebeRetornar401()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", Password = "WrongPassword!" };

            _authServiceMock
                .Setup(x => x.ValidarLoginAsync(request.Email, request.Password))
                .ReturnsAsync(ServiceResult<UsuarioLoginDto>.Fail(
                    ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    "Credenciales inválidas"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, objectResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(objectResult.Value);
            Assert.Equal(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), error.Codigo);
            _tokenServiceMock.Verify(x => x.GenerarJwtToken(It.IsAny<UsuarioLoginDto>()), Times.Never);
            _refreshTokenMock.Verify(x => x.CrearRefreshTokenAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Refresh_SinCookie_DebeRetornar401()
        {
            // Arrange
            SetRefreshTokenCookie(null);

            // Act
            var result = await _controller.Refresh();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);
            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(), error.Codigo);
            _refreshTokenMock.Verify(x => x.ValidarYRenovarAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Refresh_TokenValido_DebeRetornar200ConNuevoToken()
        {
            // Arrange
            SetRefreshTokenCookie("refresh-token");

            _refreshTokenMock
                .Setup(x => x.ValidarYRenovarAsync("refresh-token"))
                .ReturnsAsync(ServiceResult<TokenResponseDto>.Ok(new TokenResponseDto
                {
                    Token = "nuevo-jwt",
                    RefreshToken = "nuevo-refresh"
                }));

            // Act
            var result = await _controller.Refresh();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _refreshTokenMock.Verify(x => x.ValidarYRenovarAsync("refresh-token"), Times.Once);
        }

        [Fact]
        public async Task Refresh_TokenInvalido_DebeRetornar401()
        {
            // Arrange
            SetRefreshTokenCookie("refresh-invalido");

            _refreshTokenMock
                .Setup(x => x.ValidarYRenovarAsync("refresh-invalido"))
                .ReturnsAsync(ServiceResult<TokenResponseDto>.Fail(
                    ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(),
                    "Token inválido"));

            // Act
            var result = await _controller.Refresh();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);
            Assert.Equal(ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(), error.Codigo);
        }

        [Fact]
        public async Task Logout_ConToken_DebeRevocarYRetornar200()
        {
            // Arrange
            SetRefreshTokenCookie("refresh-token");

            _refreshTokenMock
                .Setup(x => x.RevocarRefreshTokenAsync("refresh-token"))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _refreshTokenMock.Verify(x => x.RevocarRefreshTokenAsync("refresh-token"), Times.Once);
        }

        [Fact]
        public async Task Logout_SinToken_DebeRetornar200SinLlamarAlRepo()
        {
            // Arrange
            SetRefreshTokenCookie(null);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _refreshTokenMock.Verify(x => x.RevocarRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAll_UsuarioAutenticado_DebeRevocarTodosYRetornar200()
        {
            // Arrange
            _userContextMock
                .Setup(x => x.GetUsuarioId())
                .Returns(1);

            _refreshTokenMock
                .Setup(x => x.RevocarTodosLosTokensAsync(1))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            var result = await _controller.LogoutAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _refreshTokenMock.Verify(x => x.RevocarTodosLosTokensAsync(1), Times.Once);
        }

        [Fact]
        public async Task Register_DatosCorrectos_DebeRetornar200()
        {
            // Arrange
            var request = new RegistroRequest
            {
                NombreTaller = "Taller Test",
                NombreAdmin = "Admin Test",
                Email = "nuevo@test.com",
                Password = "Password123!"
            };

            _registroMock
                .Setup(x => x.RegistrarNuevoClienteSaaSAsync(request))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Register_EmailDuplicado_DebeRetornar400()
        {
            // Arrange
            var request = new RegistroRequest
            {
                NombreTaller = "Taller Test",
                NombreAdmin = "Admin Test",
                Email = "duplicado@test.com",
                Password = "Password123!"
            };

            _registroMock
                .Setup(x => x.RegistrarNuevoClienteSaaSAsync(request))
                .ReturnsAsync(ServiceResult<bool>.Fail(
                    ErrorCode.REG_EMAIL_YA_REGISTRADO.ToString(),
                    "El email ya está registrado"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
            Assert.Equal(ErrorCode.REG_EMAIL_YA_REGISTRADO.ToString(), error.Codigo);
        }
    }
}
