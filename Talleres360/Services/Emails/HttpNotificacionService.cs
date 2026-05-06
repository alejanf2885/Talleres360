using System.Net.Http.Json;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Emails;

namespace Talleres360.Services.Emails
{
    public class HttpNotificacionService : INotificacionService
    {
        private readonly HttpClient _httpClient;

        public HttpNotificacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<bool>> EnviarBienvenidaAsync(Usuario usuario, string link)
        {
            try
            {
                var payload = new
                {
                    nombre = usuario.Nombre,
                    email = usuario.Email,
                    link
                };

                var response = await _httpClient.PostAsJsonAsync("api/v1/notificaciones/bienvenida", payload);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<bool>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "No se pudo enviar el email de bienvenida.");

                return ServiceResult<bool>.Ok(true);
            }
            catch
            {
                return ServiceResult<bool>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "No se pudo enviar el email de bienvenida.");
            }
        }
    }
}
