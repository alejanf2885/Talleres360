# Guía de Estándares de Implementación y Blindaje de Capas — Talleres360

Este documento define los estándares innegociables de arquitectura para asegurar que cada nueva funcionalidad del SaaS esté blindada contra errores de lógica y fallos de seguridad multi-tenant.

---

## ??? 1. El Blindaje de la Capa de Servicio (Lógica de Negocio)

Todo servicio debe implementar un flujo de "Protección por Pasos" para garantizar la integridad de los datos del taller. No se permite lógica de negocio fuera de esta capa.

### Flujo Obligatorio de Ejecución:
1. **Validación de Identidad (Multi-tenant):** Verificar siempre que el recurso pertenece al `TallerId` obtenido de `IUserContextService`.
2. **Normalización de Entradas:** Limpiar y transformar datos (ej: `Trim()`, `ToLower()`, `ToUpper()`) antes de cualquier consulta o persistencia.
3. **Verificación de Pre-condiciones:** Comprobar la existencia de entidades mediante `AnyAsync` (evitar `CountAsync`) y validar reglas de negocio (ej: no duplicar matrículas o cerrar trabajos ya finalizados).
4. **Respuesta Estandarizada:** El servicio **nunca** debe lanzar excepciones para errores controlados. Debe retornar siempre un `ServiceResult<T>`.

### Ejemplo de Implementación Correcta (Tipado Explícito):

```csharp
public async Task<ServiceResult<Trabajo>> CerrarTrabajoAsync(int tallerId, int trabajoId)
{
    Trabajo? trabajo = await _trabajoRepo.GetByIdAsync(trabajoId);
    if (trabajo == null || trabajo.TallerId != tallerId)
    {
        return ServiceResult<Trabajo>.Fail(ErrorCode.AUTH_ACCESO_DENEGADO.ToString(), "No autorizado.");
    }

    if (trabajo.Estado == "CERRADO")
    {
        return ServiceResult<Trabajo>.Fail(ErrorCode.SYS_OPERACION_INVALIDA.ToString(), "El trabajo ya está cerrado.");
    }

    trabajo.Estado = "CERRADO";
    await _unitOfWork.SaveChangesAsync();

    return ServiceResult<Trabajo>.Ok(trabajo);
}
```

---

## ??? 2. El Contrato del Controller (Capa de Orquestación)

Los controladores deben ser "Thin" (delgados). Su única responsabilidad es recibir la petición, delegar al servicio y traducir el `ServiceResult<T>` a una respuesta HTTP adecuada.

### Estándares de Respuesta:
- **Éxito (HTTP 200/201):** El objeto devuelto debe ser un `ApiResponse<T>.Ok(data, "Mensaje de éxito")`.
- **Error (HTTP 400/403/404):** Si `ServiceResult.Success` es `false`, se debe devolver un `ApiErrorResponse` con el `ErrorCode` oficial y el mensaje descriptivo.
- **Seguridad:** Uso obligatorio de atributos `[TallerAuthorize]` y `[RequiereSuscripcionActiva]` para proteger los recursos.

---

## ?? 3. Reglas de Oro para Desarrolladores

1. **Prohibido el uso de `var`:** El tipado debe ser explícito en toda la solución para mejorar la legibilidad y el mantenimiento.
2. **Sin Excepciones de Flujo:** Las excepciones (`try-catch`) solo se usan para errores críticos no controlados o infraestructura (emails, DB). La lógica de negocio se gestiona con `ServiceResult`.
3. **Eficiencia en Consultas:** Utilizar siempre `AsNoTracking` en lecturas de solo lectura y proyecciones `Select` para no cargar objetos pesados innecesariamente.
4. **Idioma:** Código y base de datos en español (salvo sufijos de arquitectura .NET).

---

**Estado:** Publicado en Scalar.
