# Talleres360 API

Backend SaaS para la gestión integral de talleres mecánicos. Construido con **.NET 10** y **ASP.NET Core Web API**, ofrece autenticación, multi-tenancy, gestión de vehículos, órdenes de trabajo, presupuestos, inventario, citas y facturación.

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | .NET 10 / ASP.NET Core 10 |
| Base de datos | SQL Server + EF Core 10 |
| Autenticación | JWT Bearer + Refresh Tokens |
| Email | [Resend](https://resend.com/) |
| Documentación API | Scalar (OpenAPI) |
| Procesamiento de imágenes | SixLabors.ImageSharp |
| Hash de contraseñas | BCrypt.Net |
| Logging | Serilog |
| Caché en memoria | IMemoryCache |
| Tests | xUnit |

---

## Arquitectura

El proyecto sigue una arquitectura en capas con separación clara de responsabilidades:

```
Talleres360/
├── Controllers/        # Thin controllers – sin lógica de negocio
├── Services/           # Lógica de negocio, validaciones, normalización
├── Repositories/       # Acceso a datos con EF Core
├── Models/             # Entidades de dominio
├── Dtos/               # Objetos de transferencia de datos (Request / Response)
├── Interfaces/         # Contratos de servicios y repositorios
├── Enums/              # Enumeraciones del dominio y códigos de error
├── Filters/            # Atributos de autorización y suscripción
├── Middlewares/        # Manejo global de excepciones
├── Configuration/      # Opciones de configuración tipadas
├── Data/               # ApplicationDbContext
└── Templates/          # Plantillas HTML para emails
```

### Patrones clave

- **ServiceResult\<T\>** – todos los servicios retornan este objeto con `Success`, `Data`, `ErrorCode` y `Message`.
- **ApiResponse** – los controllers mapean `ServiceResult` a `{ success, data, message }` o `{ success, errorCode, message }`.
- **Repository + Unit of Work** – acceso a datos centralizado con `IUnitOfWork`.
- **Multi-tenancy** – cada recurso está aislado por `TallerId`; el `IUserContextService` provee el contexto del taller autenticado.
- **Background Queue** – envío de emails asíncrono mediante `IBackgroundTaskQueue` + `EmailBackgroundWorker`.

---

## Módulos

| Módulo | Descripción |
|---|---|
| **Auth** | Registro de talleres, login, refresh token, verificación de email |
| **Talleres** | Gestión del perfil del taller (workshop) |
| **Clientes** | CRUD de clientes del taller |
| **Vehículos** | Gestión de vehículos, tipos, marcas y modelos |
| **Citas** | Programación y seguimiento de citas |
| **Trabajos** | Órdenes de trabajo con detalle de líneas |
| **Presupuestos** | Creación y gestión de presupuestos |
| **Inventario** | Productos y categorías de productos |
| **Servicios** | Catálogo de servicios del taller |
| **Documentos Comerciales** | Facturas y presupuestos formales |
| **Notas de Vehículo** | Anotaciones técnicas sobre vehículos |
| **Usuarios** | Gestión de usuarios y roles del taller |

---

## Seguridad

- **JWT** con validación de issuer, audience y tiempo de vida.
- **Rate Limiting** por IP:
  - `/auth` – 5 peticiones / 2 minutos
  - `/refresh` – 10 peticiones / minuto
  - Emails – 2 peticiones / minuto
  - Verificación – 5 peticiones / minuto
- **`TallerAuthorizeAttribute`** – verifica que el recurso pertenece al taller del token.
- **`RequiereSuscripcionActivaAttribute`** – bloquea el acceso si el taller no tiene suscripción activa.
- **Multi-tenancy** – aislamiento estricto por `TallerId` en todas las consultas.

---

## Configuración

### Variables necesarias

En `appsettings.Development.json` o mediante User Secrets:

```json
{
  "ConnectionStrings": {
    "SqlSaas": "Server=...;Database=...;User Id=...;Password=...;"
  },
  "Jwt": {
    "Key": "<clave-secreta-minimo-32-caracteres>",
    "Issuer": "Talleres360API",
    "Audience": "Talleres360Users"
  },
  "ResendSettings": {
    "ApiKey": "<tu-api-key-de-resend>",
    "TechnicalEmail": "noreply@tudominio.com",
    "DefaultSenderName": "Talleres360"
  },
  "AppSettings": {
    "FrontendUrl": "https://tudominio.com/"
  }
}
```

> **Importante:** Nunca incluyas credenciales reales en `appsettings.json`. Usa [User Secrets](https://learn.microsoft.com/es-es/aspnet/core/security/app-secrets) en desarrollo y variables de entorno en producción.

---

## Inicio rápido

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local o en la nube)

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/alejanf2885/Talleres360.git
cd Talleres360

# 2. Configurar User Secrets (o editar appsettings.Development.json)
cd Talleres360
dotnet user-secrets set "ConnectionStrings:SqlSaas" "tu-cadena-de-conexion"
dotnet user-secrets set "Jwt:Key" "tu-clave-secreta"
dotnet user-secrets set "ResendSettings:ApiKey" "tu-api-key"

# 3. Aplicar migraciones
dotnet ef database update

# 4. Ejecutar la API
dotnet run
```

La API estará disponible en `https://localhost:7xxx`. La documentación interactiva (Scalar) se expone en `/scalar/v1` en entorno de desarrollo.

---

## Tests

```bash
cd Talleres360.Test
dotnet test
```

Los tests cubren los servicios principales: `CitaService`, `TrabajoService`, `PresupuestoService`, `ServicioService` y `DocumentoComercialService`.

---

## CORS

El frontend Angular se espera en `http://localhost:4200` en desarrollo. Para producción, actualiza la política `AllowFrontend` en `Program.cs` con la URL real del frontend.

---

## Licencia

Proyecto privado – todos los derechos reservados.
