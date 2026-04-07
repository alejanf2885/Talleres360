using Moq;
using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Servicios;
using Talleres360.Models;
using Talleres360.Services.DocumentosComerciales;

namespace Talleres360.Test.Services
{
    public class DocumentoComercialServiceTests
    {
        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_Cliente_No_Existe()
        {
            Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioRepositoryMock = new Mock<IServicioRepository>();
            customerRepositoryMock
                .Setup(repo => repo.GetByIdAsync(10))
                .ReturnsAsync((Cliente?)null);

            DocumentoComercialService service = new DocumentoComercialService(customerRepositoryMock.Object, servicioRepositoryMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas = new List<LineaDocumentoComercialInput>
                {
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Filtro de aceite",
                        Cantidad = 1,
                        PrecioUnitario = 10,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado = await service.PrepararDocumentoAsync(
                1,
                TipoDocumentoComercial.PRESUPUESTO,
                "2026-000001",
                input);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.CUST_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_No_Hay_Lineas()
        {
            Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioRepositoryMock = new Mock<IServicioRepository>();
            customerRepositoryMock
                .Setup(repo => repo.GetByIdAsync(10))
                .ReturnsAsync(new Cliente
                {
                    Id = 10,
                    TallerId = 1,
                    Nombre = "Cliente"
                });

            DocumentoComercialService service = new DocumentoComercialService(customerRepositoryMock.Object, servicioRepositoryMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas = new List<LineaDocumentoComercialInput>()
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado = await service.PrepararDocumentoAsync(
                1,
                TipoDocumentoComercial.PRESUPUESTO,
                "2026-000001",
                input);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Calcular_Totales_Correctamente()
        {
            Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioRepositoryMock = new Mock<IServicioRepository>();
            customerRepositoryMock
                .Setup(repo => repo.GetByIdAsync(10))
                .ReturnsAsync(new Cliente
                {
                    Id = 10,
                    TallerId = 1,
                    Nombre = "Ana",
                    Apellidos = "López",
                    NifCif = "12345678A"
                });

            DocumentoComercialService service = new DocumentoComercialService(customerRepositoryMock.Object, servicioRepositoryMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas = new List<LineaDocumentoComercialInput>
                {
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Pastillas de freno",
                        Cantidad = 2,
                        PrecioUnitario = 50,
                        DescuentoPorcentaje = 10,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado = await service.PrepararDocumentoAsync(
                1,
                TipoDocumentoComercial.PRESUPUESTO,
                "2026-000002",
                input);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(90m, resultado.Data!.Documento.Subtotal);
            Assert.Equal(18.9m, resultado.Data.Documento.ImporteImpuestos);
            Assert.Equal(108.9m, resultado.Data.Documento.Total);
            Assert.Single(resultado.Data.Lineas);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_Servicio_No_Existe_En_Taller()
        {
            Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioRepositoryMock = new Mock<IServicioRepository>();

            customerRepositoryMock
                .Setup(repo => repo.GetByIdAsync(10))
                .ReturnsAsync(new Cliente
                {
                    Id = 10,
                    TallerId = 1,
                    Nombre = "Ana"
                });

            servicioRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(500))
                .ReturnsAsync((Servicio?)null);

            DocumentoComercialService service = new DocumentoComercialService(customerRepositoryMock.Object, servicioRepositoryMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas = new List<LineaDocumentoComercialInput>
                {
                    new LineaDocumentoComercialInput
                    {
                        ServicioId = 500,
                        Concepto = "Servicio",
                        Cantidad = 1,
                        PrecioUnitario = 200,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado = await service.PrepararDocumentoAsync(
                1,
                TipoDocumentoComercial.PRESUPUESTO,
                "2026-000003",
                input);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Usar_Precio_Y_Nombre_De_Servicio_Cuando_Viene_ServicioId()
        {
            Mock<ICustomerRepository> customerRepositoryMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioRepositoryMock = new Mock<IServicioRepository>();

            customerRepositoryMock
                .Setup(repo => repo.GetByIdAsync(10))
                .ReturnsAsync(new Cliente
                {
                    Id = 10,
                    TallerId = 1,
                    Nombre = "Ana",
                    NifCif = "123"
                });

            servicioRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(900))
                .ReturnsAsync(new Servicio
                {
                    Id = 900,
                    TallerId = 1,
                    Nombre = "ALINEACION",
                    PrecioBase = 80,
                    Activo = true,
                    Eliminado = false
                });

            DocumentoComercialService service = new DocumentoComercialService(customerRepositoryMock.Object, servicioRepositoryMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas = new List<LineaDocumentoComercialInput>
                {
                    new LineaDocumentoComercialInput
                    {
                        ServicioId = 900,
                        Concepto = "IGNORAR",
                        Cantidad = 2,
                        PrecioUnitario = 999,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                }
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado = await service.PrepararDocumentoAsync(
                1,
                TipoDocumentoComercial.PRESUPUESTO,
                "2026-000004",
                input);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(160m, resultado.Data!.Documento.Subtotal);
            Assert.Equal(33.6m, resultado.Data.Documento.ImporteImpuestos);
            Assert.Equal("ALINEACION", resultado.Data.Lineas[0].Concepto);
            Assert.Equal(80m, resultado.Data.Lineas[0].PrecioUnitario);
            Assert.Equal(900, resultado.Data.Lineas[0].ServicioId);
        }
    }
}
