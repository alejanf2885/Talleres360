# Talleres360 — Contexto del Proyecto

## Descripción General

API RESTful SaaS de gestión de talleres mecánicos construida en .NET 10 con ASP.NET Core.
Sistema multi-tenant donde cada taller tiene sus propios datos completamente aislados por `TallerId`.

## Stack Tecnológico

- **Framework:** ASP.NET Core Web API (.NET 10.0), C#
- **ORM:** Entity Framework Core 10.0.3 + SQL Server
- **Autenticación:** JWT Bearer + Refresh Tokens (cookies HttpOnly/Secure/SameSite=Strict)
- **Passwords:** BCrypt.Net-Next v4.1.0
- **Emails:** Resend v0.2.2
- **Imágenes:** SixLabors.ImageSharp v3.1.12
- **Logging:** Serilog v4.3.1
- **Docs API:** Scalar.AspNetCore v2.13.1
- **JSON:** Newtonsoft.Json v13.0.4
- **Pagos:** Stripe (integrado pero deshabilitado, `"Stripe": { "Enabled": false }`)

## Módulos del Sistema

| Módulo | Descripción |
|---|---|
| Auth | Registro de taller, login, logout, refresh token, verificación email, JWT |
| Clientes | CRUD, búsqueda, paginación, estadísticas |
| Vehículos | CRUD, catálogo de marcas/modelos (oficiales + custom por taller), tipos |
| Citas | CRUD, estados, conversión a trabajo |
| Trabajos | Órdenes de servicio, líneas de trabajo, estados, estados de pago |
| Inventario | Categorías de productos, productos, servicios, control de stock |
| Presupuestos | CRUD, líneas, conversión a trabajo |
| Facturas | DocumentosComerciales: generación, líneas, totales automáticos |
| NotasVehiculo | Notas por vehículo, tipos (GENERAL/ADVERTENCIA/ALERTA), auditoría |
| Emails | Queue de emails en background, templates HTML, integración Resend |
| Suscripciones | Planes con límites, módulos habilitables, precios mensual/anual |
| Cache | MemoryCache para catálogos (marcas, modelos, tipos) |
| Imágenes | Procesamiento (resize/compresión), FileStorage, nombres únicos |

## Estructura de Carpetas

```
Talleres360/
├── Models/              # Entidades EF Core (una por dominio)
├── Controllers/         # Endpoints API (una por dominio)
├── Services/            # Lógica de negocio (subcarpetas por dominio)
├── Repositories/        # Acceso a datos (subcarpetas por dominio)
├── Interfaces/          # Contratos (subcarpetas por dominio)
├── Dtos/                # Request y Response DTOs (subcarpetas por dominio)
│   ├── Auth/
│   ├── Clientes/
│   ├── Trabajos/
│   ├── Citas/
│   ├── Presupuestos/
│   ├── Inventario/
│   ├── Vehiculos/
│   ├── NotasVehiculo/
│   └── Responses/       # ApiResponse, ServiceResult, PagedResponse, ApiErrorResponse
├── Enums/               # Enumerados del dominio
│   └── Errors/          # ErrorCode enum centralizado
├── Filters/             # TallerAuthorize, RequiereSuscripcionActiva
├── Middlewares/         # ExceptionMiddleware
├── Extensions/          # CookieExtensions, etc.
├── Helpers/
├── Configuration/
├── Data/                # ApplicationDbContext.cs
└── Program.cs           # Configuración DI y middleware
```

## Patrones de Código — ESTÁNDARES

### Modelos (Entidades EF Core)

```csharp
[Table("Clientes")]                          // Tabla en plural
public class Cliente {
    [Key][Column("Id")] public int Id { get; set; }
    [Column("TallerId")] public int? TallerId { get; set; }  // Multi-tenancy siempre
    [Required][StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [StringLength(150)] public string? Email { get; set; }   // Nullable con ?
    [Column("Eliminado")] public bool Eliminado { get; set; } = false;  // Soft delete
    [Column("FechaCreacion")] public DateTime FechaCreacion { get; set; }
    [Column("FechaModificacion")] public DateTime? FechaModificacion { get; set; }
}
```

