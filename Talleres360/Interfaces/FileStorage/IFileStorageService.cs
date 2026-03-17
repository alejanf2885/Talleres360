
using Talleres360.Enums;

namespace Talleres360.Interfaces.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> GuardarArchivoAsync(Stream contenido, string nombreArchivo, CarpetaDestino carpeta);
        Task BorrarArchivoAsync(string rutaRelativa);
    }
}
