# Plan de Acción — Integración Verifactu (Fase 11)

**Rama:** `refactor/unified-document-model`
**Estado:** Pendiente de implementación
**Prioridad:** Alta — cumplimiento legal Veri*Factu (AEAT)

---

## Contexto

El sistema necesita enviar cada factura emitida a la AEAT a través del proveedor externo **Verifacti**,
que actúa como intermediario SaaS. La integración es multi-tenant: Verifacti gestiona el certificado
por NIF de taller, lo que significa que la implementación en nuestro lado es una simple llamada HTTP
por factura, sin gestión de certificados propia.

La arquitectura sigue el patrón **ports & adapters**: `IProveedorVerifactu` es el puerto;
`VerifactiAdapter` es el adaptador. Cambiar de proveedor en el futuro solo requiere sustituir
el adaptador.

---

## Diagnóstico: BD vs C#

### Base de datos — ya completo

- `Facturas` tiene las 11 columnas `Verifacti*`
- `EventosFacturacion` existe con todas sus columnas
- Índice `IX_Facturas_VerifactiEstado` listo para el job de reintentos
- Vistas `VW_HistoricoFacturacion` incluye `VerifactiEstado` y `VerifactiUrlValidacion`

### C# — pendiente

| Componente | Estado |
|---|---|
| `Factura.cs` — 11 columnas `Verifacti*` | Faltan |
| `Factura.cs` — campo `TallerCp` (BD: `TallerCp`, modelo: `TallerTelefono`) | Discrepancia |
| `ErrorCode.TRA_NO_FACTURABLE` | Falta en enum (ya se usa en servicio) |
| Modelo `EventoFacturacion` | No existe |
| `IEventoFacturacionRepository` + implementación | No existe |
| `IProveedorVerifactu` + `VerifactuRespuesta` | No existe |
| `VerifactiAdapter` (stub inicial) | No existe |
| `FacturacionService` — llamada Verifactu + evento | No integrado |
| Job de reintentos | Diferido |

---

## Pasos de implementación

### Paso 1 — Sincronizar `Factura.cs` con la BD

**Archivo:** `Talleres360.Shared/Models/Facturacion/Factura.cs`

Cambios:
- Eliminar `TallerTelefono` (no existe en BD)
- Añadir `TallerCp` (BD: `nvarchar(15)`)
- Añadir las 11 propiedades `Verifacti*`

```csharp
// Quitar:
[Column("TallerTelefono")] public string? TallerTelefono { get; set; }

// Añadir:
[Column("TallerCp")] [StringLength(15)] public string? TallerCp { get; set; }

[Column("VerifactiHash")]           [StringLength(128)]  public string? VerifactiHash { get; set; }
[Column("VerifactiQrBase64")]                             public string? VerifactiQrBase64 { get; set; }
[Column("VerifactiUrlValidacion")]  [StringLength(500)]  public string? VerifactiUrlValidacion { get; set; }
[Column("VerifactiCsvAeat")]        [StringLength(100)]  public string? VerifactiCsvAeat { get; set; }
[Column("VerifactiEstado")]         [StringLength(20)]   public string? VerifactiEstado { get; set; }
[Column("VerifactiFechaEnvio")]                          public DateTime? VerifactiFechaEnvio { get; set; }
[Column("VerifactiFechaRespuesta")]                      public DateTime? VerifactiFechaRespuesta { get; set; }
[Column("VerifactiIntentos")]                            public int VerifactiIntentos { get; set; } = 0;
[Column("VerifactiRespuestaRaw")]                        public string? VerifactiRespuestaRaw { get; set; }
[Column("VerifactiErrorMensaje")]   [StringLength(500)]  public string? VerifactiErrorMensaje { get; set; }
[Column("VerifactiModalidad")]      [StringLength(20)]   public string? VerifactiModalidad { get; set; }
```

Actualizar también `FacturacionService.cs`: cambiar `TallerTelefono` → `TallerCp` en la asignación.

---

### Paso 2 — Añadir `TRA_NO_FACTURABLE` al enum `ErrorCode`

**Archivo:** `Talleres360.Shared/Enums/Errors/ErrorCode.cs`

```csharp
// Bloque TRA — añadir al final:
TRA_TRANSICION_INVALIDA,
TRA_NO_FACTURABLE,   // ← nuevo
```

---

### Paso 3 — Crear el modelo `EventoFacturacion`

**Archivo nuevo:** `Talleres360.Shared/Models/Facturacion/EventoFacturacion.cs`

```csharp
[Table("EventosFacturacion")]
public class EventoFacturacion
{
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    [Column("TallerId")]
    public int TallerId { get; set; }

    [Column("FacturaId")]
    public int? FacturaId { get; set; }

    [Column("UsuarioId")]
    public int? UsuarioId { get; set; }

    [Column("TipoEvento")]
    [StringLength(50)]
    public string TipoEvento { get; set; } = string.Empty;

    [Column("Descripcion")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("DetalleJson")]
    public string? DetalleJson { get; set; }

    [Column("FechaEvento")]
    public DateTime FechaEvento { get; set; }

    [Column("IpCliente")]
    [StringLength(50)]
    public string? IpCliente { get; set; }
}
```

Registrar en `ApplicationDbContext`:
```csharp
public DbSet<EventoFacturacion> EventosFacturacion { get; set; }
```

---

### Paso 4 — `IProveedorVerifactu` + DTO de respuesta

**Archivo nuevo:** `Talleres360/Interfaces/Facturacion/IProveedorVerifactu.cs`

