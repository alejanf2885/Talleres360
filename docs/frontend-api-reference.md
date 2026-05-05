# Talleres360 — Referencia de API para el Frontend

Extraído del código fuente del backend (.NET 10 / ASP.NET Core).

---

## Base URL y CORS

```
API: http://localhost:7000   (desarrollo)
CORS habilitado para: http://localhost:4200
Credenciales habilitadas: sí (necesario para cookies del refresh token)
```

---

## Autenticación

### Flujo completo

```
1. POST /api/v1/auth/register       → crea taller + envía email de verificación
2. GET  /api/v1/verification/verify-email?token=  → activa la cuenta
3. POST /api/v1/auth/login          → devuelve JWT + cookie refreshToken
4. Guardar JWT (localStorage/sessionStorage)
5. Incluir header en cada petición autenticada:
     Authorization: Bearer {jwtToken}
6. La cookie refreshToken se envía automáticamente (HttpOnly, SameSite=Strict)
7. POST /api/v1/auth/refresh        → renueva el JWT cuando expire (15 min)
```

### Claims del JWT

| Claim | Valor |
|---|---|
| `NameIdentifier` | UsuarioId (int) |
| `Email` | Email del usuario |
| `Role` | `SUPERADMIN` / `ADMIN` / `MECANICO` / `RECEPCIONISTA` |
| `TallerId` | ID del taller (nullable) |
| `SecurityStamp` | Hash de seguridad interno |

**Duración del JWT:** 15 minutos  
**Duración del refresh token:** 7 días (cookie HttpOnly)  
**El refresh token es de un solo uso** — al renovar se genera uno nuevo

---

## Estructura de respuestas

### Éxito

```json
{
  "success": true,
  "message": "Descripción de la operación",
  "data": { },
  "timestamp": "2026-05-06T10:00:00Z"
}
```

### Éxito paginado (`data` en listados)

