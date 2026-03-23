using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Services.Vehiculos
{
    public class VehiculoService : IVehiculoService
    {
        private readonly IVehiculoRepository _vehiculoRepository;

        public VehiculoService(IVehiculoRepository vehiculoRepository)
        {
            _vehiculoRepository = vehiculoRepository;
        }

        public async Task<ServiceResult<VehiculoDetalle>> RegistrarVehiculoAsync(int tallerId, Vehiculo request)
        {
            string matriculaLimpia = request.Matricula.Trim().ToUpper().Replace("-", "").Replace(" ", "");

            bool existe = await _vehiculoRepository.ExistsAsync(matriculaLimpia);
            if (existe)
            {
                return ServiceResult<VehiculoDetalle>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    $"La matrícula {matriculaLimpia} ya está registrada en el sistema.");
            }

            Vehiculo nuevoVehiculo = new Vehiculo
            {
                TallerId = tallerId,
                ClienteId = request.ClienteId,
                VehiculoTipoId = request.VehiculoTipoId,
                MarcaId = request.MarcaId,
                ModeloId = request.ModeloId,
                Matricula = matriculaLimpia,
                Anio = request.Anio,
                KmActuales = request.KmActuales,
                PromedioKmDiarios = request.PromedioKmDiarios,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            if (nuevoVehiculo.KmActuales.HasValue)
            {
                nuevoVehiculo.FechaUltimaActualizacionKm = DateTime.UtcNow;
            }

            await _vehiculoRepository.AddAsync(nuevoVehiculo);

            VehiculoDetalle? detalle = await _vehiculoRepository.GetDetalleByIdAsync(nuevoVehiculo.Id);

            return detalle != null
                ? ServiceResult<VehiculoDetalle>.Ok(detalle)
                : ServiceResult<VehiculoDetalle>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Vehículo guardado, pero error al recuperar vista de detalles.");
        }

        public async Task<ServiceResult<VehiculoDetalle>> ActualizarVehiculoAsync(int tallerId, int id, Vehiculo request)
        {
            Vehiculo? existente = await _vehiculoRepository.GetByIdAsync(id);

            if (existente == null || existente.TallerId != tallerId)
            {
                return ServiceResult<VehiculoDetalle>.Fail(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Vehículo no encontrado.");
            }

            existente.Matricula = request.Matricula.Trim().ToUpper().Replace("-", "").Replace(" ", "");
            existente.MarcaId = request.MarcaId;
            existente.ModeloId = request.ModeloId;
            existente.VehiculoTipoId = request.VehiculoTipoId;
            existente.Anio = request.Anio;
            existente.ClienteId = request.ClienteId;

            if (request.KmActuales.HasValue)
            {
                existente.KmActuales = request.KmActuales;
                existente.FechaUltimaActualizacionKm = DateTime.UtcNow;
            }

            await _vehiculoRepository.UpdateAsync(existente);

            VehiculoDetalle? detalle = await _vehiculoRepository.GetDetalleByIdAsync(id);
            return ServiceResult<VehiculoDetalle>.Ok(detalle!);
        }

        public async Task<ServiceResult<VehiculoDetalle>> GetDetalleByIdAsync(int tallerId, int id)
        {
            VehiculoDetalle? detalle = await _vehiculoRepository.GetDetalleByIdAsync(id);

            if (detalle == null || detalle.TallerId != tallerId)
            {
                return ServiceResult<VehiculoDetalle>.Fail(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Vehículo no encontrado.");
            }

            return ServiceResult<VehiculoDetalle>.Ok(detalle);
        }

        public async Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerPagedAsync(int tallerId, int pageNumber, int pageSize, VehiculoFiltroDto? filtro = null)
        {
            return await _vehiculoRepository.GetAllDetalleByTallerAsync(tallerId, pageNumber, pageSize, filtro);
        }

        public async Task<ServiceResult<VehiculoDetalle>> GetDetalleByMatriculaAsync(int tallerId, string matricula)
        {
            string m = matricula.Trim().ToUpper();
            VehiculoDetalle? detalle = await _vehiculoRepository.GetDetalleByMatriculaAsync(m);

            if (detalle == null || detalle.TallerId != tallerId)
            {
                return ServiceResult<VehiculoDetalle>.Fail(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Vehículo no encontrado.");
            }

            return ServiceResult<VehiculoDetalle>.Ok(detalle);
        }
    }
}