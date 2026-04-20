using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Talleres360.Configuration;
using Talleres360.Data;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Archivos;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Background;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Citas;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.DocumentosComerciales;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.FileStorage;
using Talleres360.Interfaces.Imagenes;
using Talleres360.Interfaces.Inventario;
using Talleres360.Interfaces.NotasVehiculo;
using Talleres360.Interfaces.Password;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.SaneadorFotos;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Servicios;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Middlewares;
using Talleres360.Repositories;
using Talleres360.Repositories.Clientes;
using Talleres360.Repositories.Citas;
using Talleres360.Repositories.Data;
using Talleres360.Repositories.Inventario;
using Talleres360.Repositories.NotasVehiculo;
using Talleres360.Repositories.Planes;
using Talleres360.Repositories.Presupuestos;
using Talleres360.Repositories.Seguridad;
using Talleres360.Repositories.Servicios;
using Talleres360.Repositories.Talleres;
using Talleres360.Repositories.Trabajos;
using Talleres360.Repositories.Usuarios;
using Talleres360.Repositories.Vehiculos;
using Talleres360.Services.Archivos;
using Talleres360.Services.Auth;
using Talleres360.Services.Background;
using Talleres360.Services.Cache;
using Talleres360.Services.Citas;
using Talleres360.Services.Clientes;
using Talleres360.Services.DocumentosComerciales;
using Talleres360.Services.Emails;
using Talleres360.Services.FileStorage;
using Talleres360.Services.Imagenes;
using Talleres360.Services.Inventario;
using Talleres360.Services.NotasVehiculo;
using Talleres360.Services.Password;
using Talleres360.Services.Presupuestos;
using Talleres360.Services.SaneadorFotos;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Servicios;
using Talleres360.Services.Talleres;
using Talleres360.Services.Trabajos;
using Talleres360.Services.Usuarios;
using Talleres360.Services.Vehiculos;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("SqlSaas")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlBBDD'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["ResendSettings:ApiKey"] ?? "";
});
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITallerRepository, TallerRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IVehiculoTipoRepository, VehiculoTipoRepository>();
builder.Services.AddScoped<IMarcaRepository, MarcaRepository>();
builder.Services.AddScoped<IModeloRepository, ModeloRepository>();
builder.Services.AddScoped<IVerificacionRepository, VerificacionRepository>();
builder.Services.AddScoped<ICategoriaProductoRepository, CategoriaProductoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<ITrabajoRepository, TrabajoRepository>();
builder.Services.AddScoped<ICobroTrabajoRepository, CobroTrabajoRepository>();
builder.Services.AddScoped<ITarifaHoraRepository, TarifaHoraRepository>();
builder.Services.AddScoped<IDetalleTrabajoRepository, DetalleTrabajoRepository>();
builder.Services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();
builder.Services.AddScoped<INotaVehiculoRepository, NotaVehiculoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();
builder.Services.AddScoped<ITallerService, TallerService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IVehiculoTipoService, VehiculoTipoService>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
builder.Services.AddScoped<IModeloService, ModeloService>();
builder.Services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ITrabajoService, TrabajoService>();
builder.Services.AddScoped<ICobroTrabajoService, CobroTrabajoService>();
builder.Services.AddScoped<ITarifaHoraService, TarifaHoraService>();
builder.Services.AddScoped<IDetalleTrabajoService, DetalleTrabajoService>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<IDocumentoComercialService, DocumentoComercialService>();
builder.Services.AddScoped<INotaVehiculoService, NotaVehiculoService>();
builder.Services.AddScoped<IProcesadorImagenService, ProcesadorImagenService>();
builder.Services.AddScoped<IImagenService, ImagenService>();
builder.Services.AddScoped<INombreArchivoService, NombreArchivoService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<EmailBackgroundWorker>();

builder.Services.AddScoped<ISuscripcionGuardService, SuscripcionGuardService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRegistroTallerService, RegistroTallerService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IVerificacionService, VerificacionService>();

builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection("AppSettings"));

string? jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("La clave JWT (Jwt:Key) no está configurada en variables de entorno. Es obligatoria en producción.");

    jwtKey = "dev-secret-key-only-for-testing-replace-in-production-" + Guid.NewGuid().ToString()[..16];
}
byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey);

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

builder.Services.AddAuthorization();

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

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthStrict", httpContext =>
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(2),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("RefreshPolicy", httpContext =>
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("EmailStrict", httpContext =>
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 2,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("VerifyStrict", httpContext =>
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            List<object> errores = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => (object)new
                {
                    Campo = e.Key,
                    Error = e.Value?.Errors.First().ErrorMessage
                }).ToList();

            ApiErrorResponse response = new ApiErrorResponse(
                codigo: ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                mensaje: "Existen errores de validación en los datos enviados.",
                detalles: errores
            );

            return new BadRequestObjectResult(response);
        };
    });


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Talleres360 API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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