- Nombre de clase: **singular** (`Cliente`, `Vehiculo`, `Trabajo`)
- Nombre de tabla: **plural** en el atributo `[Table]`
- Siempre incluir: `TallerId`, `Eliminado`, `FechaCreacion`
- Enums se almacenan como string (configurado en DbContext con `.HasConversion<string>()`)
- Decimales con `HasPrecision(10, 2)` en DbContext
- Query filters globales para soft delete en DbContext

### DTOs

**Request DTOs** — con validaciones en español:
```csharp
public class CrearClienteRequest {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    public string Email { get; set; } = string.Empty;
}
```

**Response DTOs** — sin validaciones, solo propiedades:
```csharp
public class ClienteDto {
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    // ...
}
```

**Wrappers de respuesta** — ya existen, usar siempre:
- `ApiResponse<T>` — respuesta HTTP exitosa
- `ServiceResult<T>` — resultado interno de servicios
- `PagedResponse<T>` — listados paginados
- `ApiErrorResponse` — errores HTTP

### Controladores

```csharp
[Route("api/v1/[controller]")]   // Siempre v1
[ApiController]
[Authorize]                       // Protegido por defecto
public class ClientesController : ControllerBase {
    private readonly IClienteService _clienteService;
    private readonly IUserContextService _userContext;

    public ClientesController(IClienteService clienteService, IUserContextService userContext) {
        _clienteService = clienteService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, [FromQuery] string? buscar = null) {
        int? tallerId = _userContext.GetTallerId();
        if (!tallerId.HasValue) return Unauthorized();

        var resultado = await _clienteService.ObtenerTodosAsync(tallerId.Value, pagination, buscar);
        return Ok(ApiResponse<PagedResponse<ClienteDto>>.Ok(resultado, "Listado recuperado."));
    }

    [HttpPost]
    [RequiereSuscripcionActiva]
    public async Task<IActionResult> Create([FromBody] CrearClienteRequest request) {
        int? tallerId = _userContext.GetTallerId();
        if (!tallerId.HasValue) return Unauthorized();

        var resultado = await _clienteService.CrearAsync(tallerId.Value, request);
        if (!resultado.Success)
            return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));

        return Ok(ApiResponse<ClienteDto>.Ok(resultado.Data!, "Cliente creado."));
    }

    [HttpGet("{id:int:min(1)}")]
    [TallerAuthorize<IClienteRepository>]   // Valida que el recurso pertenece al taller
    public async Task<IActionResult> GetById(int id) { ... }
}
```

- Siempre extraer `TallerId` del contexto: `_userContext.GetTallerId()`
- Siempre validar `if (!tallerId.HasValue) return Unauthorized()`
- Retornar `ApiResponse<T>.Ok(data, mensaje)` en éxito
- Retornar `new ApiErrorResponse(codigo, mensaje)` en error
- Usar `[TallerAuthorize<IXxxRepository>]` en endpoints por ID
- Usar `[RequiereSuscripcionActiva]` en endpoints de creación

### Servicios

```csharp
public class ClienteService : IClienteService {
    private readonly IClienteRepository _clienteRepo;
    private readonly ITallerRepository _tallerRepo;

    public ClienteService(IClienteRepository clienteRepo, ITallerRepository tallerRepo) {
        _clienteRepo = clienteRepo;
        _tallerRepo = tallerRepo;
    }

    public async Task<ServiceResult<ClienteDto>> CrearAsync(int tallerId, CrearClienteRequest request) {
        // 1. Validar existencia de taller
        var taller = await _tallerRepo.GetByIdAsync(tallerId);
        if (taller == null)
            return ServiceResult<ClienteDto>.Fail(
                ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Taller no encontrado.");

        // 2. Validar límites de plan si aplica
        // 3. Normalizar datos
        string emailLimpio = request.Email.Trim().ToLower();

        // 4. Validar duplicados
        if (await _clienteRepo.ExistsByEmailAsync(tallerId, emailLimpio))
            return ServiceResult<ClienteDto>.Fail(
                ErrorCode.CUST_EMAIL_DUPLICADO.ToString(), "Ya existe un cliente con ese correo.");

        // 5. Construir entidad
        var cliente = new Cliente {
            TallerId = tallerId,
            Nombre = request.Nombre.Trim(),
            Email = emailLimpio,
            Eliminado = false,
            FechaCreacion = DateTime.UtcNow
        };

        await _clienteRepo.AddAsync(cliente);
        return ServiceResult<ClienteDto>.Ok(MapToDto(cliente));
    }
}
```

