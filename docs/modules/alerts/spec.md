# Módulo: Alerts (Safety - Emergencias/Incidentes)

## Especificación (spec.md)

### Propósito
Gestionar alertas de emergencia con geolocalización: creación, aprobación, reconocimiento, resolución, re-notificación y routing por reputación del usuario.

### Bounded Context
Safety

### Entidades y Value Objects
- `Alert`: alerta de emergencia (uuid, titulo, descripcion, latitud, longitud, estado, creado_por, credibilidad_nivel, fecha_creacion, fecha_resolucion)
- `AlertType`: tipo de alerta (uuid, nombre, descripcion)
- `Incident`: incidente vinculado (uuid, alert_id, estado, asignado_a, fecha_actualizacion)

### Reglas de Negocio
1. Geolocalización obligatoria (latitud + longitud) al crear una alerta.
2. Workflow de estados: `Reportado` → `En Proceso` → `Resuelto` → `Cerrado`.
3. Routing por credibilidad del creador:
   - Nivel >= 4: alerta va **directo a Guardias**.
   - Nivel < 4: alerta requiere **aprobación de Manager** antes de distribuirse.
4. Botón "En Camino" exclusivo de `Guardia`.
5. El creador puede enviar re-notificación a todos los vecinos si no hay respuesta.
6. Todo cambio de estado de alerta/incidente debe generar evento inmutable en `logs_eventos`.

### Eventos Obligatorios
- `ALERT_CREATED`
- `ALERT_APPROVED`
- `ALERT_ACKNOWLEDGED`
- `ALERT_RESOLVED`
- `INCIDENT_CREATED`
- `INCIDENT_STATUS_UPDATED`

### Casos de Uso
- `CreateAlert`: crear alerta con geolocalización (estado Reportado).
- `ApproveAlert`: Manager aprueba alerta de credibilidad < 4.
- `AcknowledgeAlert`: Guardia reconoce la alerta (En Proceso).
- `ResolveAlert`: marcar alerta como Resuelta.
- `CloseAlert`: cerrar alerta.
- `ReNotifyAlert`: re-enviar alerta a vecinos.
- `CreateIncident`: vincular incidente a alerta.
- `UpdateIncidentStatus`: actualizar estado de incidente.

### Edge Cases
- Alerta sin coordenadas (rechazo 400).
- Usuario nivel < 4 sin aprobación de Manager (alerta queda pendiente).
- Guardia intentando acceder al Tablón Social privado (rechazo 403).

### API Endpoints
- `POST /api/alerts`
- `POST /api/alerts/{id}/approve`
- `POST /api/alerts/{id}/acknowledge`
- `POST /api/alerts/{id}/resolve`
- `POST /api/alerts/{id}/close`
- `POST /api/alerts/{id}/renotify`
- `POST /api/incidents`
- `POST /api/incidents/{id}/status`

---

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/Alert.cs`, `Incident.cs`, `AlertType.cs`, `IncidentType.cs`.
2. Crear `UrbaPF.Domain/Enums/AlertStatus.cs` (Reportado, EnProceso, Resuelto, Cerrado).
3. Crear `UrbaPF.Domain/Interfaces/IAlertRepository.cs`, `IIncidentRepository.cs`, `IAlertTypeRepository.cs`.
4. Crear `UrbaPF.Domain/Services/IAlertDomainService.cs` (routing por credibilidad, workflow de estados).
5. Crear `UrbaPF.Application/DTOs/AlertRequestDto.cs`, `AlertResponseDto.cs`, `IncidentRequestDto.cs`, `IncidentResponseDto.cs`.
6. Crear `UrbaPF.Application/Services/AlertApplicationService.cs`.
7. Crear `UrbaPF.Infrastructure/Repositories/AlertRepository.cs`, `IncidentRepository.cs` (Dapper + PostGIS para geolocalización).
8. Crear `backend/Api/Routes/AlertRoutes.cs`, `IncidentRoutes.cs`.
9. Crear `backend/tests/Domain/AlertDomainServiceTests.cs` y `backend/tests/Services/AlertApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Identity (User, CredibilityLevel) por referencia de IDs.
- `UrbaPF.Application` -> `UrbaPF.Domain`

### Estimación
- ~10 horas de desarrollo + tests.

### Criterios de Aceptación
- Alerta requiere latitud + longitud.
- Nivel >= 4 va directo a Guardias; < 4 requiere aprobación de Manager.
- Workflow Reportado → En Proceso → Resuelto → Cerrado.
- Botón "En Camino" solo para Guardia.
- Eventos `ALERT_CREATED`, `ALERT_APPROVED`, `ALERT_ACKNOWLEDGED`, `ALERT_RESOLVED`, `INCIDENT_CREATED`, `INCIDENT_STATUS_UPDATED` insertados atómicamente.

---

## Escenarios de Prueba (tests.md)

### TDD - Red / Green / Refactor

#### CreateAlert / ApproveAlert / AcknowledgeAlert
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AL-01 | Crear alerta con coordenadas | latitud + longitud válidas | CreateAlert | estado Reportado, evento `ALERT_CREATED` |
| AL-02 | Alerta sin coordenadas | latitud o longitud nula | CreateAlert | rechazo 400 |
| AL-03 | Nivel >= 4 directo a Guardias | usuario credibilidad 4/5 | CreateAlert | estado Reportado, visible a Guardias directamente |
| AL-04 | Nivel < 4 requiere aprobación | usuario credibilidad 1-3 | CreateAlert | estado pendiente, requiere `ALERT_APPROVED` |
| AL-05 | Manager aprueba | alerta pendiente | ApproveAlert | estado Reportado, evento `ALERT_APPROVED` |
| AL-06 | Guardia reconoce | Guardia + alerta Reportada | AcknowledgeAlert | estado En Proceso, evento `ALERT_ACKNOWLEDGED` |

#### Resolve/Close/ReNotify
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AL-07 | Resolver alerta | alerta En Proceso | ResolveAlert | estado Resuelto, evento `ALERT_RESOLVED` |
| AL-08 | Cerrar alerta | alerta Resuelta | CloseAlert | estado Cerrado |
| AL-09 | Re-notificar | alerta sin respuesta | ReNotifyAlert | re-envío a vecinos |

### Coverage
- Backend: NUnit + Moq + FluentAssertions, cobertura >= 40%.
- Frontend: Vitest + React Native Testing Library (crear alerta con mapa, flujo de guardia).
