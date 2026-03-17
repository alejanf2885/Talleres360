using Talleres360.Enums;
using Talleres360.Interfaces.Archivos;
using Talleres360.Interfaces.FileStorage;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.SaneadorFotos;

namespace Talleres360.Services.Imagenes
{
    public class ImagenService : IImagenService
    {
        private INombreArchivoService _nombreService;
        private IProcesadorImagenService _procesador;
        private IFileStorageService _storage;

        public ImagenService
            (INombreArchivoService nombreService,
            IProcesadorImagenService procesador,
            IFileStorageService storage)
        {
            this._nombreService = nombreService;
            this._procesador = procesador;
            this._storage = storage;
        }

        public async Task BorrarImagenAsync(string rutaRelativa)
        {
            await this._storage.BorrarArchivoAsync(rutaRelativa);
        }

        public async Task<string> SubirImagenAsync(IFormFile archivo, CarpetaDestino carpeta, int tamano = 500)
        {

            //Quitar la extension del nombre del archivo
            string nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.FileName);

            // 1 Generar nombre unico 
            string nombreUnico = this._nombreService.GenerarNombreUnico(
                nombreOriginal: archivo.FileName,
                extensionDeseada: ".webp"
            );
            // 2 Procesamiento de la foto

            using (var streamLimpio = await this._procesador.SanearYProcesarAsync(archivo, tamano))
            {
                // 3 Guardar
                return await _storage.GuardarArchivoAsync(streamLimpio, nombreUnico, carpeta);
            }



        }
    }
}
