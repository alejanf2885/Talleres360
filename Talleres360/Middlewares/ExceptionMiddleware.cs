using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;

namespace Talleres360.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error no controlado en la ruta: {Path}", context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; 

            ApiErrorResponse errorResponse = new ApiErrorResponse(
                codigo: ErrorCode.SYS_ERROR_GENERICO.ToString(),
                mensaje: "Ha ocurrido un error interno en el servidor. Nuestro equipo ha sido notificado."
            );

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string jsonResponse = JsonSerializer.Serialize(errorResponse, options);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}