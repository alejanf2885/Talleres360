
using Talleres360.Enums;

namespace Talleres360.Interfaces.Imagenes
{
    public interface IImagenService
    {
        Task<string> SubirImagenBase64Async(string base64String, CarpetaDestino carpeta, int tamano = 500);
        Task BorrarImagenAsync(string rutaRelativa);
    }
}
