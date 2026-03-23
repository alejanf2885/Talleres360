using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
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

        public async Task<ServiceResult<Cliente>> CrearClienteAsync(
            int tallerId, CrearClienteRequest request)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null)
                return ServiceResult<Cliente>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Error: Taller no identificado.");

            if (taller.TipoSuscripcion != "TRIAL")
            {
                int totalClientes = await _customerRepo.CountByTallerIdAsync(tallerId);
                Plan? planActual = await _planRepo.GetPlanPorIdAsync(
                    taller.PlanId ?? (int)PlanTipo.FREE);

                // CAMBIO: LimiteClientes ya es int no nullable
                // Convenio: 0 = sin límite
                if (planActual != null
                    && planActual.LimiteClientes > 0
                    && totalClientes >= planActual.LimiteClientes)
                {
                    return ServiceResult<Cliente>.Fail(
                        ErrorCode.CUST_LIMITE_PLAN_ALCANZADO.ToString(),
                        $"Límite de {planActual.LimiteClientes} clientes alcanzado " +
                        $"en tu plan {planActual.Nombre}. ¡Sube a un plan superior!"
                    );
                }
            }

            string emailLimpio = request.Email.Trim().ToLower();
            bool existeEmail = await _customerRepo.ExistsByEmailAsync(tallerId, emailLimpio);
            if (existeEmail)
                return ServiceResult<Cliente>.Fail(
                    ErrorCode.CUST_EMAIL_DUPLICADO.ToString(),
                    "Ya existe un cliente con ese correo electrónico en tu taller.");

            string? nifNormalizado = null;
            if (!string.IsNullOrWhiteSpace(request.NifCif))
            {
                nifNormalizado = request.NifCif.Trim().ToUpper()
                    .Replace("-", "").Replace(" ", "");
                bool existeNif = await _customerRepo.ExistsByNifAsync(tallerId, nifNormalizado);
                if (existeNif)
                    return ServiceResult<Cliente>.Fail(
                        ErrorCode.CUST_DNI_DUPLICADO.ToString(),
                        "Ya existe un cliente con este DNI/CIF registrado en tu taller.");
            }

            Cliente nuevoCliente = new Cliente
            {
                TallerId = tallerId,
                Nombre = request.Nombre.Trim(),
                Apellidos = request.Apellidos?.Trim(),
                NifCif = nifNormalizado,
                EsEmpresa = request.EsEmpresa,
                Direccion = request.Direccion?.Trim(),
                CodigoPostal = request.CodigoPostal?.Trim(),
                Localidad = request.Localidad?.Trim(),
                Provincia = request.Provincia?.Trim(),
                Telefono = request.Telefono.Trim(),
                Email = emailLimpio,
                AceptaComunicaciones = request.AceptaComunicaciones,
                Eliminado = false,
                FechaCreacion = DateTime.UtcNow,
                FechaFirmaRgpd = DateTime.UtcNow
            };

            await _customerRepo.AddAsync(nuevoCliente);
            return ServiceResult<Cliente>.Ok(nuevoCliente);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(
            int tallerId, string? buscar = null)
        {
            return await _customerRepo.GetAllByTallerIdAsync(tallerId, buscar);
        }

        public async Task<PagedResponse<Cliente>> ObtenerTodosPagedAsync(
            int tallerId, PaginationParams pagination, string? buscar = null)
        {
            return await _customerRepo.GetAllByTallerIdPagedAsync(tallerId, pagination, buscar);
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId)
        {
            Cliente? cliente = await _customerRepo.GetByIdAsync(clienteId);

            if (cliente == null || cliente.TallerId != tallerId || cliente.Eliminado)
                return null;

            return cliente;
        }

        public async Task<ServiceResult<Cliente>> ActualizarClienteAsync(
            int tallerId, int clienteId, ActualizarClienteRequest request)
        {
            Cliente? clienteExistente = await ObtenerPorIdAsync(tallerId, clienteId);
            if (clienteExistente == null)
                return ServiceResult<Cliente>.Fail(
                    ErrorCode.CUST_NO_ENCONTRADO.ToString(),
                    "Cliente no encontrado o no pertenece a su taller.");

            string emailLimpio = request.Email.Trim().ToLower();
            if (emailLimpio != clienteExistente.Email)
            {
                bool existeEmail = await _customerRepo.ExistsByEmailAsync(tallerId, emailLimpio);
                if (existeEmail)
                    return ServiceResult<Cliente>.Fail(
                        ErrorCode.CUST_EMAIL_DUPLICADO.ToString(),
                        "El correo electrónico ya está en uso por otro cliente.");
            }

            string? nifNormalizado = null;
            if (!string.IsNullOrWhiteSpace(request.NifCif))
            {
                nifNormalizado = request.NifCif.Trim().ToUpper()
                    .Replace("-", "").Replace(" ", "");
                if (nifNormalizado != clienteExistente.NifCif)
                {
                    bool existeNif = await _customerRepo.ExistsByNifAsync(tallerId, nifNormalizado);
                    if (existeNif)
                        return ServiceResult<Cliente>.Fail(
                            ErrorCode.CUST_DNI_DUPLICADO.ToString(),
                            "El DNI/CIF ya está registrado en otro cliente.");
                }
            }

            clienteExistente.Nombre = request.Nombre.Trim();
            clienteExistente.Apellidos = request.Apellidos?.Trim();
            clienteExistente.NifCif = nifNormalizado;
            clienteExistente.EsEmpresa = request.EsEmpresa;
            clienteExistente.Direccion = request.Direccion?.Trim();
            clienteExistente.CodigoPostal = request.CodigoPostal?.Trim();
            clienteExistente.Localidad = request.Localidad?.Trim();
            clienteExistente.Provincia = request.Provincia?.Trim();
            clienteExistente.Telefono = request.Telefono.Trim();
            clienteExistente.Email = emailLimpio;
            clienteExistente.AceptaComunicaciones = request.AceptaComunicaciones;
            clienteExistente.FechaModificacion = DateTime.UtcNow;

            await _customerRepo.UpdateAsync(clienteExistente);
            return ServiceResult<Cliente>.Ok(clienteExistente);
        }

        public async Task<ServiceResult<bool>> EliminarClienteAsync(
            int tallerId, int clienteId)
        {
            Cliente? clienteExistente = await ObtenerPorIdAsync(tallerId, clienteId);
            if (clienteExistente == null)
                return ServiceResult<bool>.Fail(
                    ErrorCode.CUST_NO_ENCONTRADO.ToString(),
                    "Cliente no encontrado o no pertenece a su taller.");

            clienteExistente.Eliminado = true;
            clienteExistente.FechaModificacion = DateTime.UtcNow;

            await _customerRepo.UpdateAsync(clienteExistente);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ClienteStatsResponse> ObtenerEstadisticasAsync(int tallerId)
        {
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);
            if (taller == null) throw new Exception("Taller no encontrado.");

            Plan? plan = await _planRepo.GetPlanPorIdAsync(
                taller.PlanId ?? (int)PlanTipo.FREE);

            int total = await _customerRepo.CountByTallerIdAsync(tallerId);
            int nuevosEsteMes = await _customerRepo.CountNuevosEsteMesAsync(tallerId);

            return new ClienteStatsResponse
            {
                TotalClientes = total,
                ClientesNuevosEsteMes = nuevosEsteMes,
                LimitePlan = plan?.LimiteClientes == 0 ? null : plan?.LimiteClientes,
                NombrePlan = plan?.Nombre ?? "Desconocido"
            };
        }
    }
}