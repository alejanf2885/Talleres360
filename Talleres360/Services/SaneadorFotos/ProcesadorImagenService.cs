using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Talleres360.Interfaces.SaneadorFotos;

namespace Talleres360.Services.SaneadorFotos
{
    public class ProcesadorImagenService : IProcesadorImagenService
    {
        public async Task<Stream> SanearYProcesarStreamAsync(Stream inputStream, int tamano = 300)
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                throw new ArgumentException("El stream de la imagen no puede estar vacío.");
            }

            var outputStream = new MemoryStream();

            try
            {
                // Image.LoadAsync lee directamente de la memoria
                using (var image = await Image.LoadAsync(inputStream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(tamano, tamano),
                        Mode = ResizeMode.Crop
                    }));

                    // Generamos el archivo nuevo y limpio en WEBP
                    await image.SaveAsWebpAsync(outputStream);
                }
            }
            catch (UnknownImageFormatException)
            {
                throw new InvalidOperationException("El texto Base64 enviado no es una imagen válida o está corrupto.");
            }

            outputStream.Position = 0;
            return outputStream;
        }
    }
}