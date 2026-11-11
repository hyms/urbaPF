# Módulo: Polls (Governance - Votaciones)

## Especificación (spec.md)

### Propósito
Gestionar el ciclo de vida de votaciones vecinales: creación, aprobación, activación, voto con firma digital SHA256, cierre y resultados inmutables.

### Bounded Context
Governance

### Entidades y Value Objects
- `Poll`: votación (uuid, titulo, descripcion, estado, creado_por, aprobado_por, fecha_creacion, fecha_cierre)
- `PollOption`: opción de voto (uuid, poll_id, texto, orden)
- `Vote`: voto emitido (uuid, poll_id, option_id, user_id, firma_sha256, fecha_voto)

### Reglas de Negocio
1. Solo `Vecino` y `Encargado` pueden votar. `Admin` gestiona pero **no vota**.
2. Un `Vecino` que crea una votación requiere **2 aprobaciones de Encargados** antes de activarla.
3. Un `Encargado` crea y activa votación directamente (sin aprobaciones previas).
4. Cada voto lleva firma digital SHA256: `SHA256(UserId + OptionId + Timestamp + Secret)`.
5. Prohibido editar o eliminar votaciones en estado `Active` o `Closed`.
6. Estados de votación: `Draft` → `Approved` → `Active` → `Closed`.
7. Resultados inmutables una vez cerrada.
8. Todo cambio de estado y cada voto debe generar evento inmutable en `logs_eventos`.

### Eventos Obligatorios
- `POLL_CREATED`
- `POLL_APPROVED`
- `POLL_ACTIVATED`
- `VOTE_CAST`
- `POLL_CLOSED`

### Casos de Uso
- `CreatePoll`: crear votación en estado Draft.
- `ApprovePoll`: Encargado aprueba votación de vecino (contador de aprobaciones).
- `ActivatePoll`: pasar a Active (vecino con 2 aprobaciones; encargado directo).
- `CastVote`: emitir voto con firma SHA256.
- `ClosePoll`: cerrar votación y congelar resultados.
- `GetPollResults`: consultar resultados (solo lectura).

### Edge Cases
- Intento de votar dos veces en la misma votación (una opción por usuario).
- Intento de activar votación sin 2 aprobaciones (vecino).
- Intento de votar en votación cerrada o en borrador.
- Admin intentando votar (rechazo 403).

### API Endpoints
- `POST /api/polls`
- `POST /api/polls/{id}/approve`
- `POST /api/polls/{id}/activate`
- `POST /api/polls/{id}/vote`
- `POST /api/polls/{id}/close`
- `GET /api/polls/{id}/results`

---

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

### TDD - Red / Green / Refactor

#### CreatePoll / ApprovePoll / ActivatePoll
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-01 | Vecino crea votación | usuario Vecino | CreatePoll | estado Draft, evento `POLL_CREATED` |
| PO-02 | Encargado aprueba | Encargado + votación en Draft | ApprovePoll | contador de aprobaciones +1 |
| PO-03 | Activar sin 2 aprobaciones | vecino con 1 aprobación | ActivatePoll | rechazo, se requieren 2 |
| PO-04 | Encargado activa directo | Encargado + votación en Draft | ActivatePoll | estado Active, evento `POLL_ACTIVATED` |

#### CastVote
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-05 | Voto válido | usuario Vecino/Encargado, votación Active | CastVote | voto registrado, firma SHA256, evento `VOTE_CAST` |
| PO-06 | Voto duplicado | usuario ya votó en esa votación | CastVote | rechazo, un voto por usuario |
| PO-07 | Votar en votación cerrada | votación Closed | CastVote | rechazo 400 |
| PO-08 | Admin vota | usuario Admin | CastVote | rechazo 403 |

#### ClosePoll
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PO-09 | Cerrar votación | votación Active | ClosePoll | estado Closed, resultados congelados, evento `POLL_CLOSED` |
| PO-10 | Editar votación activa | votación Active | UpdatePoll | rechazo (inmutabilidad) |

### Coverage
- Backend: NUnit + Moq + FluentAssertions, cobertura >= 40%.
- Frontend: Vitest + React Native Testing Library (crear votación, votar, ver resultados).
