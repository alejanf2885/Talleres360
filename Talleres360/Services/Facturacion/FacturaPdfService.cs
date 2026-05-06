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
        private readonly IHttpClientFactory _httpClientFactory;

        public FacturaPdfService(IFileStorageService fileStorage, IHttpClientFactory httpClientFactory)
        {
            _fileStorage       = fileStorage;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerarYSubirAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses, string? logoUrl = null)
        {
            byte[]? logoBytes = null;
            if (!string.IsNullOrWhiteSpace(logoUrl) && logoUrl.StartsWith("http"))
            {
                try
                {
                    HttpClient http = _httpClientFactory.CreateClient();
                    logoBytes = await http.GetByteArrayAsync(logoUrl);
                }
                catch { /* logo no disponible, continuar sin él */ }
            }

            FacturaPdfTemplate template = new FacturaPdfTemplate(factura, lineas, desgloses, logoBytes);

            using MemoryStream stream = new MemoryStream();
            template.GeneratePdf(stream);
            stream.Position = 0;

            string nombreBlob = $"{factura.TallerId}/{factura.NumeroFactura}.pdf";
            return await _fileStorage.GuardarArchivoAsync(stream, nombreBlob, CarpetaDestino.PdfFacturas);
        }
    }
}
