namespace Talleres360.Notifications.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeader = "X-Api-Key";
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/openapi") || path.StartsWith("/scalar"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { codigo = "AUTH_API_KEY_REQUERIDA", mensaje = "API key requerida." });
                return;
            }

            var config = context.RequestServices.GetRequiredService<IConfiguration>();
            var validKey = config["NotificationsApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(validKey) || providedKey != validKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { codigo = "AUTH_API_KEY_INVALIDA", mensaje = "API key inválida." });
                return;
            }

            await _next(context);
        }
    }
}
