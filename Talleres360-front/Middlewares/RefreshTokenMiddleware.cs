using System.Text.Json;
using Talleres360_front.Services;

namespace Talleres360_front.Middlewares;

public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;

    public RefreshTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        string? jwt = context.Session.GetString("jwt");

        if (!string.IsNullOrEmpty(jwt) && IsExpiringSoon(jwt))
        {
            await authService.RefreshAsync(context);
        }

        await _next(context);
    }

    private static bool IsExpiringSoon(string jwt)
    {
        try
        {
            string[] parts = jwt.Split('.');
            if (parts.Length < 2) return false;

            string payload = parts[1];
            // Base64url → Base64
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "=";  break;
            }

            byte[] bytes = Convert.FromBase64String(payload);
            using JsonDocument doc = JsonDocument.Parse(bytes);

            if (!doc.RootElement.TryGetProperty("exp", out JsonElement expEl))
                return false;

            long expUnix = expEl.GetInt64();
            DateTimeOffset expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix);

            // Renovar si expira en menos de 2 minutos
            return expTime < DateTimeOffset.UtcNow.AddMinutes(2);
        }
        catch
        {
            return false;
        }
    }
}
