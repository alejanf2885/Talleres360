using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Resend;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;
using Talleres360.Data;
using Talleres360.Interfaces.Archivos;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.FileStorage;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.SaneadorFotos;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Repositories;
using Talleres360.Repositories.Clientes;
using Talleres360.Repositories.Planes;
using Talleres360.Repositories.Talleres;
using Talleres360.Repositories.Usuarios;
using Talleres360.Repositories.Vehiculos;
using Talleres360.Services.Archivos;
using Talleres360.Services.Auth;
using Talleres360.Services.Cache;
using Talleres360.Services.Clientes;
using Talleres360.Services.FileStorage;
using Talleres360.Services.Imagenes;
using Talleres360.Services.Password;
using Talleres360.Services.SaneadorFotos;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Talleres;
using Talleres360.Services.Usuarios;
using Talleres360.Services.Vehiculos;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. BASE DE DATOS
// =========================================================
string connectionString = builder.Configuration.GetConnectionString("SqlSaas")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlSaas'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================
// 2. CACHÉ
// =========================================================
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// =========================================================
// 3. INYECCIÓN DE DEPENDENCIAS (IoC)
// =========================================================
builder.Services.AddHttpContextAccessor();


// --- CONFIGURACIÓN DE RESEND  ---

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["ResendSettings:ApiKey"] ?? "";
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();


// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITallerRepository, TallerRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();

// Servicios Core
builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();
builder.Services.AddScoped<ITallerService, TallerService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IProcesadorImagenService, ProcesadorImagenService>();
builder.Services.AddScoped<IImagenService, ImagenService>();
builder.Services.AddScoped<INombreArchivoService, NombreArchivoService>();
builder.Services.AddScoped<IProcesadorImagenService, ProcesadorImagenService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Seguridad y Gestión
builder.Services.AddScoped<ISuscripcionGuardService, SuscripcionGuardService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRegistroTallerService, RegistroTallerService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>(); // NUEVO




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
        policy.WithOrigins("http://localhost:4200") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

// =========================================================
// 6. RATE LIMITER (Políticas de Seguridad)
// =========================================================
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthStrict", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(2),
            QueueLimit = 0
        });
    });

    options.AddPolicy("RefreshPolicy", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ========================================
// 7. CONTROLADORES Y OPENAPI
// ========================================
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Talleres360 API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// =========================================================
// 8. PIPELINE HTTP
// =========================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    _ = app.MapScalarApiReference(options =>
    {
        _ = options
            .WithTitle("Talleres360 API Docs")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