```json
{
  "success": true,
  "message": "...",
  "data": {
    "data": [ ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Error

```json
{
  "codigo": "AUTH_CREDENCIALES_INCORRECTAS",
  "mensaje": "Las credenciales introducidas no son válidas.",
  "detalles": null
}
```

`detalles` puede ser un objeto con errores de validación campo a campo (en errores de modelo).

---

## Status codes

| Código | Significado |
|---|---|
| 200 | OK |
| 201 | Creado (POST exitoso) |
| 400 | Datos inválidos |
| 401 | Sin autenticación o JWT inválido |
| 402 | Sin plan de suscripción activo |
| 403 | Recurso no pertenece al taller del usuario |
| 429 | Rate limiting superado |
| 500 | Error interno del servidor |

---

## Paginación

Todos los listados aceptan query params:

| Param | Default | Min | Max |
|---|---|---|---|
| `pageNumber` | 1 | 1 | — |
| `pageSize` | 10 | 5 | 50 |

---

## Códigos de error

| Código | Cuándo ocurre |
|---|---|
| `AUTH_CREDENCIALES_INCORRECTAS` | Login con email/password incorrectos |
| `AUTH_CUENTA_INACTIVA` | Email no verificado |
| `AUTH_CUENTA_BLOQUEADA` | Cuenta bloqueada |
| `AUTH_TOKEN_INVALIDO` | JWT malformado o token de verificación inválido |
| `AUTH_TOKEN_EXPIRADO` | JWT expirado |
| `AUTH_REFRESH_TOKEN_INVALIDO` | Refresh token no existe o ya fue usado |
| `AUTH_REFRESH_TOKEN_EXPIRADO` | Refresh token caducó (7 días) |
| `AUTH_LOGOUT_FALLIDO` | Error al revocar sesión |
| `AUTH_ACCESO_DENEGADO` | El recurso pedido no pertenece al taller (403) |
| `AUTH_REVOCACION_FALLIDA` | Error al cerrar todas las sesiones |
| `REG_EMAIL_YA_REGISTRADO` | Email duplicado en registro |
| `REG_CIF_DUPLICADO` | CIF del taller duplicado |
| `REG_FALLIDO` | Error genérico de registro |
| `SUBS_SIN_PLAN_ACTIVO` | Taller sin suscripción activa (402) |
| `SUBS_LIMITE_ALCANZADO` | Límite del plan alcanzado |
| `CUST_NO_ENCONTRADO` | Cliente no existe |
| `CUST_DNI_DUPLICADO` | DNI duplicado en el taller |
| `CUST_EMAIL_DUPLICADO` | Email duplicado en clientes |
| `CUST_LIMITE_PLAN_ALCANZADO` | Límite de clientes del plan |
| `CUST_ERROR_ELIMINACION` | Error eliminando cliente |
| `VEH_NO_ENCONTRADO` | Vehículo no existe |
| `VEH_MATRICULA_DUPLICADA` | Matrícula duplicada |
| `VEH_VIN_INVALIDO` | VIN inválido |
| `VEH_MARCA_NO_ENCONTRADA` | Marca no existe |
| `VEH_MODELO_NO_ENCONTRADA` | Modelo no existe |
| `MAR_NOMBRE_DUPLICADO` | Nombre de marca duplicado |
| `INV_CATEGORIA_NO_ENCONTRADA` | Categoría de producto no existe |
| `INV_CATEGORIA_NOMBRE_DUPLICADO` | Nombre de categoría duplicado |
| `INV_PRODUCTO_NO_ENCONTRADO` | Producto no existe |
| `INV_PRODUCTO_NOMBRE_DUPLICADO` | Nombre de producto duplicado |
| `INV_PRODUCTO_REFERENCIA_DUPLICADA` | Referencia de producto duplicada |
| `CITA_NO_ENCONTRADA` | Cita no existe |
| `CITA_ESTADO_INVALIDO` | Estado de cita inválido |
| `TRA_NO_ENCONTRADO` | Trabajo no existe |
| `TRA_ESTADO_INVALIDO` | Estado del trabajo inválido |
| `TRA_NO_FACTURABLE` | Trabajo no está en estado CERRADO |
| `TRA_TRANSICION_INVALIDA` | Transición de estado no permitida |
| `SYS_DATOS_INVALIDOS` | Validación de modelo fallida |
| `SYS_ENTIDAD_NO_ENCONTRADA` | Entidad no existe (genérico) |
| `SYS_ARCHIVO_DEMASIADO_GRANDE` | Archivo supera el límite de tamaño |
| `SYS_ERROR_GENERICO` | Error interno no controlado (500) |
| `SYS_ERROR_BASE_DATOS` | Error de acceso a base de datos |

---

## Endpoints por módulo

---

### Auth — `/api/v1/auth`

#### `POST /register`
Registro de nuevo taller. Rate limit: 5 req / 2 min.

**Body:**
```json
{
  "nombre": "Mi Taller",
  "cif": "B12345678",
  "email": "admin@taller.com",
  "password": "MiPassword123!"
}
```

**Respuesta 200:** `ApiResponse<object>` — envía email de verificación  
**Errores:** `REG_EMAIL_YA_REGISTRADO`, `REG_CIF_DUPLICADO`, `REG_FALLIDO`

---

#### `POST /login`
Rate limit: 5 req / 2 min.

**Body:**
```json
{
  "email": "admin@taller.com",
  "password": "MiPassword123!"
}
```

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGci...",
    "usuario": {
      "id": 1,
      "nombre": "Alejandro",
      "email": "admin@taller.com",
      "rol": "ADMIN",
      "tallerId": 5,
      "perfilConfigurado": true
    }
  }
}
```
Cookie `refreshToken` (HttpOnly, Secure, SameSite=Strict) enviada en la respuesta.

**Errores:** `AUTH_CREDENCIALES_INCORRECTAS`, `AUTH_CUENTA_INACTIVA`, `AUTH_EMAIL_NO_VERIFICADO`

---

#### `POST /refresh`
Renueva el JWT usando la cookie refreshToken. Rate limit: 10 req / 1 min.

No requiere body. Lee automáticamente la cookie `refreshToken`.