- Siempre usar `ServiceResult<T>.Ok(data)` y `ServiceResult<T>.Fail(codigo, mensaje)`
- Normalizar strings: `.Trim()`, `.ToLower()` según corresponda
- Usar `ErrorCode` enum para todos los códigos de error
- Mensajes de error en español

### Repositorios

```csharp
public class ClienteRepository : IClienteRepository {
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<PagedResponse<ClienteDto>> GetAllByTallerIdPagedAsync(
        int tallerId, PaginationParams pagination, string? buscar = null) {

        var query = _context.Clientes
            .Where(c => c.TallerId == tallerId);  // Soft delete aplicado por query filter

        if (!string.IsNullOrWhiteSpace(buscar)) {
            string criterio = buscar.Trim().ToLower();
            query = query.Where(c => c.Nombre.Contains(criterio) || c.Email!.Contains(criterio));
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => new ClienteDto { Id = c.Id, Nombre = c.Nombre })
            .ToListAsync();

        return new PagedResponse<ClienteDto> {
            Data = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Cliente?> GetByIdAsync(int id) =>
        await _context.Clientes.FindAsync(id);

    public async Task<bool> ExistsByEmailAsync(int tallerId, string email) =>
        await _context.Clientes.AnyAsync(c => c.TallerId == tallerId && c.Email == email);

    public async Task AddAsync(Cliente cliente) =>
        await _context.Clientes.AddAsync(cliente);

    public async Task<bool> PerteneceATallerAsync(int id, int tallerId) =>
        await _context.Clientes.AnyAsync(c => c.Id == id && c.TallerId == tallerId);
}
```

- Todos los métodos son `async Task<T>`
- `.AsNoTracking()` en queries de solo lectura
- Paginación siempre con `Skip/Take` + conteo total previo
- Búsqueda con `.Trim().ToLower()` para normalizar criterio
- Implementar `PerteneceATallerAsync` en todo repositorio de recurso del taller

### Wrappers de Respuesta — Implementaciones Exactas

**`ApiResponse<T>`** — respuesta HTTP exitosa (`Dtos/Responses/ApiResponse.cs`):
```csharp
public class ApiResponse<T> {
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Data = data, Message = message ?? "Operación realizada con éxito." };
}
```

**`ServiceResult<T>`** — resultado interno entre servicio y controlador (`Dtos/Responses/ServiceResult.cs`):
```csharp
public class ServiceResult<T> {
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ServiceResult<T> Fail(string errorCode, string message) =>
        new() { Success = false, ErrorCode = errorCode, Message = message };
}
```

**`ApiErrorResponse`** — error HTTP (`Dtos/Responses/ApiErrorResponse.cs`):
```csharp
public class ApiErrorResponse {
    public string Codigo { get; set; }
    public string Mensaje { get; set; }
    public object? Detalles { get; set; }

    public ApiErrorResponse(string codigo, string mensaje, object? detalles = null) {
        Codigo = codigo;
        Mensaje = mensaje;
        Detalles = detalles;
    }
}
```

**`PagedResponse<T>`** — listados paginados (`Dtos/PagedResponse.cs`):
```csharp
public class PagedResponse<T> {
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### Manejo de Errores

**Flujo completo de propagación:**
```
Repositorio → ServiceResult<T>.Fail(ErrorCode.XXX.ToString(), "mensaje") 
           → Controlador verifica !resultado.Success 
           → return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!))
```

**Middleware global** captura excepciones no controladas y retorna `ErrorCode.SYS_ERROR_GENERICO`.

**`ErrorCode` enum completo** (`Enums/Errors/ErrorCode.cs`) — siempre usar, nunca strings hardcodeados:
```csharp
public enum ErrorCode {
    // AUTH: Sesión, Acceso y Seguridad
    AUTH_CREDENCIALES_INCORRECTAS,
    AUTH_CUENTA_INACTIVA,
    AUTH_CUENTA_BLOQUEADA,
    AUTH_CUENTA_YA_ACTIVA,
    AUTH_TOKEN_INVALIDO,
    AUTH_TOKEN_EXPIRADO,
    AUTH_EMAIL_NO_VERIFICADO,
    AUTH_REFRESH_TOKEN_INVALIDO,
    AUTH_REFRESH_TOKEN_EXPIRADO,
    AUTH_LOGOUT_FALLIDO,
    AUTH_NO_AUTORIZADO,
    AUTH_FORBIDDEN,
    AUTH_ACCESO_DENEGADO,       // Usado en TallerAuthorize
    AUTH_REVOCACION_FALLIDA,

