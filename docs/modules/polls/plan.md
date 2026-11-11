# Módulo: Polls (Governance - Votaciones)

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/Poll.cs`, `PollOption.cs`, `Vote.cs`.
2. Crear `UrbaPF.Domain/Enums/PollStatus.cs` (Draft, Approved, Active, Closed).
3. Crear `UrbaPF.Domain/Interfaces/IPollRepository.cs`, `IVoteRepository.cs`, `IPollOptionRepository.cs`.
4. Crear `UrbaPF.Domain/Services/IPollDomainService.cs` (reglas de negocio de gobernanza).
5. Crear `UrbaPF.Application/DTOs/PollRequestDto.cs`, `PollResponseDto.cs`, `VoteRequestDto.cs`, `VoteResponseDto.cs`, `PollResultDto.cs`.
6. Crear `UrbaPF.Application/Services/PollApplicationService.cs`.
7. Crear `UrbaPF.Infrastructure/Services/VoteSigner.cs` (firma SHA256).
8. Crear `UrbaPF.Infrastructure/Repositories/PollRepository.cs`, `VoteRepository.cs` (Dapper).
9. Crear `backend/Api/Routes/PollRoutes.cs`.
10. Crear `backend/tests/Domain/PollDomainServiceTests.cs` y `backend/tests/Services/PollApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Identity (User, Role) solo por referencia de IDs.
- `UrbaPF.Application` -> `UrbaPF.Domain`
- `UrbaPF.Infrastructure` -> `UrbaPF.Domain`

### Estimación
- ~10 horas de desarrollo + tests.

### Criterios de Aceptación
- Vecino crea votación en Draft, requiere 2 aprobaciones para activar.
- Encargado crea y activa directamente.
- Voto con firma SHA256 válida.
- No se puede votar dos veces ni en votación cerrada.
- Resultados inmutables tras cierre.
- Eventos `POLL_CREATED`, `POLL_APPROVED`, `POLL_ACTIVATED`, `VOTE_CAST`, `POLL_CLOSED` insertados atómicamente.

---

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

#### Backend (NUnit + Moq + FluentAssertions)

#### PollDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-01 | Vecino crea votación | usuario Vecino | CreatePoll | estado Draft, evento `POLL_CREATED` |
| PO-02 | Encargado aprueba | Encargado + votación en Draft | ApprovePoll | contador de aprobaciones +1 |
| PO-03 | Activar sin 2 aprobaciones | vecino con 1 aprobación | ActivatePoll | rechazo, se requieren 2 |
| PO-04 | Encargado activa directo | Encargado + votación en Draft | ActivatePoll | estado Active, evento `POLL_ACTIVATED` |

#### Vote tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-05 | Voto válido | usuario Vecino/Encargado, votación Active | CastVote | voto registrado, firma SHA256, evento `VOTE_CAST` |
| PO-06 | Voto duplicado | usuario ya votó en esa votación | CastVote | rechazo, un voto por usuario |
| PO-07 | Votar en votación cerrada | votación Closed | CastVote | rechazo 400 |
| PO-08 | Admin vota | usuario Admin | CastVote | rechazo 403 |

#### ClosePoll tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-09 | Cerrar votación | votación Active | ClosePoll | estado Closed, resultados congelados, evento `POLL_CLOSED` |
| PO-10 | Editar votación activa | votación Active | UpdatePoll | rechazo (inmutabilidad) |

#### PollApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **PO-F-01**: Pantalla de crear votación (vecino: borrador; encargado: activo directo).
- **PO-F-02**: Flujo de aprobación de votación por Encargado.
- **PO-F-03**: Pantalla de votación con firma SHA256 y un voto por usuario.
- **PO-F-04**: Pantalla de resultados inmutables tras cierre.

### Coverage Target
- Backend: >= 40% en servicios de gobernanza.
- Frontend: >= 40% en pantallas y flujos de votación.
