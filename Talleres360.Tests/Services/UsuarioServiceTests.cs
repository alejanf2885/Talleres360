using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Services.Usuarios;

namespace Talleres360.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _userRepoMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<INotificacionService> _notificacionServiceMock;
        private readonly Mock<IVerificacionService> _verificacionServiceMock;
        private readonly Mock<ILogger<UsuarioService>> _loggerMock;
        private readonly UsuarioService _sut;

        public UsuarioServiceTests()
        {
            _userRepoMock = new Mock<IUsuarioRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _notificacionServiceMock = new Mock<INotificacionService>();
            _verificacionServiceMock = new Mock<IVerificacionService>();
            _loggerMock = new Mock<ILogger<UsuarioService>>();

            _sut = new UsuarioService(
                _userRepoMock.Object,
                _passwordServiceMock.Object,
                _notificacionServiceMock.Object,
                _verificacionServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CrearUsuarioAdminAsync_EmailDuplicado_DebeRetornarFail()
        {
            // Arrange
            string email = "existe@test.com";
            string emailNormalizado = email.Trim().ToLower();
            _userRepoMock.Setup(x => x.ExisteEmailAsync(emailNormalizado)).ReturnsAsync(true);

            // Act
            var result = await _sut.CrearUsuarioAdminAsync(1, "Nombre", email, "Pass_123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.REG_EMAIL_YA_REGISTRADO.ToString(), result.ErrorCode);
            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task CrearUsuarioAdminAsync_EmailSeGuardaNormalizado()
        {
            // Arrange
            string emailOriginal = "  TeST@ExaMPle.CoM  ";
            string emailNormalizado = "test@example.com";
            
            _userRepoMock.Setup(x => x.ExisteEmailAsync(emailNormalizado)).ReturnsAsync(false);
            
            Usuario usuarioGuardado = null;
            _userRepoMock.Setup(x => x.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => 
                { 
                    u.Id = 1; 
                    usuarioGuardado = u; 
                })
                .Returns(Task.CompletedTask);
                
            _userRepoMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _sut.CrearUsuarioAdminAsync(1, "Nombre", emailOriginal, "Pass_123");

            // Assert
            Assert.NotNull(usuarioGuardado);
            Assert.Equal(emailNormalizado, usuarioGuardado.Email);
        }

        [Fact]
        public async Task CrearUsuarioAdminAsync_UsuarioCreadoCorrectamente_DebeRetornarOk()
        {
            // Arrange
            _userRepoMock.Setup(x => x.ExisteEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            
            _userRepoMock.Setup(x => x.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => u.Id = 10)
                .Returns(Task.CompletedTask);
                
            _userRepoMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _sut.CrearUsuarioAdminAsync(1, "Nuevo Admin", "nuevo@admin.com", "Pass_123");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data.Id);
            Assert.Equal(Enum.RolesUsuario.ADMIN, result.Data.Rol);
            
            _notificacionServiceMock.Verify(x => x.EnviarBienvenidaAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CrearUsuarioAdminAsync_FalloAlGuardarUsuario_DebeRetornarFail()
        {
            // Arrange
            _userRepoMock.Setup(x => x.ExisteEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            
            // Simular que SaveChangesAsync devuelve 0 (ninguna fila afectada)
            _userRepoMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _sut.CrearUsuarioAdminAsync(1, "Nuevo Admin", "nuevo@admin.com", "Pass_123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.REG_ERROR_CREACION_USUARIO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarEmailDisponibleAsync_EmailExiste_DebeRetornarFail()
        {
            // Arrange
            string email = "existe@test.com";
            _userRepoMock.Setup(x => x.ExisteEmailAsync(email)).ReturnsAsync(true);

            // Act
            var result = await _sut.ValidarEmailDisponibleAsync(email);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.REG_EMAIL_YA_REGISTRADO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarEmailDisponibleAsync_EmailLibre_DebeRetornarOk()
        {
            // Arrange
            string email = "nuevo@test.com";
            _userRepoMock.Setup(x => x.ExisteEmailAsync(email)).ReturnsAsync(false);

            // Act
            var result = await _sut.ValidarEmailDisponibleAsync(email);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task ActivarUsuarioAsync_LlamaAlRepoCorrectamente()
        {
            // Arrange
            int usuarioId = 5;

            // Act
            var result = await _sut.ActivarUsuarioAsync(usuarioId);

            // Assert
            Assert.True(result.Success);
            _userRepoMock.Verify(x => x.ActivarUsuarioAsync(usuarioId), Times.Once);
        }
    }
}