**Respuesta 200:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGci..."
  }
}
```
Nueva cookie `refreshToken` enviada en la respuesta.

**Errores:** `AUTH_REFRESH_TOKEN_INVALIDO`, `AUTH_REFRESH_TOKEN_EXPIRADO`

---

#### `POST /logout`
Cierra la sesión actual. No requiere `[Authorize]` — pero sí la cookie.

**Respuesta 200:** `ApiResponse<bool>` (data: true)  
**Errores:** `AUTH_LOGOUT_FALLIDO`

---

#### `POST /logout-all`
Revoca todos los refresh tokens del usuario. `[Authorize]` requerido.

**Respuesta 200:** `ApiResponse<bool>` (data: true)  
**Errores:** `AUTH_REVOCACION_FALLIDA`

---

### Verificación — `/api/v1/verification`

#### `GET /verify-email?token={token}`
Verifica el email del usuario. Rate limit: 5 req / 1 min.

**Respuesta 200:** `ApiResponse<bool>`  
**Errores:** `AUTH_TOKEN_INVALIDO`

---

#### `POST /resend`
Reenvía email de verificación. Rate limit: 2 req / 1 min. Siempre responde OK.

**Body:** `{ "email": "admin@taller.com" }`

---

### Taller — `/api/v1/workshops`

#### `GET /my-workshop`
Datos del taller del usuario autenticado.

**Respuesta 200:** `ApiResponse<WorkshopDto>`  
**Errores:** `SYS_ENTIDAD_NO_ENCONTRADA`

---

#### `PUT /config`
Actualiza configuración del taller (nombre, dirección, logo, etc.).

**Body:** `ConfigurarTallerRequest`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Clientes — `/api/v1/customers`

#### `GET /`
Query: `pageNumber`, `pageSize`, `buscar` (opcional, búsqueda libre)  
**Respuesta 200:** `ApiResponse<PagedResponse<ClienteDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<ClienteDto>`  
**Errores:** `CUST_NO_ENCONTRADO`, `AUTH_ACCESO_DENEGADO` (403)

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearClienteRequest`  
**Respuesta 201:** `ApiResponse<ClienteDto>`  
**Errores:** `CUST_LIMITE_PLAN_ALCANZADO`, `CUST_DNI_DUPLICADO`, `CUST_EMAIL_DUPLICADO`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Body:** `ActualizarClienteRequest`  
**Respuesta 200:** `ApiResponse<ClienteDto>`

#### `DELETE /{id}`
**Respuesta 200:** `ApiResponse<bool>`  
**Errores:** `CUST_ERROR_ELIMINACION`

#### `GET /stats`
**Respuesta 200:** `ApiResponse<ClienteStatsResponse>`

---

### Vehículos — `/api/v1/vehicles`

#### `GET /`
Query: `pageNumber`, `pageSize`, `matricula`, `marcaId`, `modeloId`  
**Respuesta 200:** `ApiResponse<PagedResponse<VehiculoDetalle>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<VehiculoDetalle>`  
**Errores:** `VEH_NO_ENCONTRADO`, `AUTH_ACCESO_DENEGADO`

#### `GET /matricula/{matricula}`
**Respuesta 200:** `ApiResponse<VehiculoDetalle>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearVehiculoRequest`  
**Respuesta 201:** `ApiResponse<VehiculoDetalle>`  
**Errores:** `VEH_MATRICULA_DUPLICADA`, `VEH_VIN_INVALIDO`, `VEH_MARCA_NO_ENCONTRADA`, `VEH_MODELO_NO_ENCONTRADA`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Body:** `ActualizarVehiculoRequest`  
**Respuesta 200:** `ApiResponse<VehiculoDetalle>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Marcas de vehículo — `/api/v1/vehiculos/marcas`

#### `GET /`
Lista todas (oficiales + las del taller).  
**Respuesta 200:** `ApiResponse<List<MarcaVehiculoDto>>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `{ "nombre": "Mi Marca", "esOficial": false }`  
**Respuesta 201:** `ApiResponse<MarcaVehiculoDto>`  
**Errores:** `MAR_NOMBRE_DUPLICADO`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Modelos de vehículo — `/api/v1/vehiculos/modelos`

#### `GET /{marcaId}`
Modelos de una marca.  
**Respuesta 200:** `ApiResponse<List<ModeloVehiculoDto>>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearModeloVehiculoDto`  
**Respuesta 200:** `ApiResponse<ModeloVehiculoDto>`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<ModeloVehiculoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Tipos de vehículo — `/api/v1/vehiculos/tipos`

#### `GET /`
Lista estática de tipos (coche, moto, camión, etc.).  
**Respuesta 200:** `ApiResponse<List<VehiculoTipoDto>>`

---

### Citas — `/api/v1/citas`

