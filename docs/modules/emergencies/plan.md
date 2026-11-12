# Emergencies — Plan de Implementación

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Tests: [`tests.md`](tests.md)

## 1. Dependencias

- `auth-session`: rol, comunidad, sesión y credibilidad.
- `directory`: comunidad, polígono/radio y directorio de guardias/encargados.
- PostgreSQL + PostGIS.
- Cloudflare R2 para evidencias.
- Firebase Cloud Messaging y WhatsApp Cloud API.
- Contrato global de sync e idempotencia.

## 2. Entregables

### Domain

- `EmergencyAlert`, `Incident`, `AlertEvidence`, `NotificationDelivery`.
- Máquina de estados de alerta e incidente.
- `EmergencyRoutingPolicy`: credibilidad, severidad, comunidad y aprobación.
- Interfaces de repositorio, notificación y almacenamiento.

### Application

- DTOs de creación, aprobación, ack, resolución, cierre, incidente y evidencia.
- `EmergencyApplicationService`, `IncidentApplicationService`.
- Validadores de coordenadas, polígono, transición y permiso.
- Orquestación de R2, FCM, WhatsApp, idempotencia y auditoría.
- Casos de reintento y consolidación de entregas.

### Infrastructure

- Repositorios Dapper y PostGIS.
- Cliente S3/R2 y generación de URLs prefirmadas.
- Adaptador FCM y WhatsApp Cloud API.
- Tabla `notification_delivery` y worker de reintentos.
- Integración con `logs_eventos`.

### Api

- Endpoints `/api/v1/alerts` y `/api/v1/incidents`.
- Middleware de permiso por comunidad y canal.
- Upload de evidencia en dos fases: URL prefirmada + confirmación.
- Rate limiting de creación y re-notificación.

### Clientes

- `mobile-kmp`: mapa, ubicación, cámara, compresión de imagen, outbox, indicadores `pending_sync`, FCM.
- `admin-web`: cola de aprobación, detalle de alerta, supervisión de incidentes y auditoría.

## 3. Secuencia TDD

1. **Red**: routing por credibilidad, workflow de alerta e incidente.
2. **Green**: dominio y políticas puras.
3. **Red**: Application con repositorios y servicios externos dobles.
4. **Green**: orquestación y estados de notificación.
5. **Red**: PostGIS, R2 y adaptadores externos.
6. **Green**: Infrastructure.
7. **Red**: API y contratos.
8. **Green**: endpoints y workers.
9. **Refactor** y pruebas de resiliencia.

## 4. Tareas

| # | Tarea | Estimación |
|---:|---|---:|
| 1 | Modelo y máquina de estados. | 1.0 d |
| 2 | Routing y aprobación. | 1.0 d |
| 3 | CRUD operacional y “En Camino”. | 1.0 d |
| 4 | Incidentes y transiciones. | 0.75 d |
| 5 | R2 + compresión/validación de evidencia. | 1.0 d |
| 6 | FCM + WhatsApp + reintentos. | 1.5 d |
| 7 | Outbox móvil y pull delta. | 1.5 d |
| 8 | Admin Vue de emergencias. | 1.0 d |
| 9 | Tests de integración y caos controlado. | 1.25 d |

## 5. Migraciones

- `emergency_alerts`, `alert_evidence`, `alert_recipients`, `incidents`, `incident_updates`, `notification_deliveries`.
- Índices espaciales GIST para ubicación y polígono de comunidad.
- Índices por `community_id`, `status`, `severity`, `created_at` y `assigned_to`.
- Restricción de integridad alerta–incidente y comunidad.
- Backfill de alertas existentes: `Closed` si hay resolución; `Reported` si no hay evidencia de workflow.
- Datos maestros de tipos y severidades.

## 6. Estrategia de pruebas

- Unitarias: routing, transiciones, permisos, cálculo de visibilidad.
- Integración: PostGIS, R2 y proveedores simulados.
- Contrato: OpenAPI y estados de notificación.
- Resiliencia: caída de red, reintentos, proceso muerto durante `in_flight`.
- Carga moderada: creación de alerta crítica y fan-out a guardias.
- Cobertura mínima global 40%; dominio ≥ 80%.

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| Notificación externa confirmada como atención. | Separar `alert.status` de `notification_delivery`; UI muestra ambos. |
| Duplicación de alertas por doble toque. | UUID de cliente + idempotencia + deduplicación por ventana temporal. |
| Ubicación imprecisa en interior. | `location_accuracy_m`; mostrar advertencia y permitir ajuste manual. |
| WhatsApp/R2 caídos. | Cola de reintentos, métricas, fallback y runbook. |
| Guardia fuera de zona recibe alerta. | Validación de comunidad/polígono y suscripción por rol/condominio. |
| Acuse de “En Camino” en carrera. | Transacción con `FOR UPDATE`; un solo ack ganador. |

## 8. Criterios de aceptación

- Una alerta creada offline se confirma, rechaza o reconcilia sin duplicarse.
- La política de credibilidad 4–5 vs. 1–3 está cubierta.
- “En Camino” solo lo ejecuta un Guardia.
- Una alerta crítica dispara push y WhatsApp con estados observables.
- Ni el fallo de notificación ni la caída del outbox borran la alerta.
- Las coordenadas exactas no se exponen a roles no autorizados.
- La auditoría cubre todo el ciclo de vida.