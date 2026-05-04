# Plan: Refactor Modelo Unificado Presupuesto/Trabajo/Factura

**Fecha:** 21-04-2026  
**Rama:** `refactor/unified-document-model`  
**Origen:** `CAMBIOS_BD_REFACTOR.md`  
**Estimación:** 3-5 días

---

## Objetivo

Adaptar el backend a los cambios de BD del hardening del 20-21 de abril:

1. **Modelo unificado**: los presupuestos dejan de vivir en `Facturas` y pasan a `Trabajos` con `Estado = 'PRESUPUESTO'`.
2. **TallerId en tablas hijas**: `DetallesTrabajo`, `LineasFactura`, `DesglosesIva`, `NotasVehiculo`, `CobrosTrabajo` ahora exigen `TallerId` en el INSERT.
3. **Nueva máquina de estados** de `Trabajos` con 10 estados (incluye `PRESUPUESTO`, `PRESUPUESTO_ENVIADO`, `RECHAZADO`, `CADUCADO`, `FACTURADO`).
4. **Facturación como snapshot** inmutable: `POST /trabajos/:id/facturar` genera copia en `Facturas`+`LineasFactura`+`DesglosesIva`.
5. **Nuevos campos** en `Trabajos` y `Citas`.

---

## Fases

### FASE 1 — Modelos y Enums (Talleres360.Shared)

**Archivos afectados:**
- `Talleres360.Shared/Enums/TrabajoEstado.cs`
- `Talleres360.Shared/Enums/TipoDocumentoComercial.cs`
- `Talleres360.Shared/Models/Operaciones/Trabajo.cs`
- `Talleres360.Shared/Models/Operaciones/DetalleTrabajo.cs`
- `Talleres360.Shared/Models/Operaciones/CobroTrabajo.cs`
- `Talleres360.Shared/Models/Operaciones/Cita.cs`
- `Talleres360.Shared/Models/Facturacion/LineaFactura.cs`
- `Talleres360.Shared/Models/Facturacion/DesgloseIva.cs`

**Cambios:**

#### TrabajoEstado enum — añadir estados nuevos
```
PRESUPUESTO
PRESUPUESTO_ENVIADO
RECHAZADO      (terminal)
CADUCADO       (terminal)
ABIERTO        (ya existe)
EN_PROCESO     (ya existe)
PENDIENTE_PIEZAS (ya existe)
CERRADO        (ya existe)
FACTURADO      (nuevo, terminal)
CANCELADO      (ya existe, terminal)
```

#### TipoDocumentoComercial enum — eliminar PRESUPUESTO
Los valores válidos pasan a ser: `FACTURA`, `FACTURA_RECTIFICATIVA`, `ALBARAN`.

#### Trabajo — campos nuevos
```csharp
public int? CitaId { get; set; }
public DateTime? FechaEnvioPresupuesto { get; set; }
public DateTime? FechaAceptacionPresupuesto { get; set; }
public DateTime? ValidezHastaPresupuesto { get; set; }
public string? FirmaAceptacionUrl { get; set; }   // nvarchar(500)
public string? MotivoRechazo { get; set; }         // nvarchar(500)
public DateTime? FechaRechazo { get; set; }
```
También: `TallerId` pasar de `int?` a `int` (NOT NULL en BD).

#### DetalleTrabajo — añadir TallerId
```csharp
[Column("TallerId")]
public int TallerId { get; set; }
```

#### CobroTrabajo — ya tiene TallerId ✓ (no requiere cambio)

#### Cita — añadir TrabajoId
```csharp
[Column("TrabajoId")]
public int? TrabajoId { get; set; }
```

#### LineaFactura — añadir TallerId
```csharp
[Column("TallerId")]
public int TallerId { get; set; }
```

#### DesgloseIva — añadir TallerId
```csharp
[Column("TallerId")]
public int TallerId { get; set; }
```

---

### FASE 2 — Migración EF Core

Generar y revisar la migración:
```bash
dotnet ef migrations add UnifiedDocumentModel --project Talleres360 --startup-project Talleres360
dotnet ef database update
```

**Puntos a revisar en la migración:**
- Que `TipoDocumentoComercial.PRESUPUESTO` no rompa registros existentes antes de aplicar (limpiar datos de prueba si los hay).
- Que los nuevos campos nullable no afecten filas existentes.

---

### FASE 3 — Repositorios

#### 3.1 TrabajoRepository
- Nuevo método: `ObtenerPresupuestosPagedAsync(int tallerId, PaginationParams paginacion)` — filtra `Estado IN (PRESUPUESTO, PRESUPUESTO_ENVIADO, RECHAZADO, CADUCADO)`.
- Actualizar `ObtenerTodosPagedAsync` para excluir estados de presupuesto: `Estado IN (ABIERTO, EN_PROCESO, PENDIENTE_PIEZAS, CERRADO, FACTURADO)`.
- Nuevo método: `GenerarNumeroDocumentoTrabajoAsync(int tallerId)` — secuencia propia (NO usa `sp_SiguienteNumeroDocumento`). Implementación inicial: `$"PRES-{DateTime.UtcNow.Year}-{nuevoId:D6}"` o secuencia en BD si se crea.

