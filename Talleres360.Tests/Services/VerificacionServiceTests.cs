using Microsoft.Extensions.Options;
using Moq;
using Talleres360.Configuration;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;
using Talleres360.Services.Seguridad;

namespace Talleres360.Tests.Services
{
    public class VerificacionServiceTests
    {
        private readonly Mock<IVerificacionRepository> _verificacionRepoMock;
        private readonly VerificacionService _sut;

        public VerificacionServiceTests()
        {
            _verificacionRepoMock = new Mock<IVerificacionRepository>();

            UrlSettings settings = new UrlSettings
            {
                FrontendUrl = "https://frontend.test"
            };

            _sut = new VerificacionService(
                _verificacionRepoMock.Object,
                Options.Create(settings)
            );
        }

        [Fact]
        public async Task GenerarTokenRegistroAsync_GeneraTokenUrlSafe()
        {
            // Arrange
            UsuarioVerificacion? entidadGuardada = null;

            _verificacionRepoMock
                .Setup(x => x.AddAsync(It.IsAny<UsuarioVerificacion>()))
                .Callback<UsuarioVerificacion>(token => entidadGuardada = token)
                .Returns(Task.CompletedTask);

            // Act
            string token = await _sut.GenerarTokenRegistroAsync(1);

            // Assert
            Assert.DoesNotContain("+", token);
            Assert.DoesNotContain("/", token);
            Assert.DoesNotContain("=", token);
            Assert.NotNull(entidadGuardada);
            Assert.Equal(token, entidadGuardada!.Token);
            _verificacionRepoMock.Verify(x => x.AddAsync(It.IsAny<UsuarioVerificacion>()), Times.Once);
        }

        [Fact]
        public async Task ValidarYConsumirTokenAsync_TokenNoExiste_RetornaFail()
        {
            // Arrange
            _verificacionRepoMock
                .Setup(x => x.GetByTokenAsync("token-inexistente"))
                .ReturnsAsync((UsuarioVerificacion?)null);

            // Act
            var resultado = await _sut.ValidarYConsumirTokenAsync("token-inexistente");

            // Assert
            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.AUTH_TOKEN_INVALIDO.ToString(), resultado.ErrorCode);
            _verificacionRepoMock.Verify(x => x.DeleteAsync(It.IsAny<UsuarioVerificacion>()), Times.Never);
        }

        [Fact]
        public async Task ValidarYConsumirTokenAsync_TokenExpirado_RetornaFailYSeElimina()
        {
            // Arrange
            var token = new UsuarioVerificacion
            {
                UsuarioId       = 1,
                Token           = "expirado",
                Tipo            = "EMAIL",
                FechaCreacion   = DateTime.UtcNow.AddHours(-2),
                FechaExpiracion = DateTime.UtcNow.AddMinutes(-10)
            };

            _verificacionRepoMock
                .Setup(x => x.GetByTokenAsync("expirado"))
                .ReturnsAsync(token);

            // Act
            var resultado = await _sut.ValidarYConsumirTokenAsync("expirado");

            // Assert
            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.AUTH_TOKEN_EXPIRADO.ToString(), resultado.ErrorCode);
            _verificacionRepoMock.Verify(x => x.DeleteAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidarYConsumirTokenAsync_TokenValido_RetornaOkConUsuarioId()
        {
            // Arrange
            var token = new UsuarioVerificacion
            {
                UsuarioId       = 5,
                Token           = "valido",
                Tipo            = "EMAIL",
                FechaCreacion   = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(1)
            };

            _verificacionRepoMock
                .Setup(x => x.GetByTokenAsync("valido"))
                .ReturnsAsync(token);

            // Act
            var resultado = await _sut.ValidarYConsumirTokenAsync("valido");

            // Assert
            Assert.True(resultado.Success);
            Assert.Equal(5, resultado.Data);
            _verificacionRepoMock.Verify(x => x.DeleteAsync(token), Times.Once);
        }
    }
}
