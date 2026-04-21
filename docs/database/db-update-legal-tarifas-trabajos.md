# Plan: Actualización BD — Facturas Legales, TarifasHora, Trabajos Operativos

**Fecha:** 2026-04-20  
**Estado:** PENDIENTE  
**Scope:** Backend (.NET 10 API). Frontend MVC referenciado como notas (proyecto separado).

---

## Resumen de cambios SQL aplicados

| # | Objeto | Tipo cambio | Descripción |
|---|--------|-------------|-------------|
| 1 | `Facturas` | ALTER TABLE | +8 columnas: emisor snapshot, serie, cliente email/tel |
| 2 | `DesglosesIva` | CREATE TABLE | Desglose IVA por tramo por factura |
| 3 | `TarifasHora` | CREATE TABLE | Historial de tarifas hora mano de obra por taller |
| 4 | `DetallesTrabajo` | ALTER TABLE | +3 columnas: FK tarifa, precio aplicado, horas |
| 5 | `Trabajos` | ALTER TABLE | +6 columnas operativas: cierre, entrega, km, fotos, obs |
| 6 | `AuditLog` | ALTER TABLE | RegistroId → BIGINT, + ValoresAnteriores/Nuevos |
| 7 | `TarifasHora` | RLS | Política seguridad fn_FiltroTaller |
| 8 | `sp_SiguienteNumeroDocumento` | ALTER SP | Acepta @Serie, formato SERIE-YYYY-000001 |

---

## BLOQUE A — Facturas: datos emisor + serie + cliente

### A1. Modificar `Models/Factura.cs`
Añadir propiedades:
```csharp
[Column("TallerNombre")][StringLength(150)] public string TallerNombre { get; set; } = string.Empty;
[Column("TallerCif")][StringLength(20)]     public string TallerCif { get; set; } = string.Empty;
[Column("TallerDireccion")][StringLength(500)] public string? TallerDireccion { get; set; }
[Column("TallerLocalidad")][StringLength(150)] public string? TallerLocalidad { get; set; }
[Column("TallerCp")][StringLength(15)]      public string? TallerCp { get; set; }
[Column("Serie")][StringLength(10)]         public string Serie { get; set; } = "A";
[Column("ClienteEmail")][StringLength(150)] public string? ClienteEmail { get; set; }
[Column("ClienteTelefono")][StringLength(20)] public string? ClienteTelefono { get; set; }
```

### A2. Crear `Models/DesglosesIva.cs`
Nueva entidad con: Id, FacturaId, TipoIvaPorcentaje, BaseImponible, CuotaIva.  
FK cascade hacia Facturas.

### A3. Modificar `Data/ApplicationDbContext.cs`
- Añadir `DbSet<DesgloseIva> DesglosesIva`
- Añadir precisiones decimales para DesglosesIva (DECIMAL 18,2 → `HasPrecision(18,2)`)
- Añadir índice único `[FacturaId, TipoIvaPorcentaje]`

### A4. Crear `Dtos/Facturas/` (nueva carpeta)
- `FacturaDto.cs` — respuesta completa con TallerNombre, TallerCif, Serie, lista DesgloseIvaDto
- `DesgloseIvaDto.cs` — TipoIvaPorcentaje, BaseImponible, CuotaIva
- `CrearFacturaRequest.cs` — Serie opcional (default "A"), ClienteId, TrabajoId, Lineas

### A5. Modificar servicio de facturas (crear si no existe)
- Al crear factura: cargar snapshot taller (Nombre, Cif, Direccion, Localidad) desde `_tallerRepo`
- Al crear factura: cargar snapshot cliente (Email, Telefono) desde `_customerRepo`
- Calcular DesglosesIva agrupando líneas por ImpuestoPorcentaje:
  ```
  BaseImponible = SUM(SubtotalLinea) where ImpuestoPorcentaje == X
  CuotaIva = BaseImponible * X / 100
  ```
- Llamar `sp_SiguienteNumeroDocumento` con parámetro `@Serie`

### A6. Modificar `sp_SiguienteNumeroDocumento` wrapper
El SP ya fue modificado en BD. Actualizar la llamada en código para pasar `@Serie` y recibir formato `SERIE-YYYY-000001`.

---

## BLOQUE B — TarifasHora: modelo, CRUD y auto-relleno

### B1. Crear `Models/TarifaHora.cs`
```csharp
[Table("TarifasHora")]
public class TarifaHora {
    public int Id { get; set; }
    public int TallerId { get; set; }
    public decimal PrecioHora { get; set; }
    public string? Descripcion { get; set; }
    public DateOnly FechaVigencia { get; set; }
    public bool Activa { get; set; } = true;
    public int? CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; }
}
```

