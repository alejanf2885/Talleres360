# Talleres360 Frontend — Plan de Implementación por Fases

Stack: ASP.NET Core 10 MVC · Tailwind CSS · Vanilla JS (fetch API) · HttpOnly cookies para refresh token

---

## Resumen de fases

| Fase | Nombre | Descripción |
|---|---|---|
| 0 | Infraestructura base | Tailwind, layout, design system, HTTP client |
| 1 | Autenticación | Login, registro, logout, refresh token |
| 1.5 | Onboarding del taller | Wizard de configuración inicial cuando `PerfilConfigurado = false` |
| 2 | Verificación de email | Confirmar cuenta, reenviar email |
| 3 | Dashboard y shell | Sidebar, header, página de inicio |
| 4 | Clientes | CRUD completo + búsqueda y paginación |
| 5 | Vehículos | CRUD, marcas/modelos, vinculación a clientes |
| 6 | Citas | Agenda, estados, conversión a trabajo |
| 7 | Trabajos | Órdenes de servicio, líneas, estados, cobros |
| 8 | Inventario | Categorías, productos, servicios |
| 9 | Presupuestos | CRUD, líneas, conversión a trabajo |
| 10 | Facturación | Documentos comerciales, PDF, Verifactu QR |
| 11 | Notas de vehículo | Notas por vehículo, tipos, auditoría |
| 12 | Suscripción y cuenta | Plan, límites, perfil de taller |

---

## Fase 0 — Infraestructura base

**Objetivo:** proyecto MVC limpio con Tailwind y el design system funcionando.

### Tareas

- [ ] Eliminar Bootstrap y jQuery de `_Layout.cshtml` y `wwwroot/lib`
- [ ] Agregar Tailwind CSS vía CDN play (para desarrollo rápido):
  ```html
  <script src="https://cdn.tailwindcss.com"></script>
  ```
  Configurar paleta personalizada en `<script>tailwind.config = { ... }</script>` o en `site.js`
- [ ] Definir variables CSS del design system en `wwwroot/css/site.css`:
  - Colores: `--color-bg`, `--color-surface`, `--color-accent`, `--color-text`, `--color-muted`
  - Tipografía: Inter / system-ui
- [ ] Rediseñar `_Layout.cshtml`:
  - Shell de dos columnas: sidebar fijo izquierdo + área de contenido principal
  - Header top bar con nombre del taller + usuario + botón logout
  - Sidebar con navegación principal (oculto antes de login)
  - `@RenderBody()` en el área principal
  - `@await RenderSectionAsync("Scripts", required: false)` al final
- [ ] Crear partial `_Sidebar.cshtml` con los enlaces de navegación
- [ ] Crear partial `_Header.cshtml` con el top bar
- [ ] Configurar `HttpClient` tipado en `Program.cs` apuntando a la API backend:
  ```csharp
  builder.Services.AddHttpClient("API", c => c.BaseAddress = new Uri("http://localhost:7000"));
  ```
- [ ] Crear `Services/ApiClient.cs` — wrapper que:
  - Inyecta el JWT desde `IHttpContextAccessor` (sesión/claim)
  - Añade `Authorization: Bearer {token}` a cada petición
  - Envía cookies automáticamente con `HttpCompletionOption`
  - Maneja respuestas `ApiResponse<T>` y `ApiErrorResponse`
- [ ] Crear `Models/ApiResponse.cs` y `Models/ApiErrorResponse.cs` — mirrors de los DTOs del backend
- [ ] Configurar sesión en `Program.cs` para almacenar JWT en memoria de servidor:
  ```csharp
  builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(20); });
  app.UseSession();
  ```
- [ ] `dotnet build` — sin errores

**Criterio de salida:** se ve el layout con sidebar y área de contenido al navegar a `/`.

---

## Fase 1 — Autenticación

**Objetivo:** login y registro funcionando, JWT en sesión, refresh automático.

### Endpoints backend

