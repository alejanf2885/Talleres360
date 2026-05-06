using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;

namespace Talleres360_front.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SessionJwt               = "jwt";
    private const string SessionRefreshToken      = "refresh_token";
    private const string SessionNombreUsuario     = "nombre_usuario";
    private const string SessionNombreTaller      = "nombre_taller";
    private const string SessionTallerId          = "taller_id";
    private const string SessionRol               = "rol";
    private const string SessionPerfilConfigurado = "perfil_configurado";
    private const string RefreshTokenCookieName   = "refreshToken";

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(
        HttpContext httpContext, LoginRequest request)
    {
        HttpClient client = _httpClientFactory.CreateClient("API");
        StringContent body = BuildJsonBody(request);

        HttpResponseMessage response = await client.PostAsync("api/v1/auth/login", body);
        string raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
            return (false, error?.Mensaje ?? "Error al iniciar sesión.");
        }

        ApiResponse<AuthResponseDto>? result =
            JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(raw, JsonOptions);

        if (result?.Data is null)
            return (false, "Respuesta inesperada del servidor.");

        GuardarSesion(httpContext, result.Data);
        CapturarRefreshToken(httpContext, response);

        return (true, null);
    }

    public async Task<bool> RefreshAsync(HttpContext httpContext)
    {
        string? refreshToken = httpContext.Session.GetString(SessionRefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        HttpClient client = _httpClientFactory.CreateClient("API");
        HttpRequestMessage request = new(HttpMethod.Post, "api/v1/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshTokenCookieName}={refreshToken}");

        HttpResponseMessage response = await client.SendAsync(request);
        string raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ClearSession(httpContext);
            return false;
        }

        ApiResponse<AuthResponseDto>? result =
            JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(raw, JsonOptions);

        if (result?.Data is null)
        {
            ClearSession(httpContext);
            return false;
        }

        GuardarSesion(httpContext, result.Data);
        CapturarRefreshToken(httpContext, response);

        return true;
    }

    public async Task LogoutAsync(HttpContext httpContext)
    {
        string? jwt = httpContext.Session.GetString(SessionJwt);

        if (!string.IsNullOrEmpty(jwt))
        {
            HttpClient client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            await client.PostAsync("api/v1/auth/logout", null);
        }

        ClearSession(httpContext);
    }

    public async Task<(bool Success, string? ErrorMessage)> VerificarEmailAsync(string token)
    {
        HttpClient client = _httpClientFactory.CreateClient("API");
        HttpResponseMessage response = await client.GetAsync($"api/v1/verification/verify-email?token={Uri.EscapeDataString(token)}");
        string raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
            return (false, error?.Mensaje ?? "El enlace no es válido o ha expirado.");
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> OAuthLoginAsync(
        HttpContext httpContext, string provider, string email, string providerKey)
    {
        HttpClient client = _httpClientFactory.CreateClient("API");
        StringContent body = BuildJsonBody(new { Provider = provider, Email = email, ProviderKey = providerKey });

        HttpResponseMessage response = await client.PostAsync("api/v1/auth/oauth-login", body);
        string raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
            return (false, error?.Mensaje ?? "Error al iniciar sesión con Google.");
        }

        ApiResponse<AuthResponseDto>? result =
            JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(raw, JsonOptions);

        if (result?.Data is null)
            return (false, "Respuesta inesperada del servidor.");

        GuardarSesion(httpContext, result.Data);
        CapturarRefreshToken(httpContext, response);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegistroRequest request)
    {
        HttpClient client = _httpClientFactory.CreateClient("API");
        StringContent body = BuildJsonBody(request);

        HttpResponseMessage response = await client.PostAsync("api/v1/auth/register", body);
        string raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
            return (false, error?.Mensaje ?? "Error al registrar el taller.");
        }

        return (true, null);
    }

    private static void GuardarSesion(HttpContext httpContext, AuthResponseDto data)
    {
        httpContext.Session.SetString(SessionJwt, data.Token);

        if (data.Usuario is not null)
        {
            httpContext.Session.SetString(SessionNombreUsuario, data.Usuario.Nombre ?? string.Empty);
            httpContext.Session.SetString(SessionRol, data.Usuario.Rol ?? string.Empty);
            httpContext.Session.SetString(SessionTallerId,
                data.Usuario.TallerId?.ToString() ?? string.Empty);
            httpContext.Session.SetString(SessionPerfilConfigurado,
                data.Usuario.PerfilConfigurado ? "true" : "false");
        }
    }

    public static void MarcarPerfilConfigurado(HttpContext httpContext)
    {
        httpContext.Session.SetString(SessionPerfilConfigurado, "true");
    }

    public static bool EsPerfilConfigurado(HttpContext httpContext)
    {
        return httpContext.Session.GetString(SessionPerfilConfigurado) == "true";
    }

    private static void CapturarRefreshToken(HttpContext httpContext, HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
            return;

        foreach (string cookie in cookies)
        {
            if (!cookie.StartsWith($"{RefreshTokenCookieName}=", StringComparison.OrdinalIgnoreCase))
                continue;

            string value = cookie.Split(';')[0][$"{RefreshTokenCookieName}=".Length..];
            httpContext.Session.SetString(SessionRefreshToken, value);
            break;
        }
    }

    public static void GuardarNombreTaller(HttpContext httpContext, string nombre)
    {
        httpContext.Session.SetString(SessionNombreTaller, nombre);
    }

    private static void ClearSession(HttpContext httpContext)
    {
        httpContext.Session.Clear();
    }

    private static StringContent BuildJsonBody(object body)
    {
        string json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
