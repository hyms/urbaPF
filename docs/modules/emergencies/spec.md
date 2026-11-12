# Emergencies — Especificación

- Versión: 1.0.0
- Estado: propuesta para revisión
- Spec global: [`docs/global-spec.md`](../../global-spec.md)

## 1. Propósito

`emergencies` gestiona alertas geolocalizadas, enrutamiento por credibilidad, reconocimiento por guardias, resolución, cierre e incidentes asociados. También define la entrega asíncrona por FCM y WhatsApp Cloud API sin convertir la notificación en una confirmación de atención.

## 2. Actores y canales

| Actor | Canal | Capacidad |
|---|---|---|
| Vecino | `MobileApp` | Crear alerta, re-notificar, ver estado. |
| Guardia | `MobileApp` | Recibir alertas, marcar “En Camino”, actualizar incidente. |
| Encargado | `WebAdmin`/`MobileApp` | Aprobar alertas de baja credibilidad, supervisar, cerrar. |
| Admin | `WebAdmin` | Supervisión y auditoría; no opera el campo. |
| Visitante | — | No puede crear alertas. |

## 3. Modelo de dominio

| Entidad | Campos relevantes |
|---|---|
| `EmergencyAlert` | `id`, `community_id`, `type`, `severity`, `status`, `description`, `latitude`, `longitude`, `location_accuracy_m`, `created_by`, `requires_approval`, `version`, `deleted_at` |
| `AlertEvidence` | `id`, `alert_id`, `r2_object_key`, `mime_type`, `size_bytes`, `compressed_in_client`, `created_at` |
| `AlertRecipient` | `id`, `alert_id`, `role`, `user_id`, `channel`, `delivered_at` |
| `Incident` | `id`, `alert_id`, `status`, `priority`, `assigned_to`, `opened_at`, `resolved_at`, `closed_at`, `version` |
| `IncidentUpdate` | `id`, `incident_id`, `actor_id`, `from_status`, `to_status`, `note`, `created_at` |
| `NotificationDelivery` | `id`, `alert_id`, `provider`, `recipient_hash`, `status`, `attempts`, `last_error`, `sent_at`, `delivered_at` |

Estados de alerta: `Reported`, `PendingApproval`, `Acknowledged`, `Resolved`, `Closed`, `Rejected`, `Cancelled`.
Estados de incidente: `Open`, `InProgress`, `Resolved`, `Closed`.

## 4. Reglas de negocio

### 4.1 Creación

1. Solo usuarios autenticados con rol `Vecino`, `Encargado` o `Admin` pueden crear alertas; un Guardia puede crear incidencias operativas.
2. `community_id`, `type`, `description`, `latitude` y `longitude` son obligatorios.
3. La geolocalización debe estar dentro de la comunidad o de su polígono; si no hay polígono, se valida el radio configurable.
4. `severity` usa `Low`, `Medium`, `High`, `Critical`.
5. La severidad `Critical` exige confirmación explícita del usuario y evidencia opcional.
6. La creación offline es válida localmente como `pending_sync`, pero no se considera reportada al servidor hasta que el outbox la confirme.

### 4.2 Enrutamiento y aprobación

1. Si la credibilidad del creador es 4 o 5, la alerta pasa a `Reported` y se notifica directamente a guardias.
2. Si la credibilidad es 1–3, la alerta pasa a `PendingApproval` y requiere aprobación de un Encargado/Admin.
3. Un Guardia no puede aprobar una alerta.
4. Aprobar una alerta pendiente la mueve a `Reported` y dispara notificaciones.
5. Rechazarla la mueve a `Rejected`, con motivo, y notifica al creador.
6. Una alerta no puede ser `Acknowledged` antes de estar `Reported`.

### 4.3 Atención operativa

1. Un Guardia marca “En Camino” con `POST /alerts/{id}/ack`; la alerta pasa a `Acknowledged` y se emite `GUARD_EN_ROUTE`.
2. Solo un Guardia autenticado puede marcar “En Camino”.
3. Un Encargado/Admin puede resolver y cerrar; un Guardia puede resolver su intervención y dejar el cierre administrativo al Encargado.
4. `Resolved` no permite volver a `Acknowledged`; cualquier corrección requiere un incidente o una nueva alerta.
5. `Closed` requiere que exista una resolución registrada.
6. La alerta y su incidente comparten comunidad y no pueden pertenecer a comunidades distintas.