```
POST /api/v1/auth/register    → crea taller
POST /api/v1/auth/login       → JWT + cookie refreshToken
POST /api/v1/auth/refresh     → renueva JWT (cookie auto-enviada)
POST /api/v1/auth/logout      → invalida cookie
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/AuthController.cs` | Login, Register, Logout, Refresh |
| `Views/Auth/Login.cshtml` | Formulario de login |
| `Views/Auth/Register.cshtml` | Formulario de registro de taller |
| `Models/Auth/LoginRequest.cs` | ViewModel del login |
| `Models/Auth/RegisterRequest.cs` | ViewModel del registro |
| `Services/AuthService.cs` | Llama a la API, guarda JWT en sesión |
| `Middlewares/RefreshTokenMiddleware.cs` | Intercepta peticiones, renueva JWT si caduca |

### Flujo de login

1. Usuario envía form → `AuthController.Login (POST)`
2. `AuthService` llama a `POST /api/v1/auth/login`
3. Si OK: guarda JWT en `HttpContext.Session["jwt"]` + claims en cookie de sesión
4. Redirect a `/` (dashboard)
5. Si error: devuelve mensaje del campo `mensaje` del `ApiErrorResponse`

### Flujo de refresh (middleware)

- En cada request autenticado, leer JWT de sesión
- Si expirado (verificar `exp` claim): llamar `POST /api/v1/auth/refresh` con cookie HttpOnly (se envía automáticamente via `HttpClientHandler`)
- Si OK: actualizar JWT en sesión y continuar
- Si error (401): redirect a `/auth/login`

### Registro

- Campos: `NombreTaller`, `NombreUsuario`, `Email`, `Password`, `ConfirmPassword`, `Telefono`, `CIF`
- Tras registro exitoso → redirect a `/auth/verificacion-pendiente` (mensaje estático)

### Páginas con layout distinto

Login y Register usan `_LayoutAuth.cshtml` (sin sidebar, centrado, fondo oscuro).

### Criterio de salida

- Login con credenciales correctas → entra al dashboard (o al wizard si `PerfilConfigurado = false`)
- Login con credenciales incorrectas → muestra error en el formulario
- JWT renovado automáticamente sin que el usuario lo note

---

## Fase 1.5 — Onboarding del taller

**Objetivo:** wizard de configuración inicial que se muestra después del primer login cuando
`PerfilConfigurado = false`. Bloquea el acceso al dashboard hasta completarlo.

### Por qué existe

El registro solo recoge `Nombre` del taller y el usuario. Los datos fiscales/operativos
(CIF, dirección, localidad, teléfono, logo) se completan en este paso posterior.
El backend marca `PerfilConfigurado = true` al llamar a `PUT /api/v1/workshops/config`.

### Endpoint backend

```
PUT /api/v1/workshops/config
Body: { CIF, Direccion, Localidad, Telefono, Logo (base64, opcional en frontend) }
→ marca PerfilConfigurado = true en BD
```

### Cambios necesarios en código existente

- **`AuthService.GuardarSesion`**: guardar `data.Usuario.PerfilConfigurado` en sesión
  como `"perfil_configurado"` (`"true"` / `"false"`).
- **`AuthController.Login` (POST)**: tras login exitoso, leer `perfil_configurado` de sesión;
  si es `"false"` → `RedirectToAction("Setup", "Taller")` en lugar de ir al dashboard.
