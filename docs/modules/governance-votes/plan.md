# Governance & Votes — Plan de Implementación

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Tests: [`tests.md`](tests.md)

## 1. Dependencias

- Contrato de idempotencia y sync global.
- `auth-session` para rol, comunidad y credibilidad.
- Configuración de secreto de comunidad para firmas.
- Repositorio de auditoría y reloj del servidor.

## 2. Entregables

### Domain

- `Poll`, `PollOption`, `PollApproval`, `Vote`.
- Máquina de estados de `Poll`.
- `PollPolicy`: elegibilidad, aprobaciones, cierre y firma.
- Interfaces de repositorio y eventos de dominio.

### Application

- DTOs de creación, aprobación, voto, resultado y cierre.
- `PollApplicationService` y `VoteApplicationService`.
- Validadores de fecha, opciones, permiso y versión.
- Orquestación idempotente y auditoría.

### Infrastructure

- Repositorios Dapper para polls, opciones, aprobaciones, votos y snapshots.
-Índices únicos y constraints.
- Servicio de firma/verificación.
- Consulta de resultados agregados.
- Integración con `logs_eventos`.

### Api

- Endpoints `/api/v1/polls/*`.
- Middleware de idempotencia y `channel`.
- Errores de negocio con códigos estables.
- Rate limiting para creación y voto.

### Clientes

- `mobile-kmp`: editor de propuesta offline, outbox de voto, caché de resultados.
- `admin-web`: tabla de votaciones, aprobaciones, activación/cierre y detalle de resultados.
- Ambos clientes muestran estado `pending` y no cuentan un voto local como definitivo.

## 3. Secuencia TDD

1. **Red**: máquina de estados, aprobaciones, elegibilidad y firma inválida.
2. **Green**: dominio puro.
3. **Red**: servicios de aplicación con repositorios dobles; idempotencia y conflictos.
4. **Green**: Application.
5. **Red**: consultas Dapper y constraints.
6. **Green**: Infrastructure y migraciones.
7. **Red**: contratos de API y códigos HTTP.
8. **Green**: endpoints.
9. **Refactor** y revisión de cobertura.

## 4. Tareas

| # | Tarea | Estimación |
|---:|---|---:|
| 1 | Máquina de estados y políticas. | 1.0 d |
| 2 | CRUD de borrador y aprobaciones. | 1.0 d |
| 3 | Emisión/verificación de voto. | 1.0 d |
| 4 | Resultados agregados y snapshot. | 0.75 d |
| 5 | Outbox móvil y conciliación. | 1.0 d |
| 6 | Admin Vue: gestión y resultados. | 1.0 d |
| 7 | Tests de integración y contrato. | 1.0 d |

## 5. Migraciones

- `polls`, `poll_options`, `poll_approvals`, `votes`, `poll_result_snapshots`.
- Único `(poll_id, user_id)` en `votes`.
- Único `(poll_id, approver_id)` en `poll_approvals`.
- Índices por `community_id`, `status`, `closes_at` y `deleted_at`.
- Backfill: votaciones existentes se clasifican como `Closed` solo si ya tienen resultado; en caso de duda, `Draft` para revisión.

## 6. Pruebas y datos

- Unitarias: transiciones, padrón, empate, firma, cierre.
- Integración: doble voto, carrera de dos dispositivos, reintento idempotente.
- Contrato: OpenAPI, códigos 403/409/422.
- Cobertura mínima global 40%; dominio ≥ 80%.

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| Reutilización de firmas entre comunidades. | Incluir `community_id` en el material firmado y validar comunidad del usuario. |
| Voto offline a votación cerrada. | Rechazo explícito y UX de “no contabilizado”. |
| Resultado filtrado por inferencia. | API solo agregada; sin lista de votantes. |
| Carrera de cierre vs. voto. | Transacción con `FOR UPDATE` sobre la votación y verificación de `closes_at` del servidor. |

## 8. Aceptación

- Dos aprobaciones distintas promueven la propuesta.
- Un usuario no puede votar dos veces ni siquiera con reintentos.
- Votación cerrada rechaza votos con `POLL_CLOSED`.
- Resultados son agregados y congelados tras `POLL_RESULT_FINALIZED`.
- Ningún evento de voto expone secretos.