#### 3.2 DetalleTrabajoRepository
- En `AddAsync` y cualquier INSERT: incluir `TallerId` del trabajo padre.

#### 3.3 CobroTrabajoRepository
- Ya incluye `TallerId` ✓. Verificar que los SELECTs también filtran por él.

#### 3.4 NotaVehiculoRepository
- Verificar INSERT incluye `TallerId`.

#### 3.5 PresupuestoRepository — ELIMINAR
- Toda la lógica migra a `TrabajoRepository`. El repositorio queda vacío y se elimina.

#### 3.6 DocumentoComercialService / FacturaRepository (para snapshot)
- El método de facturación necesita INSERT en `LineasFactura` y `DesglosesIva` con `TallerId`.

---

### FASE 4 — Servicios

#### 4.1 PresupuestoService → TrabajoService (unificación)
Migrar toda la lógica de `PresupuestoService` a `TrabajoService`:

| Método antiguo (PresupuestoService) | Método nuevo (TrabajoService) |
|---|---|
| `CrearAsync` | `CrearPresupuestoAsync` — crea `Trabajo` con `Estado = PRESUPUESTO` |
| `ObtenerTodosAsync` | `ObtenerPresupuestosAsync` — filtra por estados de presupuesto |
| `ObtenerPorIdAsync` | `ObtenerPresupuestoPorIdAsync` |
| `ActualizarAsync` | `ActualizarPresupuestoAsync` — solo si estado es PRESUPUESTO o PRESUPUESTO_ENVIADO |
| `EliminarAsync` | Soft-delete → `Estado = CANCELADO` |

Nuevos métodos en `TrabajoService`:
- `EnviarPresupuestoAsync(int tallerId, int trabajoId)` — PRESUPUESTO → PRESUPUESTO_ENVIADO, setea `FechaEnvioPresupuesto` y `ValidezHastaPresupuesto = UtcNow + 30d`.
- `AceptarPresupuestoAsync(int tallerId, int trabajoId, AceptarPresupuestoRequest request)` — PRESUPUESTO_ENVIADO → ABIERTO, setea `FechaAceptacionPresupuesto`.
- `RechazarPresupuestoAsync(int tallerId, int trabajoId, RechazarPresupuestoRequest request)` — PRESUPUESTO_ENVIADO → RECHAZADO.
- `FacturarTrabajoAsync(int tallerId, int trabajoId)` — CERRADO → FACTURADO + genera snapshot en `Facturas`.

#### 4.2 Máquina de estados (TrabajoService)
Centralizar en un método `ValidarTransicion(TrabajoEstado estadoActual, TrabajoEstado estadoNuevo)`:

```
PRESUPUESTO       → PRESUPUESTO_ENVIADO, ABIERTO, CANCELADO
PRESUPUESTO_ENVIADO → ABIERTO, RECHAZADO, CADUCADO, CANCELADO
ABIERTO           → EN_PROCESO, PENDIENTE_PIEZAS, CANCELADO
EN_PROCESO        → PENDIENTE_PIEZAS, CERRADO, CANCELADO
PENDIENTE_PIEZAS  → EN_PROCESO, CANCELADO
CERRADO           → FACTURADO, CANCELADO
FACTURADO         → (ninguna)
RECHAZADO         → (ninguna)
CADUCADO          → (ninguna)
CANCELADO         → (ninguna)
```

#### 4.3 DocumentoComercialService — FacturarTrabajoAsync
Implementar en una única transacción:
1. Validar `Trabajo.Estado == CERRADO`.
2. Obtener número fiscal con `sp_SiguienteNumeroDocumento` (tipo `FACTURA`).
3. Denormalizar datos del cliente y del taller.
4. INSERT `Factura`.
5. Por cada `DetalleTrabajo` no eliminado: INSERT `LineaFactura` (con `TallerId`).
6. Calcular y INSERT `DesglosesIva` agrupando por `ImpuestoPorcentaje` (con `TallerId`).
7. UPDATE `Trabajo.Estado = FACTURADO`.

#### 4.4 PresupuestoService — ELIMINAR
Tras migrar todo a `TrabajoService`.

---

### FASE 5 — Controllers y Endpoints

#### 5.1 PresupuestosController — refactorizar
Todos los endpoints pasan a trabajar con `TrabajoService`:

| Endpoint | Cambio |
|---|---|
| `POST /api/v1/presupuestos` | Llama `TrabajoService.CrearPresupuestoAsync` |
| `GET /api/v1/presupuestos` | Llama `TrabajoService.ObtenerPresupuestosAsync` |
| `GET /api/v1/presupuestos/:id` | Llama `TrabajoService.ObtenerPresupuestoPorIdAsync` |
| `PUT /api/v1/presupuestos/:id` | Llama `TrabajoService.ActualizarPresupuestoAsync` |
| `DELETE /api/v1/presupuestos/:id` | Soft-delete via `TrabajoService` |
| `POST /api/v1/presupuestos/:id/enviar` | **NUEVO** → `EnviarPresupuestoAsync` |
| `POST /api/v1/presupuestos/:id/aceptar` | **NUEVO** → `AceptarPresupuestoAsync` |
| `POST /api/v1/presupuestos/:id/rechazar` | **NUEVO** → `RechazarPresupuestoAsync` |

#### 5.2 TrabajosController — actualizar
- `GET /api/v1/trabajos` — excluir estados de presupuesto del listado.
- `POST /api/v1/trabajos/:id/cerrar` — UPDATE `Estado = CERRADO`, `FechaCierre = UtcNow`.
- `POST /api/v1/trabajos/:id/facturar` — **NUEVO** → `TrabajoService.FacturarTrabajoAsync`.

#### 5.3 CitasController — nuevo endpoint
- `POST /api/v1/citas/:id/generar-trabajo` — crea `Trabajo` (Estado=ABIERTO) y enlaza `Cita.TrabajoId` + `Trabajo.CitaId`.

---

### FASE 6 — DTOs

#### Nuevos DTOs en `Dtos/Presupuestos/`
- `CrearPresupuestoRequest` — sin `ClienteId` de factura, ahora incluye `VehiculoId`, campos de presupuesto.
- `PresupuestoDto` — refleja los campos del modelo `Trabajo` en estado de presupuesto.
- `EnviarPresupuestoRequest` — `ValidezHasta` (opcional, default 30 días).
- `AceptarPresupuestoRequest` — `FirmaAceptacionUrl` (opcional).
- `RechazarPresupuestoRequest` — `MotivoRechazo` (requerido).

#### Actualizar DTOs existentes
- `TrabajoDto` — añadir `CitaId`, `NumeroDocumento`.
- `CrearTrabajoRequest` — añadir `CitaId` (opcional).
- `DetalleTrabajoDto` / `CrearDetalleTrabajoRequest` — `TallerId` se propaga internamente, no se expone en request.

---

### FASE 7 — Limpieza y Program.cs

- Eliminar registro de `IPresupuestoRepository` / `PresupuestoRepository` en `Program.cs`.
- Eliminar registro de `IPresupuestoService` / `PresupuestoService` en `Program.cs`.
- Verificar que `ITrabajoService` y `ITrabajoRepository` cubren las nuevas interfaces necesarias.

---

### FASE 8 — Tests

Actualizar/crear tests en `Talleres360.Test`:
- `TrabajoRepositoryTests` — nuevos métodos de presupuesto.
- `TrabajoServiceTests` — máquina de estados (todas las transiciones válidas e inválidas).
- `PresupuestoRepositoryTests` — eliminar o vaciar.
- Tests de snapshot de factura: verificar que `LineasFactura` y `DesglosesIva` se crean correctamente.

---

## Orden de ejecución recomendado

```
FASE 1 (modelos/enums)
  ↓
FASE 2 (migración EF)
  ↓
FASE 3 (repositorios)
  ↓
FASE 4 (servicios)
  ↓
FASE 5 (controllers)
  ↓
FASE 6 (DTOs)  ← puede hacerse en paralelo con FASE 3-4
  ↓
FASE 7 (limpieza Program.cs)
  ↓
FASE 8 (tests)
```

---

## Riesgos y decisiones pendientes

| Riesgo | Decisión |
|---|---|
| `NumeroDocumento` de Trabajos — no usa `sp_SiguienteNumeroDocumento` | Implementar secuencia propia en BD o generar en app. **Pendiente decisión.** |
| Datos existentes en `Facturas` con `TipoDocumento = PRESUPUESTO` | Migrar a `Trabajos` con script SQL antes de quitar el enum. Solo afecta datos de prueba en dev. |
| RLS / `SESSION_CONTEXT` | No se implementa en este refactor. Queda pendiente para cuando la Security Policy se active. |
| Veri*Factu | Fuera de scope. Pendiente decisión estratégica. |

---

## Checklist de "Definition of Done"

- [ ] Compila sin errores ni warnings relevantes.
- [ ] `dotnet ef database update` aplica sin errores.
- [ ] `POST /presupuestos` crea un `Trabajo` con `Estado = PRESUPUESTO`.
- [ ] `GET /presupuestos` no devuelve trabajos en estado `ABIERTO`/`EN_PROCESO`.
- [ ] `GET /trabajos` no devuelve presupuestos.
- [ ] Flujo completo: PRESUPUESTO → PRESUPUESTO_ENVIADO → ABIERTO → EN_PROCESO → CERRADO → FACTURADO.
- [ ] La factura generada tiene snapshot de datos del cliente y del taller.
- [ ] `DetallesTrabajo`, `LineasFactura`, `DesglosesIva` incluyen `TallerId` en el INSERT.
- [ ] Tests pasan en verde.