#### `GET /`
Query: `pageNumber`, `pageSize`, `fechaDesde`, `fechaHasta`, `estado`, `vehiculoId`  
**Respuesta 200:** `ApiResponse<PagedResponse<CitaDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<CitaDto>`  
**Errores:** `CITA_NO_ENCONTRADA`, `AUTH_ACCESO_DENEGADO`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearCitaRequest`  
**Respuesta 201:** `ApiResponse<CitaDto>`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Body:** `ActualizarCitaRequest`  
**Respuesta 200:** `ApiResponse<CitaDto>`  
**Errores:** `CITA_ESTADO_INVALIDO`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

#### `POST /{id}/convertir-a-trabajo`
`[RequiereSuscripcionActiva]`  
**Body:** `ConvertirCitaTrabajoRequest`  
**Respuesta 200:** `ApiResponse<CitaTrabajoDto>`

---

### Trabajos — `/api/v1/trabajos`

**Estados posibles:** `ABIERTO | EN_PROCESO | PENDIENTE_PIEZAS | CERRADO | CANCELADO | FACTURADO`  
**Estados de pago:** `PENDIENTE | PARCIAL | PAGADO | ANULADO`

#### `GET /`
Query: `pageNumber`, `pageSize`, `estado`, `vehiculoId`, `datosIncompletos`  
**Respuesta 200:** `ApiResponse<PagedResponse<TrabajoDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<TrabajoDto>`  
**Errores:** `TRA_NO_ENCONTRADO`, `AUTH_ACCESO_DENEGADO`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearTrabajoRequest`  
**Respuesta 201:** `ApiResponse<TrabajoDto>`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Body:** `ActualizarTrabajoRequest`  
**Respuesta 200:** `ApiResponse<TrabajoDto>`  
**Errores:** `TRA_ESTADO_INVALIDO`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

#### `POST /{id}/facturar`
`[RequiereSuscripcionActiva]`  
Convierte el trabajo en factura. El trabajo debe estar en estado `CERRADO`.  
**Respuesta 200:** `ApiResponse<TrabajoDto>` (con estado actualizado a `FACTURADO`)  
**Errores:** `TRA_NO_FACTURABLE`, `TRA_ESTADO_INVALIDO`

---

### Líneas de trabajo — `/api/v1/trabajos/{trabajoId}/detalles`

#### `GET /`
**Respuesta 200:** `ApiResponse<IEnumerable<DetalleTrabajoDto>>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearDetalleTrabajoRequest`  
**Respuesta 200:** `ApiResponse<DetalleTrabajoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Cobros de trabajo — `/api/v1/trabajos/{trabajoId}/cobros`

Cobros parciales de un trabajo. Recalculan `EstadoPago` automáticamente.

#### `GET /`
**Respuesta 200:** `ApiResponse<PagedResponse<CobroTrabajoDto>>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:**
```json
{
  "importe": 150.00,
  "metodoPago": "EFECTIVO",
  "referencia": "REC-001",
  "notas": "Cobro parcial",
  "fechaCobro": "2026-05-06T10:00:00Z"
}
```
`metodoPago`: `EFECTIVO | TARJETA | TRANSFERENCIA | BIZUM | OTRO`  
**Respuesta 201:** `ApiResponse<CobroTrabajoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Presupuestos — `/api/v1/presupuestos`

#### `GET /`
Query: `pageNumber`, `pageSize`  
**Respuesta 200:** `ApiResponse<PagedResponse<TrabajoDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<TrabajoDto>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearTrabajoRequest`  
**Respuesta 201:** `ApiResponse<TrabajoDto>`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Body:** `ActualizarTrabajoRequest`  
**Respuesta 200:** `ApiResponse<TrabajoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

#### `POST /{id}/enviar`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<TrabajoDto>`  
**Errores:** `TRA_TRANSICION_INVALIDA`

#### `POST /{id}/aceptar`
`[RequiereSuscripcionActiva]`  
**Body (opcional):** `{ "firmaAceptacionUrl": "..." }`  
**Respuesta 200:** `ApiResponse<TrabajoDto>`

#### `POST /{id}/rechazar`
`[RequiereSuscripcionActiva]`  
**Body:** `{ "motivoRechazo": "..." }`  
**Respuesta 200:** `ApiResponse<TrabajoDto>`

---

### Facturas — `/api/v1/facturas`

Las facturas se generan con `POST /api/v1/trabajos/{id}/facturar`.  
Estos endpoints son solo de consulta.

#### `GET /`
Query: `pageNumber`, `pageSize`  
**Respuesta 200:** `ApiResponse<PagedResponse<FacturaDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<FacturaDto>`  
**Errores:** `AUTH_ACCESO_DENEGADO`

