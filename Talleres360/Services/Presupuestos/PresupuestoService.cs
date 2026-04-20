using Talleres360.Dtos;
using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.DocumentosComerciales;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Models;

namespace Talleres360.Services.Presupuestos
{
    public class PresupuestoService : IPresupuestoService
    {
        private readonly IPresupuestoRepository _presupuestoRepository;
        private readonly IDocumentoComercialService _documentoComercialService;

        public PresupuestoService(
            IPresupuestoRepository presupuestoRepository,
            IDocumentoComercialService documentoComercialService)
        {
            _presupuestoRepository = presupuestoRepository;
            _documentoComercialService = documentoComercialService;
        }

        public async Task<PagedResponse<PresupuestoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion)
        {
            PagedResponse<PresupuestoDto> presupuestos = await _presupuestoRepository.ObtenerTodosPagedAsync(tallerId, paginacion);
            return presupuestos;
        }

        public async Task<ServiceResult<PresupuestoDto>> ObtenerPorIdAsync(int tallerId, int presupuestoId)
        {
            Factura? entidad = await _presupuestoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.TipoDocumento != TipoDocumentoComercial.PRESUPUESTO)
            {
                return ServiceResult<PresupuestoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Presupuesto no encontrado.");
            }

            PresupuestoDto? detalle = await _presupuestoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            if (detalle == null)
            {
                return ServiceResult<PresupuestoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Presupuesto no encontrado.");
            }

            return ServiceResult<PresupuestoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<PresupuestoDto>> CrearAsync(int tallerId, CrearPresupuestoRequest request)
        {
            string numero = await _presupuestoRepository.GenerarNumeroDocumentoAsync(tallerId, TipoDocumentoComercial.PRESUPUESTO);
            if (string.IsNullOrWhiteSpace(numero))
            {
                return ServiceResult<PresupuestoDto>.Fail(
                    ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                    "No se pudo generar el n�mero de presupuesto.");
            }

            DocumentoComercialInput inputDocumento = new DocumentoComercialInput
            {
                ClienteId = request.ClienteId,
                TrabajoId = request.TrabajoId,
                FechaVencimiento = request.FechaVencimiento,
                MetodoPago = request.MetodoPago,
                NotasLegales = request.NotasLegales,
                Lineas = request.Lineas
                    .Select(linea => new LineaDocumentoComercialInput
                    {
                        ServicioId = linea.ServicioId,
                        Concepto = linea.Concepto,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = linea.PrecioUnitario,
                        DescuentoPorcentaje = linea.DescuentoPorcentaje,
                        ImpuestoPorcentaje = linea.ImpuestoPorcentaje
                    })
                    .ToList()
            };

            ServiceResult<DocumentoComercialPreparado> preparadoResultado = await _documentoComercialService.PrepararDocumentoAsync(
                tallerId,
                TipoDocumentoComercial.PRESUPUESTO,
                numero,
                inputDocumento);

            if (!preparadoResultado.Success || preparadoResultado.Data == null)
            {
                return ServiceResult<PresupuestoDto>.Fail(
                    preparadoResultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    preparadoResultado.Message ?? "No se pudo preparar el presupuesto.");
            }

            DocumentoComercialPreparado preparado = preparadoResultado.Data;

            await _presupuestoRepository.AddAsync(preparado.Documento, preparado.Lineas, preparado.DesglosesIva);

            PresupuestoDto? detalle = await _presupuestoRepository.ObtenerDetallePorIdAsync(preparado.Documento.Id);
            if (detalle == null)
            {
                return ServiceResult<PresupuestoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Presupuesto creado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<PresupuestoDto>.Ok(detalle);
        }
    }
}