    // REG: Registro de Taller y Onboarding
    REG_FALLIDO,
    REG_PLAN_NO_ENCONTRADO,
    REG_EMAIL_YA_REGISTRADO,
    REG_CIF_DUPLICADO,
    REG_ERROR_SUBIDA_IMAGEN,
    REG_ERROR_CREACION_USUARIO,
    REG_TALLER_YA_EXISTE,
    REG_EMAIL_DUPLICADO,

    // SUBS: Suscripciones y Pagos (SaaS)
    SUBS_SIN_PLAN_ACTIVO,
    SUBS_LIMITE_ALCANZADO,
    SUBS_PAGO_RECHAZADO,

    // CUST: Gestión de Clientes
    CUST_NO_ENCONTRADO,
    CUST_DNI_DUPLICADO,
    CUST_EMAIL_DUPLICADO,
    CUST_TELEFONO_INVALIDO,
    CUST_SIN_FIRMA_RGPD,
    CUST_ERROR_ELIMINACION,
    CUST_LIMITE_PLAN_ALCANZADO,

    // VEH: Gestión de Vehículos
    VEH_NO_ENCONTRADO,
    VEH_MATRICULA_DUPLICADA,
    VEH_VIN_INVALIDO,
    VEH_ERROR_MAQUINA_ESTADO,
    VEH_MARCA_NO_ENCONTRADA,
    VEH_MODELO_NO_ENCONTRADA,

    // MAR: Marcas
    MAR_NOMBRE_DUPLICADO,

    // INV: Inventario
    INV_CATEGORIA_NO_ENCONTRADA,
    INV_CATEGORIA_NOMBRE_DUPLICADO,
    INV_PRODUCTO_NO_ENCONTRADO,
    INV_PRODUCTO_NOMBRE_DUPLICADO,
    INV_PRODUCTO_REFERENCIA_DUPLICADA,
    INV_PRECIO_INVALIDO,

    // CITA: Gestión de Citas
    CITA_NO_ENCONTRADA,
    CITA_ESTADO_INVALIDO,

    // TRA: Gestión de Trabajos
    TRA_NO_ENCONTRADO,
    TRA_ESTADO_INVALIDO,
    TRA_ESTADO_PAGO_INVALIDO,

    // SYS: Sistema y Errores Globales
    SYS_DATOS_INVALIDOS,
    SYS_ERROR_GENERICO,
    SYS_ENTIDAD_NO_ENCONTRADA,
    SYS_ARCHIVO_DEMASIADO_GRANDE,
    SYS_OPERACION_INVALIDA,
    SYS_ERROR_BASE_DATOS,
    SYS_SERVICIO_NO_DISPONIBLE,
}
```

### Convenciones de Nomenclatura

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases | PascalCase | `ClienteService`, `CrearClienteRequest` |
| Métodos | PascalCase + verbo, `Async` si es async | `CrearAsync`, `ObtenerTodosAsync`, `ExistsByEmailAsync` |
| Variables/Params | camelCase | `tallerId`, `emailLimpio`, `nuevoCliente` |
| Campos privados | `_camelCase` | `_clienteService`, `_context` |
| Propiedades | PascalCase | `Nombre`, `TallerId`, `FechaCreacion` |
| Enums/Constantes | UPPER_SNAKE_CASE | `ErrorCode.CUST_EMAIL_DUPLICADO`, `PlanTipo.Basico` |
| Tablas BD | Plural en español | `Clientes`, `Vehiculos`, `Trabajos` |
| Interfaces | `I` + PascalCase | `IClienteService`, `IClienteRepository` |

### Registro de Dependencias (Program.cs)

```csharp
// Repositorios y Servicios: Scoped
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Servicios stateless (ej. Bcrypt, Cache): Singleton
builder.Services.AddSingleton<IPasswordService, BcryptPasswordService>();
```

### Multi-Tenancy

- **Siempre** extraer `TallerId` del JWT con `_userContext.GetTallerId()`
- **Siempre** filtrar todas las queries por `TallerId`
- Query filters de EF Core aplican soft delete automáticamente
- `[TallerAuthorize<IXxxRepository>]` para validar ownership en endpoints por ID

### Imports — Orden Estándar

```csharp
// 1. System
using System.ComponentModel.DataAnnotations;

