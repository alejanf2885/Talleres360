# Máquina de Estados — Trabajo

**Archivo:** `Talleres360.Shared/Enums/TrabajoEstadoExtensions.cs`

---

## Diagrama de flujo

```
                                ┌──────────────────────────────────────────────────────┐
                                │                  CICLO PRESUPUESTO                   │
                                │                                                      │
                                │           /enviar                                    │
                                │  PRESUPUESTO ──────────► PRESUPUESTO_ENVIADO         │
                                │       │                        │                     │
                                │       │ /aceptar               │ /aceptar            │
                                │       │ (directo)              │ /rechazar            │
                                │       │                        │ /caducar             │
                                │       ▼                        ▼                     │
                                │     ABIERTO ◄─────────── ABIERTO                    │
                                │                           RECHAZADO ●                │
                                │                           CADUCADO  ●                │
                                └──────────────────────────────────────────────────────┘

                                ┌──────────────────────────────────────────────────────┐
                                │                   CICLO TRABAJO                      │
                                │                                                      │
                                │  ABIERTO ──► EN_PROCESO ──► CERRADO ──► FACTURADO ● │
                                │     │    ╲       │    ╲         │                   │
                                │     │     ╲      │     ╲        ▼                   │
                                │     │      ╲     │   PENDIENTE_PIEZAS               │
                                │     │       ╲    │       │                          │
                                │     └────────────┘───────┘                          │
                                │                                                      │
                                │  Todos los estados no terminales → CANCELADO ●      │
                                └──────────────────────────────────────────────────────┘

● = Estado terminal (sin transiciones salientes)
```

---

## Tabla de transiciones

| Estado actual | Transiciones permitidas | Acción que la dispara |
|---|---|---|
| `PRESUPUESTO` | `PRESUPUESTO_ENVIADO` | `POST /presupuestos/:id/enviar` |
| `PRESUPUESTO` | `ABIERTO` | `POST /presupuestos/:id/aceptar` (directo, sin envío previo) |
| `PRESUPUESTO` | `CANCELADO` | `DELETE /presupuestos/:id` |
| `PRESUPUESTO_ENVIADO` | `ABIERTO` | `POST /presupuestos/:id/aceptar` |
| `PRESUPUESTO_ENVIADO` | `RECHAZADO` | `POST /presupuestos/:id/rechazar` |
| `PRESUPUESTO_ENVIADO` | `CADUCADO` | Job automático cuando `ValidezHastaPresupuesto` expira |
| `PRESUPUESTO_ENVIADO` | `CANCELADO` | `DELETE /presupuestos/:id` |
| `ABIERTO` | `EN_PROCESO` | `PUT /trabajos/:id` con `Estado = EN_PROCESO` |
| `ABIERTO` | `PENDIENTE_PIEZAS` | `PUT /trabajos/:id` con `Estado = PENDIENTE_PIEZAS` |
| `ABIERTO` | `CANCELADO` | `DELETE /trabajos/:id` |
| `EN_PROCESO` | `PENDIENTE_PIEZAS` | `PUT /trabajos/:id` con `Estado = PENDIENTE_PIEZAS` |
| `EN_PROCESO` | `CERRADO` | `PUT /trabajos/:id` con `Estado = CERRADO` |
| `EN_PROCESO` | `CANCELADO` | `DELETE /trabajos/:id` |
| `PENDIENTE_PIEZAS` | `EN_PROCESO` | `PUT /trabajos/:id` con `Estado = EN_PROCESO` |
| `PENDIENTE_PIEZAS` | `CANCELADO` | `DELETE /trabajos/:id` |
| `CERRADO` | `FACTURADO` | `POST /trabajos/:id/facturar` |
| `CERRADO` | `CANCELADO` | `DELETE /trabajos/:id` |
| `FACTURADO` | *(ninguna)* | — |
| `RECHAZADO` | *(ninguna)* | — |
| `CADUCADO` | *(ninguna)* | — |
| `CANCELADO` | *(ninguna)* | — |

