using Talleres360.Interfaces.Emails;

namespace Talleres360.Services.Emails
{
    public class TemplateService : ITemplateService
    {
        public async Task<string> ObtenerPlantillaAsync(string nombreArchivo, Dictionary<string, string> reemplazos)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Templates", $"{nombreArchivo}.html");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"La plantilla {nombreArchivo} no existe en {filePath}");

            string contenido = await File.ReadAllTextAsync(filePath);

            foreach (KeyValuePair<string, string> item in reemplazos)
            {
                contenido = contenido.Replace(item.Key, item.Value);
            }

            return contenido;
        }
    }
}