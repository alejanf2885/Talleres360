using Moq;
using Talleres360.Dtos.Talleres;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Services.Talleres;

namespace Talleres360.Tests.Services
{
    public class TallerServiceTests
    {
        private readonly Mock<ITallerRepository> _tallerRepoMock;
        private readonly Mock<IImagenService> _imagenServiceMock;
        private readonly TallerService _sut;

        public TallerServiceTests()
        {
            _tallerRepoMock = new Mock<ITallerRepository>();
            _imagenServiceMock = new Mock<IImagenService>();

            _sut = new TallerService(
                _tallerRepoMock.Object,
                _imagenServiceMock.Object
            );
        }

        [Fact]
        public async Task ConfigurarPerfilAsync_CifDuplicadoDeOtroTaller_RetornaFail()
        {
            // Arrange
            var request = new ConfigurarTallerRequest
            {
                CIF       = "B12345678",
                Direccion = "Calle 1",
                Localidad = "Madrid",
                Telefono  = "600000001",
                Logo      = string.Empty
            };

            var tallerActual = new Taller
            {
                Id     = 1,
                Nombre = "Taller Uno"
            };

            var tallerDuplicado = new Taller
            {
                Id     = 2,
                Nombre = "Taller Dos",
                Cif    = "B12345678"
            };

            _tallerRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(tallerActual);

            _tallerRepoMock
                .Setup(x => x.GetByCifAsync(request.CIF))
                .ReturnsAsync(tallerDuplicado);

            // Act
            var resultado = await _sut.ConfigurarPerfilAsync(1, request);

            // Assert
            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.REG_CIF_DUPLICADO.ToString(), resultado.ErrorCode);
            _tallerRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Taller>()), Times.Never);
        }

        [Fact]
        public async Task ConfigurarPerfilAsync_DatosCorrectos_RetornaOk()
        {
            // Arrange
            var request = new ConfigurarTallerRequest
            {
                CIF       = "B12345678",
                Direccion = "Calle 1",
                Localidad = "Madrid",
                Telefono  = "600000001",
                Logo      = string.Empty
            };

            var tallerActual = new Taller
            {
                Id                = 1,
                Nombre            = "Taller Uno",
                PerfilConfigurado = false
            };

            _tallerRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(tallerActual);

            _tallerRepoMock
                .Setup(x => x.GetByCifAsync(request.CIF))
                .ReturnsAsync((Taller?)null);

            // Act
            var resultado = await _sut.ConfigurarPerfilAsync(1, request);

            // Assert
            Assert.True(resultado.Success);
            Assert.True(resultado.Data);
            _tallerRepoMock.Verify(
                x => x.UpdateAsync(It.Is<Taller>(t => t.Id == 1 && t.PerfilConfigurado)),
                Times.Once);
        }

        [Fact]
        public async Task ObtenerTallerPorIdAsync_TallerNoExiste_RetornaFail()
        {
            // Arrange
            _tallerRepoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Taller?)null);

            // Act
            var resultado = await _sut.ObtenerTallerPorIdAsync(99);

            // Assert
            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }
    }
}
