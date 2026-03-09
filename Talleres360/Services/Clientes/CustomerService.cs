using Talleres360.Dtos.Clientes;
using Talleres360.Enums;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Services.Clientes
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly ITallerRepository _tallerRepo;

        public CustomerService(ICustomerRepository customerRepo, ITallerRepository tallerRepo)
        {
            _customerRepo = customerRepo;
            _tallerRepo = tallerRepo;
        }

        public async Task<(bool Success, string Message, Cliente? Cliente)> CrearClienteAsync(int tallerId, CrearClienteRequest request)
        {
            // 1. Obtener datos del taller para validar plan y trial
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
            {
                return (false, "Error: Taller no identificado.", null);
            }

            // 2. Lógica de Límites: Solo se aplica si el periodo TRIAL ha terminado
            if (taller.TipoSuscripcion != "TRIAL")
            {
                int totalClientes = await _customerRepo.CountByTallerIdAsync(tallerId);
                PlanTipo planActual = (PlanTipo)(taller.PlanId ?? (int)PlanTipo.FREE);

                if (planActual == PlanTipo.FREE && totalClientes >= 10)
                    return (false, "Límite de 10 clientes alcanzado en el plan FREE.", null);

                if (planActual == PlanTipo.BASICO && totalClientes >= 20)
                    return (false, "Límite de 20 clientes alcanzado en el plan BÁSICO.", null);
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
                FechaCreacion = DateTime.UtcNow 
            };

            await _customerRepo.AddAsync(nuevoCliente);

            return (true, "Cliente registrado con éxito.", nuevoCliente);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(int tallerId, string? buscar = null)
        {
            IEnumerable<Cliente> listaClientes = await _customerRepo.GetAllByTallerIdAsync(tallerId, buscar);
            return listaClientes;
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId)
        {
            Cliente? cliente = await _customerRepo.GetByIdAsync(clienteId);

            // SEGURIDAD: Validar que el cliente pertenezca al taller de quien pregunta
            if (cliente == null || cliente.TallerId != tallerId || cliente.Eliminado)
            {
                return null;
            }

            return cliente;
        }
    }
}