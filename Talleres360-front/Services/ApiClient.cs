using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Talleres360.Dtos.Responses;

namespace Talleres360_front.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
    {
        HttpClient client = BuildClient();
        HttpResponseMessage response = await client.GetAsync(endpoint);
        return await DeserializeAsync<T>(response);
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object body)
    {
        HttpClient client = BuildClient();
        StringContent content = SerializeBody(body);
        HttpResponseMessage response = await client.PostAsync(endpoint, content);
        return await DeserializeAsync<T>(response);
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object body)
    {
        HttpClient client = BuildClient();
        StringContent content = SerializeBody(body);
        HttpResponseMessage response = await client.PutAsync(endpoint, content);
        return await DeserializeAsync<T>(response);
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint)
    {
        HttpClient client = BuildClient();
        HttpResponseMessage response = await client.DeleteAsync(endpoint);
        return await DeserializeAsync<T>(response);
    }

    public async Task<(bool Success, string? ErrorMessage, ApiErrorResponse? Error)> PostRawAsync(
        string endpoint, object body)
    {
        HttpClient client = BuildClient();
        StringContent content = SerializeBody(body);
        HttpResponseMessage response = await client.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
            return (true, null, null);

        string raw = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
        return (false, error?.Mensaje ?? "Error desconocido.", error);
    }

    public async Task<ApiErrorResponse?> GetErrorAsync(HttpResponseMessage response)
    {
        string raw = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions);
    }

    private HttpClient BuildClient()
    {
        HttpClient client = _httpClientFactory.CreateClient("API");

        string? token = _httpContextAccessor.HttpContext?.Session.GetString("jwt");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static StringContent SerializeBody(object body)
    {
        string json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<ApiResponse<T>?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        string raw = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);
    }
}
