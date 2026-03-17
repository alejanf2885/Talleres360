using Talleres360.Models;

namespace Talleres360.Interfaces.Talleres
{
    public interface ITallerService
    {
        Task<Taller> CrearTallerBaseAsync(string nombreNegocio, int planId);
        Task<bool> ConfigurarPerfilAsync(int tallerId, string cif, string direccion, string localidad, string telefono, IFormFile? logo);
    }
}
