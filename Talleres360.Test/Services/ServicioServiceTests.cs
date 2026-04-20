using Moq;
using Talleres360.Dtos.Servicios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Servicios;
using Talleres360.Services.Servicios;

namespace Talleres360.Test.Services
{
    public class ServicioServiceTests
    {
        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Nombre_Duplicado()
        {
            Mock<IServicioRepository> repositoryMock = new Mock<IServicioRepository>();
            repositoryMock
                .Setup(repo => repo.ExisteNombreAsync(1, "ALINEACION", null))
                .ReturnsAsync(true);

            ServicioService service = new ServicioService(repositoryMock.Object);

            CrearServicioRequest request = new CrearServicioRequest
            {
                Nombre = "ALINEACION",
                PrecioBase = 50,
                ImpuestoPorcentaje = 21,
                Activo = true
            };

            Dtos.Responses.ServiceResult<ServicioDto> resultado = await service.CrearAsync(1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_OPERACION_INVALIDA.ToString(), resultado.ErrorCode);
            repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Servicio>()), Times.Never);
        }

        [Fact]
        public async Task CrearAsync_Debe_Crear_Cuando_Datos_Son_Validos()
        {
            Mock<IServicioRepository> repositoryMock = new Mock<IServicioRepository>();
            repositoryMock
                .Setup(repo => repo.ExisteNombreAsync(1, "ALINEACION", null))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Servicio>()))
                .Callback<Servicio>(servicio => servicio.Id = 15)
                .Returns(Task.CompletedTask);

            repositoryMock
                .Setup(repo => repo.ObtenerDetallePorIdAsync(15))
                .ReturnsAsync(new ServicioDto
                {
                    Id = 15,
                    Nombre = "ALINEACION",
                    PrecioBase = 50,
                    ImpuestoPorcentaje = 21,
                    Activo = true
                });

            ServicioService service = new ServicioService(repositoryMock.Object);

            CrearServicioRequest request = new CrearServicioRequest
            {
                Nombre = "ALINEACION",
                PrecioBase = 50,
                ImpuestoPorcentaje = 21,
                Activo = true
            };

            Dtos.Responses.ServiceResult<ServicioDto> resultado = await service.CrearAsync(1, request);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(15, resultado.Data!.Id);
        }

        [Fact]
        public async Task ActualizarAsync_Debe_Fallar_Cuando_No_Existe()
        {
            Mock<IServicioRepository> repositoryMock = new Mock<IServicioRepository>();
            repositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(100))
                .ReturnsAsync((Servicio?)null);

            ServicioService service = new ServicioService(repositoryMock.Object);

            ActualizarServicioRequest request = new ActualizarServicioRequest
            {
                Nombre = "SERVICIO",
                PrecioBase = 10,
                ImpuestoPorcentaje = 21,
                Activo = true
            };

            Dtos.Responses.ServiceResult<ServicioDto> resultado = await service.ActualizarAsync(1, 100, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Marcar_Eliminado_Y_Devolver_Ok()
        {
            Mock<IServicioRepository> repositoryMock = new Mock<IServicioRepository>();
            Servicio entidad = new Servicio
            {
                Id = 8,
                TallerId = 1,
                Nombre = "SERVICIO",
                PrecioBase = 10,
                ImpuestoPorcentaje = 21,
                Activo = true,
                Eliminado = false
            };

            repositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(8))
                .ReturnsAsync(entidad);

            repositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Servicio>()))
                .Returns(Task.CompletedTask);

            ServicioService service = new ServicioService(repositoryMock.Object);
            Dtos.Responses.ServiceResult<bool> resultado = await service.EliminarAsync(1, 8);

            Assert.True(resultado.Success);
            Assert.True(entidad.Eliminado);
        }
    }
}
