namespace Talleres360.Interfaces.Facturacion
{
    public interface IFacturaRepository
    {
        Task<string> GenerarNumeroFacturaAsync(int tallerId);
        Task<List<DetalleTrabajo>> ObtenerDetallesParaFacturarAsync(int trabajoId);
        Task GuardarSnapshotAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses);
    }
}
