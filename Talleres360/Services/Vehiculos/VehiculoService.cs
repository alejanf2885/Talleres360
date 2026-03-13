using System.Threading.Tasks;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
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

  
        public async Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerAsync(
            int tallerId,
            int pageNumber,
            int pageSize,
            VehiculoFiltroDto? filtro = null)
        {
            return await _vehiculoRepository.GetAllDetalleByTallerAsync(
                tallerId,
                pageNumber,
                pageSize,
                filtro);
        }

     
        public async Task<VehiculoDetalle?> GetDetalleByIdAsync(int id)
        {
            return await _vehiculoRepository.GetDetalleByIdAsync(id);
        }

      
        public async Task<VehiculoDetalle?> GetDetalleByMatriculaAsync(string matricula)
        {
            return await _vehiculoRepository.GetDetalleByMatriculaAsync(matricula);
        }

       
        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await _vehiculoRepository.GetByIdAsync(id);
        }

        public async Task<Vehiculo?> GetByMatriculaAsync(string matricula)
        {
            return await _vehiculoRepository.GetByMatriculaAsync(matricula);
        }

        public async Task AddAsync(Vehiculo vehiculo)
        {
            await _vehiculoRepository.AddAsync(vehiculo);
        }

        public async Task UpdateAsync(Vehiculo vehiculo)
        {
            await _vehiculoRepository.UpdateAsync(vehiculo);
        }

        public async Task<bool> ExistsAsync(string matricula)
        {
            return await _vehiculoRepository.ExistsAsync(matricula);
        }
    }
}