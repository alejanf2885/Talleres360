namespace Talleres360.Interfaces.Emails
{
    public interface ITemplateService
    {
        Task<string> ObtenerPlantillaAsync(string nombreArchivo, Dictionary<string, string> reemplazos);
    }
}
