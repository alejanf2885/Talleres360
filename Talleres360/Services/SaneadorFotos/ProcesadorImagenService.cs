using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Talleres360.Interfaces.SaneadorFotos;

namespace Talleres360.Services.SaneadorFotos
{
    public class ProcesadorImagenService : IProcesadorImagenService
    {
        public async Task<Stream> SanearYProcesarAsync(IFormFile archivoImagen, int tamano = 300)
        {
            if (archivoImagen == null || archivoImagen.Length == 0)
            {
                throw new ArgumentException("El archivo de imagen no puede estar vacío.", nameof(archivoImagen));
            }

            var outputStream = new MemoryStream();

            try
            {

                // Image.LoadAsync intenta interpretar los píxeles 
                //  Si el archivo es falso o corrupto, lanzará una excepcion
                using (var inputStream = archivoImagen.OpenReadStream())
                {
                    using (var image = await Image.LoadAsync(inputStream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(tamano, tamano),
                            Mode = ResizeMode.Crop
                        }));

                        //Generamos el archivo nuevo y limpio en WEBP
                        await image.SaveAsWebpAsync(outputStream);
                    }
                }
            }
            
            catch (UnknownImageFormatException)
            {
                throw new InvalidOperationException("El archivo subido no es una imagen válida o está corrupto.");
            }

            outputStream.Position = 0;
            return outputStream;
        }
    }
}