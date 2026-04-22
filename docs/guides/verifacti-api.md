# Verifacti API — Referencia de Integración

Documentación extraída de la colección Postman oficial de Verifacti.
Docs completa: https://www.verifacti.com/docs

---

## Autenticación

**Bearer Token** — en cada petición incluir el header:
```
Authorization: Bearer {verifactuApiKey}
```

Las API Keys son por NIF y entorno:
- Test: comienzan con `vf_test_`
- Producción: comienzan con `vf_prod_`

Configurar en `appsettings`:
```json
"Verifactu": {
  "ApiUrl": "https://api.verifacti.com",
  "ApiKey": "vf_test_..."
}
```

---

## Base URL

Variable `{{apiUrl}}` en Postman. Confirmar con Verifacti el host exacto de producción.

---

## Endpoints

### `GET /verifactu/health`
Comprueba si la API está operativa. No requiere body.

---

### `POST /verifactu/create`
Registra una factura en Veri*Factu y la envía a la AEAT.

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

**Campos:**

| Campo | Tipo | Req | Descripción |
|---|---|---|---|
| `serie` | string | Sí | Serie de la factura |
| `numero` | string | Sí | Número (serie+numero ≤ 60 chars) |
| `fecha_expedicion` | string `DD-MM-YYYY` | Sí | Fecha de emisión |
| `fecha_operacion` | string `DD-MM-YYYY` | No | Fecha de la operación (si difiere) |
| `tipo_factura` | string | Sí | Ver tabla de tipos |
| `descripcion` | string | Sí | Descripción de la operación |
| `nif` | string | Sí* | NIF/CIF del destinatario (no en F2) |
| `nombre` | string | Sí* | Nombre del destinatario (no en F2) |
| `id_otro` | object | No | Destinatario extranjero sin NIF español |
| `lineas` | array | Sí | Máximo 12 líneas de desglose IVA |
| `importe_total` | string | Sí | Total de la factura |

**Campos de línea (`lineas[]`):**

| Campo | Tipo | Descripción |
|---|---|---|
| `base_imponible` | string | Base imponible del tramo |
| `tipo_impositivo` | string | Tipo de IVA (%) |
| `cuota_repercutida` | string | Cuota de IVA calculada |
| `operacion_exenta` | string | Código exención: E1, E2, E5 |
| `calificacion_operacion` | string | S1, S2, N1, N2 |
| `clave_regimen` | string | Régimen especial: 02, 03, 17, 18 |
| `impuesto` | string | 02=IPSI, 03=IGIC |
| `tipo_recargo_equivalencia` | string | Recargo de equivalencia |

**Tipos de factura:**

| Código | Tipo |
|---|---|
| `F1` | Factura normal (con datos destinatario) |
| `F2` | Factura simplificada (sin NIF destinatario) |
| `F3` | Factura sustitutiva |
| `R1`–`R5` | Facturas rectificativas |

**Respuesta exitosa (200):**
Incluye código QR en Base64, UUID del registro, huella de encadenamiento y estado de envío a la AEAT.

---

### `POST /verifactu/create_bulk`
Registra múltiples facturas en una sola llamada. Body: array de objetos con la misma estructura que `create`.

```json
[
  { "serie": "B", "numero": "1", ... },
  { "serie": "B", "numero": "2", ... }
]
```

---

### `PUT /verifactu/modify`
Subsana (corrige) un registro ya enviado a la AEAT que fue rechazado o tiene errores.

```json
{
  "rechazo_previo": "N",
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025",
  "tipo_factura": "F1",
  "descripcion": "Prestación de servicios",
  "nif": "A15022510",
  "nombre": "Empresa de prueba SL",
  "lineas": [
    { "base_imponible": "200", "tipo_impositivo": "21", "cuota_repercutida": "42" }
  ],
  "importe_total": "242.00"
}
```

---

### `POST /verifactu/cancel`
Anula una factura ya registrada.

```json
{
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025"
}
```

---

### `POST /verifactu/status`
Consulta el estado de una factura por serie/número/fecha.

```json
{
  "serie": "A",
  "numero": "1",
  "fecha_expedicion": "05-06-2025"
}
```

---

### `GET /verifactu/status?uuid={uuid}`
Consulta el estado de un registro por su UUID.

```
GET /verifactu/status?uuid=b018ced3-b362-4494-8776-9eefff1c160c
```

**Estados de registro en AEAT:**
- `PENDIENTE` — en proceso de remisión
- `CORRECTO` — aceptado por la AEAT
- `RECHAZADO` / `CON_ERRORES` — requiere subsanación o rectificativa

---

### `POST /verifactu/list`
Lista facturas registradas con filtros.

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
Descarga el XML generado para una factura concreta.

```json
{
  "serie": "A",
  "numero": "1"
}
```

---

### `POST /verifactu/export`
Exporta todos los XMLs de un ejercicio/periodo en un ZIP.

```json
{
  "ejercicio": "2025",
  "periodo": "06"
}
```

---

### `GET /verifactu/declaracion`
Descarga la declaración del NIF registrado.

---

## Códigos HTTP

| Código | Significado |
|---|---|
| 200 | Operación exitosa |
| 400 | Error de validación (formato, autenticación, sintaxis) |
| 429 | Límite de llamadas por minuto superado |
| 500 | Error interno de Verifacti |

---

## Notas de Integración

- **No se pueden borrar registros** — solo anular (`cancel`) o subsanar (`modify`)
- **No se pueden modificar facturas** — solo rectificar (factura rectificativa Rx)
- La AEAT puede tardar 100–200 segundos en responder (throttling administrativo)
- Límite estándar: 3.000 facturas/NIF/mes
- No se requieren certificados propios — Verifacti usa certificados de representación
- Respuesta de `create` incluye QR (Base64) que debe imprimirse en la factura PDF
- Comunicación TLS 1.2+
