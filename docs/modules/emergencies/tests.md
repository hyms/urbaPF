# Emergencies — Estrategia de Pruebas

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Plan: [`plan.md`](plan.md)

## 1. Alcance

Probar creación, geolocalización, routing, aprobación, reconocimiento, resolución, cierre, incidentes, notificaciones, evidencias y comportamiento offline.

## 2. Escenarios de dominio

| ID | Given | When | Then |
|---|---|---|---|
| EM-D-001 | Creador con credibilidad 4 | Crea alerta | Pasa a `Reported` y notifica guardias. |
| EM-D-002 | Creador con credibilidad 3 | Crea alerta | Pasa a `PendingApproval`. |
| EM-D-003 | Alerta pendiente | Encargado aprueba | Pasa a `Reported`. |
| EM-D-004 | Alerta pendiente | Encargado rechaza | Pasa a `Rejected` con motivo. |
| EM-D-005 | Alerta `Reported` | Guardia marca “En Camino” | Pasa a `Acknowledged`. |
| EM-D-006 | Alerta `Acknowledged` | Segundo guardia marca “En Camino” | Rechaza `ALERT_ALREADY_ACKNOWLEDGED`. |
| EM-D-007 | Alerta sin ubicación | Intenta crear | Rechaza `VALIDATION_ERROR`. |
| EM-D-008 | Ubicación fuera de la comunidad | Intenta crear | Rechaza `LOCATION_OUT_OF_COMMUNITY`. |
| EM-D-009 | Alerta `Closed` | Intenta resolver | Rechaza `INVALID_ALERT_TRANSITION`. |
| EM-D-010 | Incidente `Open` | Intenta cerrar | Rechaza `INVALID_ALERT_TRANSITION`. |
| EM-D-011 | Incidente `Resolved` | Cierra | Pasa a `Closed`. |
| EM-D-012 | Alerta crítica sin evidencia | Se confirma | Alerta válida; evidencia opcional. |

## 3. Escenarios de aplicación

| ID | Escenario | Resultado esperado |
|---|---|---|
| EM-A-001 | Crear alerta online | Persiste, audita y encola notificaciones. |
| EM-A-002 | Fallo de FCM | Alerta permanece; `NOTIFICATION_FAILED` y reintento. |
| EM-A-003 | Fallo de WhatsApp | Reintento con backoff; estado `failed` tras el límite. |
| EM-A-004 | Re-notificación repetida con misma clave | Una sola entrega adicional. |
| EM-A-005 | Re-notificación con otra clave | Nueva entrega permitida si hay cuota. |
| EM-A-006 | Evidencia > límite | `413 EVIDENCE_TOO_LARGE`. |
| EM-A-007 | URL R2 expirada | Cliente solicita una nueva; no reutiliza. |
| EM-A-008 | Alerta de baja credibilidad notificada antes de aprobación | No debe ocurrir; test bloquea el fan-out. |

## 4. Escenarios de API

| ID | Escenario | Resultado esperado |
|---|---|---|
| EM-C-001 | `POST /alerts` sin idempotencia | `400 VALIDATION_ERROR`. |
| EM-C-002 | `POST /alerts/{id}/ack` como Encargado | `403 NOT_GUARD`. |
| EM-C-003 | `POST /alerts/{id}/approve` como Guardia | `403 NOT_ALERT_MANAGER`. |
| EM-C-004 | `GET /alerts/{id}` de otra comunidad | `404 ALERT_NOT_FOUND`. |
| EM-C-005 | Respuesta de alerta a vecino | Coordenadas aproximadas si no está autorizado. |
| EM-C-006 | Respuesta de alerta a guardia | Coordenadas exactas. |
| EM-C-007 | `POST /incidents` manual sin alerta | `201` con ubicación y comunidad. |
| EM-C-008 | Payload de notificación | No contiene tokens FCM ni número completo. |

## 5. Escenarios offline y sync

| ID | Escenario | Resultado esperado |
|---|---|---|
| EM-S-001 | Crear alerta sin red | Local `pending_sync`; no se muestra como reportada al servidor. |
| EM-S-002 | Recuperar red y sincronizar | Alerta confirmada, notificaciones disparadas. |
| EM-S-003 | Reintento del mismo `mutationId` | Un solo registro confirmado. |
| EM-S-004 | Alerta rechazada por ubicación | Resultado `rejected` con motivo. |
| EM-S-005 | Guardia marca “En Camino” offline | Al sincronizar gana o recibe conflicto si otro guardia ya ack. |
| EM-S-006 | Resolución offline | Al sincronizar pasa a `resolved`; cierre posterior requiere permiso. |
| EM-S-007 | Evidencia subida parcialmente | Alerta confirmada; adjunto queda pendiente y visible. |
| EM-S-008 | Watermark delta repetido | No duplica alertas/incidentes. |

## 6. Resiliencia y seguridad

| ID | Escenario | Resultado esperado |
|---|---|---|
| EM-X-001 | Proceso cae después de enviar push | Al recuperar, no duplica push ya aceptado; usa idempotencia de proveedor. |
| EM-X-002 | 50 alertas críticas simultáneas | Fan-out limitado y cola no bloquea la creación. |
| EM-X-003 | Usuario falsea lat/long | Validación de polígono/radio y auditoría. |
| EM-X-004 | Guardia intenta ver otra comunidad | `404` sin filtrar existencia. |
| EM-X-005 | Token FCM de sesión revocada | No recibe notificaciones. |
| EM-X-006 | Cierre sin resolución | Rechazado por workflow. |

## 7. Cobertura mínima

| Área | Mínimo |
|---|---:|
| Domain | 80% |
| Application | 70% |
| Integración/adaptadores | 60% |
| Global | 40% |

## 8. Criterios de aceptación

- Credibilidad 4–5 y 1–3 siguen rutas distintas y auditadas.
- “En Camino” es exclusivo de Guardia.
- Ninguna notificación externa se presenta como atención confirmada.
- Una alerta offline se reconcilia sin duplicar ni perder efectos.
- Los fallos de proveedores no eliminan la alerta.
- La cadena de auditoría cubre creación, aprobación, atención, resolución y cierre.