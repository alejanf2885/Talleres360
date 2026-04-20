using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.DocumentosComerciales;
using Talleres360.Interfaces.Servicios;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Services.DocumentosComerciales
{
    public class DocumentoComercialService : IDocumentoComercialService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly ITallerRepository _tallerRepository;

        public DocumentoComercialService(
            ICustomerRepository customerRepository,
            IServicioRepository servicioRepository,
            ITallerRepository tallerRepository)
        {
            _customerRepository = customerRepository;
            _servicioRepository = servicioRepository;
            _tallerRepository   = tallerRepository;
        }

        public async Task<ServiceResult<DocumentoComercialPreparado>> PrepararDocumentoAsync(
            int tallerId,
            TipoDocumentoComercial tipoDocumento,
            string numeroDocumento,
            DocumentoComercialInput input)
        {
            Taller? taller = await _tallerRepository.GetByIdAsync(tallerId);
            if (taller == null)
            {
                return ServiceResult<DocumentoComercialPreparado>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Taller no encontrado.");
            }

            Cliente? cliente = await _customerRepository.GetByIdAsync(input.ClienteId);
            if (cliente == null || cliente.TallerId != tallerId)
            {
                return ServiceResult<DocumentoComercialPreparado>.Fail(
                    ErrorCode.CUST_NO_ENCONTRADO.ToString(),
                    "Cliente no encontrado o no pertenece a tu taller.");
            }

            if (input.Lineas.Count == 0)
            {
                return ServiceResult<DocumentoComercialPreparado>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "Debe indicar al menos una l�nea del documento.");
            }

            decimal subtotal = 0;
            decimal impuestos = 0;
            List<LineaFactura> lineas = new List<LineaFactura>();

            foreach (LineaDocumentoComercialInput lineaInput in input.Lineas)
            {
                decimal precioUnitarioLinea = lineaInput.PrecioUnitario;
                string conceptoLinea = lineaInput.Concepto;

                if (lineaInput.ServicioId.HasValue)
                {
                    Servicio? servicio = await _servicioRepository.ObtenerEntidadPorIdAsync(lineaInput.ServicioId.Value);
                    if (servicio == null || servicio.TallerId != tallerId || servicio.Eliminado || !servicio.Activo)
                    {
                        return ServiceResult<DocumentoComercialPreparado>.Fail(
                            ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                            "El servicio indicado no existe o no est� disponible en tu taller.");
                    }

                    precioUnitarioLinea = servicio.PrecioBase;
                    conceptoLinea = servicio.Nombre;
                }

                decimal baseLinea = lineaInput.Cantidad * precioUnitarioLinea;
                decimal descuentoLinea = baseLinea * (lineaInput.DescuentoPorcentaje / 100m);
                decimal subtotalLinea = baseLinea - descuentoLinea;
                decimal impuestoLinea = subtotalLinea * (lineaInput.ImpuestoPorcentaje / 100m);
                decimal totalLinea = subtotalLinea + impuestoLinea;

                subtotal += subtotalLinea;
                impuestos += impuestoLinea;

                LineaFactura linea = new LineaFactura
                {
                    ServicioId = lineaInput.ServicioId,
                    Concepto = conceptoLinea.Trim(),
                    Cantidad = lineaInput.Cantidad,
                    PrecioUnitario = precioUnitarioLinea,
                    DescuentoPorcentaje = lineaInput.DescuentoPorcentaje,
                    ImpuestoPorcentaje = lineaInput.ImpuestoPorcentaje,
                    SubtotalLinea = subtotalLinea,
                    TotalLinea = totalLinea
                };

                lineas.Add(linea);
            }

            Factura documento = new Factura
            {
                TallerId = tallerId,
                TrabajoId = input.TrabajoId,
                ClienteId = input.ClienteId,
                NumeroFactura = numeroDocumento,
                TipoDocumento = tipoDocumento,
                FacturaRectificadaId = null,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = input.FechaVencimiento,
                Subtotal = subtotal,
                ImporteImpuestos = impuestos,
                Total = subtotal + impuestos,
                EstadoPago = "PENDIENTE",
                MetodoPago = string.IsNullOrWhiteSpace(input.MetodoPago) ? null : input.MetodoPago.Trim().ToUpper(),
                NotasLegales = input.NotasLegales,
                ClienteNombre = string.IsNullOrWhiteSpace(cliente.Apellidos)
                    ? cliente.Nombre
                    : $"{cliente.Nombre} {cliente.Apellidos}".Trim(),
                ClienteNifCif = string.IsNullOrWhiteSpace(cliente.NifCif) ? "-" : cliente.NifCif,
                ClienteDireccion = cliente.Direccion,
                ClienteCodigoPostal = cliente.CodigoPostal,
                ClienteLocalidad = cliente.Localidad,
                ClienteProvincia = cliente.Provincia,
                ClienteEmail = cliente.Email,
                ClienteTelefono = cliente.Telefono,
                TallerNombre = taller.Nombre,
                TallerCif = taller.Cif,
                TallerDireccion = taller.Direccion,
                TallerLocalidad = taller.Localidad,
                TallerTelefono = taller.Telefono
            };

            List<DesgloseIva> desglosesIva = lineas
                .GroupBy(l => l.ImpuestoPorcentaje)
                .Select(g => new DesgloseIva
                {
                    TipoIvaPorcentaje = g.Key,
                    BaseImponible     = g.Sum(l => l.SubtotalLinea),
                    CuotaIva          = g.Sum(l => l.TotalLinea - l.SubtotalLinea)
                })
                .ToList();

            DocumentoComercialPreparado preparado = new DocumentoComercialPreparado
            {
                Documento    = documento,
                Lineas       = lineas,
                DesglosesIva = desglosesIva
            };

            return ServiceResult<DocumentoComercialPreparado>.Ok(preparado);
        }
    }
}
