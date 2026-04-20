using Moq;
using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Servicios;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Services.DocumentosComerciales;

namespace Talleres360.Test.Services
{
    public class DocumentoComercialServiceTests
    {
        private static DocumentoComercialService BuildService(
            Mock<ICustomerRepository> customerMock,
            Mock<IServicioRepository> servicioMock,
            Mock<ITallerRepository>? tallerMock = null)
        {
            tallerMock ??= new Mock<ITallerRepository>();
            tallerMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Taller { Id = 1, Nombre = "Taller Test", Cif = "B12345678" });

            return new DocumentoComercialService(customerMock.Object, servicioMock.Object, tallerMock.Object);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_Taller_No_Existe()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            Mock<ITallerRepository> tallerMock = new Mock<ITallerRepository>();
            tallerMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Taller?)null);

            DocumentoComercialService service = new DocumentoComercialService(
                customerMock.Object, servicioMock.Object, tallerMock.Object);

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000001",
                    new DocumentoComercialInput { ClienteId = 1, Lineas = [] });

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_Cliente_No_Existe()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            customerMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Cliente?)null);

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Filtro de aceite",
                        Cantidad = 1,
                        PrecioUnitario = 10,
                        DescuentoPorcentaje = 0,
                        ImpuestoPorcentaje = 21
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000001", input);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.CUST_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_No_Hay_Lineas()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            customerMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Cliente { Id = 10, TallerId = 1, Nombre = "Cliente" });

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000001",
                    new DocumentoComercialInput { ClienteId = 10, Lineas = [] });

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Calcular_Totales_Correctamente()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            customerMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Cliente
            {
                Id = 10, TallerId = 1, Nombre = "Ana", Apellidos = "L\u00f3pez", NifCif = "12345678A"
            });

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Pastillas de freno",
                        Cantidad = 2,
                        PrecioUnitario = 50,
                        DescuentoPorcentaje = 10,
                        ImpuestoPorcentaje = 21
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000002", input);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(90m, resultado.Data!.Documento.Subtotal);
            Assert.Equal(18.9m, resultado.Data.Documento.ImporteImpuestos);
            Assert.Equal(108.9m, resultado.Data.Documento.Total);
            Assert.Single(resultado.Data.Lineas);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Incluir_Snapshot_De_Taller()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            Mock<ITallerRepository> tallerMock = new Mock<ITallerRepository>();

            tallerMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Taller
            {
                Id = 1,
                Nombre = "Taller El Mecánico",
                Cif = "B87654321",
                Direccion = "Calle Falsa 123",
                Localidad = "Madrid",
                Telefono = "912345678"
            });

            customerMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Cliente
            {
                Id = 5, TallerId = 1, Nombre = "Juan", Email = "juan@test.com", Telefono = "600111222"
            });

            DocumentoComercialService service = new DocumentoComercialService(
                customerMock.Object, servicioMock.Object, tallerMock.Object);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 5,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Aceite", Cantidad = 1, PrecioUnitario = 20,
                        DescuentoPorcentaje = 0, ImpuestoPorcentaje = 21
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.FACTURA, "2026-000010", input);

            Assert.True(resultado.Success);
            Factura doc = resultado.Data!.Documento;
            Assert.Equal("Taller El Mecánico", doc.TallerNombre);
            Assert.Equal("B87654321", doc.TallerCif);
            Assert.Equal("Calle Falsa 123", doc.TallerDireccion);
            Assert.Equal("Madrid", doc.TallerLocalidad);
            Assert.Equal("912345678", doc.TallerTelefono);
            Assert.Equal("juan@test.com", doc.ClienteEmail);
            Assert.Equal("600111222", doc.ClienteTelefono);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Generar_DesglosesIva_Por_Tipo()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();
            customerMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Cliente
            {
                Id = 5, TallerId = 1, Nombre = "Juan"
            });

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 5,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Filtro", Cantidad = 1, PrecioUnitario = 100,
                        DescuentoPorcentaje = 0, ImpuestoPorcentaje = 21
                    },
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Mano obra", Cantidad = 2, PrecioUnitario = 50,
                        DescuentoPorcentaje = 0, ImpuestoPorcentaje = 21
                    },
                    new LineaDocumentoComercialInput
                    {
                        Concepto = "Tasa", Cantidad = 1, PrecioUnitario = 10,
                        DescuentoPorcentaje = 0, ImpuestoPorcentaje = 10
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.FACTURA, "2026-000011", input);

            Assert.True(resultado.Success);
            List<DesgloseIva> desglosesIva = resultado.Data!.DesglosesIva;
            Assert.Equal(2, desglosesIva.Count);

            DesgloseIva iva21 = desglosesIva.Single(d => d.TipoIvaPorcentaje == 21m);
            Assert.Equal(200m, iva21.BaseImponible);
            Assert.Equal(42m, iva21.CuotaIva);

            DesgloseIva iva10 = desglosesIva.Single(d => d.TipoIvaPorcentaje == 10m);
            Assert.Equal(10m, iva10.BaseImponible);
            Assert.Equal(1m, iva10.CuotaIva);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Fallar_Cuando_Servicio_No_Existe_En_Taller()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();

            customerMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Cliente { Id = 10, TallerId = 1, Nombre = "Ana" });
            servicioMock.Setup(r => r.ObtenerEntidadPorIdAsync(500)).ReturnsAsync((Servicio?)null);

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        ServicioId = 500, Concepto = "Servicio", Cantidad = 1,
                        PrecioUnitario = 200, DescuentoPorcentaje = 0, ImpuestoPorcentaje = 21
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000003", input);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task PrepararDocumentoAsync_Debe_Usar_Precio_Y_Nombre_De_Servicio_Cuando_Viene_ServicioId()
        {
            Mock<ICustomerRepository> customerMock = new Mock<ICustomerRepository>();
            Mock<IServicioRepository> servicioMock = new Mock<IServicioRepository>();

            customerMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Cliente
            {
                Id = 10, TallerId = 1, Nombre = "Ana", NifCif = "123"
            });

            servicioMock.Setup(r => r.ObtenerEntidadPorIdAsync(900)).ReturnsAsync(new Servicio
            {
                Id = 900, TallerId = 1, Nombre = "ALINEACION", PrecioBase = 80,
                Activo = true, Eliminado = false
            });

            DocumentoComercialService service = BuildService(customerMock, servicioMock);

            DocumentoComercialInput input = new DocumentoComercialInput
            {
                ClienteId = 10,
                Lineas =
                [
                    new LineaDocumentoComercialInput
                    {
                        ServicioId = 900, Concepto = "IGNORAR", Cantidad = 2,
                        PrecioUnitario = 999, DescuentoPorcentaje = 0, ImpuestoPorcentaje = 21
                    }
                ]
            };

            Dtos.Responses.ServiceResult<DocumentoComercialPreparado> resultado =
                await service.PrepararDocumentoAsync(1, TipoDocumentoComercial.PRESUPUESTO, "2026-000004", input);

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