### B2. Añadir a `ApplicationDbContext`
- `DbSet<TarifaHora> TarifasHora`
- HasPrecision(10,2) para PrecioHora
- QueryFilter: NO — tarifas inactivas deben ser visibles en historial
- Índice único filtrado: `WHERE Activa = 1` por TallerId (ya existe en BD, solo documentar)

### B3. Crear `Interfaces/Trabajos/ITarifaHoraRepository.cs`
Métodos:
- `Task<TarifaHora?> ObtenerActivaAsync(int tallerId)`
- `Task<IEnumerable<TarifaHora>> ObtenerHistorialAsync(int tallerId)`
- `Task<TarifaHora?> GetByIdAsync(int id, int tallerId)`
- `Task AddAsync(TarifaHora tarifa)`
- `Task<bool> PerteneceATallerAsync(int id, int tallerId)`

### B4. Crear `Repositories/Trabajos/TarifaHoraRepository.cs`
- `ObtenerActivaAsync`: `WHERE TallerId = X AND Activa = 1`
- `AddAsync`: antes de insertar, desactivar tarifa activa anterior en misma transacción

### B5. Crear `Interfaces/Trabajos/ITarifaHoraService.cs`
Métodos:
- `Task<ServiceResult<TarifaHoraDto>> CrearAsync(int tallerId, int usuarioId, CrearTarifaHoraRequest request)`
- `Task<ServiceResult<IEnumerable<TarifaHoraDto>>> ObtenerHistorialAsync(int tallerId)`
- `Task<ServiceResult<TarifaHoraDto?>> ObtenerActivaAsync(int tallerId)`

### B6. Crear `Services/Trabajos/TarifaHoraService.cs`
- Al crear: desactivar anterior + insertar nueva (usar `IUnitOfWork`)
- Validar `PrecioHora >= 0`

### B7. Crear `Dtos/Trabajos/TarifaHoraDto.cs` y `CrearTarifaHoraRequest.cs`
Request:
```csharp
[Required][Range(0, double.MaxValue, ErrorMessage = "El precio debe ser >= 0")]
public decimal PrecioHora { get; set; }
[StringLength(150)] public string? Descripcion { get; set; }
public DateOnly? FechaVigencia { get; set; }  // default hoy
```

### B8. Crear `Controllers/TarifasHoraController.cs`
Ruta base: `api/v1/tarifas-hora`
- `GET /` — historial del taller
- `GET /activa` — tarifa activa actual
- `POST /` — crear nueva (desactiva anterior)

---

## BLOQUE C — DetallesTrabajo: tarifa hora + horas aplicadas

### C1. Modificar `Models/DetalleTrabajo.cs`
Añadir:
```csharp
[Column("TarifaHoraId")]       public int? TarifaHoraId { get; set; }
[Column("PrecioHoraAplicado")] public decimal? PrecioHoraAplicado { get; set; }
[Column("HorasAplicadas")]     public decimal? HorasAplicadas { get; set; }
```

### C2. Modificar `ApplicationDbContext`
Precisiones: PrecioHoraAplicado `HasPrecision(10,2)`, HorasAplicadas `HasPrecision(6,2)`

### C3. Modificar DTOs de DetallesTrabajo
- Añadir `TarifaHoraId?`, `PrecioHoraAplicado?`, `HorasAplicadas?` a request y response

### C4. Modificar servicio DetalleTrabajo (o TrabajoService)
Lógica al añadir línea `EsManoObra = true`:
1. Si no viene `PrecioHoraAplicado`: cargar tarifa activa del taller y asignar automáticamente
2. Guardar `TarifaHoraId` de la tarifa usada como snapshot de auditoría
3. `PrecioUnitario = PrecioHoraAplicado` (precio facturado)
4. `Cantidad = HorasAplicadas`

---

## BLOQUE D — Trabajos: campos operativos

### D1. Modificar `Models/Trabajo.cs`
Añadir:
```csharp
[Column("FechaCierre")]          public DateTime? FechaCierre { get; set; }
[Column("FechaEntregaEstimada")] public DateTime? FechaEntregaEstimada { get; set; }
[Column("KmSalida")]             public int? KmSalida { get; set; }
[Column("FotoEntradaUrl")][StringLength(500)] public string? FotoEntradaUrl { get; set; }
[Column("FotoSalidaUrl")][StringLength(500)]  public string? FotoSalidaUrl { get; set; }
[Column("ObservacionesEntrega")][StringLength(1000)] public string? ObservacionesEntrega { get; set; }
```

### D2. Modificar `Dtos/Trabajos/TrabajoDto.cs`
Exponer todos los campos nuevos.