### 4.4 Incidentes

1. Toda alerta crítica o de severidad `High` genera un incidente al ser confirmada.
2. Un incidente puede existir sin alerta solo si un Guardia/Admin lo crea manualmente; en ese caso se asocia a una ubicación y comunidad.
3. El incidente pasa por `Open → InProgress → Resolved → Closed`.
4. `Closed` requiere `Resolved`.
5. Toda transición genera `INCIDENT_STATUS_UPDATED` con actor, nota y timestamp.

### 4.5 Notificaciones

1. FCM se usa para push a dispositivos autenticados de guardias/encargados del condominio.
2. WhatsApp Cloud API se usa para alertas `High`/`Critical` o cuando el destinatario no tiene push.
3. La notificación es asíncrona y su estado se persiste en `notification_delivery`.
4. Un fallo de FCM no revierte la alerta; se reintenta y se audita `NOTIFICATION_FAILED`.
5. Un fallo de WhatsApp se reintenta con backoff y límite; si falla definitivamente, el incidente queda marcado para revisión manual.
6. No se notifica a usuarios sin sesión válida ni a tokens desvinculados.

### 4.6 Re-notificación

1. El creador puede re-notificar una alerta `Reported` o `Acknowledged`.
2. La re-notificación requiere `X-Idempotency-Key` y respeta un límite por alerta/hora.
3. No crea una nueva alerta ni reinicia el workflow.

## 5. Casos de uso

| ID | Caso | Actor | Resultado |
|---|---|---|---|
| EM-01 | Crear emergencia | Vecino | Alerta creada y enrutada; evento y notificación. |
| EM-02 | Crear emergencia offline | Vecino | `pending_sync` local; confirmación posterior. |
| EM-03 | Aprobar alerta | Encargado | Pasa de `PendingApproval` a `Reported`. |
| EM-04 | Rechazar alerta | Encargado | `Rejected` con motivo. |
| EM-05 | Marcar “En Camino” | Guardia | `Acknowledged` y `GUARD_EN_ROUTE`. |
| EM-06 | Resolver alerta | Guardia/Encargado | `Resolved`. |
| EM-07 | Cerrar alerta | Encargado/Admin | `Closed` y auditoría. |
| EM-08 | Crear incidente manual | Guardia/Encargado | Incidente `Open`. |
| EM-09 | Actualizar incidente | Asignado | Transición auditada. |
| EM-10 | Reintentar notificación | Sistema | Estado actualizado, sin duplicar alerta. |

## 6. Contrato API

| Método | Ruta | Permiso | Propósito |
|---|---|---|---|
| GET | `/alerts` | Autenticado | Listar alertas visibles/paginadas. |
| POST | `/alerts` | Vecino/Encargado/Admin | Crear alerta; exige idempotencia. |
| GET | `/alerts/{id}` | Autenticado | Detalle y estado. |
| POST | `/alerts/{id}/approve` | Encargado/Admin | Aprobar alerta pendiente. |
| POST | `/alerts/{id}/reject` | Encargado/Admin | Rechazar con motivo. |
| POST | `/alerts/{id}/ack` | Guardia | Marcar “En Camino”. |
| POST | `/alerts/{id}/resolve` | Guardia/Encargado | Resolver. |
| POST | `/alerts/{id}/close` | Encargado/Admin | Cerrar. |
| POST | `/alerts/{id}/renotify` | Creador | Re-notificar; idempotente. |
| POST | `/alerts/{id}/evidence` | Creador | Solicitar URL prefirmada R2. |
| POST | `/incidents` | Guardia/Encargado | Crear incidente manual. |
| GET | `/incidents` | Autenticado | Listar incidentes. |
| PATCH | `/incidents/{id}` | Asignado/Encargado | Actualizar estado/asignación. |

Reglas:

- `X-Idempotency-Key` es obligatoria en POST/PATCH/DELETE que cambian estado.
- La respuesta de creación incluye `syncStatus`, `requiresApproval`, `routedRoles` y `notificationStatus`.
- Las coordenadas exactas solo se exponen a guardias/encargados autorizados; otros roles ven una ubicación aproximada.
- La API no devuelve tokens FCM ni números de WhatsApp completos.

## 7. Sincronización offline

| Colección | Contenido | Regla |
|---|---|---|
| `emergencies.active` | Alertas activas relevantes y su estado. | Pull delta por comunidad. |
| `emergencies.incidents` | Incidentes asignados o visibles. | Pull delta. |
| `emergencies.deleted` | Tombstones. | Purga local controlada. |

