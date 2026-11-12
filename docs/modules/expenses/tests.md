# Expenses — Estrategia de Pruebas

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Plan: [`plan.md`](plan.md)

## 1. Alcance

Probar gastos, reparto, cargos, pagos, verificación, evidencias, ledger, conciliación, cierre, recordatorios y comportamiento offline.

## 2. Escenarios de dominio

| ID | Given | When | Then |
|---|---|---|---|
| EX-D-001 | Gasto de 100 BOB y 3 lotes | Se publica | Cargos de 33.33, 33.33 y 33.34. |
| EX-D-002 | Gasto de 100 BOB y 6 lotes | Se publica | Seis cargos de 16.666... redondeados y residuo controlado. |
| EX-D-003 | Cargo con saldo 50 | Pago de 60 | Rechaza `PAYMENT_AMOUNT_EXCEEDS_BALANCE`. |
| EX-D-004 | Cargo con saldo 50 | Pago de 20 | Queda `PendingVerification`. |
| EX-D-005 | Pago pendiente | Admin confirma | Cargo `PartiallyPaid` y ledger generado. |
| EX-D-006 | Pago confirmado | Admin intenta confirmar otra vez | `409 PAYMENT_ALREADY_VERIFIED`. |
| EX-D-007 | Gasto `Published` | Admin edita monto | Rechaza `EXPENSE_NOT_DRAFT`. |
| EX-D-008 | Gasto `Closed` | Vecino paga | Rechaza `EXPENSE_CLOSED`. |
| EX-D-009 | Gasto con diferencia de conciliación | Admin cierra | Rechaza `RECONCILIATION_MISMATCH`. |
| EX-D-010 | Cargo `Paid` | Pago adicional | Rechaza por saldo cero. |

## 3. Escenarios de aplicación

| ID | Escenario | Resultado esperado |
|---|---|---|
| EX-A-001 | Crear gasto | `Draft` y `EXPENSE_CREATED`. |
| EX-A-002 | Publicar gasto | Cargos generados y auditados. |
| EX-A-003 | Reintentar publicación con misma idempotencia | Una sola generación de cargos. |
| EX-A-004 | Registrar pago con evidencia | Pago pendiente y objeto R2 asociado. |
| EX-A-005 | Evidencia demasiado grande | `413 EVIDENCE_TOO_LARGE`. |
| EX-A-006 | Rechazar pago | Motivo visible y notificación al vecino. |
| EX-A-007 | Recordatorio repetido mismo día/canal | Una sola notificación. |
| EX-A-008 | Pago confirmado | Notificación de confirmación y evento `EXPENSE_PAID`. |

## 4. Escenarios de API

| ID | Escenario | Resultado esperado |
|---|---|---|
| EX-C-001 | Vecino consulta cargo de otro lote | `403 CHARGE_ACCESS_DENIED`. |
| EX-C-002 | Vecino intenta confirmar un pago | `403 EXPENSE_ADMIN_FORBIDDEN`. |
| EX-C-003 | `GET /expenses` como vecino | `403`. |
| EX-C-004 | `POST /expenses/{id}/publish` sin idempotencia | `400 VALIDATION_ERROR`. |
| EX-C-005 | Respuesta de pago | No contiene URL persistente de R2. |
| EX-C-006 | Error de versión | `409 VERSION_CONFLICT` con versión actual. |
| EX-C-007 | Consulta de conciliación como Encargado | `403` si la política reserva conciliación a Admin. |
| EX-C-008 | Crear gasto sin moneda | `400 VALIDATION_ERROR` o default documentado `BOB`. |

## 5. Escenarios offline y sync

| ID | Escenario | Resultado esperado |
|---|---|---|
| EX-S-001 | Pago creado sin red | Local `pending_sync`; no se muestra confirmado. |
| EX-S-002 | Recuperar red | Pago `PendingVerification` y evidencia asociada. |
| EX-S-003 | Reintento del mismo pago | Un solo pago confirmado pendiente. |
| EX-S-004 | Cargo cerrado antes del sync | Resultado `rejected` con `EXPENSE_CLOSED`. |
| EX-S-005 | Monto alterado offline | Servidor valida saldo y rechaza. |
| EX-S-006 | Evidencia sube después del pago | Pago queda pendiente hasta Asociar evidencia; no se pierde. |
| EX-S-007 | Watermark delta repetido | No duplica cargos ni pagos. |
| EX-S-008 | Usuario pierde acceso a la comunidad | Delta y caché local dejan de exponer cargos. |

## 6. Conciliación y seguridad

| ID | Escenario | Resultado esperado |
|---|---|---|
| EX-X-001 | Suma de pagos confirmados igual al cargo | Cargo `Paid` y conciliación correcta. |
| EX-X-002 | Pago rechazado tras confirmación | Operación bloqueada; requiere ajuste auditable. |
| EX-X-003 | Dos pagos simultáneos por saldo total | Uno confirma; el otro queda rechazado o parcial según saldo. |
| EX-X-004 | Evidencia de otro usuario | Acceso denegado y evento de seguridad. |
| EX-X-005 | Confirmación sin evidencia para QR | Rechazada por política. |
| EX-X-006 | Modificación directa de ledger | Repositorio no expone update/delete; integridad verificada. |

## 7. Cobertura mínima

| Área | Mínimo |
|---|---:|
| Domain | 80% |
| Application | 70% |
| Integración/ledger | 60% |
| Global | 40% |

## 8. Criterios de aceptación

- El reparto cuadra exactamente y el residuo es determinista.
- Ningún cliente puede confirmar un pago.
- La idempotencia evita pagos duplicados.
- La conciliación bloquea cierres inconsistentes.
- La evidencia y el ledger se conservan tras rechazos o cancelaciones.
- Los recordatorios son idempotentes por cargo/día/canal.