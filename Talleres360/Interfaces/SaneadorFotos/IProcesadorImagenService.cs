namespace Talleres360.Interfaces.SaneadorFotos
{
    public interface IProcesadorImagenService
    {
        Task<Stream> SanearYProcesarAsync(IFormFile archivoImagen, int tamano = 300);
    }
}
