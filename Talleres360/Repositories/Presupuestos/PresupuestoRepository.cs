using Talleres360.Dtos;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Enums;
using Talleres360.Interfaces.Presupuestos;

namespace Talleres360.Repositories.Presupuestos
{
    /// <summary>
    /// DEPRECATED: Este repositorio será eliminado en Fase 4.
    /// Los presupuestos ahora viven en la tabla Trabajos (Estado = PRESUPUESTO).
    /// La lógica se ha migrado a TrabajoRepository.
    /// </summary>
    public class PresupuestoRepository : IPresupuestoRepository
    {
        public Task<PagedResponse<PresupuestoDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion)
            => throw new NotImplementedException("Los presupuestos se gestionan ahora a través de TrabajoRepository. Este repositorio será eliminado.");

        public Task<PresupuestoDto?> ObtenerDetallePorIdAsync(int presupuestoId)
            => throw new NotImplementedException("Los presupuestos se gestionan ahora a través de TrabajoRepository. Este repositorio será eliminado.");

        public Task<Factura?> ObtenerEntidadPorIdAsync(int presupuestoId)
            => throw new NotImplementedException("Los presupuestos se gestionan ahora a través de TrabajoRepository. Este repositorio será eliminado.");

        public Task AddAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva>? desglosesIva = null)
            => throw new NotImplementedException("Los presupuestos se gestionan ahora a través de TrabajoRepository. Este repositorio será eliminado.");

        public Task<string> GenerarNumeroDocumentoAsync(int tallerId, TipoDocumentoComercial tipoDocumento)
            => throw new NotImplementedException("Usar TrabajoRepository.GenerarNumeroDocumentoTrabajoAsync en su lugar.");

        public Task<bool> PerteneceATallerAsync(int id, int tallerId)
            => throw new NotImplementedException("Usar TrabajoRepository.PerteneceATallerAsync en su lugar.");
    }
}
