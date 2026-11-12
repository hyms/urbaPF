# Governance & Votes — Estrategia de Pruebas

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Plan: [`plan.md`](plan.md)

## 1. Alcance

Probar creación, aprobación, activación, emisión de voto, unicidad, firma, inmutabilidad, cierre, resultados y sincronización offline.

## 2. Escenarios de dominio

| ID | Given | When | Then |
|---|---|---|---|
| GV-D-001 | Propuesta vecinal sin aprobaciones | Se aprueba una vez | Sigue `PendingApproval`. |
| GV-D-002 | Propuesta con 2 aprobaciones distintas | Se procesa la segunda | Pasa a `Approved`. |
| GV-D-003 | Encargado intenta aprobar su propuesta | Emite aprobación | Rechaza `POLL_CREATION_FORBIDDEN`. |
| GV-D-004 | Votación `Draft` | Se activa | Rechaza `POLL_NOT_APPROVED`. |
| GV-D-005 | Votación `Active` y usuario `Admin` | Vota | Rechaza `VOTE_FORBIDDEN`. |
| GV-D-006 | Votación `Active` y usuario `Vecino` | Vota | Acepta y emite `VOTE_CAST`. |
| GV-D-007 | Votación `Closed` | Vota | Rechaza `POLL_CLOSED`. |
| GV-D-008 | Votación activa sin opciones | Se emite voto | Rechaza `VALIDATION_ERROR`. |
| GV-D-009 | Firma calculada con otro secreto | Verifica voto | Rechaza `SIGNATURE_INVALID`. |

## 3. Escenarios de aplicación

| ID | Escenario | Resultado esperado |
|---|---|---|
| GV-A-001 | Crear propuesta como vecino | Queda pendiente y auditada. |
| GV-A-002 | Crear votación como Encargado | Queda aprobada y auditada. |
| GV-A-003 | Doble aprobación simultánea | Una gana; la otra recibe conflicto o idempotencia. |
| GV-A-004 | Reintentar voto con misma clave y payload | Misma respuesta, un solo voto persistido. |
| GV-A-005 | Reintentar voto con misma clave y otro payload | `409 IDEMPOTENCY_KEY_REUSED`. |
| GV-A-006 | Cerrar votación con votos pendientes de conciliación | Primero concilia; luego congela snapshot. |
| GV-A-007 | Actualizar votación activa | Rechaza por inmutabilidad. |

## 4. Escenarios de API

| ID | Escenario | Resultado esperado |
|---|---|---|
| GV-C-001 | `POST /polls/{id}/votes` sin idempotencia | `400 VALIDATION_ERROR`. |
| GV-C-002 | `GET /polls/{id}/results` como visitante | Resultado agregado sin datos individuales. |
| GV-C-003 | `GET /polls` sin autenticación | `401`. |
| GV-C-004 | `PATCH` de votación `Active` | `409 POLL_NOT_PENDING`. |
| GV-C-005 | Respuesta de voto | Contiene `voteId`, `pollId`, `castAt` y recibo; no secretos. |
| GV-C-006 | Error 422 | `application/problem+json` con `signatureInvalid`. |

## 5. Escenarios de sincronización

| ID | Escenario | Resultado esperado |
|---|---|---|
| GV-S-001 | Voto creado offline | Queda `pending` local; al sincronizar se confirma o rechaza. |
| GV-S-002 | Votación cierra antes del sync | Resultado `rejected` con `POLL_CLOSED`. |
| GV-S-003 | Voto repetido por reinicio de app | Un solo voto; respuesta idempotente. |
| GV-S-004 | Delta paginado | Watermark avanza solo con `hasMore=false`. |
| GV-S-005 | Cambio de rol mientras hay voto pendiente | Servidor revalida y rechaza si ya no es elegible. |
| GV-S-006 | Resultado cacheado obsoleto | Delta posterior reemplaza snapshot y contadores. |

## 6. Escenarios de concurrencia y seguridad

| ID | Escenario | Resultado esperado |
|---|---|---|
| GV-X-001 | Dos dispositivos emiten el mismo voto UUID | Un voto confirmado; el otro es idempotente. |
| GV-X-002 | Dos dispositivos emiten votos distintos | Un voto confirmado; el otro recibe `DUPLICATE_VOTE`. |
| GV-X-003 | Voto manipulado con `option_id` inexistente | `VALIDATION_ERROR`. |
| GV-X-004 | Firma enviada desde comunidad incorrecta | `SIGNATURE_INVALID` o `403`. |
| GV-X-005 | Listar votantes | No existe endpoint público; cualquier intento es `403/404`. |
| GV-X-006 | Votación eliminada lógicamente | No aparece en sync activo; tombstone sí. |

## 7. Cobertura mínima

| Área | Mínimo |
|---|---:|
| Domain | 80% |
| Application | 70% |
| API | 60% |
| Global | 40% |

## 8. Criterios de aceptación

- La máquina de estados no permite saltos inválidos.
- La unicidad del voto está garantizada por dominio, transacción e índice.
- Los reintentos no duplican votos.
- Los resultados son agregados e inmutables tras el cierre.
- La auditoría cubre creación, aprobación, activación, voto, cierre y finalización.