// 2. Microsoft
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// 3. Librerías externas
using Newtonsoft.Json;

// 4. Proyecto
using Talleres360.Data;
using Talleres360.Dtos.Clientes;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Models;
```

## Base de Datos — Esquema

### Seguridad y Multi-tenancy

**Sin RLS (Row-Level Security)**. El aislamiento entre talleres se gestiona íntegramente en la capa
de aplicación: el `TallerId` se extrae del JWT y se filtra en cada query de EF Core.
No hay política de seguridad a nivel de base de datos.

### Tablas

| Tabla | Descripción |
|---|---|
| Planes | Planes SaaS con límites y módulos habilitables |
| Talleres | Taller (tenant raíz). FK → Planes |
| Usuarios | Usuarios del taller. FK → Talleres |
| Credenciales | Credenciales de login. FK → Usuarios |
| TokensSeguridad | Refresh tokens, reset password, activación. FK → Usuarios |
| UsuarioVerificaciones | Verificación email/teléfono/2FA. FK → Usuarios |
| Clientes | Clientes del taller. FK → Talleres |
| Marcas | Marcas de vehículos (oficiales globales + custom por taller) |
| Modelos | Modelos de vehículos. FK → Marcas, VehiculoTipos, Talleres |
| VehiculoTipos | Catálogo de tipos (coche, moto…) — tabla estática |
| Vehiculos | Vehículos. FK → Talleres, Clientes, Marcas, Modelos, VehiculoTipos |
| NotasVehiculo | Notas por vehículo. FK → Talleres, Vehiculos, Usuarios |
| Trabajos | Órdenes de servicio. FK → Talleres, Vehiculos, Usuarios (mecánico/creador/modificador) |
| DetallesTrabajo | Líneas de un trabajo (productos/servicios/mano de obra). FK → Trabajos, Productos |
| CobrosTrabajo | Cobros parciales de un trabajo. FK → Talleres, Trabajos |
| CategoriasProducto | Categorías del inventario. FK → Talleres |
| Productos | Productos del inventario (stock, precios). FK → Talleres, CategoriasProducto |
| Servicios | Servicios del taller con precio base. FK → Talleres |
| ServiciosTarifas | Tarifas específicas por modelo de vehículo. FK → Servicios, Modelos |
| Citas | Citas/agenda. FK → Talleres, Vehiculos |
| Facturas | Documentos comerciales. FK → Talleres, Clientes, Trabajos |
| LineasFactura | Líneas de factura. FK → Facturas (ON DELETE CASCADE) |
| SecuenciasDocumentos | Contadores de numeración atómica por tipo/taller. FK → Talleres |
| AlertasMantenimiento | Alertas de mantenimiento programadas. FK → Talleres, Vehiculos |
| TallerNotificaciones | Configuración de canales de notificación por taller. FK → Talleres |
| PagosSuscripcion | Pagos Stripe de suscripción. FK → Talleres |
| AuditLog | Log de auditoría de operaciones (INSERT/UPDATE/DELETE) |

### Valores válidos por CHECK constraint

Estos son los únicos valores que acepta la base de datos para cada campo. Deben coincidir
con los enums C# almacenados como string.

```
Talleres.EstadoSuscripcion    → 'ACTIVO' | 'GRACE_PERIOD' | 'SUSPENDIDO' | 'CANCELADO'
Talleres.TipoSuscripcion      → 'MENSUAL' | 'ANUAL' | 'TRIAL' | 'CANCELADO'

Usuarios.Rol                  → 'SUPERADMIN' | 'ADMIN' | 'MECANICO' | 'RECEPCIONISTA'

Credenciales.TipoInicioSesion → 'LOCAL' | 'GOOGLE' | 'MICROSOFT' | 'APPLE'

TokensSeguridad.TipoToken     → 'REFRESH_TOKEN' | 'RESET_PASSWORD' | 'INVITACION' | 'ACTIVACION'

