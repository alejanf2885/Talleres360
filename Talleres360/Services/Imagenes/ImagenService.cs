using Talleres360.Enums;
using Talleres360.Interfaces.Archivos;
using Talleres360.Interfaces.FileStorage;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.SaneadorFotos;

namespace Talleres360.Services.Imagenes
{
    public class ImagenService : IImagenService
    {
        private readonly INombreArchivoService _nombreService;
        private readonly IProcesadorImagenService _procesador;
        private readonly IFileStorageService _storage;

        public ImagenService(
            INombreArchivoService nombreService,
            IProcesadorImagenService procesador,
            IFileStorageService storage)
        {
            _nombreService = nombreService;
            _procesador = procesador;
            _storage = storage;
        }

        public async Task BorrarImagenAsync(string rutaRelativa)
        {
            await _storage.BorrarArchivoAsync(rutaRelativa);
        }

        public async Task<string> SubirImagenBase64Async(string base64String, CarpetaDestino carpeta, int tamano = 500)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return string.Empty;

            string cleanBase64 = base64String;
            if (base64String.Contains(","))
            {
                cleanBase64 = base64String.Substring(base64String.IndexOf(",") + 1);
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);

                using var inputStream = new MemoryStream(imageBytes);

                string nombreUnico = _nombreService.GenerarNombreUnico(
                    nombreOriginal: "img_upload",
                    extensionDeseada: ".webp"
                );

                using (var streamLimpio = await _procesador.SanearYProcesarStreamAsync(inputStream, tamano))
                {
                    return await _storage.GuardarArchivoAsync(streamLimpio, nombreUnico, carpeta);
                }
            }
            catch (FormatException)
            {
                throw new Exception("El formato Base64 no es válido.");
            }
        }
    }
}