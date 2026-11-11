# Módulo: Incidents (Seguridad - Incidentes)

## Especificación (spec.md)

### Propósito
Gestionar incidentes de seguridad vinculados a alertas: creación, seguimiento de estado, asignación a personal y cierre.

### Bounded Context
Safety

### Entidades y Value Objects
- `Incident`: incidente (uuid, alert_id, titulo, descripcion, estado, asignado_a, fecha_creacion, fecha_cierre, prioridad)
- `IncidentType`: tipo de incidente (uuid, nombre, descripcion)

### Reglas de Negocio
1. Un incidente se vincula a una alerta existente (`alert_id`).
2. Estados de incidente: `Abierto` → `En Proceso` → `Resuelto` → `Cerrado`.
3. Un incidente puede ser asignado a un `Guardia` o `Encargado`.
4. Prioridad define urgencia (Alta, Media, Baja).
5. Cierre de incidente requiere estado `Resuelto` previo.
6. Todo cambio de estado de incidente debe generar evento `INCIDENT_STATUS_UPDATED` en `logs_eventos`.

### Eventos Obligatorios
- `INCIDENT_CREATED`
- `INCIDENT_STATUS_UPDATED`

### Casos de Uso
- `CreateIncident`: crear incidente vinculado a alerta.
- `AssignIncident`: asignar incidente a personal (Guardia/Encargado).
- `UpdateIncidentStatus`: cambiar estado de incidente.
- `CloseIncident`: cerrar incidente (requiere Resuelto).
- `GetIncidentsByAlert`: listar incidentes de una alerta.

### Edge Cases
- Incidente sin alerta vinculada (rechazo 400).
- Cerrar incidente sin estar resuelto (rechazo 400).
- Asignar incidente a usuario sin rol operativo (rechazo 403).

### API Endpoints
- `POST /api/incidents`
- `POST /api/incidents/{id}/assign`
- `POST /api/incidents/{id}/status`
- `POST /api/incidents/{id}/close`
- `GET /api/alerts/{alertId}/incidents`

---

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/Incident.cs`, `IncidentType.cs` (si no existen en Alerts).
2. Crear `UrbaPF.Domain/Enums/IncidentStatus.cs`, `IncidentPriority.cs`.
3. Crear `UrbaPF.Domain/Interfaces/IIncidentRepository.cs`, `IIncidentTypeRepository.cs`.
4. Crear `UrbaPF.Domain/Services/IIncidentDomainService.cs`.
5. Crear `UrbaPF.Application/DTOs/IncidentRequestDto.cs`, `IncidentResponseDto.cs`.
6. Crear `UrbaPF.Application/Services/IncidentApplicationService.cs`.
7. Crear `UrbaPF.Infrastructure/Repositories/IncidentRepository.cs` (Dapper).
8. Crear `backend/Api/Routes/IncidentRoutes.cs`.
9. Crear `backend/tests/Domain/IncidentDomainServiceTests.cs` y `backend/tests/Services/IncidentApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Alerts (Alert) y Identity (User) por referencia de IDs.
- `UrbaPF.Application` -> `UrbaPF.Domain`

### Estimación
- ~6 horas de desarrollo + tests.

### Criterios de Aceptación
- Incidente vinculado a alerta existente.
- Estados Abierto → En Proceso → Resuelto → Cerrado.
- Asignación solo a Guardia/Encargado.
- Cierre requiere Resuelto previo.
- Evento `INCIDENT_STATUS_UPDATED` en cada cambio de estado.

---

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

#### Backend (NUnit + Moq + FluentAssertions)

#### IncidentDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| IN-01 | Crear incidente | alerta existente | CreateIncident | incidente Abierto vinculado, evento `INCIDENT_CREATED` |
| IN-02 | Incidente sin alerta | alerta inexistente | CreateIncident | rechazo 400 |
| IN-03 | Asignar a Guardia | Guardia + incidente Abierto | AssignIncident | incidente asignado |
| IN-04 | Asignar a no operativo | usuario sin rol operativo | AssignIncident | rechazo 403 |
| IN-05 | Cambiar estado | incidente Abierto | UpdateIncidentStatus | estado En Proceso, evento `INCIDENT_STATUS_UPDATED` |
| IN-06 | Cerrar sin resolver | incidente Abierto/En Proceso | CloseIncident | rechazo 400 |
| IN-07 | Cerrar resuelto | incidente Resuelto | CloseIncident | estado Cerrado |

#### IncidentApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| IN-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **IN-F-01**: Pantalla de creación de incidente desde alerta.
- **IN-F-02**: Asignación de incidente a Guardia/Encargado.
- **IN-F-03**: Cambio de estado de incidente.
- **IN-F-04**: Listado de incidentes por alerta.

### Coverage Target
- Backend: >= 40% en servicios de incidentes.
- Frontend: >= 40% en pantallas de incidentes.
