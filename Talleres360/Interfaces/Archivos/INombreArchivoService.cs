namespace Talleres360.Interfaces.Archivos
{
    public interface INombreArchivoService
    {
        string GenerarNombreUnico(string nombreOriginal, string sufijo = "", string extensionDeseada = null);
    }
}
