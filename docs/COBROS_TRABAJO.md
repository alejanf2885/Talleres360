# CobrosTrabajo - API Documentation

## Overview

The **CobrosTrabajo** (Work Payment Receipts) module enables tracking of partial and full payments against work orders (Trabajos). This feature allows workshops to register multiple payment transactions per job and automatically calculates the payment status of the parent work order.

## Features

- **Partial Payment Tracking**: Record multiple payment transactions per work order
- **Automatic State Recalculation**: Work order payment status (`EstadoPago`) is automatically recalculated based on total payments
- **Payment Methods**: Support for multiple payment methods (Cash, Card, Transfer, Bizum, Other)
- **Soft Delete**: Payments can be logically deleted, and parent work order status is automatically recalculated
- **Transactional Consistency**: Payment creation and deletion operations maintain database consistency through transactions

## Payment States

Work orders have a payment state (`EstadoPago`) that is automatically calculated:

| State | Condition | Description |
|-------|-----------|-------------|
| `PENDIENTE` | Total paid = 0 | No payments received |
| `PARCIAL` | 0 < Total paid < Work total | Partial payment received |
| `PAGADO` | Total paid ≥ Work total | Full payment received |

## API Endpoints

### Base Route
```
api/v1/trabajos/{trabajoId:int:min(1)}/cobros
```

### 1. List Payments for Work Order

**Endpoint:** `GET /api/v1/trabajos/{trabajoId}/cobros`

**Authentication:** Required (Bearer JWT)

**Authorization:** `[TallerAuthorize<ITrabajoRepository>]` — Validates that the work order belongs to the authenticated workshop.

