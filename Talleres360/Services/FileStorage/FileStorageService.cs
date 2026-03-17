using Talleres360.Enums;
using Talleres360.Interfaces.FileStorage;

namespace Talleres360.Services.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task BorrarArchivoAsync(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa))
            {
                return Task.CompletedTask;
            }

            // Evitamos borrar la imagen por defecto
            if (rutaRelativa.Contains("default-avatar.png", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            try
            {
                // PLAN B: Evitar que WebRootPath sea null
                string webRootPath = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
                }

                string rutaLimpia = rutaRelativa.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
                string rutaFisica = Path.Combine(webRootPath, rutaLimpia);

                if (File.Exists(rutaFisica))
                {
                    File.Delete(rutaFisica);
                }
            }
            catch (Exception)
            {
                // Ignoramos silenciosamente si el archivo no se pudo borrar
            }

            return Task.CompletedTask;
        }

        public async Task<string> GuardarArchivoAsync(Stream contenido, string nombreArchivo, CarpetaDestino carpeta)
        {
            string nombreCarpeta = carpeta.ToString().ToLower();

            string webRootPath = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            // Ahora combinamos usando la ruta segura
            string rutaCarpeta = Path.Combine(webRootPath, "images", nombreCarpeta);

            // Creamos los directorios (wwwroot, images y la subcarpeta) si no existen
            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            string rutaFisica = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var fileStream = new FileStream(rutaFisica, FileMode.Create))
            {
                await contenido.CopyToAsync(fileStream);
            }

            return $"/images/{nombreCarpeta}/{nombreArchivo}";
        }
    }
}