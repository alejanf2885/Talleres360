using QuestPDF.Fluent;
using Talleres360.Enums;
using Talleres360.Interfaces.Facturacion;
using Talleres360.Interfaces.FileStorage;
using Talleres360.Models.Facturacion;

namespace Talleres360.Services.Facturacion
{
    public class FacturaPdfService : IFacturaPdfService
    {
        private readonly IFileStorageService _fileStorage;

        public FacturaPdfService(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<string> GenerarYSubirAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses)
        {
            FacturaPdfTemplate template = new FacturaPdfTemplate(factura, lineas, desgloses);

            using MemoryStream stream = new MemoryStream();
            template.GeneratePdf(stream);
            stream.Position = 0;

            // Ruta final en talleres360-facturas: pdfFacturas/{tallerId}/{numeroFactura}.pdf
            string nombreBlob = $"{factura.TallerId}/{factura.NumeroFactura}.pdf";
            return await _fileStorage.GuardarArchivoAsync(stream, nombreBlob, CarpetaDestino.PdfFacturas);
        }
    }
}
