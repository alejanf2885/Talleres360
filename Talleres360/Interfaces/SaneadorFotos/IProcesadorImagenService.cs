namespace Talleres360.Interfaces.SaneadorFotos
{
    public interface IProcesadorImagenService
    {
        Task<Stream> SanearYProcesarStreamAsync(Stream inputStream, int tamano = 300);
    }
}