---

## Clasificación de estados

### Por ciclo de vida

| Grupo | Estados | Listado donde aparecen |
|---|---|---|
| Presupuesto | `PRESUPUESTO`, `PRESUPUESTO_ENVIADO`, `RECHAZADO`, `CADUCADO` | `GET /presupuestos` |
| Trabajo | `ABIERTO`, `EN_PROCESO`, `PENDIENTE_PIEZAS`, `CERRADO`, `FACTURADO` | `GET /trabajos` |

> `CANCELADO` no aparece en ninguno de los dos listados (soft-delete con `Eliminado = true`).

### Terminales

Estados que no permiten ninguna transición saliente:

| Estado | Motivo |
|---|---|
| `FACTURADO` | Documento fiscal generado, inmutable |
| `RECHAZADO` | Cliente rechazó el presupuesto |
| `CADUCADO` | Presupuesto expiró (`ValidezHastaPresupuesto`) |
| `CANCELADO` | Cancelado manualmente; se marca además `Eliminado = true` |

### `PermiteEdicion()`

Solo `PRESUPUESTO` y `PRESUPUESTO_ENVIADO` permiten editar campos del documento vía `PUT /presupuestos/:id`. Los trabajos en estado `ABIERTO` o posterior se actualizan vía `PUT /trabajos/:id`, que valida la transición de estado antes de aplicar cambios.

---

## Flujo completo — camino feliz

```
POST /presupuestos
        │
        │  Estado: PRESUPUESTO
        ▼
POST /presupuestos/:id/enviar
        │
        │  Estado: PRESUPUESTO_ENVIADO
        │  FechaEnvioPresupuesto = UtcNow
        │  ValidezHastaPresupuesto = UtcNow + 30 días
        ▼
POST /presupuestos/:id/aceptar
        │
        │  Estado: ABIERTO
        │  FechaAceptacionPresupuesto = UtcNow
        │  FirmaAceptacionUrl = (opcional)
        ▼
PUT /trabajos/:id   { Estado: "EN_PROCESO" }
        │
        ▼
PUT /trabajos/:id   { Estado: "CERRADO" }
        │
        │  FechaCierre = UtcNow
        ▼
POST /trabajos/:id/facturar
        │
        │  Estado: FACTURADO
        │  Genera snapshot en Facturas + LineasFactura + DesglosesIva
        ▼
        ●  Terminal
```

---

## Uso en código

### Validar una transición antes de ejecutarla

```csharp
if (!trabajo.Estado.PuedeTransicionarA(nuevoEstado))
{
    return ServiceResult<TrabajoDto>.Fail(
        ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
        $"No se puede cambiar el estado de {trabajo.Estado} a {nuevoEstado}.");
}
```

### Saber si un documento es presupuesto o trabajo

```csharp
if (trabajo.Estado.EsPresupuesto())
{
    // Solo opera PresupuestoService
}

if (trabajo.Estado.EsTrabajo())
{
    // Solo opera TrabajoService
}
```

### Bloquear edición en estados avanzados

```csharp
if (!trabajo.Estado.PermiteEdicion())
{
    return ServiceResult<TrabajoDto>.Fail(
        ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
        "Solo se pueden editar presupuestos en estado PRESUPUESTO o PRESUPUESTO_ENVIADO.");
}
```

### Consultar qué transiciones están disponibles

```csharp
TrabajoEstado[] siguientes = trabajo.Estado.ObtenerTransicionesPermitidas();
// Útil para construir botones de acción en el frontend
```

### Filtrar listados por ciclo

```csharp
// En TrabajoRepository — excluye presupuestos del listado de trabajos
query = query.Where(t => !TrabajoEstadoExtensions.EstadosPresupuesto.Contains(t.Estado));

// En TrabajoRepository — solo presupuestos
query = query.Where(t => TrabajoEstadoExtensions.EstadosPresupuesto.Contains(t.Estado));
```
