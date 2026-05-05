# Plan: Generación de PDF de Facturas + Azure Blob Storage

## Contexto

El sistema ya genera facturas como snapshots inmutables (Facturas + LineasFactura + DesglosesIva).
El objetivo es, al momento de facturar un trabajo, generar automáticamente el PDF de esa factura,
subirlo a Azure Blob Storage y guardar la URL resultante en la base de datos.

---

## Decisiones de Diseño

| Decisión | Elección | Motivo |
|---|---|---|
| Librería PDF | QuestPDF | Fluent API en C#, sin dependencias nativas, community license |
| Blob Storage | Azure Blob Storage (SDK v12) | `Azure.Storage.Blobs` |
| Cuándo generar | Dentro de `FacturacionService.FacturarTrabajoAsync()` | Garantiza que toda factura tiene PDF; flujo atómico |
| Nombre del blob | `{tallerId}/facturas/{numeroFactura}.pdf` | Organizado por taller, legible, sin colisiones |
| Container | `facturas` (privado) | Los PDFs son documentos sensibles; URL con SAS token o acceso via API |
| URL en BD | URL pública (o SAS con larga expiración) | Permite acceso directo desde frontend |

---

## Dependencias NuGet a Agregar

Al proyecto `Talleres360` (API):

```
QuestPDF                  (última versión estable ≥ 2024.x)
Azure.Storage.Blobs       (≥ 12.x)
```

---

## Cambios en Base de Datos

### Columna nueva en `Facturas`

```sql
ALTER TABLE Facturas ADD UrlPdf NVARCHAR(500) NULL;
```

Vía migración EF Core:
- Añadir `public string? UrlPdf { get; set; }` al modelo `Factura`
- `dotnet ef migrations add AddUrlPdfToFacturas`
- `dotnet ef database update`

---

## Configuración

### `appsettings.json`

```json
"AzureBlobStorage": {
  "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
  "ContainerName": "facturas"
}
```

### `appsettings.Development.json`

Puede usar Azurite (emulador local):
```json
"AzureBlobStorage": {
  "ConnectionString": "UseDevelopmentStorage=true",
  "ContainerName": "facturas"
}
```

---

## Arquitectura: Archivos Nuevos

### 1. `Configuration/AzureBlobStorageSettings.cs`
POCO para bindear la configuración del blob.

### 2. `Interfaces/Storage/IBlobStorageService.cs`
```csharp
public interface IBlobStorageService
{
    Task<string> SubirAsync(Stream contenido, string nombreBlob, string contentType);
    Task EliminarAsync(string nombreBlob);
}
```

### 3. `Services/Storage/AzureBlobStorageService.cs`
Implementación con `BlobServiceClient`. Crea el container si no existe (en el constructor/init).
Devuelve la URL pública del blob subido.

### 4. `Interfaces/Facturacion/IFacturaPdfService.cs`
```csharp
public interface IFacturaPdfService
{
    Task<string> GenerarYSubirAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses);
}
```
Devuelve la URL del PDF en Azure.

### 5. `Services/Facturacion/FacturaPdfService.cs`
- Recibe los datos de la factura ya guardada (snapshot completo)
- Usa QuestPDF para renderizar el PDF en un `MemoryStream`
- Llama a `IBlobStorageService.SubirAsync()`
- Devuelve la URL

### 6. `Services/Facturacion/FacturaPdfTemplate.cs`
Clase que define el layout del PDF con QuestPDF.
Separada del servicio para mantener la lógica de presentación aislada.

**Secciones del PDF:**
```
┌─────────────────────────────────────────────┐
│  LOGO TALLER (si existe)    FACTURA #xxx     │
│  Datos del taller           Fecha emisión    │
├──────────────────┬──────────────────────────┤
│  DATOS CLIENTE   │  DATOS FACTURA           │
│  Nombre          │  Tipo documento           │
│  NIF/CIF         │  Serie                   │
│  Dirección       │  Estado pago              │
│  Email           │  Método pago              │
├──────────────────┴──────────────────────────┤
│  LÍNEAS                                      │
│  Concepto | Cant | Precio | Dto% | IVA% | Total │
│  ...                                         │
├─────────────────────────────────────────────┤
│  DESGLOSE IVA              SUBTOTAL:  xx,xx  │
│  Base   | IVA% | Cuota    IVA:       xx,xx  │
│  ...                       TOTAL:     xx,xx  │
├─────────────────────────────────────────────┤
│  Notas legales                               │
└─────────────────────────────────────────────┘
```

---

## Archivos a Modificar

### `Talleres360.Shared/Models/Facturacion/Factura.cs`
Añadir:
```csharp
[Column("UrlPdf")]
[StringLength(500)]
public string? UrlPdf { get; set; }
```

### `Interfaces/Facturacion/IFacturacionService.cs`
Sin cambios en la firma pública. El PDF se genera internamente.