### D3. Modificar `Dtos/Trabajos/ActualizarTrabajoRequest.cs`
Añadir campos editables: FechaEntregaEstimada, KmSalida, ObservacionesEntrega.  
`FechaCierre` se auto-asigna cuando `Estado → CERRADO`, no manual.  
`FotoEntradaUrl`/`FotoSalidaUrl` se gestionan via endpoint de imagen separado.

### D4. Modificar `Services/Trabajos/TrabajoService.cs`
- Al cambiar `Estado → CERRADO`: asignar `FechaCierre = DateTime.UtcNow` automáticamente
- Validar `KmSalida >= KmEntrada` si ambos presentes

---

## BLOQUE E — AuditLog: BIGINT + valores antes/después

### E1. Buscar modelo AuditLog (si existe en código)
Si hay `Models/AuditLog.cs`: cambiar `RegistroId` de `int` a `long` (BIGINT).  
Añadir `string? ValoresAnteriores` y `string? ValoresNuevos`.

### E2. Actualizar cualquier servicio que escriba en AuditLog
Pasar JSON de valores anteriores/nuevos en operaciones UPDATE y DELETE.

---

## Archivos a crear/modificar — resumen rápido

### CREAR
| Archivo | Bloque |
|---------|--------|
| `Models/DesgloseIva.cs` | A2 |
| `Models/TarifaHora.cs` | B1 |
| `Dtos/Facturas/FacturaDto.cs` | A4 |
| `Dtos/Facturas/DesgloseIvaDto.cs` | A4 |
| `Dtos/Facturas/CrearFacturaRequest.cs` | A4 |
| `Dtos/Trabajos/TarifaHoraDto.cs` | B7 |
| `Dtos/Trabajos/CrearTarifaHoraRequest.cs` | B7 |
| `Interfaces/Trabajos/ITarifaHoraRepository.cs` | B3 |
| `Interfaces/Trabajos/ITarifaHoraService.cs` | B5 |
| `Repositories/Trabajos/TarifaHoraRepository.cs` | B4 |
| `Services/Trabajos/TarifaHoraService.cs` | B6 |
| `Controllers/TarifasHoraController.cs` | B8 |

### MODIFICAR
| Archivo | Bloque | Cambio |
|---------|--------|--------|
| `Models/Factura.cs` | A1 | +8 propiedades |
| `Models/DetalleTrabajo.cs` | C1 | +3 propiedades |
| `Models/Trabajo.cs` | D1 | +6 propiedades |
| `Data/ApplicationDbContext.cs` | A3, B2, C2 | DbSets + precisiones + índices |
| `Dtos/Trabajos/TrabajoDto.cs` | D2 | +6 campos |
| `Dtos/Trabajos/ActualizarTrabajoRequest.cs` | D3 | +3 campos editables |
| `Services/Trabajos/TrabajoService.cs` | D4 | FechaCierre auto, validar KmSalida |
| Servicio facturas | A5 | Snapshot taller+cliente, DesglosesIva, @Serie |
| `Program.cs` | B | Registrar ITarifaHoraRepository e ITarifaHoraService |

---

## Notas Frontend (proyecto MVC futuro)

Cuando exista proyecto MVC separado, necesitará:

- **Facturas:** mostrar bloque emisor (logo + datos taller), tabla desglose IVA por tramos, campo serie seleccionable
- **TarifasHora:** página configuración taller → sección "Tarifa hora mano de obra", histórico, indicador "tarifa activa"
- **DetalleTrabajo:** al marcar línea como mano de obra → mostrar campo horas, auto-rellenar precio desde API, editable
- **Trabajos:** campos FechaEntregaEstimada (datepicker), KmSalida, ObservacionesEntrega en formulario edición; FechaCierre solo lectura

---

## Orden de implementación recomendado

```
E (AuditLog)  →  D (Trabajos campos)  →  B (TarifasHora)  →  C (DetallesTrabajo tarifa)  →  A (Facturas)
```

Razón: dependencias en cadena. TarifaHora debe existir antes de referenciarla en DetallesTrabajo. Facturas van al final porque requieren snapshot de taller+cliente que ya existe, y desglose IVA que depende de líneas.

---

## Tests requeridos (Paso 3)

| Test | Tipo | Qué verifica |
|------|------|--------------|
| `TarifaHoraRepositoryTests` | Integración | ObtenerActiva, historial, desactivación anterior |
| `TarifaHoraServiceTests` | Unitario | Crear desactiva anterior, validación precio |
| `TrabajoServiceTests` | Unitario | FechaCierre auto al cerrar, validar KmSalida |
| `FacturaServiceTests` | Unitario | Snapshot emisor, cálculo DesglosesIva agrupado |
| `DetalleTrabajoServiceTests` | Unitario | Auto-relleno tarifa al marcar EsManoObra |