```csharp
public interface IProveedorVerifactu
{
    Task<VerifactuRespuesta> EnviarFacturaAsync(Factura factura, string nifEmisor);
}

public class VerifactuRespuesta
{
    public bool Exito { get; set; }
    public string? Hash { get; set; }
    public string? QrBase64 { get; set; }
    public string? UrlValidacion { get; set; }
    public string? CsvAeat { get; set; }
    public string? Modalidad { get; set; }
    public string? RespuestaRaw { get; set; }
    public string? ErrorMensaje { get; set; }
}
```

---

### Paso 5 — `IEventoFacturacionRepository` + implementación

**Archivo nuevo:** `Talleres360/Interfaces/Facturacion/IEventoFacturacionRepository.cs`

```csharp
public interface IEventoFacturacionRepository
{
    Task RegistrarAsync(EventoFacturacion evento);
}
```

**Archivo nuevo:** `Talleres360/Repositories/Facturas/EventoFacturacionRepository.cs`

```csharp
public class EventoFacturacionRepository : IEventoFacturacionRepository
{
    private readonly ApplicationDbContext _context;

    public EventoFacturacionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(EventoFacturacion evento)
    {
        await _context.EventosFacturacion.AddAsync(evento);
        await _context.SaveChangesAsync();
    }
}
```

Registrar en `Program.cs`:
```csharp
builder.Services.AddScoped<IEventoFacturacionRepository, EventoFacturacionRepository>();
```

---

### Paso 6 — `VerifactiAdapter` (stub inicial)

**Archivo nuevo:** `Talleres360/Services/Facturacion/VerifactiAdapter.cs`

```csharp
public class VerifactiAdapter : IProveedorVerifactu
{
    // TODO: integración real con API Verifacti cuando se tengan las credenciales
    public Task<VerifactuRespuesta> EnviarFacturaAsync(Factura factura, string nifEmisor)
    {
        return Task.FromResult(new VerifactuRespuesta
        {
            Exito      = true,
            Modalidad  = "SIMULADO",
            Hash       = Guid.NewGuid().ToString("N"),
            RespuestaRaw = "{\"simulado\": true}"
        });
    }
}
```

Registrar en `Program.cs`:
```csharp
builder.Services.AddScoped<IProveedorVerifactu, VerifactiAdapter>();
```

---

### Paso 7 — Actualizar `FacturacionService`

Inyectar `IProveedorVerifactu` y `IEventoFacturacionRepository`.

Flujo de `FacturarTrabajoAsync` tras `GuardarSnapshotAsync`:

```
1. GuardarSnapshotAsync(factura, lineas, desgloses)   ← ya existe

2. Marcar PENDIENTE antes de llamar (garantiza retry si la API cae):
   factura.VerifactiEstado = "PENDIENTE"
   factura.VerifactiIntentos = 0
   await _facturaRepository.UpdateAsync(factura)

3. Llamar al proveedor:
   VerifactuRespuesta resp = await _verifactu.EnviarFacturaAsync(factura, taller.Cif)

4. Actualizar campos según respuesta:
   if (resp.Exito)  → VerifactiEstado = "ENVIADO", VerifactiHash, QrBase64, UrlValidacion, CsvAeat, Modalidad
   else             → VerifactiEstado = "ERROR", VerifactiErrorMensaje = resp.ErrorMensaje
   VerifactiIntentos = 1
   VerifactiFechaEnvio = UtcNow
   VerifactiFechaRespuesta = UtcNow
   VerifactiRespuestaRaw = resp.RespuestaRaw
   await _facturaRepository.UpdateAsync(factura)

5. Registrar evento de auditoría:
   await _eventoRepo.RegistrarAsync(new EventoFacturacion {
       TallerId    = tallerId,
       FacturaId   = factura.Id,
       TipoEvento  = resp.Exito ? "VERIFACTU_ENVIADO" : "VERIFACTU_ERROR",
       Descripcion = resp.Exito ? $"Factura {factura.NumeroFactura} enviada a Verifactu"
                                 : $"Error al enviar factura {factura.NumeroFactura}: {resp.ErrorMensaje}",
       FechaEvento = DateTime.UtcNow
   })

6. trabajo.Estado = FACTURADO  ← igual que antes
   await _trabajoRepository.UpdateAsync(trabajo)
```

---

### Paso 8 — Job de reintentos (diferido)

Implementar como `IHostedService` con timer periódico (cada 15 min).

Consulta base:
```csharp
List<Factura> pendientes = await _facturaRepository.ObtenerParaReintentarAsync(maxIntentos: 3);
// WHERE VerifactiEstado IN ('PENDIENTE', 'ERROR') AND VerifactiIntentos < 3
```

Por cada factura: llamar a `IProveedorVerifactu`, actualizar campos, registrar evento.
Después de 3 fallos el estado queda en `ERROR` y requiere intervención manual.

---

## Orden de ejecución recomendado

| # | Paso | Tiempo estimado |
|---|---|---|
| 1 | `ErrorCode.TRA_NO_FACTURABLE` | 2 min |
| 2 | `Factura.cs` — añadir columnas Verifacti + fix `TallerCp` | 10 min |
| 3 | Modelo `EventoFacturacion` + DbSet | 5 min |
| 4 | `IProveedorVerifactu` + `VerifactuRespuesta` | 10 min |
| 5 | `IEventoFacturacionRepository` + repositorio | 10 min |
| 6 | `VerifactiAdapter` stub | 5 min |
| 7 | `FacturacionService` integrado | 20 min |
| 8 | Job de reintentos | diferido |

Total estimado (pasos 1–7): ~60 min
