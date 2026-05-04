# Verifacti API — Referencia de Integración

Fuente: OpenAPI spec oficial + colección Postman de Verifacti.  
Docs completa: https://www.verifacti.com/docs

---

## Autenticación

**Bearer Token** en cada petición:
```
Authorization: Bearer {verifactuApiKey}
```

Las API Keys son por NIF y entorno:
- Test: `vf_test_...`
- Producción: `vf_prod_...`

El NIF del emisor y el entorno quedan determinados por la API key utilizada.

**Config en `appsettings`:**
```json
"Verifactu": {
  "ApiUrl": "https://api.verifacti.com",
  "ApiKey": "vf_test_..."
}
```

---

## Base URL

```
https://api.verifacti.com
```

---

## Endpoints

### `GET /verifactu/health`
Comprueba el estado de la API key y devuelve el NIF y entorno asociados.

**Respuesta 200:**
```json
{
  "estado": "OK",
  "nif": "B75777847",
  "entorno": "test"
}
```

---

### `POST /verifactu/create`
Registra una factura nueva. La encola y la envía a la AEAT en ~1 minuto.

**Comportamiento:**
- **200**: Factura encolada. Devuelve QR y UUID inmediatamente. Estado siempre `Pendiente`.
- **400**: JSON inválido. No se genera ningún registro ni envío a la AEAT.
- **500**: Error de servidor. Reintentar más tarde.

**Request:**
```json
{
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2026",
  "tipo_factura": "F1",
  "descripcion": "Venta de bienes",
  "nif": "A15022510",
  "nombre": "Empresa de prueba SL",
  "lineas": [
    {
      "base_imponible": "200",
      "tipo_impositivo": "21",
      "cuota_repercutida": "42"
    },
    {
      "base_imponible": "100",
      "tipo_impositivo": "10",
      "cuota_repercutida": "10"
    }
  ],
  "importe_total": "352.00"
}
```

**Campos del request:**

| Campo | Tipo | Req | Descripción |
|---|---|---|---|
| `serie` | string | Sí | Serie de la factura |
| `numero` | string | Sí | Número (`serie`+`numero` ≤ 60 chars) |
| `fecha_expedicion` | `DD-MM-YYYY` | Sí | Fecha de emisión (no futura) |
| `fecha_operacion` | `DD-MM-YYYY` | No | Fecha de la operación si difiere de emisión |
| `tipo_factura` | string | Sí | Ver tabla de tipos |
| `tipo_rectificativa` | string | No* | `S` (sustitución) o `I` (diferencias) — solo en Rx |
| `descripcion` | string | Sí | Descripción de la operación |
| `nif` | string | No* | NIF/CIF destinatario (obligatorio en F1, omitir en F2) |
| `nombre` | string | No* | Nombre destinatario (obligatorio en F1, omitir en F2) |
| `id_otro` | object | No | Destinatario extranjero sin NIF español |
| `lineas` | array | Sí | Máx. 12 líneas de desglose IVA |
| `importe_total` | string | Sí | Total de la factura |
| `importe_rectificativa` | object | No* | Solo en rectificativas: `base_rectificada`, `cuota_rectificada` |
| `facturas_rectificadas` | array | No* | Facturas originales en rectificativas Rx |
| `facturas_sustituidas` | array | No* | Facturas sustituidas en F3 |

**Campos de `lineas[]`:**

| Campo | Tipo | Descripción |
|---|---|---|
| `base_imponible` | string | Base imponible (puede ser negativo en abonos) |
| `tipo_impositivo` | string | Tipo IVA en % |
| `cuota_repercutida` | string | Cuota IVA calculada |
| `operacion_exenta` | string | Código exención: `E1`, `E2`, `E5` |
| `calificacion_operacion` | string | `S1`, `S2` (sujeta), `N1`, `N2` (no sujeta) |
| `clave_regimen` | string | Régimen especial: `02`, `03`, `17`, `18` |
| `impuesto` | string | `02`=IPSI, `03`=IGIC |
| `tipo_recargo_equivalencia` | string | Recargo de equivalencia |

**Tipos de factura:**

| Código | Tipo |
|---|---|
| `F1` | Normal (con datos destinatario) |
| `F2` | Simplificada (sin NIF destinatario) |
| `F3` | Sustitutiva / Canje de simplificadas |
| `R1`–`R5` | Rectificativas |

**Respuesta 200:**
```json
{
  "uuid": "b018ced3-b362-4494-8776-9eefff1c160c",
  "estado": "Pendiente",
  "url": "https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?nif=A15022510&numserie=A1&fecha=05-06-2026&importe=352",
  "qr": "iVBORw0KGgoAAAANSUhEUgAA..."
}
```

> El `qr` es un PNG en Base64. Debe incluirse en la factura PDF entregada al cliente.  
> El `uuid` se usa para consultar el estado del envío con `GET /verifactu/status?uuid=`.

---

### `POST /verifactu/create_bulk`
Registra múltiples facturas en una sola llamada. Body: array con la misma estructura que `create`.

```json
[
  { "serie": "B", "numero": "1", "tipo_factura": "F1", ... },
  { "serie": "B", "numero": "2", "tipo_factura": "F2", ... }
]
```

---

### `PUT /verifactu/modify`
Subsana (corrige) un registro ya enviado que fue rechazado o tiene errores.

```json
{
  "rechazo_previo": "N",
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025",
  "tipo_factura": "F1",
  "descripcion": "Descripción corregida",
  "nif": "A15022510",
  "nombre": "Empresa de prueba SL",
  "lineas": [
    { "base_imponible": "200", "tipo_impositivo": "21", "cuota_repercutida": "42" }
  ],
  "importe_total": "242.00"
}
```

