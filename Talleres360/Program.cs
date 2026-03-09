using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

// --- TUS NAMESPACES ---
using Talleres360.Data;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Repositories.Planes;
using Talleres360.Repositories.Talleres;
using Talleres360.Repositories.Usuarios;
using Talleres360.Services.Cache;
using Talleres360.Services.Password;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Talleres;
using Talleres360.Services.Usuarios;

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
// Acceso al contexto HTTP (Necesario para leer el Token desde servicios)
builder.Services.AddHttpContextAccessor();

// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITallerRepository, TallerRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();

// Servicios Core
builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();
builder.Services.AddScoped<ITallerService, TallerService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Tu nuevo servicio para extraer datos del Token fácilmente
builder.Services.AddScoped<IUserContextService, UserContextService>();

// =========================================================
// 4. AUTENTICACIÓN JWT (Configuración de la "llave")
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
// 7. CONTROLADORES Y DOCUMENTACIÓN NATIVA (OpenAPI)
// =========================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// =========================================================
// PIPELINE HTTP (Orden de ejecución)
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 1. CORS
app.UseCors("AllowFrontend");

// 2. Limitar peticiones
app.UseRateLimiter();

// 3. Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// 4. Mapear rutas
app.MapControllers();

app.Run();