### `Services/Facturacion/FacturacionService.cs`
Después de `GuardarSnapshotAsync()`, añadir:
```csharp
// Generar PDF y guardar URL
string urlPdf = await _facturaPdfService.GenerarYSubirAsync(factura, lineas, desgloses);
await _facturaRepository.ActualizarUrlPdfAsync(factura.Id, urlPdf);
```
Requiere inyectar `IFacturaPdfService` y el nuevo método en el repositorio.

### `Interfaces/Facturacion/IFacturaRepository.cs`
Añadir:
```csharp
Task ActualizarUrlPdfAsync(int facturaId, string urlPdf);
Task<FacturaCompletaDto?> ObtenerCompletaAsync(int facturaId, int tallerId);
```

### `Repositories/Facturas/FacturaRepository.cs`
Implementar los dos métodos nuevos:
- `ActualizarUrlPdfAsync`: `UPDATE Facturas SET UrlPdf = @url WHERE Id = @id`
- `ObtenerCompletaAsync`: carga `Factura` + `LineasFactura` + `DesglosesIva`

### `Dtos/Facturas/FacturaCompletaDto.cs` *(nuevo)*
DTO de lectura que incluye la URL del PDF:
```csharp
public class FacturaCompletaDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; }
    public string? UrlPdf { get; set; }
    // ...resto de campos
    public List<LineaFacturaDto> Lineas { get; set; }
    public List<DesgloseIvaDto> DesglosesIva { get; set; }
}
```

### `Program.cs`
Registrar nuevos servicios:
```csharp
// Blob Storage
builder.Services.Configure<AzureBlobStorageSettings>(
    builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

// PDF
builder.Services.AddScoped<IFacturaPdfService, FacturaPdfService>();

// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;
```

---

## Flujo Completo (después del cambio)

```
POST /api/v1/trabajos/{id}/facturar
  │
  ▼
TrabajosController.FacturarAsync()
  │
  ▼
TrabajoService.FacturarAsync()
  │
  ▼
FacturacionService.FacturarTrabajoAsync()
  ├─ Valida trabajo CERRADO
  ├─ Obtiene datos taller + cliente
  ├─ ObtenerDetallesParaFacturarAsync()
  ├─ GenerarNumeroFacturaAsync()
  ├─ Calcula líneas + desgloses IVA
  ├─ GuardarSnapshotAsync()          ← ya existente
  ├─ GenerarYSubirAsync()            ← NUEVO (QuestPDF → Azure)
  ├─ ActualizarUrlPdfAsync()         ← NUEVO
  └─ Retorna TrabajoDto (con UrlPdf en la factura)
```

---

## Fases de Implementación

### Fase 1 — Infraestructura de Storage
1. Añadir NuGet `Azure.Storage.Blobs`
2. Crear `AzureBlobStorageSettings.cs`
3. Crear `IBlobStorageService.cs`
4. Crear `AzureBlobStorageService.cs`
5. Registrar en `Program.cs`
6. Probar subida de archivo de prueba

### Fase 2 — Migración de BD
1. Añadir `UrlPdf` al modelo `Factura`
2. Generar y aplicar migración EF Core
3. Añadir `ActualizarUrlPdfAsync` al repositorio e interfaz

### Fase 3 — Template PDF (QuestPDF)
1. Añadir NuGet `QuestPDF` + configurar license
2. Crear `FacturaPdfTemplate.cs` con el layout
3. Iterar el diseño (secciones: cabecera, datos, líneas, totales, notas)

### Fase 4 — Servicio PDF
1. Crear `IFacturaPdfService.cs`
2. Crear `FacturaPdfService.cs` (orquesta template + blob)
3. Registrar en `Program.cs`

### Fase 5 — Integración
1. Inyectar `IFacturaPdfService` en `FacturacionService`
2. Llamar a `GenerarYSubirAsync` después del snapshot
3. Actualizar `UrlPdf` en BD
4. Verificar que `TrabajoDto` expone la `UrlPdf` de la factura

### Fase 6 — Pruebas
1. Probar el endpoint `POST /api/v1/trabajos/{id}/facturar` completo
2. Verificar que el PDF se sube correctamente a Azure
3. Verificar que la URL queda guardada en BD
4. Abrir el PDF y validar el contenido visual

---

## Consideraciones

- **Atomicidad parcial:** El snapshot se guarda en BD antes del PDF. Si el PDF falla, la factura
  queda sin URL pero el número ya fue consumido. Mitigación: loguear el error y considerar un
  job de retry para facturas sin `UrlPdf`.
- **Logo del taller:** Si en el futuro se añade logo, se puede recuperar de Azure (otra imagen)
  y embeber en el PDF. Por ahora, solo texto.
- **Tamaño del PDF:** QuestPDF genera PDFs compactos. No se espera superar 200 KB por factura.
- **Acceso al PDF:** El container es privado. El frontend accederá vía la URL guardada.
  Si el container es privado, habrá que generar SAS tokens o hacer el container público
  (decisión de seguridad a tomar antes de Fase 1).