#### `GET /trabajo/{trabajoId}`
Obtiene la factura asociada a un trabajo.  
**Respuesta 200:** `ApiResponse<FacturaDto>`

---

### Inventario — Categorías — `/api/v1/inventario/categorias`

#### `GET /`
**Respuesta 200:** `ApiResponse<List<CategoriaProductoDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<CategoriaProductoDto>`  
**Errores:** `INV_CATEGORIA_NO_ENCONTRADA`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearCategoriaProductoRequest`  
**Respuesta 201:** `ApiResponse<CategoriaProductoDto>`  
**Errores:** `INV_CATEGORIA_NOMBRE_DUPLICADO`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<CategoriaProductoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Inventario — Productos — `/api/v1/inventario/productos`

#### `GET /`
Query: `pageNumber`, `pageSize`, `buscar`, `categoriaId`  
**Respuesta 200:** `ApiResponse<PagedResponse<ProductoDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<ProductoDto>`  
**Errores:** `INV_PRODUCTO_NO_ENCONTRADO`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearProductoRequest`  
**Respuesta 201:** `ApiResponse<ProductoDto>`  
**Errores:** `INV_PRODUCTO_NOMBRE_DUPLICADO`, `INV_PRODUCTO_REFERENCIA_DUPLICADA`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<ProductoDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Servicios — `/api/v1/servicios`

#### `GET /`
Query: `pageNumber`, `pageSize`, `buscar`, `activo` (bool)  
**Respuesta 200:** `ApiResponse<PagedResponse<ServicioDto>>`

#### `GET /{id}`
**Respuesta 200:** `ApiResponse<ServicioDto>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearServicioRequest`  
**Respuesta 201:** `ApiResponse<ServicioDto>`

#### `PUT /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<ServicioDto>`

#### `DELETE /{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

### Tarifas de hora — `/api/v1/tarifas-hora`

#### `GET /`
Historial de tarifas.  
**Respuesta 200:** `ApiResponse<IEnumerable<TarifaHoraDto>>`

#### `GET /activa`
Tarifa vigente.  
**Respuesta 200:** `ApiResponse<TarifaHoraDto?>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearTarifaHoraRequest`  
**Respuesta 200:** `ApiResponse<TarifaHoraDto>`

---

### Notas de vehículo — anidadas en `/api/v1/vehiculos/{vehiculoId}/notas`

#### `GET /`
**Respuesta 200:** `ApiResponse<List<NotaVehiculoDto>>`

#### `POST /`
`[RequiereSuscripcionActiva]`  
**Body:** `CrearNotaVehiculoRequest`  
**Tipos de nota:** `GENERAL | CLIENTE | PENDIENTE | AVISO`  
**Respuesta 201:** `ApiResponse<NotaVehiculoDto>`

#### `GET /api/v1/notas-vehiculo/{id}`
**Respuesta 200:** `ApiResponse<NotaVehiculoDto>`

#### `PUT /api/v1/notas-vehiculo/{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<NotaVehiculoDto>`

#### `PATCH /api/v1/notas-vehiculo/{id}/resolver`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<NotaVehiculoDto>`

#### `DELETE /api/v1/notas-vehiculo/{id}`
`[RequiereSuscripcionActiva]`  
**Respuesta 200:** `ApiResponse<bool>`

---

## Notas para la implementación del cliente HTTP

### Interceptor de autenticación
Añadir automáticamente `Authorization: Bearer {token}` a todas las peticiones.

### Interceptor de refresh
Cuando el backend devuelva 401, intentar renovar el token con `POST /auth/refresh` (1 vez) y reintentar la petición original. Si el refresh también falla → redirigir al login.

### Manejo de errores por código

| Código HTTP | Acción recomendada |
|---|---|
| 400 | Mostrar `mensaje` del error al usuario. Si `detalles` existe, mostrar errores de campo |
| 401 | Intentar refresh; si falla → logout y redirigir al login |
| 402 | Mostrar pantalla de "plan requerido" / upgrade |
| 403 | Mostrar "No tienes acceso a este recurso" |
| 429 | Mostrar "Demasiadas peticiones, espera un momento" |
| 500 | Mostrar "Error del servidor, inténtalo más tarde" |

### Envío de cookies
La cookie `refreshToken` es HttpOnly — el navegador la gestiona automáticamente.  
El cliente HTTP debe enviar credenciales: `credentials: 'include'` (fetch) o `withCredentials: true` (axios/HttpClient).
