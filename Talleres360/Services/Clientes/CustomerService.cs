using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Enums;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Services.Clientes
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly ITallerRepository _tallerRepo;
        private readonly IPlanRepository _planRepo;

        public CustomerService(
            ICustomerRepository customerRepo,
            ITallerRepository tallerRepo,
            IPlanRepository planRepo)
        {
            _customerRepo = customerRepo;
            _tallerRepo = tallerRepo;
            _planRepo = planRepo;
        }

        public async Task<(bool Success, string Message, Cliente? Cliente)> CrearClienteAsync(int tallerId, CrearClienteRequest request)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null) return (false, "Error: Taller no identificado.", null);

            if (taller.TipoSuscripcion != "TRIAL")
            {
                int totalClientes = await _customerRepo.CountByTallerIdAsync(tallerId);
                // Leemos el plan de la BD para obtener el límite real
                Plan? planActual = await _planRepo.GetPlanPorIdAsync(taller.PlanId ?? (int)PlanTipo.FREE);

                // Si el LimiteClientes no es null y nos hemos pasado...
                if (planActual != null && planActual.LimiteClientes.HasValue && totalClientes >= planActual.LimiteClientes.Value)
                {
                    return (false, $"Límite de {planActual.LimiteClientes.Value} clientes alcanzado en tu plan {planActual.Nombre}. ¡Sube a un plan superior!", null);
                }
            }

            Cliente nuevoCliente = new Cliente
            {
                TallerId = tallerId,
                Nombre = request.Nombre,
                Apellidos = request.Apellidos,
                Telefono = request.Telefono,
                Email = request.Email,
                AceptaComunicaciones = request.AceptaComunicaciones,
                Eliminado = false,
                FechaCreacion = DateTime.UtcNow,
                FechaFirmaRGPD = DateTime.UtcNow
            };

            await _customerRepo.AddAsync(nuevoCliente);

            return (true, "Cliente registrado con éxito.", nuevoCliente);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(int tallerId, string? buscar = null)
        {
            return await _customerRepo.GetAllByTallerIdAsync(tallerId, buscar);
        }

        public async Task<PagedResponse<Cliente>> ObtenerTodosPagedAsync(int tallerId, PaginationParams pagination, string? buscar = null)
        {
            return await _customerRepo.GetAllByTallerIdPagedAsync(tallerId, pagination, buscar);
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId)
        {
            Cliente? cliente = await _customerRepo.GetByIdAsync(clienteId);

            if (cliente == null || cliente.TallerId != tallerId || cliente.Eliminado)
            {
                return null;
            }

            return cliente;
        }

        public async Task<(bool Success, string Message, Cliente? Cliente)> ActualizarClienteAsync(int tallerId, int clienteId, ActualizarClienteRequest request)
        {
            // 1. Buscamos y validamos propiedad (Reutilizamos la lógica del GetById)
            Cliente? clienteExistente = await ObtenerPorIdAsync(tallerId, clienteId);

            if (clienteExistente == null)
                return (false, "Cliente no encontrado o no pertenece a su taller.", null);

            // 2. Actualizamos campos
            clienteExistente.Nombre = request.Nombre;
            clienteExistente.Apellidos = request.Apellidos;
            clienteExistente.Telefono = request.Telefono;
            clienteExistente.Email = request.Email;
            clienteExistente.AceptaComunicaciones = request.AceptaComunicaciones;

            // Opcional: Podrías añadir un campo FechaModificacion en tu modelo
            // clienteExistente.FechaModificacion = DateTime.UtcNow;

            // 3. Guardamos
            await _customerRepo.UpdateAsync(clienteExistente);

            return (true, "Cliente actualizado con éxito.", clienteExistente);
        }

        public async Task<(bool Success, string Message)> EliminarClienteAsync(int tallerId, int clienteId)
        {
            Cliente? clienteExistente = await ObtenerPorIdAsync(tallerId, clienteId);

            if (clienteExistente == null)
                return (false, "Cliente no encontrado o no pertenece a su taller.");

            // Soft Delete
            clienteExistente.Eliminado = true;

            await _customerRepo.UpdateAsync(clienteExistente);

            return (true, "Cliente eliminado correctamente.");
        }

        public async Task<ClienteStatsResponse> ObtenerEstadisticasAsync(int tallerId)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null) throw new Exception("Taller no encontrado.");

            Plan? plan = await _planRepo.GetPlanPorIdAsync(taller.PlanId ?? (int)PlanTipo.FREE);

            int total = await _customerRepo.CountByTallerIdAsync(tallerId);
            int nuevosEsteMes = await _customerRepo.CountNuevosEsteMesAsync(tallerId);

            return new ClienteStatsResponse
            {
                TotalClientes = total,
                ClientesNuevosEsteMes = nuevosEsteMes,
                LimitePlan = plan?.LimiteClientes,
                NombrePlan = plan?.Nombre ?? "Desconocido"
            };
        }
    }
}
