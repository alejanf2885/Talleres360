using Moq;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Services.Trabajos;

namespace Talleres360.Test.Services
{
    public class CobroTrabajoServiceTests
    {
        [Fact]
        public async Task ObtenerTodosPorTrabajoAsync_Debe_Retornar_Lista_Vacia_Cuando_Trabajo_No_Existe()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync((Trabajo?)null);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            PagedResponse<CobroTrabajoDto> resultado = await service.ObtenerTodosPorTrabajoAsync(
                1, 1, new PaginationParams { PageNumber = 1, PageSize = 10 });

            Assert.NotNull(resultado);
            Assert.Empty(resultado.Data);
            Assert.Equal(0, resultado.TotalCount);
        }

        [Fact]
        public async Task ObtenerTodosPorTrabajoAsync_Debe_Retornar_Lista_Vacia_Cuando_Trabajo_Pertenece_A_Otro_Taller()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajoOtroTaller = new Trabajo
            {
                Id = 1,
                TallerId = 99, // Different workshop
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                Eliminado = false
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajoOtroTaller);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            PagedResponse<CobroTrabajoDto> resultado = await service.ObtenerTodosPorTrabajoAsync(
                1, 1, new PaginationParams { PageNumber = 1, PageSize = 10 });

            Assert.NotNull(resultado);
            Assert.Empty(resultado.Data);
            Assert.Equal(0, resultado.TotalCount);
        }

        [Fact]
        public async Task ObtenerTodosPorTrabajoAsync_Debe_Retornar_Cobros_Cuando_Trabajo_Existe()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajo = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PARCIAL,
                Eliminado = false
            };

            PagedResponse<CobroTrabajoDto> esperada = new PagedResponse<CobroTrabajoDto>
            {
                Data = new List<CobroTrabajoDto>
                {
                    new CobroTrabajoDto
                    {
                        Id = 1,
                        TrabajoId = 1,
                        Importe = 500,
                        MetodoPago = CobroMetodoPago.TARJETA,
                        FechaCobro = DateTime.UtcNow
                    }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajo);

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerTodosPorTrabajoPagedAsync(1, It.IsAny<PaginationParams>()))
                .ReturnsAsync(esperada);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            PagedResponse<CobroTrabajoDto> resultado = await service.ObtenerTodosPorTrabajoAsync(
                1, 1, new PaginationParams { PageNumber = 1, PageSize = 10 });

            Assert.NotNull(resultado);
            Assert.Single(resultado.Data);
            Assert.Equal(1, resultado.TotalCount);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Trabajo_No_Existe()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync((Trabajo?)null);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCobroTrabajoRequest request = new CrearCobroTrabajoRequest
            {
                Importe = 500,
                MetodoPago = CobroMetodoPago.EFECTIVO,
                FechaCobro = DateTime.UtcNow
            };

            ServiceResult<CobroTrabajoDto> resultado = await service.CrearAsync(1, 1, 1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Trabajo_Pertenece_A_Otro_Taller()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajoOtroTaller = new Trabajo
            {
                Id = 1,
                TallerId = 99,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                Eliminado = false
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajoOtroTaller);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCobroTrabajoRequest request = new CrearCobroTrabajoRequest
            {
                Importe = 500,
                MetodoPago = CobroMetodoPago.EFECTIVO,
                FechaCobro = DateTime.UtcNow
            };

            ServiceResult<CobroTrabajoDto> resultado = await service.CrearAsync(1, 1, 1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Importe_Es_Cero()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajo = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Total = 1000,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                Eliminado = false
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajo);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCobroTrabajoRequest request = new CrearCobroTrabajoRequest
            {
                Importe = 0,
                MetodoPago = CobroMetodoPago.EFECTIVO,
                FechaCobro = DateTime.UtcNow
            };

            ServiceResult<CobroTrabajoDto> resultado = await service.CrearAsync(1, 1, 1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Crear_Cobro_Y_Cambiar_EstadoPago_A_PARCIAL()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajo = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Total = 1000,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                Eliminado = false
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajo);

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerTotalCobradoAsync(1))
                .ReturnsAsync(500);

            unitOfWorkMock
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            unitOfWorkMock
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCobroTrabajoRequest request = new CrearCobroTrabajoRequest
            {
                Importe = 500,
                MetodoPago = CobroMetodoPago.EFECTIVO,
                FechaCobro = DateTime.UtcNow
            };

            ServiceResult<CobroTrabajoDto> resultado = await service.CrearAsync(1, 1, 1, request);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(500, resultado.Data.Importe);

            // Verify transaction was committed
            unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(), Times.Once);

            // Verify EstadoPago was recalculated to PARCIAL
            Assert.Equal(TrabajoEstadoPago.PARCIAL, trabajo.EstadoPago);
        }

        [Fact]
        public async Task CrearAsync_Debe_Cambiar_EstadoPago_A_PAGADO_Cuando_Importe_Cubre_Total()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Trabajo trabajo = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Total = 1000,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                Eliminado = false
            };

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajo);

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerTotalCobradoAsync(1))
                .ReturnsAsync(1000);

            unitOfWorkMock
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            unitOfWorkMock
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCobroTrabajoRequest request = new CrearCobroTrabajoRequest
            {
                Importe = 1000,
                MetodoPago = CobroMetodoPago.TRANSFERENCIA,
                FechaCobro = DateTime.UtcNow
            };

            ServiceResult<CobroTrabajoDto> resultado = await service.CrearAsync(1, 1, 1, request);

            Assert.True(resultado.Success);
            Assert.Equal(TrabajoEstadoPago.PAGADO, trabajo.EstadoPago);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Fallar_Cuando_Cobro_No_Existe()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync((CobroTrabajo?)null);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            ServiceResult<bool> resultado = await service.EliminarAsync(1, 1);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Fallar_Cuando_Cobro_Pertenece_A_Otro_Taller()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            CobroTrabajo cobroOtroTaller = new CobroTrabajo
            {
                Id = 1,
                TallerId = 99,
                TrabajoId = 1,
                Importe = 500,
                Eliminado = false
            };

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(cobroOtroTaller);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            ServiceResult<bool> resultado = await service.EliminarAsync(1, 1);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Eliminar_Cobro_Y_Recalcular_EstadoPago()
        {
            Mock<ICobroTrabajoRepository> cobroRepositoryMock = new Mock<ICobroTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                Eliminado = false
            };

            Trabajo trabajo = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Total = 1000,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PAGADO,
                Eliminado = false
            };

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(cobro);

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(trabajo);

            cobroRepositoryMock
                .Setup(repo => repo.ObtenerTotalCobradoAsync(1))
                .ReturnsAsync(500); // After deletion, only remaining payments

            unitOfWorkMock
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            unitOfWorkMock
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            CobroTrabajoService service = new CobroTrabajoService(
                cobroRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                unitOfWorkMock.Object);

            ServiceResult<bool> resultado = await service.EliminarAsync(1, 1);

            Assert.True(resultado.Success);
            Assert.True(cobro.Eliminado);
            Assert.Equal(TrabajoEstadoPago.PARCIAL, trabajo.EstadoPago);

            // Verify transaction operations
            unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
        }
    }
}