**Query Parameters:**
- `pageNumber` (int, default: 1) — Page number for pagination
- `pageSize` (int, default: 10) — Number of items per page

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Listado de cobros recuperado correctamente.",
  "data": {
    "data": [
      {
        "id": 1,
        "trabajoId": 5,
        "importe": 150.50,
        "metodoPago": "TARJETA",
        "referencia": "TXN-123456",
        "notas": "Pago con tarjeta de crédito",
        "fechaCobro": "2026-04-13T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "timestamp": "2026-04-13T14:22:15Z"
}
```

**Error Responses:**
- `401 Unauthorized` — Missing authentication or invalid token
- `403 Forbidden` — Work order does not belong to user's workshop

---

### 2. Create Payment

**Endpoint:** `POST /api/v1/trabajos/{trabajoId}/cobros`

**Authentication:** Required (Bearer JWT)

**Authorization:** 
- `[TallerAuthorize<ITrabajoRepository>]` — Validates work order ownership
- `[RequiereSuscripcionActiva]` — Workshop must have active subscription

**Request Body:**
```json
{
  "importe": 500.00,
  "metodoPago": "EFECTIVO",
  "referencia": "REC-2026-001",
  "notas": "Pago en efectivo, cambio: 50€",
  "fechaCobro": "2026-04-13T10:30:00Z"
}
```

**Request Validation Rules:**

| Field | Type | Validation | Error Message |
|-------|------|-----------|----------------|
| `importe` | decimal | Required, > 0.01 | "El importe es obligatorio" / "El importe debe ser mayor a 0" |
| `metodoPago` | enum | Optional | Must be: EFECTIVO, TARJETA, TRANSFERENCIA, BIZUM, OTRO |
| `referencia` | string | Optional, max 100 chars | "Máximo 100 caracteres" |
| `notas` | string | Optional, max 500 chars | "Máximo 500 caracteres" |
| `fechaCobro` | DateTime | Required | "La fecha de cobro es obligatoria" |

**Success Response (201 Created):**
```json
{
  "success": true,
  "message": "Cobro registrado correctamente.",
  "data": {
    "id": 1,
    "trabajoId": 5,
    "importe": 500.00,
    "metodoPago": "EFECTIVO",
    "referencia": "REC-2026-001",
    "notas": "Pago en efectivo, cambio: 50€",
    "fechaCobro": "2026-04-13T10:30:00Z"
  },
  "timestamp": "2026-04-13T14:22:15Z"
}
```

**Side Effects:**
- Parent work order's `EstadoPago` is automatically recalculated
- If total payments now equal or exceed work total, status changes to `PAGADO`
- If total payments are partial, status changes to `PARCIAL`

**Error Responses:**
- `400 Bad Request` — Validation failed (invalid importe, missing fechaCobro, etc.)
  ```json
  {
    "codigo": "SYS_DATOS_INVALIDOS",
    "mensaje": "Existen errores de validación en los datos enviados.",
    "detalles": [
      { "Campo": "Importe", "Error": "El importe debe ser mayor a 0" }
    ]
  }
  ```
- `401 Unauthorized` — Missing authentication
- `403 Forbidden` — Work order does not belong to user's workshop
- `402 Payment Required` — Workshop subscription is not active (RequiereSuscripcionActiva)

---

### 3. Delete Payment

**Endpoint:** `DELETE /api/v1/trabajos/{trabajoId}/cobros/{id}`

**Authentication:** Required (Bearer JWT)

**Authorization:** `[TallerAuthorize<ITrabajoRepository>]` — Validates work order ownership

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Cobro eliminado correctamente.",
  "data": true,
  "timestamp": "2026-04-13T14:22:15Z"
}
```

**Side Effects:**
- Payment is soft-deleted (marked as `Eliminado = true`)
- Parent work order's `EstadoPago` is automatically recalculated
- If no payments remain, status reverts to `PENDIENTE`
- If remaining payments are partial, status remains `PARCIAL`

**Error Responses:**
- `400 Bad Request` — Payment not found or does not belong to workshop
  ```json
  {
    "codigo": "SYS_ENTIDAD_NO_ENCONTRADA",
    "mensaje": "Cobro no encontrado."
  }
  ```
- `401 Unauthorized` — Missing authentication
- `403 Forbidden` — Work order does not belong to user's workshop

---

## Payment Methods Enum

```csharp
public enum CobroMetodoPago
{
    EFECTIVO,        // Cash
    TARJETA,         // Credit/Debit Card
    TRANSFERENCIA,   // Bank Transfer
    BIZUM,           // Bizum (Spanish instant payment)
    OTRO             // Other
}
```

---

## Data Model

### CobroTrabajo Entity

| Field | Type | Nullable | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `Id` | int | No | PK, Auto | Primary key |
| `TallerId` | int | No | FK | Workshop ID (multi-tenancy) |
| `TrabajoId` | int | No | FK | Parent work order ID |
| `Importe` | decimal(10,2) | No | > 0 | Payment amount |
| `MetodoPago` | string | Yes | Enum | Payment method |
| `Referencia` | string | Yes | Max 100 | Transaction reference/receipt number |
| `Notas` | string | Yes | Max 500 | Internal notes about the payment |
| `FechaCobro` | DateTime | No | | Payment date/time |
| `CreadoPorId` | int | Yes | FK → Usuario | User who registered the payment |
| `Eliminado` | bool | No | Default: false | Soft delete flag |

---

## Business Logic

### Automatic Payment State Recalculation

When a payment is created or deleted, the parent work order's `EstadoPago` is automatically recalculated:

```
totalCobrado = SUM(Importe) of all active (Eliminado = false) cobros

if totalCobrado == 0
    → EstadoPago = PENDIENTE

else if totalCobrado >= trabajo.Total
    → EstadoPago = PAGADO

else (0 < totalCobrado < trabajo.Total)
    → EstadoPago = PARCIAL
```

This operation is **transactional**: if any step fails, the entire operation is rolled back.

### Transactional Consistency

- **Create Payment**: Begin transaction → Add cobro → Recalculate EstadoPago → Commit
- **Delete Payment**: Begin transaction → Soft-delete cobro → Recalculate EstadoPago → Commit

If any database operation fails, the transaction is rolled back and the work order state remains unchanged.

---

## Examples

### Example 1: Registering a 50% Payment

**Initial State:**
- Work Order Total: €1000
- EstadoPago: PENDIENTE
- Payments: None

**Request:**
```bash
POST /api/v1/trabajos/5/cobros
Content-Type: application/json

{
  "importe": 500,
  "metodoPago": "TARJETA",
  "fechaCobro": "2026-04-13T10:30:00Z"
}
```

**Result:**
- Payment is recorded with ID 1
- Work Order EstadoPago automatically changes to **PARCIAL**
- Response includes created payment with ID 1

---

### Example 2: Completing Payment with Second Transaction

**Current State (after Example 1):**
- Work Order Total: €1000
- EstadoPago: PARCIAL
- Payments: €500

**Request:**
```bash
POST /api/v1/trabajos/5/cobros
Content-Type: application/json

{
  "importe": 500,
  "metodoPago": "TRANSFERENCIA",
  "referencia": "TRANSFERENCIA-2026-001",
  "fechaCobro": "2026-04-13T14:30:00Z"
}
```

**Result:**
- Second payment is recorded with ID 2
- Total cobrado now = €1000 (equals work order total)
- Work Order EstadoPago automatically changes to **PAGADO**
- Response includes created payment with ID 2

---

### Example 3: Reversing a Payment

**Current State (after Example 2):**
- Work Order Total: €1000
- EstadoPago: PAGADO
- Payments: €500 + €500 = €1000 (IDs 1, 2)

**Request:**
```bash
DELETE /api/v1/trabajos/5/cobros/2
```

**Result:**
- Payment with ID 2 is marked as deleted (Eliminado = true)
- Total cobrado recalculated = €500 (only active payment)
- Work Order EstadoPago automatically changes back to **PARCIAL**
- Response indicates successful deletion

---

## Implementation Notes

### Multi-Tenancy

- All operations validate that the work order (and thus payment) belongs to the authenticated user's workshop via `TallerAuthorize<ITrabajoRepository>`
- The `TallerId` is automatically injected from the JWT context

### Soft Delete

- Payments are never permanently deleted; they are marked with `Eliminado = true`
- EF Core query filters automatically exclude deleted payments from all queries
- This allows for audit trails and potential recovery if needed

### Error Handling

- Invalid data triggers HTTP 400 with detailed validation errors
- Ownership violations trigger HTTP 403 Forbidden
- Database failures trigger HTTP 400 with generic error message
- All errors are wrapped in `ApiErrorResponse` with error code and Spanish message

---

## Related Features

- **Trabajos (Work Orders)**: Parent entity for payments
- **EstadoPago Enum**: PENDIENTE | PARCIAL | PAGADO | ANULADO
- **RequiereSuscripcionActiva**: Authorization filter ensuring active subscription
- **TallerAuthorize**: Authorization filter for multi-tenancy validation
