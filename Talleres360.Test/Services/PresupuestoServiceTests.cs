using Moq;
using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.DocumentosComerciales;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Services.Presupuestos;

namespace Talleres360.Test.Services
{
    public class PresupuestoServiceTests
    {
        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_No_Se_Puede_Generar_Numero()
        {
            Mock<IPresupuestoRepository> presupuestoRepositoryMock = new Mock<IPresupuestoRepository>();
            Mock<IDocumentoComercialService> documentoComercialServiceMock = new Mock<IDocumentoComercialService>();

            presupuestoRepositoryMock
                .Setup(repo => repo.GenerarNumeroDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO))
                .ReturnsAsync(string.Empty);

            PresupuestoService service = new PresupuestoService(
                presupuestoRepositoryMock.Object,
                documentoComercialServiceMock.Object);

            CrearPresupuestoRequest request = new CrearPresupuestoRequest
            {
                ClienteId = 10,
                Lineas = new List<LineaPresupuestoRequest>
                {
                    new LineaPresupuestoRequest
                    {
                        Concepto = "Aceite",
                        Cantidad = 1,
                        PrecioUnitario = 10,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<PresupuestoDto> resultado = await service.CrearAsync(1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ERROR_BASE_DATOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Preparacion_De_Documento_Falla()
        {
            Mock<IPresupuestoRepository> presupuestoRepositoryMock = new Mock<IPresupuestoRepository>();
            Mock<IDocumentoComercialService> documentoComercialServiceMock = new Mock<IDocumentoComercialService>();

            presupuestoRepositoryMock
                .Setup(repo => repo.GenerarNumeroDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO))
                .ReturnsAsync("2026-000001");

            documentoComercialServiceMock
                .Setup(service => service.PrepararDocumentoAsync(
                    1,
                    TipoDocumentoComercial.PRESUPUESTO,
                    "2026-000001",
                    It.IsAny<DocumentoComercialInput>()))
                .ReturnsAsync(Dtos.Responses.ServiceResult<DocumentoComercialPreparado>.Fail(
                    ErrorCode.CUST_NO_ENCONTRADO.ToString(),
                    "Cliente no encontrado"));

            PresupuestoService service = new PresupuestoService(
                presupuestoRepositoryMock.Object,
                documentoComercialServiceMock.Object);

            CrearPresupuestoRequest request = new CrearPresupuestoRequest
            {
                ClienteId = 10,
                Lineas = new List<LineaPresupuestoRequest>
                {
                    new LineaPresupuestoRequest
                    {
                        Concepto = "Aceite",
                        Cantidad = 1,
                        PrecioUnitario = 10,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<PresupuestoDto> resultado = await service.CrearAsync(1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.CUST_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
            presupuestoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Factura>(), It.IsAny<List<LineaFactura>>(), It.IsAny<List<DesgloseIva>?>()), Times.Never);
        }

        [Fact]
        public async Task CrearAsync_Debe_Completar_Flujo_Cuando_Datos_Son_Validos()
        {
            Mock<IPresupuestoRepository> presupuestoRepositoryMock = new Mock<IPresupuestoRepository>();
            Mock<IDocumentoComercialService> documentoComercialServiceMock = new Mock<IDocumentoComercialService>();

            Factura facturaPreparada = new Factura
            {
                Id = 0,
                TallerId = 1,
                ClienteId = 10,
                NumeroFactura = "2026-000002",
                TipoDocumento = TipoDocumentoComercial.PRESUPUESTO,
                Subtotal = 100,
                ImporteImpuestos = 21,
                Total = 121,
                EstadoPago = "PENDIENTE",
                ClienteNombre = "Ana"
            };

            List<LineaFactura> lineasPreparadas = new List<LineaFactura>
            {
                new LineaFactura
                {
                    Concepto = "Filtro",
                    Cantidad = 1,
                    PrecioUnitario = 100,
                    DescuentoPorcentaje = 0,
                    ImpuestoPorcentaje = 21,
                    SubtotalLinea = 100,
                    TotalLinea = 121
                }
            };

            presupuestoRepositoryMock
                .Setup(repo => repo.GenerarNumeroDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO))
                .ReturnsAsync("2026-000002");

            documentoComercialServiceMock
                .Setup(service => service.PrepararDocumentoAsync(
                    1,
                    TipoDocumentoComercial.PRESUPUESTO,
                    "2026-000002",
                    It.IsAny<DocumentoComercialInput>()))
                .ReturnsAsync(Dtos.Responses.ServiceResult<DocumentoComercialPreparado>.Ok(new DocumentoComercialPreparado
                {
                    Documento = facturaPreparada,
                    Lineas = lineasPreparadas
                }));

            presupuestoRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Factura>(), It.IsAny<List<LineaFactura>>(), It.IsAny<List<DesgloseIva>?>()))
                .Callback<Factura, List<LineaFactura>, List<DesgloseIva>?>((factura, lineas, desglosesIva) => factura.Id = 300)
                .Returns(Task.CompletedTask);

            presupuestoRepositoryMock
                .Setup(repo => repo.ObtenerDetallePorIdAsync(300))
                .ReturnsAsync(new PresupuestoDto
                {
                    Id = 300,
                    NumeroDocumento = "2026-000002",
                    ClienteId = 10,
                    Total = 121
                });

            PresupuestoService service = new PresupuestoService(
                presupuestoRepositoryMock.Object,
                documentoComercialServiceMock.Object);

            CrearPresupuestoRequest request = new CrearPresupuestoRequest
            {
                ClienteId = 10,
                Lineas = new List<LineaPresupuestoRequest>
                {
                    new LineaPresupuestoRequest
                    {
                        Concepto = "Filtro",
                        Cantidad = 1,
                        PrecioUnitario = 100,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<PresupuestoDto> resultado = await service.CrearAsync(1, request);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(300, resultado.Data!.Id);
            presupuestoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Factura>(), It.IsAny<List<LineaFactura>>(), It.IsAny<List<DesgloseIva>?>()), Times.Once);
        }
    }
}
