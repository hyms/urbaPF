# Módulo: Incidents (Seguridad - Incidentes)

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

### Backend (NUnit + Moq + FluentAssertions)

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
