# Directory — Estrategia de Pruebas

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Plan: [`plan.md`](plan.md)

## 1. Alcance

Probar comunidad, lotes, fichas, contactos, visibilidad, enmascaramiento, importación, roles vecinales y sincronización.

## 2. Escenarios de dominio

| ID | Given | When | Then |
|---|---|---|---|
| DR-D-001 | Lote con manzano A y número 10 | Se crea otro igual | Rechaza `DUPLICATE_LOT`. |
| DR-D-002 | Usuario con ficha activa | Se crea otra activa | Rechaza `DUPLICATE_PRIMARY_ENTRY`. |
| DR-D-003 | Ficha `Private` | Vecino solicita detalle | Recibe `404` o campos mínimos sin contacto. |
| DR-D-004 | Ficha `Community` | Vecino de la misma comunidad consulta | Recibe campos autorizados. |
| DR-D-005 | Ficha `Guards` | Guardia de otra comunidad consulta | Recibe `404`. |
| DR-D-006 | Ficha con historial de voto | Se elimina | Soft delete; historial permanece. |
| DR-D-007 | Rol con `valid_to` anterior a `valid_from` | Se asigna | Rechaza `VALIDATION_ERROR`. |
| DR-D-008 | Importación con 1 fila inválida | Se confirma | Rechaza todo el lote con reporte. |

## 3. Escenarios de aplicación

| ID | Escenario | Resultado esperado |
|---|---|---|
| DR-A-001 | Crear comunidad | Auditada y visible a Admin. |
| DR-A-002 | Crear lote | Auditado y asociado a comunidad. |
| DR-A-003 | Crear ficha para residente sin cuenta | Ficha activa sin credenciales. |
| DR-A-004 | Actualizar contacto propio | Cambio permitido y auditado. |
| DR-A-005 | Actualizar contacto de otro usuario | `403 DIRECTORY_FIELD_FORBIDDEN`. |
| DR-A-006 | Importación válida | Lote `Committed` y evento. |
| DR-A-007 | Importación duplicada | Lote `Failed` con reporte descargable. |
| DR-A-008 | Petición con `baseVersion` antigua | `409 VERSION_CONFLICT`. |

## 4. Escenarios de API

| ID | Escenario | Resultado esperado |
|---|---|---|
| DR-C-001 | `GET /directory` sin paginación | Debe usar cursor por defecto y límite máximo. |
| DR-C-002 | `GET /directory` como visitante | `401`. |
| DR-C-003 | `GET /directory` de otra comunidad | `403 COMMUNITY_ACCESS_DENIED`. |
| DR-C-004 | Respuesta de contacto privado | Enmascarado salvo permiso explícito. |
| DR-C-005 | `POST /directory/import` sin rol Admin | `403`. |
| DR-C-006 | `POST /directory` con idempotencia repetida | Respuesta original sin duplicar. |
| DR-C-007 | `PATCH` de ficha eliminada | `404 ENTRY_NOT_FOUND`. |
| DR-C-008 | Error de importación | `422` con `importBatchId` y reporte. |

## 5. Escenarios de sincronización

| ID | Escenario | Resultado esperado |
|---|---|---|
| DR-S-001 | Nueva ficha autorizada | Aparece en delta del vecino. |
| DR-S-002 | Ficha cambia a privada | Deja de aparecer en el siguiente delta. |
| DR-S-003 | Ficha eliminada | Tombstone y purga local. |
| DR-S-004 | Delta repetido | No duplica fichas ni contactos. |
| DR-S-005 | Usuario pierde acceso a comunidad | Su caché se purga al sincronizar/logout. |
| DR-S-006 | Perfil propio editado offline | Se encola `directory.entry.update` y se reconcilia. |

## 6. Seguridad y privacidad

| ID | Escenario | Resultado esperado |
|---|---|---|
| DR-SEC-001 | Enumerar IDs de fichas privadas | No filtrable; respuestas consistentes con no existencia. |
| DR-SEC-002 | Buscar por teléfono en API | Búsqueda no expone coincidencias sin permiso. |
| DR-SEC-003 | Contacto privado en log | No aparece valor plano. |
| DR-SEC-004 | Exportar directorio sin rol | `403`. |
| DR-SEC-005 | Caché móvil en dispositivo perdido | Datos cifrados; purga tras logout remoto. |

## 7. Cobertura mínima

| Área | Mínimo |
|---|---:|
| Domain | 80% |
| Application | 70% |
| API/visibilidad | 70% |
| Global | 40% |

## 8. Criterios de aceptación

- La visibilidad se decide en servidor, no en el cliente.
- Los contactos privados nunca se exponen por error de serialización.
- La importación es atómica por lote.
- El delta incluye altas, cambios y borrados sin duplicar.
- La eliminación lógica preserva integridad histórica.