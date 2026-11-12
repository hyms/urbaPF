# Directory — Especificación

- Versión: 1.0.0
- Estado: propuesta para revisión
- Spec global: [`docs/global-spec.md`](../../global-spec.md)

## 1. Propósito

`directory` modela la estructura física y social de la comunidad: condominio, manzanos/lotes, fichas de residentes, roles vecinales y contactos visibles. Es la fuente administrativa para saber quién vive dónde y a quién contactar, respetando visibilidad y minimización de datos.

## 2. Actores y canales

| Actor | Canal | Capacidad |
|---|---|---|
| Vecino | `MobileApp` | Ver directorio autorizado; actualizar campos propios permitidos. |
| Guardia | `MobileApp` | Ver directorio operacional y contactos de emergencia autorizados. |
| Encargado | `WebAdmin` | Validar y administrar fichas, asignar lotes y roles vecinales. |
| Admin | `WebAdmin` | CRUD completo de comunidad, lotes y fichas. |
| Visitante | — | Sin acceso. |

## 3. Modelo de dominio

| Entidad | Campos relevantes |
|---|---|
| `Community` | `id`, `name`, `admin_user_id`, `timezone`, `boundary`, `status`, `version` |
| `Lot` | `id`, `community_id`, `block`, `number`, `lot_type`, `status`, `version` |
| `DirectoryEntry` | `id`, `community_id`, `user_id?`, `lot_id?`, `display_name`, `entry_type`, `visibility`, `status`, `version`, `deleted_at` |
| `DirectoryContact` | `id`, `entry_id`, `channel`, `value`, `value_encrypted`, `visibility`, `verified_at` |
| `CommunityRoleAssignment` | `id`, `entry_id`, `role`, `valid_from`, `valid_to`, `assigned_by` |
| `DirectoryImportBatch` | `id`, `community_id`, `status`, `created_by`, `error_report_key`, `version` |

`DirectoryEntry.entry_type` admite `Resident`, `Guard`, `Manager`, `Household`, `Contact`.
`visibility` admite `Community`, `Managers`, `Guards`, `Private`.
`status` admite `Draft`, `Active`, `Suspended`, `Deleted`.

## 4. Reglas de negocio

### 4.1 Comunidad y lotes

1. Toda ficha pertenece a una única comunidad.
2. El lote se identifica de forma única por `(community_id, block, number)`.
3. Un lote no puede estar activo en dos comunidades.
4. Solo un Admin crea o desactiva comunidades.
5. Un Admin/Encargado crea y desactiva lotes; la desactivación es lógica si existen fichas o expensas asociadas.
6. La geometría de la comunidad se usa para validar ubicaciones de emergencia y reportes.

### 4.2 Fichas y contactos

1. Una ficha activada requiere `display_name`, `entry_type` y comunidad.
2. Un usuario autenticado puede tener una ficha principal; no se permiten dos fichas activadas para el mismo `user_id`.
3. Un Encargado/Admin puede crear fichas para residentes sin cuenta.
4. Un contacto pertenece a una ficha y tiene un `channel` (`Phone`, `Email`, `WhatsApp`, `Internal`).
5. Los números y correos se persisten cifrados en reposo cuando están marcados como privados; la API devuelve una versión enmascarada salvo permiso.
6. La visibilidad se evalúa por rol y relación:
   - `Private`: solo la ficha y Admin.
   - `Managers`: Encargado/Admin.
   - `Guards`: Guardias/Encargados/Admin en la misma comunidad.
   - `Community`: vecinos autenticados de la comunidad.
7. Un usuario puede actualizar únicamente su propio nombre visible y contactos marcados como editables; cualquier cambio administrativo es auditado.
8. La eliminación es lógica (`deleted_at`); nunca se borran fichas con historial de votación, incidentes o expensas.
9. La desactivación de una ficha no elimina su usuario de autenticación.

### 4.3 Roles vecinales

1. `CommunityRoleAssignment` representa roles operativos (`Guard`, `Manager`, `Committee`) distintos del rol global de autenticación.
2. Solo Admin asigna o revoca roles; Encargado puede proponer cambios.
3. Un rol no puede expirar antes de `valid_from`.
4. La asignación genera evento y auditoría.

### 4.4 Importación

1. El Admin puede importar CSV/JSON de fichas.
2. La importación es un proceso por lotes, no una mutación masiva síncrona.
3. Un lote tiene estados `Validating`, `Committed`, `Failed`, `Cancelled`.
4. No se confirma parcialmente una importación fallida; se devuelve un reporte de errores.
5. La importación nunca crea usuarios con contraseña.

## 5. Casos de uso

| ID | Caso | Actor | Resultado |
|---|---|---|---|
| DR-01 | Crear condominio | Admin | `CONDOMINIUM_CREATED`. |
| DR-02 | Crear manzano/lote | Admin/Encargado | `LOT_CREATED`. |
| DR-03 | Crear ficha | Admin/Encargado | `DIRECTORY_ENTRY_CREATED`. |
| DR-04 | Actualizar ficha | Admin/Encargado/propietario | `DIRECTORY_ENTRY_UPDATED`. |
| DR-05 | Desactivar ficha | Admin | Soft delete y evento. |
| DR-06 | Actualizar contacto propio | Vecino | Cambio visible y auditado. |
| DR-07 | Ver directorio | Vecino/Guardia | Solo campos autorizados. |
| DR-08 | Asignar rol vecinal | Admin | `ROLE_ASSIGNED`. |
| DR-09 | Importar fichas | Admin | Lote procesado con reporte. |
| DR-10 | Sincronizar directorio | Móvil | Delta incremental aplicado localmente. |