- **`AuthRequiredAttribute`** o middleware: si el JWT existe pero `perfil_configurado = false`
  y la ruta no es `/taller/setup` → redirigir al wizard (evita que el usuario navegue al
  dashboard directamente con la URL).

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/TallerController.cs` | Acción GET/POST Setup |
| `Views/Taller/Setup.cshtml` | Formulario wizard (CIF, dirección, localidad, teléfono, logo) |
| `Models/Taller/SetupTallerForm.cs` | ViewModel del wizard |
| `Services/TallerService.cs` | Llama a `PUT /api/v1/workshops/config` |

### UX del wizard

- Usar `_LayoutAuth.cshtml` (sin sidebar) para dar sensación de "paso obligatorio antes de entrar".
- Pasos visuales opcionales (step indicator): "1 · Datos del taller → 2 · Listo".
- Logo: `<input type="file" accept="image/*">` + conversión a base64 en JS antes de enviar.
  Si no sube logo → enviar string vacío (el backend lo ignora si está en blanco).
- Al guardar con éxito: actualizar `perfil_configurado` en sesión a `"true"` y redirigir al dashboard.

### Criterio de salida

- Nuevo taller que hace login por primera vez → ve el wizard, no el dashboard.
- Tras completar el wizard → accede al dashboard normalmente.
- Si intenta ir a `/` sin completar el wizard → redirigido de vuelta a `/taller/setup`.

---

## Fase 2 — Verificación de email

**Objetivo:** activar cuenta desde el enlace del email y gestionar reenvíos.

### Endpoints backend

```
GET  /api/v1/verification/verify-email?token=   → activa la cuenta
POST /api/v1/verification/resend-verification   → reenvía email
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/VerificacionController.cs` | Procesar token, reenviar email |
| `Views/Verificacion/Pendiente.cshtml` | "Revisa tu correo" |
| `Views/Verificacion/Confirmada.cshtml` | "Cuenta activada" |
| `Views/Verificacion/Error.cshtml` | Token inválido o expirado |

### Flujo

1. Usuario hace clic en enlace del email: `GET /verificacion/confirmar?token=XYZ`
2. El controller llama a `GET /api/v1/verification/verify-email?token=XYZ`
3. Si OK → muestra página de cuenta activada + enlace a login
4. Si error → muestra error con opción de reenviar email

### Criterio de salida

- Enlace del email activa la cuenta y muestra confirmación
- Token expirado muestra error con botón de reenvío

---

## Fase 3 — Dashboard y shell de navegación

**Objetivo:** página de inicio útil, sidebar completo, navegación funcional.

### Archivos a crear/modificar

| Archivo | Descripción |
|---|---|
| `Controllers/HomeController.cs` | Acción Index protegida |
| `Views/Home/Index.cshtml` | Dashboard con tarjetas de resumen |
| `Views/Shared/_Sidebar.cshtml` | Menú de navegación completo |
| `Views/Shared/_Header.cshtml` | Top bar con usuario y taller |

### Dashboard — tarjetas de resumen (placeholders hasta tener datos reales)

- Trabajos abiertos hoy
- Citas pendientes
- Clientes total
- Facturas pendientes de cobro

### Sidebar — secciones

```
─ Dashboard
─ Clientes
─ Vehículos
─ Citas
─ Trabajos
─ Inventario
  ─ Categorías
  ─ Productos
  ─ Servicios
