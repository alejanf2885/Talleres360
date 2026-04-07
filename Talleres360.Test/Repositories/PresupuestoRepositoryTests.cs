using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Enums;
using Talleres360.Repositories.Presupuestos;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class PresupuestoRepositoryTests
    {
        [Fact]
        public async Task ObtenerDetallePorIdAsync_Debe_Incluir_Lineas_Del_Presupuesto()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Factura presupuesto = new Factura
            {
                Id = 1,
                TallerId = 1,
                ClienteId = 10,
                NumeroFactura = "2026-000001",
                TipoDocumento = TipoDocumentoComercial.PRESUPUESTO,
                FechaEmision = DateTime.UtcNow,
                Subtotal = 100,
                ImporteImpuestos = 21,
                Total = 121,
                EstadoPago = "PENDIENTE",
                ClienteNombre = "Cliente",
                ClienteNifCif = "123"
            };

            await context.Facturas.AddAsync(presupuesto);
            await context.LineasFactura.AddAsync(new LineaFactura
            {
                Id = 1,
                FacturaId = 1,
                Concepto = "Línea",
                Cantidad = 1,
                PrecioUnitario = 100,
                DescuentoPorcentaje = 0,
                ImpuestoPorcentaje = 21,
                SubtotalLinea = 100,
                TotalLinea = 121
            });
            await context.SaveChangesAsync();

            PresupuestoRepository repository = new PresupuestoRepository(context);
            Talleres360.Dtos.Presupuestos.PresupuestoDto? detalle = await repository.ObtenerDetallePorIdAsync(1);

            Assert.NotNull(detalle);
            Assert.Single(detalle!.Lineas);
            Assert.Equal("Línea", detalle.Lineas[0].Concepto);
        }

        [Fact]
        public async Task ObtenerTodosPagedAsync_Debe_Traer_Solo_Documentos_Presupuesto()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Facturas.AddRangeAsync(
                new Factura
                {
                    Id = 1,
                    TallerId = 1,
                    ClienteId = 1,
                    NumeroFactura = "P-1",
                    TipoDocumento = TipoDocumentoComercial.PRESUPUESTO,
                    FechaEmision = DateTime.UtcNow,
                    Subtotal = 10,
                    ImporteImpuestos = 2,
                    Total = 12,
                    EstadoPago = "PENDIENTE",
                    ClienteNombre = "A",
                    ClienteNifCif = "1"
                },
                new Factura
                {
                    Id = 2,
                    TallerId = 1,
                    ClienteId = 1,
                    NumeroFactura = "F-1",
                    TipoDocumento = TipoDocumentoComercial.FACTURA,
                    FechaEmision = DateTime.UtcNow,
                    Subtotal = 10,
                    ImporteImpuestos = 2,
                    Total = 12,
                    EstadoPago = "PENDIENTE",
                    ClienteNombre = "A",
                    ClienteNifCif = "1"
                });
            await context.SaveChangesAsync();

            PresupuestoRepository repository = new PresupuestoRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            Dtos.PagedResponse<Talleres360.Dtos.Presupuestos.PresupuestoDto> resultado = await repository.ObtenerTodosPagedAsync(1, paginacion);

            Assert.Single(resultado.Data);
            Assert.Equal("P-1", resultado.Data.First().NumeroDocumento);
        }

        [Fact]
        public async Task AddAsync_Debe_Asignar_FacturaId_En_Lineas()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            PresupuestoRepository repository = new PresupuestoRepository(context);

            Factura factura = new Factura
            {
                TallerId = 1,
                ClienteId = 2,
                NumeroFactura = "P-100",
                TipoDocumento = TipoDocumentoComercial.PRESUPUESTO,
                FechaEmision = DateTime.UtcNow,
                Subtotal = 10,
                ImporteImpuestos = 2,
                Total = 12,
                EstadoPago = "PENDIENTE",
                ClienteNombre = "Cliente",
                ClienteNifCif = "000"
            };

            List<LineaFactura> lineas = new List<LineaFactura>
            {
                new LineaFactura
                {
                    Concepto = "Mano de obra",
                    Cantidad = 1,
                    PrecioUnitario = 10,
                    DescuentoPorcentaje = 0,
                    ImpuestoPorcentaje = 21,
                    SubtotalLinea = 10,
                    TotalLinea = 12.1m
                }
            };

            await repository.AddAsync(factura, lineas);

            LineaFactura? lineaGuardada = await context.LineasFactura.FirstOrDefaultAsync();
            Assert.NotNull(lineaGuardada);
            Assert.Equal(factura.Id, lineaGuardada!.FacturaId);
        }

        [Fact]
        public async Task ObtenerEntidadPorIdAsync_Y_PerteneceATallerAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Factura factura = new Factura
            {
                Id = 50,
                TallerId = 3,
                ClienteId = 2,
                NumeroFactura = "P-50",
                TipoDocumento = TipoDocumentoComercial.PRESUPUESTO,
                FechaEmision = DateTime.UtcNow,
                Subtotal = 20,
                ImporteImpuestos = 4,
                Total = 24,
                EstadoPago = "PENDIENTE",
                ClienteNombre = "Cliente",
                ClienteNifCif = "A"
            };

            await context.Facturas.AddAsync(factura);
            await context.SaveChangesAsync();

            PresupuestoRepository repository = new PresupuestoRepository(context);
            Factura? entidad = await repository.ObtenerEntidadPorIdAsync(50);
            bool pertenece = await repository.PerteneceATallerAsync(50, 3);

            Assert.NotNull(entidad);
            Assert.True(pertenece);
        }

        private static ApplicationDbContext CrearContexto(string nombreDb)
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDb)
                .Options;
            return new ApplicationDbContext(options);
        }
    }
}
