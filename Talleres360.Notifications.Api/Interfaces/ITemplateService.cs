namespace Talleres360.Notifications.Api.Interfaces
{
    public interface ITemplateService
    {
        Task<string> ObtenerPlantillaAsync(string nombreArchivo, Dictionary<string, string> reemplazos);
    }
}
