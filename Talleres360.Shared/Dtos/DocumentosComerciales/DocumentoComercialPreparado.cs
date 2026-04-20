using Talleres360.Models.Facturacion;

namespace Talleres360.Dtos.DocumentosComerciales
{
    public class DocumentoComercialPreparado
    {
        public Factura Documento { get; set; } = new Factura();
        public List<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();
        public List<DesgloseIva> DesglosesIva { get; set; } = new List<DesgloseIva>();
    }
}
