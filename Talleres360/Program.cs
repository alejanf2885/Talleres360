using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Scalar.AspNetCore; // <-- Nuevo: Para la interfaz de documentación

// --- TUS NAMESPACES ---
using Talleres360.Data;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Clientes; // Añadido
using Talleres360.Repositories.Planes;
using Talleres360.Repositories.Talleres;
using Talleres360.Repositories.Usuarios;
using Talleres360.Repositories.Clientes; // Añadido
using Talleres360.Services.Cache;
using Talleres360.Services.Password;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Talleres;
using Talleres360.Services.Usuarios;
using Talleres360.Services.Clientes; // Añadido

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. BASE DE DATOS
// =========================================================
string connectionString = builder.Configuration.GetConnectionString("SqlSaas")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlSaas'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================
// 2. MOTOR DE CACHÉ
// =========================================================
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// =========================================================
// 3. REPOSITORIOS E INYECCIÓN DE DEPENDENCIAS
// =========================================================
builder.Services.AddHttpContextAccessor();

// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITallerRepository, TallerRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>(); // El nuevo de clientes

// Servicios Core
builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();
builder.Services.AddScoped<ITallerService, TallerService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Seguridad y Clientes
builder.Services.AddScoped<ISuscripcionGuardService, SuscripcionGuardService>(); // El Portero
builder.Services.AddScoped<ICustomerService, CustomerService>(); // Servicio de Clientes

// =========================================================
// 4. AUTENTICACIÓN JWT
// =========================================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TuSuperClaveSecretaDeDesarrolloMuyLarga123456789!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Talleres360API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Talleres360Users",
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// =========================================================
// 5. CORS
// =========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================================================
// 6. RATE LIMITER
// =========================================================
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(2),
            QueueLimit = 0
        });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// =========================================================
// 7. CONTROLADORES Y OPENAPI (Scalar)
// =========================================================
builder.Services.AddControllers();

// Configuración nativa de OpenAPI para .NET 10 con soporte JWT
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
    // Genera el documento JSON
    app.MapOpenApi();

    // Activa la interfaz visual moderna de Scalar
    _ = app.MapScalarApiReference(options =>
    {
        _ = options
            .WithTitle("Talleres360 Documentation")
            .WithTheme(ScalarTheme.Moon) // Tema oscuro profesional
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();