using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Talleres360.Dtos.Usuarios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;
using Talleres360.Services.Auth;
using Talleres360.Enums;

namespace Talleres360.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _userRepoMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<ITallerService> _tallerServiceMock;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUsuarioRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _tallerServiceMock = new Mock<ITallerService>();

            _sut = new AuthService(
                _userRepoMock.Object,
                _passwordServiceMock.Object,
                _tallerServiceMock.Object
            );
        }

        [Fact]
        public async Task ValidarLoginAsync_EmailNoExiste_DebeRetornarFail()
        {
            // Arrange
            string email = "noexiste@test.com";
            string password = "Password123!";
            string emailNormalizado = email.Trim().ToLower();

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailNormalizado))
                .ReturnsAsync((Usuario)null!);

            // Act
            var result = await _sut.ValidarLoginAsync(email, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarLoginAsync_UsuarioEliminado_DebeRetornarFail()
        {
            // Arrange
            string email = "eliminado@test.com";
            string password = "Password123!";
            string emailNormalizado = email.Trim().ToLower();
            
            var usuario = new Usuario { Id = 1, Email = email, Eliminado = true };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailNormalizado))
                .ReturnsAsync(usuario);

            // Act
            var result = await _sut.ValidarLoginAsync(email, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarLoginAsync_CuentaInactiva_DebeRetornarFail_CUENTA_INACTIVA()
        {
            // Arrange
            string email = "inactivo@test.com";
            string password = "Password123!";
            string emailNormalizado = email.Trim().ToLower();
            
            var usuario = new Usuario { Id = 1, Email = email, Eliminado = false, Activo = false };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailNormalizado))
                .ReturnsAsync(usuario);

            // Act
            var result = await _sut.ValidarLoginAsync(email, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarLoginAsync_ContrasenaIncorrecta_DebeRetornarFail_CREDENCIALES_INCORRECTAS()
        {
            // Arrange
            string email = "valido@test.com";
            string password = "WrongPassword!";
            string emailNormalizado = email.Trim().ToLower();
            
            var usuario = new Usuario { Id = 1, Email = email, Eliminado = false, Activo = true };
            var credencial = new Credencial { PasswordHash = "hash_antiguo" };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailNormalizado))
                .ReturnsAsync(usuario);

            _userRepoMock
                .Setup(x => x.GetCredencialLocalByUsuarioIdAsync(usuario.Id))
                .ReturnsAsync(credencial);

            _passwordServiceMock
                .Setup(x => x.VerifyPassword(password, credencial.PasswordHash))
                .Returns(false);

            // Act
            var result = await _sut.ValidarLoginAsync(email, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarLoginAsync_CredencialesValidas_DebeRetornarOk_ConSecurityStampRelleno()
        {
            // Arrange
            string email = "valido@test.com";
            string password = "Password123!";
            string emailNormalizado = email.Trim().ToLower();
            
            var usuario = new Usuario 
            { 
                Id = 1, 
                Email = email, 
                Nombre = "Juan",
                Rol = Enum.RolesUsuario.ADMIN,
                TallerId = 10,
                SecurityStamp = "STAMP123",
                Eliminado = false, 
                Activo = true 
            };
            
            var credencial = new Credencial { PasswordHash = "hash_valido" };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailNormalizado))
                .ReturnsAsync(usuario);

            _userRepoMock
                .Setup(x => x.GetCredencialLocalByUsuarioIdAsync(usuario.Id))
                .ReturnsAsync(credencial);

            _passwordServiceMock
                .Setup(x => x.VerifyPassword(password, credencial.PasswordHash))
                .Returns(true);

            _tallerServiceMock
                .Setup(x => x.VerificarPerfilConfiguradoAsync(usuario.TallerId.Value))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ValidarLoginAsync(email, password);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(usuario.Id, result.Data.Id);
            Assert.Equal(usuario.Email, result.Data.Email);
            Assert.Equal(usuario.Rol.ToString(), result.Data.Rol);
            Assert.Equal("STAMP123", result.Data.SecurityStamp);
            Assert.True(result.Data.PerfilConfigurado);
            
            _userRepoMock.Verify(x => x.ActualizarUltimoAccesoAsync(usuario.Id), Times.Once);
        }

        [Fact]
        public async Task ValidarLoginAsync_EmailSeNormalizaAntesDeBuscar()
        {
            // Arrange
            string emailOriginal = "  ValiDo@TeST.Com  ";
            string emailEsperado = "valido@test.com";
            string password = "Password123!";
            
            _userRepoMock
                .Setup(x => x.GetByEmailAsync(emailEsperado))
                .ReturnsAsync((Usuario)null!);

            // Act
            await _sut.ValidarLoginAsync(emailOriginal, password);

            // Assert
            _userRepoMock.Verify(x => x.GetByEmailAsync(emailEsperado), Times.Once);
        }
    }
}
