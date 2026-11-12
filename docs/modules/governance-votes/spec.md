# Governance & Votes — Especificación

- Versión: 1.0.0
- Estado: propuesta para revisión
- Spec global: [`docs/global-spec.md`](../../global-spec.md)

## 1. Propósito

`governance-votes` permite crear, aprobar, activar y cerrar votaciones comunitarias, además de emitir votos verificables. El módulo garantiza elegibilidad, unicidad del voto, inmutabilidad del resultado y auditoría de cada transición.

## 2. Actores y canales

| Actor | Canal | Capacidad |
|---|---|---|
| Vecino | `MobileApp` | Crear propuesta, votar, ver resultados. |
| Guardia | `MobileApp` | Ver votaciones; no vota. |
| Encargado | `WebAdmin`/`MobileApp` | Aprobar propuestas, crear votaciones directas, activar y cerrar. |
| Admin | `WebAdmin` | Gestionar votaciones; no vota. |

## 3. Modelo de dominio

| Entidad | Campos relevantes |
|---|---|
| `Poll` | `id`, `community_id`, `question`, `description`, `status`, `created_by`, `created_at`, `activated_at`, `closes_at`, `closed_at`, `version`, `deleted_at` |
| `PollOption` | `id`, `poll_id`, `label`, `position`, `version` |
| `PollApproval` | `id`, `poll_id`, `approver_id`, `decision`, `comment`, `created_at` |
| `Vote` | `id`, `poll_id`, `option_id`, `user_id`, `signature`, `cast_at`, `client_mutation_id`, `version` |
| `PollResultSnapshot` | `poll_id`, `option_id`, `vote_count`, `calculated_at` |

Estados de `Poll`: `Draft`, `PendingApproval`, `Approved`, `Active`, `Closed`, `Cancelled`.

## 4. Reglas de negocio

### 4.1 Creación

1. Un Vecino puede crear una propuesta en `PendingApproval`.
2. Un Encargado puede crear una votación directamente en `Approved`.
3. Un Admin puede crear o gestionar una votación, pero no puede votar.
4. La pregunta y al menos dos opciones son obligatorias.
5. Las opciones no se repiten dentro de una votación.
6. `closes_at` debe ser futuro al activar.
7. Una propuesta no puede editar opciones después de salir de `Draft`/`PendingApproval` si ya existe un voto o fue activada.

### 4.2 Aprobación

1. Una propuesta vecinal requiere **2 aprobaciones de Encargados distintos**.
2. Un Encargado no puede aprobar su propia propuesta.
3. Rechazar una propuesta requiere motivo y la mueve a `Cancelled`.
4. La aprobación no activa automáticamente la votación; un Encargado o Admin debe activarla.

### 4.3 Votación

1. Solo usuarios con rol `Vecino` o `Encargado` pueden votar.
2. `Guardia`, `Visitante` y `Admin` no pueden votar.
3. Un usuario puede emitir un voto por votación; el índice único `(poll_id, user_id)` es la barrera final.
4. Un voto solo es válido en estado `Active` y antes de `closes_at`.
5. El voto incluye una firma SHA256 calculada con el secreto de comunidad:
   `SHA256(user_id + poll_id + option_id + timestamp + community_secret)`.
6. El secreto de comunidad no se expone al cliente y puede rotarse; los votos conservan la firma con la que fueron emitidos.
7. El servidor recalcula y valida la firma antes de persistir.
8. El resultado es agregado y no permite inferir votos individuales en la API pública.
9. Una votación `Active` o `Closed` es inmutable: no se editan pregunta, opciones ni resultados.
10. El cierre congela resultados y emite `POLL_RESULT_FINALIZED`.

### 4.4 Creación offline

1. La app móvil puede redactar una propuesta offline y encolarla como `poll.create`.
2. El voto offline se encola como `poll.vote` con `client_mutation_id` y `baseVersion` de la votación.
3. Al sincronizar, el servidor revalida rol, estado, cierre y unicidad.
4. Si la votación cerró antes del sync, el voto pasa a `rejected` con `POLL_CLOSED`; el recibo local se conserva para trazabilidad.

## 5. Casos de uso

| ID | Caso | Actor | Resultado |
|---|---|---|---|
| GV-01 | Crear propuesta vecinal | Vecino | `PendingApproval` y `POLL_CREATED`. |
| GV-02 | Crear votación directa | Encargado | `Approved` y `POLL_CREATED`. |
| GV-03 | Aprobar propuesta | Encargado | Primer voto; sigue pendiente. |
| GV-04 | Segunda aprobación | Encargado | `Approved`. |
| GV-05 | Rechazar propuesta | Encargado | `Cancelled` con motivo. |
| GV-06 | Activar votación | Encargado/Admin | `Active`, `activated_at` y `POLL_ACTIVATED`. |
| GV-07 | Votar online | Vecino/Encargado | `VOTE_CAST`, recibo y resultado actualizado. |
| GV-08 | Votar offline y sincronizar | Vecino | `applied` o `POLL_CLOSED` según estado. |
| GV-09 | Cerrar votación | Encargado/Admin | `Closed`, snapshot y `POLL_RESULT_FINALIZED`. |
| GV-10 | Ver resultados | Cualquier rol autorizado | Conteo agregado y estado. |