## 6. Contrato API

| Método | Ruta | Permiso | Propósito |
|---|---|---|---|
| GET | `/communities/{id}` | Autenticado de la comunidad | Estructura y lotes visibles. |
| POST | `/communities` | Admin | Crear comunidad. |
| POST | `/communities/{id}/lots` | Admin/Encargado | Crear lote. |
| PATCH | `/lots/{id}` | Admin/Encargado | Actualizar/desactivar lote. |
| GET | `/directory` | Autenticado de la comunidad | Listado paginado y filtrable. |
| POST | `/directory` | Admin/Encargado | Crear ficha. |
| GET | `/directory/{id}` | Según visibilidad | Detalle. |
| PATCH | `/directory/{id}` | Propietario/Admin | Actualizar ficha. |
| DELETE | `/directory/{id}` | Admin | Soft delete. |
| POST | `/directory/import` | Admin | Crear lote de importación. |
| GET | `/directory/import/{id}` | Admin | Estado y reporte. |

Reglas:

- `GET /directory` nunca devuelve una colección completa sin paginación.
- Los contactos privados se devuelven enmascarados por defecto.
- Las respuestas incluyen `visibility` y `version`.
- Las mutaciones administrativas usan `X-Idempotency-Key` cuando son POST/PATCH/DELETE.

## 7. Sincronización

Colecciones delta:

| Colección | Contenido |
|---|---|
| `directory.community` | Comunidad, polígono, zona horaria. |
| `directory.lots` | Lotes activos y tombstones. |
| `directory.entries` | Fichas autorizadas según rol del usuario. |
| `directory.contacts` | Contactos visibles; los privados nunca se envían a otro usuario. |

Reglas:

- El móvil es principalmente consumidor; la edición de perfil propio se encola como mutación `directory.entry.update`.
- El watermark avanza solo tras persistir todos los lotes.
- Un usuario que pierde acceso deja de recibir entradas en el siguiente delta; el cliente puede conservar caché cifrada y purgarla al cerrar sesión.
- Las fichas eliminadas lógicamente se envían como tombstones.
- La API filtra por comunidad y visibilidad antes de serializar; no confía en filtros del cliente.

## 8. Eventos de auditoría

| Evento | Payload mínimo |
|---|---|
| `CONDOMINIUM_CREATED` | `community_id`, `name`, `admin_user_id`. |
| `LOT_CREATED` | `lot_id`, `community_id`, `block`, `number`. |
| `DIRECTORY_ENTRY_CREATED` | `entry_id`, `community_id`, `entry_type`. |
| `DIRECTORY_ENTRY_UPDATED` | `entry_id`, `campos`, `actor_id`. |
| `DIRECTORY_ENTRY_DELETED` | `entry_id`, `actor_id`, `motivo`. |
| `ROLE_ASSIGNED` | `entry_id`, `role`, `valid_from`, `valid_to`. |

## 9. Errores

| HTTP | `errorCode` | Causa |
|---:|---|---|
| 400 | `VALIDATION_ERROR` | Payload o rango inválido. |
| 403 | `COMMUNITY_ACCESS_DENIED` | Usuario de otra comunidad. |
| 403 | `DIRECTORY_FIELD_FORBIDDEN` | Intento de ver/editar campo privado. |
| 404 | `ENTRY_NOT_FOUND` | Ficha inexistente o no visible. |
| 409 | `DUPLICATE_LOT` | Mismo manzano/número. |
| 409 | `DUPLICATE_PRIMARY_ENTRY` | Usuario con otra ficha activa. |
| 409 | `VERSION_CONFLICT` | `baseVersion` desactualizada. |
| 409 | `IMPORT_IN_PROGRESS` | Ya existe un lote en ejecución. |
| 422 | `IMPORT_VALIDATION_FAILED` | El archivo contiene filas inválidas. |

## 10. Edge cases

- Dos lotes con el mismo número y manzano: el segundo es rechazado.
- Usuario sin cuenta con ficha: no puede autenticarse; solo aparece en directorio.
- Ficha eliminada y usuario activo: el usuario conserva acceso; la ficha no reaparece.
- Contacto privado solicitado por guardia: se devuelve enmascarado salvo `Guards` autorizado.
- Comunidad sin polígono: las validaciones geográficas de otros módulos usan el centroide/radio configurado.
- Importación con filas duplicadas: reporte de errores, sin confirmación parcial.
- Cambio de comunidad de un lote: prohibido; crear nuevo lote y migración controlada.

## 11. Fuera de alcance

- Geocodificación masiva de direcciones.
- Chat o mensajería entre vecinos.
- Directorio público abierto a internet.
- Sincronización offline de mutaciones administrativas masivas.

## 12. Trazabilidad

- Global: [`docs/global-spec.md`](../../global-spec.md)
- Plan: [`plan.md`](plan.md)
- Tests: [`tests.md`](tests.md)