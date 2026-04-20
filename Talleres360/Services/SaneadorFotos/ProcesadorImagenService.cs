using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp; 
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
                throw new ArgumentException("El stream de la imagen no puede estar vacío.", nameof(inputStream));
            }

            MemoryStream outputStream = new MemoryStream();

            try
            {
                using (Image image = await Image.LoadAsync(inputStream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(tamano, tamano),
                        Mode = ResizeMode.Crop
                    }));

                    WebpEncoder encoder = new WebpEncoder
                    {
                        Quality = 80
                    };

                    await image.SaveAsWebpAsync(outputStream, encoder);
                }
            }
            catch (UnknownImageFormatException ex)
            {
                throw new InvalidOperationException("El archivo enviado no es una imagen válida o está corrupto.", ex);
            }

            outputStream.Position = 0;
            return outputStream;
        }
    }
}