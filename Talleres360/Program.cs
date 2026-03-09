using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Scalar.AspNetCore;

using Talleres360.Data;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Clientes;
using Talleres360.Repositories.Planes;
using Talleres360.Repositories.Talleres;
using Talleres360.Repositories.Usuarios;
using Talleres360.Repositories.Clientes;
using Talleres360.Services.Cache;
using Talleres360.Services.Password;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Talleres;
using Talleres360.Services.Usuarios;
using Talleres360.Services.Clientes;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. BASE DE DATOS
// =========================================================

// Obtiene la cadena de conexión desde appsettings.json
// Si no existe lanza una excepción para evitar que la app arranque mal configurada
string connectionString = builder.Configuration.GetConnectionString("SqlSaas")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlSaas'.");

// Registro del DbContext usando SQL Server
// DbContext se registra como Scoped por defecto (una instancia por request)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================
// 2. MOTOR DE CACHÉ
// =========================================================

// Activa el sistema de caché en memoria de ASP.NET
builder.Services.AddMemoryCache();

// Servicio propio que encapsula el uso del caché
builder.Services.AddScoped<ICacheService, CacheService>();

// =========================================================
// 3. REPOSITORIOS E INYECCIÓN DE DEPENDENCIAS
// =========================================================

// Permite acceder al HttpContext desde servicios (por ejemplo para obtener usuario actual)
builder.Services.AddHttpContextAccessor();

// ---------------------------
// Repositorios (acceso a datos)
// ---------------------------

// Gestión de usuarios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Gestión de talleres
builder.Services.AddScoped<ITallerRepository, TallerRepository>();

// Gestión de planes de suscripción
builder.Services.AddScoped<IPlanRepository, PlanRepository>();

// Gestión de clientes de un taller
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>(); // El nuevo de clientes

// ---------------------------
// Servicios Core (lógica negocio)
// ---------------------------

// Servicio para hash de contraseñas (bcrypt)
// Singleton porque no mantiene estado y es seguro reutilizarlo
builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();

// Lógica de negocio relacionada con talleres
builder.Services.AddScoped<ITallerService, TallerService>();

// Servicio central de identidad (login, registro, etc.)
builder.Services.AddScoped<IIdentityService, IdentityService>();

// Generación y validación de JWT
builder.Services.AddScoped<ITokenService, TokenService>();

// Obtiene información del usuario actual desde el token
builder.Services.AddScoped<IUserContextService, UserContextService>();

// ---------------------------
// Seguridad y gestión de clientes
// ---------------------------

// Servicio que controla si el taller tiene suscripción activa
// Actúa como "portero" antes de permitir ciertas acciones
builder.Services.AddScoped<ISuscripcionGuardService, SuscripcionGuardService>();

// Lógica de negocio relacionada con clientes del taller
builder.Services.AddScoped<ICustomerService, CustomerService>();

// =========================================================
// 4. AUTENTICACIÓN JWT
// =========================================================

// Obtiene la clave secreta desde configuración
// Si no existe usa una de desarrollo
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TuSuperClaveSecretaDeDesarrolloMuyLarga123456789!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// Configuración del middleware de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verifica que el emisor del token sea válido
            ValidateIssuer = true,

            // Verifica que el destinatario del token sea válido
            ValidateAudience = true,

            // Verifica que el token no haya expirado
            ValidateLifetime = true,

            // Verifica la firma del token
            ValidateIssuerSigningKey = true,

            // Valores válidos configurados
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Talleres360API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Talleres360Users",

            // Clave usada para firmar el JWT
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// =========================================================
// 5. CORS
// =========================================================

// Permite que el frontend (React, Vue, etc) pueda llamar a la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()   // Permite cualquier dominio
              .AllowAnyHeader()   // Permite cualquier header
              .AllowAnyMethod();  // Permite GET, POST, PUT, DELETE, etc
    });
});

// =========================================================
// 6. RATE LIMITER
// =========================================================

// Sistema de limitación de peticiones para evitar abuso (ej: ataques de fuerza bruta)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginLimiter", httpContext =>
    {
        // Usa la IP del cliente como identificador
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Configuración del limitador
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,                 // Máximo 5 peticiones
            Window = TimeSpan.FromMinutes(2),// Cada 2 minutos
            QueueLimit = 0                   // No permite cola
        });
    });

    // Código HTTP que se devuelve cuando se supera el límite
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// =========================================================
// 7. CONTROLADORES Y OPENAPI (Scalar)
// =========================================================

// Habilita el uso de controladores MVC en la API
builder.Services.AddControllers();

// Configuración nativa de OpenAPI para .NET 10
// Permite generar documentación automática de la API
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Talleres360 API - Gestión de Talleres SaaS";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// =========================================================
// PIPELINE HTTP (Orden de ejecución)
// =========================================================

if (app.Environment.IsDevelopment())
{
    // Genera el documento OpenAPI JSON
    app.MapOpenApi();

    // Interfaz visual moderna de documentación (alternativa a Swagger)
    _ = app.MapScalarApiReference(options =>
    {
        _ = options
            .WithTitle("Talleres360 Documentation")
            .WithTheme(ScalarTheme.Moon) // Tema oscuro profesional
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Redirección automática a HTTPS
app.UseHttpsRedirection();

// Aplica la política CORS
app.UseCors("AllowFrontend");

// Activa el rate limiter
app.UseRateLimiter();

// Middleware de autenticación JWT
app.UseAuthentication();

// Middleware de autorización (roles, policies, etc)
app.UseAuthorization();

// Mapea los controladores de la API
app.MapControllers();

// Arranca la aplicación
app.Run();