## 6. Contrato API

| Método | Ruta | Permiso | Propósito |
|---|---|---|---|
| GET | `/polls` | Autenticado | Listar votaciones visibles. |
| POST | `/polls` | Vecino/Encargado/Admin | Crear propuesta o votación directa. |
| GET | `/polls/{id}` | Autenticado | Detalle con opciones y estado. |
| PATCH | `/polls/{id}` | Autor/Admin | Editar solo en `Draft`/`PendingApproval`. |
| POST | `/polls/{id}/approvals` | Encargado/Admin | Aprobar o rechazar. |
| POST | `/polls/{id}/activate` | Encargado/Admin | Activar. |
| POST | `/polls/{id}/votes` | Vecino/Encargado | Emitir voto; requiere idempotencia. |
| POST | `/polls/{id}/close` | Encargado/Admin | Cerrar y congelar resultado. |
| GET | `/polls/{id}/results` | Autenticado | Resultado agregado. |

Reglas de headers:

- `POST /polls/{id}/votes` exige `X-Idempotency-Key`.
- El voto offline incluye `clientMutationId`; se mapea a la idempotency key.
- La API devuelve `202` si el voto queda en proceso asíncrono; el cliente debe confirmar por GET o respuesta final. En v1.0.0 la escritura es síncrona y devuelve `201`.

## 7. Sincronización

Colecciones delta relevantes:

| Colección | Contenido | Permiso |
|---|---|---|
| `governance.polls` | Polls visibles, opciones, estado y resultados agregados. | Autenticado. |
| `governance.myVotes` | Votos del usuario actual para evitar reintentos. | Autenticado. |
| `governance.deleted` | Tombstones de votaciones eliminadas lógicamente. | Autenticado. |

Reglas:

- El cliente no descarga el padrón completo de votantes.
- El watermark avanza solo después de persistir todos los lotes.
- Un voto pendiente se reconcilia por `clientMutationId`.
- La eliminación lógica de una votación es una eliminación de directorio de gobierno, no de auditoría.
- Si el usuario pierde elegibilidad entre la redacción y el sync, la API rechaza el voto y el cliente muestra el motivo.

## 8. Eventos de auditoría

| Evento | Payload mínimo |
|---|---|
| `POLL_CREATED` | `poll_id`, `created_by`, `requires_approval`. |
| `POLL_APPROVED` | `poll_id`, `approver_id`, `approval_count`. |
| `POLL_ACTIVATED` | `poll_id`, `activated_by`, `closes_at`. |
| `VOTE_CAST` | `poll_id`, `user_id`, `option_id`, `signature_prefix`; nunca la firma completa si es secreta. |
| `POLL_CLOSED` | `poll_id`, `closed_by`, `vote_count`. |
| `POLL_RESULT_FINALIZED` | `poll_id`, `snapshot_hash`, `total_votes`. |

## 9. Errores

| HTTP | `errorCode` | Causa |
|---:|---|---|
| 400 | `VALIDATION_ERROR` | Pregunta, opciones o fecha inválida. |
| 403 | `POLL_CREATION_FORBIDDEN` | Actor sin permiso. |
| 403 | `VOTE_FORBIDDEN` | Rol no habilitado para votar. |
| 404 | `POLL_NOT_FOUND` | Encuesta inexistente o eliminada. |
| 409 | `POLL_ALREADY_APPROVED` | El usuario ya aprobó. |
| 409 | `POLL_NOT_PENDING` | Transición inválida. |
| 409 | `POLL_CLOSED` | Voto posterior al cierre. |
| 409 | `DUPLICATE_VOTE` | Ya existe voto confirmado del usuario. |
| 409 | `VERSION_CONFLICT` | `baseVersion` desactualizada. |
| 409 | `IDEMPOTENCY_KEY_REUSED` | Clave repetida con payload distinto. |
| 422 | `SIGNATURE_INVALID` | Firma recalculada no coincide. |

## 10. Edge cases

- Empate: se declara empate; no hay desempate oculto.
- Cierre exacto en el límite: se usa reloj del servidor; `closes_at` es inclusivo hasta el segundo anterior.
- Usuario pierde rol de Vecino después de abrir la app pero antes de sincronizar: el servidor rechaza.
- Dos dispositivos del mismo usuario intentan votar: la idempotencia y el índice único dejan un solo voto.
- Reintento de POST de voto: devuelve el mismo recibo.
- Propuesta editada mientras está pendiente: solo se permiten cambios de texto; no se cambia autor.
- Votación cancelada: no acepta votos ni activación.

## 11. Fuera de alcance

- Voto secreto con criptografía avanzada o zero-knowledge.
- Votación multi-comunidad.
- Delegación de voto.
- Resultados en tiempo real por WebSocket.

## 12. Trazabilidad

- Global: [`docs/global-spec.md`](../../global-spec.md)
- Plan: [`plan.md`](plan.md)
- Tests: [`tests.md`](tests.md)