─ Presupuestos
─ Facturas
─ Configuración
```

### Protección de rutas

Crear `Filters/AuthRequiredAttribute.cs` (o usar `[Authorize]` con cookie de sesión):
- Verifica que existe JWT válido en `Session["jwt"]`
- Si no → redirect a `/auth/login`
- Aplicar a todos los controllers excepto `AuthController` y `VerificacionController`

### Criterio de salida

- Sidebar muestra sección activa resaltada
- Header muestra nombre del taller y usuario
- Ruta protegida sin sesión redirige a login

---

## Fase 4 — Clientes

**Objetivo:** CRUD completo de clientes con paginación y búsqueda.

### Endpoints backend

```
GET    /api/v1/clientes?pageNumber=&pageSize=&buscar=
GET    /api/v1/clientes/{id}
POST   /api/v1/clientes
PUT    /api/v1/clientes/{id}
DELETE /api/v1/clientes/{id}
GET    /api/v1/clientes/{id}/estadisticas
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/ClientesController.cs` | CRUD + listado |
| `Views/Clientes/Index.cshtml` | Tabla paginada con buscador |
| `Views/Clientes/Detalle.cshtml` | Ficha del cliente + vehículos |
| `Views/Clientes/Crear.cshtml` | Formulario de alta |
| `Views/Clientes/Editar.cshtml` | Formulario de edición |
| `Models/Clientes/ClienteViewModel.cs` | ViewModel de listado/detalle |
| `Models/Clientes/CrearClienteForm.cs` | ViewModel del formulario |
| `Services/ClienteService.cs` | Llama a la API |

### UX

- Buscador en tiempo real (debounce 300ms, fetch a la API)
- Paginación con flechas anterior/siguiente + info "Página X de Y"
- Confirmación modal antes de eliminar
- Toast de éxito/error tras cada operación

### Criterio de salida

- Listar, crear, editar y eliminar un cliente funciona end-to-end

---

## Fase 5 — Vehículos

**Objetivo:** CRUD de vehículos vinculados a clientes, con catálogo de marcas/modelos.

### Endpoints backend

```
GET    /api/v1/vehiculos?pageNumber=&pageSize=&buscar=&clienteId=
GET    /api/v1/vehiculos/{id}
POST   /api/v1/vehiculos
PUT    /api/v1/vehiculos/{id}
DELETE /api/v1/vehiculos/{id}
GET    /api/v1/vehiculos/marcas
GET    /api/v1/vehiculos/marcas/{marcaId}/modelos
GET    /api/v1/vehiculos/tipos
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/VehiculosController.cs` | CRUD + listado |
| `Views/Vehiculos/Index.cshtml` | Tabla con búsqueda |
| `Views/Vehiculos/Detalle.cshtml` | Ficha + historial de trabajos |
| `Views/Vehiculos/Crear.cshtml` | Formulario con select dinámico de marcas/modelos |
| `Views/Vehiculos/Editar.cshtml` | Edición |
| `Services/VehiculoService.cs` | Llama a la API |

### UX especial

- Al seleccionar marca, cargar modelos dinámicamente (fetch al endpoint de modelos)
- Badge de tipo de vehículo (coche, moto, furgoneta…)

### Criterio de salida

- Crear vehículo con marca/modelo dinámico funciona
- Detalle del vehículo muestra historial de trabajos

---

## Fase 6 — Citas

**Objetivo:** gestión de citas con estados y conversión a trabajo.

### Endpoints backend

```
GET    /api/v1/citas?pageNumber=&pageSize=&estado=&fecha=
GET    /api/v1/citas/{id}
POST   /api/v1/citas
PUT    /api/v1/citas/{id}
DELETE /api/v1/citas/{id}
POST   /api/v1/citas/{id}/convertir-a-trabajo
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/CitasController.cs` | CRUD + convertir |
| `Views/Citas/Index.cshtml` | Listado con filtro de estado y fecha |
| `Views/Citas/Detalle.cshtml` | Ficha de la cita |
| `Views/Citas/Crear.cshtml` | Formulario de alta |
| `Views/Citas/Editar.cshtml` | Edición |
| `Services/CitaService.cs` | Llama a la API |

### Estados de cita

`PENDIENTE` · `CONFIRMADA` · `EN_PROCESO` · `COMPLETADA` · `CANCELADA`

Badge de color por estado en la tabla.

### Criterio de salida

- CRUD funciona, estados se muestran con badge de color
- "Convertir a trabajo" crea el trabajo y redirige a su detalle

---

## Fase 7 — Trabajos

**Objetivo:** órdenes de servicio completas con líneas de detalle, estados y cobros parciales.

### Endpoints backend

```
GET    /api/v1/trabajos?pageNumber=&pageSize=&estado=&estadoPago=
GET    /api/v1/trabajos/{id}
POST   /api/v1/trabajos
PUT    /api/v1/trabajos/{id}
DELETE /api/v1/trabajos/{id}

— Líneas —
GET    /api/v1/trabajos/{id}/detalles
POST   /api/v1/trabajos/{id}/detalles
PUT    /api/v1/trabajos/{id}/detalles/{lineaId}
DELETE /api/v1/trabajos/{id}/detalles/{lineaId}

— Cobros —
GET    /api/v1/trabajos/{id}/cobros
POST   /api/v1/trabajos/{id}/cobros
DELETE /api/v1/trabajos/{id}/cobros/{cobroId}
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/TrabajosController.cs` | CRUD + estado + líneas |
| `Controllers/CobrosController.cs` | Cobros parciales |
| `Views/Trabajos/Index.cshtml` | Tabla con filtros de estado y pago |
| `Views/Trabajos/Detalle.cshtml` | Ficha + líneas + cobros + totales |
| `Views/Trabajos/Crear.cshtml` | Formulario de alta |
| `Views/Trabajos/Editar.cshtml` | Edición de cabecera |
| `Services/TrabajoService.cs` | Llama a la API |

### UX especial

- Detalle muestra tabla de líneas (producto/servicio, cantidad, precio, subtotal)
- Panel de cobros con método de pago, importe y fecha
- Badge de `EstadoPago`: PENDIENTE / PARCIAL / PAGADO
- Barra de progreso de cobro: `totalCobrado / total`

### Criterio de salida

- Crear trabajo con líneas, registrar cobro parcial, verificar que `EstadoPago` pasa a PARCIAL

---

## Fase 8 — Inventario

**Objetivo:** gestión de categorías, productos (con stock) y servicios.

### Endpoints backend

```
— Categorías —
GET    /api/v1/inventario/categorias
POST   /api/v1/inventario/categorias
PUT    /api/v1/inventario/categorias/{id}
DELETE /api/v1/inventario/categorias/{id}

