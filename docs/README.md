# Documentación — Talleres360

## Estructura

```
docs/
├── architecture/   Diagramas y modelos de dominio
├── database/       Cambios de BD, migraciones y esquemas
├── features/       Planes de implementación de features
└── guides/         Guías de estándares y buenas prácticas
```

---

## architecture/

| Documento | Descripción |
|---|---|
| [trabajo-estado-machine.md](architecture/trabajo-estado-machine.md) | Máquina de estados de `Trabajo`: diagrama, tabla de transiciones, clasificación y ejemplos de código |

---

## database/

| Documento | Descripción |
|---|---|
| [cobros-trabajo.md](database/cobros-trabajo.md) | Esquema y lógica de cobros parciales por trabajo |
| [db-update-legal-tarifas-trabajos.md](database/db-update-legal-tarifas-trabajos.md) | Migración de campos legales, tarifas hora y DetallesTrabajo |

---

## features/

| Documento | Estado | Descripción |
|---|---|---|
| [refactor-unified-document-model.md](features/refactor-unified-document-model.md) | En progreso | Refactor: modelo unificado Presupuesto/Trabajo, 3 servicios SOLID |
| [verifactu-integration.md](features/verifactu-integration.md) | Pendiente | Integración Veri*Factu vía Verifacti (cumplimiento AEAT) |

---

## guides/

| Documento | Descripción |
|---|---|
| [estandares-implementacion-capas.md](guides/estandares-implementacion-capas.md) | Guía de estándares de implementación y blindaje por capas |