Mutaciones permitidas offline:

- Crear alerta.
- Marcar “En Camino” (guardia).
- Resolver alerta.
- Añadir actualización de incidente.
- Re-notificar.

Reglas:

1. La UI distingue `pending_sync` de `confirmed`.
2. La alerta creada offline usa UUID de cliente y `mutation_id` global.
3. Al sincronizar, el servidor puede rechazar por ubicación inválida, permiso, cierre o duplicado.
4. Un fallo de notificación no borra la alerta; se reintenta.
5. El outbox no se limpia al perder la sesión; se pausa y avisa.
6. La evidencia sube a R2 antes de confirmar la mutación si el cliente tiene conectividad; si no, la alerta se confirma sin evidencia y el adjunto queda pendiente.

## 8. Eventos de auditoría

| Evento | Payload mínimo |
|---|---|
| `EMERGENCY_CREATED` | `alert_id`, `type`, `severity`, `requires_approval`, `source=online|offline`. |
| `EMERGENCY_APPROVED` | `alert_id`, `approver_id`. |
| `EMERGENCY_REJECTED` | `alert_id`, `approver_id`, `motivo`. |
| `GUARD_EN_ROUTE` | `alert_id`, `guard_id`, `eta_minutes?`. |
| `EMERGENCY_RESOLVED` | `alert_id`, `actor_id`, `resolucion`. |
| `EMERGENCY_CLOSED` | `alert_id`, `actor_id`. |
| `INCIDENT_CREATED` | `incident_id`, `alert_id?`, `priority`. |
| `INCIDENT_STATUS_UPDATED` | `incident_id`, `from`, `to`, `actor_id`. |
| `NOTIFICATION_FAILED` | `alert_id`, `provider`, `errorCode`; sin PII. |

## 9. Errores

| HTTP | `errorCode` | Causa |
|---:|---|---|
| 400 | `VALIDATION_ERROR` | Coordenadas, severidad o fechas inválidas. |
| 401 | `AUTH_REQUIRED` | Sin token válido. |
| 403 | `ALERT_CREATION_FORBIDDEN` | Rol no permitido. |
| 403 | `NOT_ALERT_MANAGER` | Intento de aprobar sin rol. |
| 403 | `NOT_GUARD` | Intento de “En Camino” por otro rol. |
| 404 | `ALERT_NOT_FOUND` | Alerta inexistente o fuera de comunidad. |
| 409 | `INVALID_ALERT_TRANSITION` | Transición no permitida. |
| 409 | `ALERT_ALREADY_ACKNOWLEDGED` | Ya existe guardia en camino. |
| 409 | `IDEMPOTENCY_KEY_REUSED` | Clave repetida con otro payload. |
| 413 | `EVIDENCE_TOO_LARGE` | Archivo supera el límite. |
| 422 | `LOCATION_OUT_OF_COMMUNITY` | Coordenada fuera del polígono/radio. |
| 429 | `RENOTIFY_RATE_LIMITED` | Límite de re-notificación excedido. |

## 10. Edge cases

- Dos guardias marcan “En Camino” simultáneamente: uno gana; el otro recibe `ALERT_ALREADY_ACKNOWLEDGED` y ve el primer ack.
- Creador pierde credibilidad mientras la alerta está pendiente: se mantiene el enrutamiento calculado al crear; la aprobación se registra con la política vigente.
- Notificación WhatsApp entregada pero FCM falla: el estado consolidado es `delivered`; se registra el fallo del otro proveedor.
- Evidencia corrupta: se rechaza el upload; la alerta puede confirmarse sin adjunto.
- Alerta cancelada por el creador: no notifica a guardias si aún no fue reportada.
- Sin señal al resolver: se encola y se marca `pending_sync`; el estado visible local no debe presentarse como confirmado.
- Zona sin polígono: se usa el centroide y radio configurable de la comunidad.

## 11. Fuera de alcance

- Despacho automático a servicios públicos externos.
- Videollamadas o chat de emergencia.
- Rastreo continuo de ubicación en segundo plano.
- Integración de mapas offline con tiles empaquetados en v1.0.0.

## 12. Trazabilidad

- Global: [`docs/global-spec.md`](../../global-spec.md)
- Plan: [`plan.md`](plan.md)
- Tests: [`tests.md`](tests.md)