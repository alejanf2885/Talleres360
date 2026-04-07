using Talleres360.Dtos.DocumentosComerciales;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;

namespace Talleres360.Interfaces.DocumentosComerciales
{
    public interface IDocumentoComercialService
    {
        Task<ServiceResult<DocumentoComercialPreparado>> PrepararDocumentoAsync(
            int tallerId,
            TipoDocumentoComercial tipoDocumento,
            string numeroDocumento,
            DocumentoComercialInput input);
    }
}