UsuarioVerificaciones.Tipo    → 'EMAIL' | 'TELEFONO' | 'DOS_FACTORES'

NotasVehiculo.Tipo            → 'GENERAL' | 'CLIENTE' | 'PENDIENTE' | 'AVISO'

Citas.Estado                  → 'PENDIENTE' | 'CONFIRMADA' | 'EN_PROCESO' | 'COMPLETADA' | 'CANCELADA'

Trabajos.Estado               → 'ABIERTO' | 'EN_PROCESO' | 'PENDIENTE_PIEZAS' | 'CERRADO' | 'CANCELADO'
Trabajos.EstadoPago           → 'PENDIENTE' | 'PARCIAL' | 'PAGADO' | 'ANULADO'

DetallesTrabajo.EstadoMaterial → 'PENDIENTE' | 'SOLICITADO' | 'RECIBIDO' | 'MONTADO'  (nullable)

CobrosTrabajo.MetodoPago      → 'EFECTIVO' | 'TARJETA' | 'TRANSFERENCIA' | 'BIZUM' | 'OTRO'  (nullable)

Facturas.TipoDocumento        → 'FACTURA' | 'PRESUPUESTO' | 'ALBARAN' | 'FACTURA_RECTIFICATIVA'
Facturas.EstadoPago           → 'PENDIENTE' | 'PARCIAL' | 'PAGADO' | 'ANULADO'
Facturas.MetodoPago           → 'EFECTIVO' | 'TARJETA' | 'TRANSFERENCIA' | 'BIZUM' | 'OTRO'  (nullable)

SecuenciasDocumentos.TipoDocumento → 'FACTURA' | 'PRESUPUESTO' | 'ALBARAN' | 'FACTURA_RECTIFICATIVA'

AlertasMantenimiento.CanalAviso    → 'EMAIL' | 'WHATSAPP' | 'SMS' | 'MANUAL'  (nullable)
AlertasMantenimiento.ResultadoEnvio → 'PENDIENTE' | 'ENVIADO' | 'ERROR' | 'REBOTADO'  (nullable)

TallerNotificaciones.Canal    → 'EMAIL' | 'WHATSAPP' | 'SMS'

PagosSuscripcion.Estado       → 'PAGADO' | 'PENDIENTE' | 'FALLIDO' | 'REEMBOLSADO'

AuditLog.Operacion            → 'INSERT' | 'UPDATE' | 'DELETE'
```

### Vista: VW_VehiculoDetalles

Join de `Vehiculos` + `Marcas` + `Modelos` + `VehiculoTipos` con campos calculados:
- `MarcaNombre`, `ModeloNombre`, `TipoNombre` — nombres resueltos de FK
- `NotasPendientes` — COUNT de notas activas (`Eliminado=0`, `Resuelta=0`)
- `TieneAviso` — `1` si tiene alguna nota tipo `'AVISO'` activa

En EF Core: `modelBuilder.Entity<VehiculoDetalle>().ToView("VW_VehiculoDetalles").HasNoKey()`

### Stored Procedure: sp_SiguienteNumeroDocumento

Genera números de documento secuenciales de forma **atómica** (transacción + `ROWLOCK/UPDLOCK`).
Formato de salida: `{Prefijo}{Año}-{6 dígitos}` → ej. `2026-000001`

```sql
DECLARE @num NVARCHAR(100);
EXEC dbo.sp_SiguienteNumeroDocumento
    @TallerId = 1,
    @TipoDocumento = 'FACTURA',
    @NumeroGenerado = @num OUTPUT;
-- @num = '2026-000001'
```

Crea la fila en `SecuenciasDocumentos` si no existe, luego incrementa y retorna el número.

## Comandos de Desarrollo

```bash
dotnet run                                   # Ejecutar API
dotnet build                                 # Compilar
dotnet ef migrations add NombreMigracion    # Nueva migración
dotnet ef database update                   # Aplicar migraciones
```

## Variables de Entorno

Configuradas en `appsettings.json` y `appsettings.Development.json`:
- `ConnectionStrings:SqlSaas` — BD SaaS (planes, talleres)
- `ConnectionStrings:SqlBBDD` — BD producción
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
- `ResendSettings:ApiKey`, `ResendSettings:TechnicalEmail`
- `AppSettings:FrontendUrl`
- `Stripe:Enabled` — actualmente `false`