— Productos —
GET    /api/v1/inventario/productos?pageNumber=&pageSize=&buscar=&categoriaId=
GET    /api/v1/inventario/productos/{id}
POST   /api/v1/inventario/productos
PUT    /api/v1/inventario/productos/{id}
DELETE /api/v1/inventario/productos/{id}

— Servicios —
GET    /api/v1/servicios
POST   /api/v1/servicios
PUT    /api/v1/servicios/{id}
DELETE /api/v1/servicios/{id}
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/InventarioController.cs` | Productos + categorías |
| `Controllers/ServiciosController.cs` | Servicios |
| `Views/Inventario/Productos.cshtml` | Tabla + filtro por categoría |
| `Views/Inventario/Servicios.cshtml` | Tabla de servicios |
| `Services/InventarioService.cs` | Llama a la API |

### UX

- Indicador de stock bajo (alerta si `StockActual < StockMinimo`)
- Selector de categoría en sidebar de filtros

### Criterio de salida

- CRUD de productos con stock funciona
- Stock bajo aparece resaltado en la tabla

---

## Fase 9 — Presupuestos

**Objetivo:** creación de presupuestos con líneas y conversión a trabajo.

### Endpoints backend

```
GET    /api/v1/documentos?tipo=PRESUPUESTO&pageNumber=&pageSize=
GET    /api/v1/documentos/{id}
POST   /api/v1/documentos          (tipo: PRESUPUESTO)
PUT    /api/v1/documentos/{id}
DELETE /api/v1/documentos/{id}
POST   /api/v1/documentos/{id}/convertir-a-trabajo
— Líneas —
POST   /api/v1/documentos/{id}/lineas
DELETE /api/v1/documentos/{id}/lineas/{lineaId}
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/PresupuestosController.cs` | CRUD + conversión |
| `Views/Presupuestos/Index.cshtml` | Listado |
| `Views/Presupuestos/Detalle.cshtml` | Ficha + líneas + totales |
| `Views/Presupuestos/Crear.cshtml` | Formulario con líneas dinámicas |
| `Services/PresupuestoService.cs` | Llama a la API |

### Criterio de salida

- Presupuesto con líneas se crea y los totales (subtotal, impuestos, total) son correctos
- "Convertir a trabajo" funciona

---

## Fase 10 — Facturación

**Objetivo:** documentos comerciales (facturas, albaranes), descarga de PDF, QR de Verifactu.

### Endpoints backend

```
GET    /api/v1/documentos?tipo=FACTURA&pageNumber=&pageSize=
GET    /api/v1/documentos/{id}
POST   /api/v1/documentos          (tipo: FACTURA)
PUT    /api/v1/documentos/{id}
DELETE /api/v1/documentos/{id}
GET    /api/v1/documentos/{id}/pdf      → descarga PDF
— Líneas —
POST   /api/v1/documentos/{id}/lineas
DELETE /api/v1/documentos/{id}/lineas/{lineaId}
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/FacturasController.cs` | CRUD + descargar PDF |
| `Views/Facturas/Index.cshtml` | Listado con estado de pago |
| `Views/Facturas/Detalle.cshtml` | Ficha + QR Verifactu + enlace PDF |
| `Views/Facturas/Crear.cshtml` | Formulario con líneas dinámicas |
| `Services/FacturaService.cs` | Llama a la API |

### UX especial

- QR de Verifactu: mostrar imagen en el detalle (`<img src="data:image/png;base64,{qr}">`)
- Enlace "Ver en AEAT" con la URL del QR
- Badge de estado Verifactu: Pendiente / Correcto / Incorrecto
- Botón "Descargar PDF" que hace fetch al endpoint y lanza descarga del navegador

### Criterio de salida

- Factura creada muestra QR de Verifactu
- PDF descarga correctamente desde el navegador

---

## Fase 11 — Notas de vehículo

**Objetivo:** notas asociadas a vehículos (advertencias, alertas, pendientes).

### Endpoints backend

```
GET    /api/v1/vehiculos/{vehiculoId}/notas
POST   /api/v1/vehiculos/{vehiculoId}/notas
PUT    /api/v1/vehiculos/{vehiculoId}/notas/{id}
DELETE /api/v1/vehiculos/{vehiculoId}/notas/{id}
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/NotasVehiculoController.cs` | CRUD |
| `Views/NotasVehiculo/_ListaNotas.cshtml` | Partial embebido en detalle del vehículo |
| `Services/NotaVehiculoService.cs` | Llama a la API |

### Tipos de nota

`GENERAL` · `CLIENTE` · `PENDIENTE` · `AVISO`

Badge de color por tipo. Las notas de tipo AVISO aparecen resaltadas en el detalle del vehículo.

### Criterio de salida

- Notas aparecen en la ficha del vehículo
- AVISO muestra badge rojo

---

## Fase 12 — Suscripción y configuración de cuenta

**Objetivo:** mostrar el plan actual, límites usados, y datos del perfil del taller.

### Endpoints backend

```
GET /api/v1/suscripcion/estado
GET /api/v1/taller/perfil
PUT /api/v1/taller/perfil
```

### Archivos a crear

| Archivo | Descripción |
|---|---|
| `Controllers/CuentaController.cs` | Perfil + plan |
| `Views/Cuenta/Perfil.cshtml` | Datos del taller (nombre, CIF, logo, etc.) |
| `Views/Cuenta/Suscripcion.cshtml` | Plan actual, límites, fecha de renovación |
| `Services/CuentaService.cs` | Llama a la API |

### UX

- Barras de progreso para límites del plan (ej. Clientes 45/100)
- Badge de estado: ACTIVO / GRACE_PERIOD / SUSPENDIDO
- Si `SUBS_SIN_PLAN_ACTIVO` (402) → banner global en el layout

### Criterio de salida

- Plan muestra límites reales
- Editar perfil guarda los cambios

---

## Consideraciones transversales

### Gestión de errores global

Crear `Views/Shared/_Toast.cshtml` partial con sistema de notificaciones:
- Verde: operación exitosa
- Rojo: error del servidor o validación
- Amarillo: advertencia (ej. cuenta sin verificar)

Usar `TempData["Success"]` / `TempData["Error"]` del controller → renderizado en el layout.

### Manejo de 401 / sesión expirada

El `RefreshTokenMiddleware` gestiona la renovación automática.
Si el refresh también falla → redirect a `/auth/login` con mensaje "Sesión expirada".

### Manejo de 402 (suscripción)

Intercepción global en `ApiClient.cs`:
Si la API devuelve 402 → redirect a `/cuenta/suscripcion` con banner de aviso.

### Manejo de 403 (recurso de otro taller)

Mostrar página de error 403 (`Views/Shared/Error403.cshtml`).

### Paginación reutilizable

Crear `Views/Shared/_Pagination.cshtml` partial que recibe `PagedResponse` y renderiza los controles.

### Formularios con validación del lado cliente

Usar `jquery-validation-unobtrusive` (ya incluido) para validación antes de enviar.
Mostrar errores del servidor campo a campo si la API devuelve `detalles` con errores de modelo.

---

## Orden de implementación recomendado

```
Fase 0 → Fase 1 → Fase 1.5 → Fase 2 → Fase 3 → Fase 4 → Fase 5
→ Fase 7 (Trabajos, sin cobros) → Fase 6 (Citas) → Fase 7 cobros
→ Fase 8 → Fase 9 → Fase 10 → Fase 11 → Fase 12
```

Justificación: los Trabajos dependen de Clientes y Vehículos. Las Citas también. Facturación
depende de Trabajos. Inventario es independiente y puede avanzar en paralelo con Citas/Trabajos.
La Fase 1.5 (onboarding) debe ir inmediatamente después del login porque bloquea el acceso
al resto de la app hasta que el taller esté configurado.
