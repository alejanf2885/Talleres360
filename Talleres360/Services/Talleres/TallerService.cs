using Talleres360.Models;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Services.Talleres
{
    public class TallerService : ITallerService
    {
        private readonly ITallerRepository _tallerRepo;

        public TallerService(ITallerRepository tallerRepo)
        {
            _tallerRepo = tallerRepo;
        }

        public async Task<bool> ConfigurarPerfilAsync(int tallerId, string cif, string direccion, string localidad, string telefono)
        {
            // 1. Buscamos la entidad original en la DB
            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId);

            // Si no existe, devolvemos false para que el controlador sepa que algo fue mal
            if (taller == null)
            {
                return false;
            }

            // 2. Actualizamos los campos con la información de configuración
            taller.CIF = cif;
            taller.Direccion = direccion;
            taller.Localidad = localidad;
            taller.Telefono = telefono;

            // 3. Marcamos el perfil como completado
            taller.PerfilConfigurado = true;
            taller.FechaActualizacion = DateTime.UtcNow; // Usamos UTC para evitar líos de zonas horarias

            // 4. Persistimos los cambios en la base de datos
            await _tallerRepo.UpdateAsync(taller);

            return true;
        }

        public async Task<Taller> CrearTallerBaseAsync(string nombre, int planId)
        {
            Taller taller = new Taller
            {
                Nombre = nombre,
                PlanId = planId,
                Activo = true,
                PerfilConfigurado = false,
                FechaCreacion = DateTime.Now 
            };

            await _tallerRepo.AddAsync(taller);
            return taller;
        }
    }
}