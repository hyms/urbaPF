# Módulo: Alerts (Safety - Emergencias/Incidentes)

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

### Backend (NUnit + Moq + FluentAssertions)

#### AlertDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AL-01 | Crear alerta con coordenadas | latitud + longitud válidas | CreateAlert | estado Reportado, evento `ALERT_CREATED` |
| AL-02 | Alerta sin coordenadas | latitud o longitud nula | CreateAlert | rechazo 400 |
| AL-03 | Nivel >= 4 directo a Guardias | usuario credibilidad 4/5 | CreateAlert | estado Reportado, visible a Guardias directamente |
| AL-04 | Nivel < 4 requiere aprobación | usuario credibilidad 1-3 | CreateAlert | estado pendiente, requiere `ALERT_APPROVED` |
| AL-05 | Manager aprueba | alerta pendiente | ApproveAlert | estado Reportado, evento `ALERT_APPROVED` |
| AL-06 | Guardia reconoce | Guardia + alerta Reportada | AcknowledgeAlert | estado En Proceso, evento `ALERT_ACKNOWLEDGED` |

#### Resolve/Close/ReNotify tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AL-07 | Resolver alerta | alerta En Proceso | ResolveAlert | estado Resuelto, evento `ALERT_RESOLVED` |
| AL-08 | Cerrar alerta | alerta Resuelta | CloseAlert | estado Cerrado |
| AL-09 | Re-notificar | alerta sin respuesta | ReNotifyAlert | re-envío a vecinos |

#### AlertApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AL-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **AL-F-01**: Pantalla de crear alerta con mapa (geolocalización obligatoria).
- **AL-F-02**: Pantalla de guardia con botón "En Camino".
- **AL-F-03**: Flujo de aprobación de alerta por Manager (nivel < 4).
- **AL-F-04**: Re-notificación de alerta a vecinos.

### Coverage Target
- Backend: >= 40% en servicios de seguridad.
- Frontend: >= 40% en pantallas y flujos de alertas.