`rechazo_previo`: `N` (subsanación normal), `S` (tras rechazo previo), `X` (alta tras rechazo previo)

---

### `POST /verifactu/cancel`
Anula una factura registrada.

```json
{
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025"
}
```

Campos opcionales: `rechazo_previo` (`S`/`N`), `sin_registro_previo` (`S`/`N`)

---

### `POST /verifactu/status`
Consulta el estado de una factura **en el sistema de la AEAT**. Solo facturas ya procesadas.

```json
{
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025"
}
```

Respuesta: `$ref estadoFactura` (ver schema más abajo)

---

### `GET /verifactu/status?uuid={uuid}`
Consulta el estado de un **registro de facturación** por su UUID (devuelto por `create`).

```
GET /verifactu/status?uuid=b018ced3-b362-4494-8776-9eefff1c160c
```

**Respuesta 200:**
```json
{
  "nif": "A15022510",
  "serie": "A",
  "numero": "34547",
  "fecha_expedicion": "22-04-2026",
  "operacion": "Alta",
  "estado": "Correcto",
  "url": "https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?...",
  "qr": "jBBWRw0KGgoABAANSUhEAH0CAIAAABE...",
  "codigo_error": null,
  "mensaje_error": null,
  "estado_registro_duplicado": null
}
```

**Estados posibles del registro:**

| Estado | Descripción |
|---|---|
| `Pendiente` | Encolado, aún no procesado |
| `Correcto` | Aceptado por la AEAT |
| `Aceptado con errores` | Aceptado pero requiere subsanación |
| `Incorrecto` | Rechazado — requiere `modify` con `rechazo_previo` |
| `Duplicado` | Ya existe un registro con misma serie/numero/fecha |
| `Anulado` | Anulación procesada correctamente |
| `Factura inexistente` | Anulación rechazada — la factura no existe en AEAT |
| `No registrado` | Rechazado por la AEAT |
| `Error servidor AEAT` | Error en AEAT — se reintentará automáticamente |

> En entorno test, los registros se guardan máximo 90 días.  
> Los endpoints de consulta de facturas tienen datos históricos (consultan directamente a la AEAT).

---

### `POST /verifactu/list`
Lista facturas con filtros de ejercicio, periodo o rango de fechas.

```json
{
  "ejercicio": "2025",
  "periodo": "04",
  "rango_fecha_expedicion": {
    "desde": "03-04-2025",
    "hasta": "08-04-2025"
  }
}
```

---

### `POST /verifactu/downloadXML`
Descarga el XML Veri*Factu generado para una factura concreta.

```json
{ "serie": "A", "numero": "1" }
```

---

### `POST /verifactu/export`
Exporta todos los XMLs de un ejercicio/periodo como ZIP.

```json
{ "ejercicio": "2025", "periodo": "06" }
```

---

### `GET /verifactu/declaracion`
Descarga la declaración del NIF registrado.

---

## Códigos HTTP

| Código | Significado |
|---|---|
| 200 | Operación exitosa (o registro encolado) |
| 400 | Error de validación — no se genera registro |
| 404 | Registro no encontrado |
| 429 | Límite de llamadas por minuto superado |
| 500 | Error interno de Verifacti — reintentar |

---

## Casos de uso especiales

| Caso | Tipo factura | Campos adicionales |
|---|---|---|
| Factura de abono | `F1` | `base_imponible` y `cuota_repercutida` negativos |
| Exenta de IVA | `F1` | `operacion_exenta: "E1"` — sin `tipo_impositivo` |
| No sujeta | `F1` | `calificacion_operacion: "N1"` |
| Inversión sujeto pasivo | `F1` | `calificacion_operacion: "S2"`, IVA a 0 |
| B2B intracomunitaria (bienes) | `F1` | `id_otro`, `operacion_exenta: "E5"` |
| B2B intracomunitaria (servicios) | `F1` | `id_otro`, `calificacion_operacion: "N2"` |
| B2C intracomunitaria debajo umbral | `F1` | `id_otro`, IVA normal |
| B2C intracomunitaria encima umbral | `F1` | `id_otro`, `clave_regimen: "17"`, `calificacion_operacion: "N2"` |
| Exportación extracomunitaria | `F1` | `id_otro`, `clave_regimen: "02"`, `operacion_exenta: "E2"` |
| IGIC (Canarias) | `F1` | `impuesto: "03"` en la línea |
| IPSI (Ceuta/Melilla) | `F1` | `impuesto: "02"` en la línea |
| Rectificativa sustitución | `R1`–`R5` | `tipo_rectificativa: "S"`, `importe_rectificativa`, `facturas_rectificadas` |
| Rectificativa diferencias | `R1`–`R5` | `tipo_rectificativa: "I"`, `importe_rectificativa`, `facturas_rectificadas` |
| Canje de simplificadas | `F3` | `facturas_sustituidas` |

---

## Notas de integración para Talleres360

- Tras `create`, persistir `uuid` y `qr` en la tabla `Facturas`
- El QR (Base64 PNG) se imprime en el PDF de la factura
- La AEAT puede tardar ~1 min en procesar; el estado final se consulta con `GET /status?uuid=`
- No se pueden borrar registros — solo anular (`cancel`) o subsanar (`modify`)
- No se pueden modificar facturas — solo rectificar (emitir Rx) o subsanar si fue rechazada
- Comunicación TLS 1.2+
- Límite estándar: 3.000 facturas/NIF/mes
- Los talleres usarán **F1** (con NIF cliente) como caso principal
- Para facturas sin cliente identificado: **F2** (simplificada)
