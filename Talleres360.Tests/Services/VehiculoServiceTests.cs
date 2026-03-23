using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Talleres360.Services.Vehiculos;

namespace Talleres360.Tests.Services
{
    public class VehiculoServiceTests
    {
        private readonly Mock<IVehiculoRepository> _vehiculoRepoMock;
        private readonly VehiculoService _sut;

        public VehiculoServiceTests()
        {
            _vehiculoRepoMock = new Mock<IVehiculoRepository>();
            _sut = new VehiculoService(_vehiculoRepoMock.Object);
        }

        [Fact]
        public async Task RegistrarVehiculoAsync_MatriculaDuplicada_DebeRetornarFail()
        {
            // Arrange
            var vehiculo = new Vehiculo { Matricula = "1234ABC" };
            _vehiculoRepoMock.Setup(x => x.ExistsAsync("1234ABC")).ReturnsAsync(true);

            // Act
            var result = await _sut.RegistrarVehiculoAsync(1, vehiculo);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.SYS_OPERACION_INVALIDA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task RegistrarVehiculoAsync_MatriculaSeNormaliza()
        {
            // Arrange
            var vehiculo = new Vehiculo { Matricula = " 1234 - abc " };
            _vehiculoRepoMock.Setup(x => x.ExistsAsync("1234ABC")).ReturnsAsync(true);

            // Act
            await _sut.RegistrarVehiculoAsync(1, vehiculo);

            // Assert
            _vehiculoRepoMock.Verify(x => x.ExistsAsync("1234ABC"), Times.Once);
        }

        [Fact]
        public async Task RegistrarVehiculoAsync_KmActualesRellenaFechaUltimaActualizacionKm()
        {
            // Arrange
            var vehiculo = new Vehiculo { Matricula = "1234ABC", KmActuales = 150000 };
            _vehiculoRepoMock.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            
            Vehiculo guardado = null;
            _vehiculoRepoMock.Setup(x => x.AddAsync(It.IsAny<Vehiculo>()))
                .Callback<Vehiculo>(v => { 
                    v.Id = 1; 
                    guardado = v; 
                })
                .Returns(Task.CompletedTask);

            var detalleMock = new VehiculoDetalle { Id = 1, Matricula = "1234ABC" };
            _vehiculoRepoMock.Setup(x => x.GetDetalleByIdAsync(1)).ReturnsAsync(detalleMock);

            // Act
            await _sut.RegistrarVehiculoAsync(1, vehiculo);

            // Assert
            Assert.NotNull(guardado);
            Assert.True(guardado.FechaUltimaActualizacionKm.HasValue);
            Assert.Equal(150000, guardado.KmActuales);
        }

        [Fact]
        public async Task RegistrarVehiculoAsync_VehiculoCreado_DebeRetornarOk()
        {
            // Arrange
            var vehiculo = new Vehiculo { Matricula = "5678DEF" };
            _vehiculoRepoMock.Setup(x => x.ExistsAsync("5678DEF")).ReturnsAsync(false);
            
            _vehiculoRepoMock.Setup(x => x.AddAsync(It.IsAny<Vehiculo>()))
                .Callback<Vehiculo>(v => v.Id = 10)
                .Returns(Task.CompletedTask);

            var detalleMock = new VehiculoDetalle { Id = 10, Matricula = "5678DEF" };
            _vehiculoRepoMock.Setup(x => x.GetDetalleByIdAsync(10)).ReturnsAsync(detalleMock);

            // Act
            var result = await _sut.RegistrarVehiculoAsync(1, vehiculo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data.Id);
            Assert.Equal("5678DEF", result.Data.Matricula);
        }

        [Fact]
        public async Task ActualizarVehiculoAsync_VehiculoDeOtroTaller_DebeRetornarFail()
        {
            // Arrange
            var vehiculoModificado = new Vehiculo { Id = 5, Matricula = "9999ZZZ" };
            
            var vehiculoExistente = new Vehiculo { Id = 5, TallerId = 2 }; // Taller diferente
            _vehiculoRepoMock.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(vehiculoExistente);

            // Act
            var result = await _sut.ActualizarVehiculoAsync(1, 5, vehiculoModificado);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task GetDetalleByIdAsync_VehiculoDeOtroTaller_DebeRetornarFail()
        {
            // Arrange
            _vehiculoRepoMock.Setup(x => x.GetDetalleByIdAsync(10)).ReturnsAsync(new VehiculoDetalle { TallerId = 2 }); // Other taller

            // Act
            var result = await _sut.GetDetalleByIdAsync(1, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task GetDetalleByMatriculaAsync_MatriculaSeNormaliza()
        {
            // Arrange
            string matricula = " 9999 - xx  ";
            string matriculaNormalizada = "9999 - XX";
            
            _vehiculoRepoMock.Setup(x => x.GetDetalleByMatriculaAsync(matriculaNormalizada)).ReturnsAsync(new VehiculoDetalle { Id = 5, TallerId = 1 });

            // Act
            await _sut.GetDetalleByMatriculaAsync(1, matricula);

            // Assert
            _vehiculoRepoMock.Verify(x => x.GetDetalleByMatriculaAsync(matriculaNormalizada), Times.Once);
        }
    }
}