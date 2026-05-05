using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Facturacion;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Facturacion
{
    public class FacturacionService : IFacturacionService
    {
        private readonly ITrabajoRepository _trabajoRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly ITallerRepository _tallerRepository;
        private readonly IVehiculoRepository _vehiculoRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFacturaPdfService _facturaPdfService;
        private readonly ILogger<FacturacionService> _logger;

        public FacturacionService(
            ITrabajoRepository trabajoRepository,
            IFacturaRepository facturaRepository,
            ITallerRepository tallerRepository,
            IVehiculoRepository vehiculoRepository,
            ICustomerRepository customerRepository,
            IFacturaPdfService facturaPdfService,
            ILogger<FacturacionService> logger)
        {
            _trabajoRepository  = trabajoRepository;
            _facturaRepository  = facturaRepository;
            _tallerRepository   = tallerRepository;
            _vehiculoRepository = vehiculoRepository;
            _customerRepository = customerRepository;
            _facturaPdfService  = facturaPdfService;
            _logger             = logger;
        }

        public async Task<ServiceResult<TrabajoDto>> FacturarTrabajoAsync(int tallerId, int trabajoId)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            if (trabajo.Estado != TrabajoEstado.CERRADO)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_FACTURABLE.ToString(),
                    $"Solo se pueden facturar trabajos en estado CERRADO. Estado actual: {trabajo.Estado}.");
            }

            Taller? taller = await _tallerRepository.GetByIdAsync(tallerId);
            if (taller == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Taller no encontrado.");
            }

            Cliente? cliente = null;
            if (trabajo.VehiculoId.HasValue)
            {
                Vehiculo? vehiculo = await _vehiculoRepository.GetByIdAsync(trabajo.VehiculoId.Value, tallerId);
                if (vehiculo?.ClienteId != null)
                {
                    cliente = await _customerRepository.GetByIdAsync(vehiculo.ClienteId.Value, tallerId);
                }
            }

            List<DetalleTrabajo> detalles = await _facturaRepository.ObtenerDetallesParaFacturarAsync(trabajoId);
            if (detalles.Count == 0)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El trabajo no tiene líneas de detalle para facturar.");
            }

            string numeroFactura;
            try
            {
                numeroFactura = await _facturaRepository.GenerarNumeroFacturaAsync(tallerId);
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    return ServiceResult<TrabajoDto>.Fail(
                        ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                        "No se pudo generar el número de factura.");
                }
            }
            catch (Exception)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                    "Error al generar el número de factura.");
            }

            Factura factura = new Factura
            {
                TallerId            = tallerId,
                TrabajoId           = trabajoId,
                ClienteId           = cliente != null ? cliente.Id : 0,
                NumeroFactura       = numeroFactura,
                TipoDocumento       = TipoDocumentoComercial.FACTURA,
                FechaEmision        = DateTime.UtcNow,
                Subtotal            = trabajo.Subtotal,
                ImporteImpuestos    = trabajo.ImporteImpuestos,
                Total               = trabajo.Total,
                EstadoPago          = "PENDIENTE",
                ClienteNombre       = cliente != null
                    ? (string.IsNullOrWhiteSpace(cliente.Apellidos) ? cliente.Nombre : $"{cliente.Nombre} {cliente.Apellidos}".Trim())
                    : "Sin cliente",
                ClienteNifCif       = cliente?.NifCif ?? "-",
                ClienteDireccion    = cliente?.Direccion,
                ClienteCodigoPostal = cliente?.CodigoPostal,
                ClienteLocalidad    = cliente?.Localidad,
                ClienteProvincia    = cliente?.Provincia,
                ClienteEmail        = cliente?.Email,
                ClienteTelefono     = cliente?.Telefono,
                TallerNombre        = taller.Nombre,
                TallerCif           = taller.Cif,
                TallerDireccion     = taller.Direccion,
                TallerLocalidad     = taller.Localidad,
                TallerTelefono      = taller.Telefono
            };

            List<LineaFactura> lineas = detalles.Select(d => new LineaFactura
            {
                FacturaId           = 0,
                TallerId            = tallerId,
                ServicioId          = d.ProductoId,
                Concepto            = d.Concepto,
                Cantidad            = d.Cantidad,
                PrecioUnitario      = d.PrecioUnitario,
                DescuentoPorcentaje = d.DescuentoPorcentaje,
                ImpuestoPorcentaje  = d.ImpuestoPorcentaje,
                SubtotalLinea       = d.Cantidad * d.PrecioUnitario * (1 - d.DescuentoPorcentaje / 100m),
                TotalLinea          = d.Cantidad * d.PrecioUnitario * (1 - d.DescuentoPorcentaje / 100m) * (1 + d.ImpuestoPorcentaje / 100m)
            }).ToList();

            List<DesgloseIva> desgloses = lineas
                .GroupBy(l => l.ImpuestoPorcentaje)
                .Select(g => new DesgloseIva
                {
                    FacturaId         = 0,
                    TallerId          = tallerId,
                    TipoIvaPorcentaje = g.Key,
                    BaseImponible     = g.Sum(l => l.SubtotalLinea),
                    CuotaIva          = g.Sum(l => l.TotalLinea - l.SubtotalLinea)
                }).ToList();

            await _facturaRepository.GuardarSnapshotAsync(factura, lineas, desgloses);

            try
            {
                string urlPdf = await _facturaPdfService.GenerarYSubirAsync(factura, lineas, desgloses);
                await _facturaRepository.ActualizarUrlPdfAsync(factura.Id, urlPdf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para factura {FacturaId} ({NumeroFactura}). La factura se ha guardado correctamente pero sin PDF.", factura.Id, factura.NumeroFactura);
            }

            trabajo.Estado = TrabajoEstado.FACTURADO;
            if (trabajo.FechaCierre == null)
                trabajo.FechaCierre = DateTime.UtcNow;
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(trabajoId);
            return detalle != null
                ? ServiceResult<TrabajoDto>.Ok(detalle)
                : ServiceResult<TrabajoDto>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Factura generada pero no se pudo recuperar el trabajo.");
        }
    }
}
