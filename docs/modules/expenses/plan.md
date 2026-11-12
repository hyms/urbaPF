# Expenses — Plan de Implementación

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Tests: [`tests.md`](tests.md)

## 1. Dependencias

- `directory` para lotes, comunidad y autorización de cargos.
- `auth-session` para rol y usuario.
- Cloudflare R2 para evidencias.
- FCM/WhatsApp para recordatorios.
- Contrato global de idempotencia y sync.

## 2. Entregables

### Domain

- `Expense`, `ExpenseCharge`, `Payment`, `ExpenseLedgerEntry`.
- `ExpenseCalculator`: reparto, redondeo y residuo.
- Máquinas de estados de gasto, cargo y pago.
- Políticas de saldo, cierre y reconciliación.
- Interfaces de repositorio, evidencia, notificación y ledger.

### Application

- DTOs de gasto, cargo, pago, evidencia, confirmación y conciliación.
- `ExpenseApplicationService`, `PaymentApplicationService`.
- Validadores de monto, moneda, fecha, cargo propio y evidencia.
- Orquestación de publicación, pagos, confirmación, rechazo, recordatorios y auditoría.
- Idempotencia por `mutationId`.

### Infrastructure

- Repositorios Dapper.
- Cálculo y persistencia de ledger en transacción.
- Cliente R2 y URLs prefirmadas.
- Worker de recordatorios y conciliación.
- Integración con `logs_eventos`.

### Api

- Endpoints `/api/v1/expenses`, `/charges`, `/payments`.
- Middleware de propiedad de cargo y rol administrativo.
- Errores de saldo, versión y evidencia.
- Rate limiting de recordatorios y confirmaciones.

### Clientes

- `mobile-kmp`: consulta de cargos, formulario de pago, compresión de imagen, outbox y estados `pending_sync`/`rejected`.
- `admin-web`: CRUD de gastos, publicación, tabla de cargos, confirmación/rechazo, evidencias y reportes.

## 3. Secuencia TDD

1. **Red**: reparto, redondeo, saldo, transiciones y cierre.
2. **Green**: dominio puro.
3. **Red**: Application con repositorios dobles; idempotencia y pago offline.
4. **Green**: orquestación.
5. **Red**: ledger, R2, conciliación y recordatorios.
6. **Green**: Infrastructure.
7. **Red**: API y autorización por lote.
8. **Green**: endpoints.
9. **Refactor** y pruebas de conciliación.

## 4. Tareas

| # | Tarea | Estimación |
|---:|---|---:|
| 1 | Gasto, cargos y reparto. | 1.25 d |
| 2 | Pago y verificación. | 1.25 d |
| 3 | Ledger y conciliación. | 1.0 d |
| 4 | Evidencias R2. | 0.75 d |
| 5 | Recordatorios FCM/WhatsApp. | 1.0 d |
| 6 | Sync offline de pagos. | 1.25 d |
| 7 | Admin Vue de finanzas. | 1.5 d |
| 8 | Tests de integración y conciliación. | 1.0 d |

## 5. Migraciones

- `expenses`, `expense_charges`, `payments`, `payment_evidence`, `expense_ledger_entries`, `expense_notifications`.
- Restricciones `amount >= 0`, `currency='BOB'` y `CHECK` de monto.
- Índices por `community_id`, `period_month`, `lot_id`, `status`, `due_at`, `paid_at`.
- Backfill de gastos existentes y clasificación de cargos por lote.
- No convertir saldos históricos sin conciliación humana.

## 6. Estrategia de pruebas

- Unitarias: reparto, residuo, saldo, cierre, transiciones y permisos.
- Integración: conciliación, ledger, R2 y concurrencia.
- Contrato: OpenAPI, códigos de error, idempotencia.
- Offline: pago encolado, reinicio, reintento y evidencia diferida.
- Cobertura mínima global 40%; dominio ≥ 80%.

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| Duplicar pagos por reintento. | Idempotencia + índice de `client_mutation_id` + verificación de saldo. |
| Reparto no cuadra por redondeo. | Residuo al último cargo y prueba de suma exacta. |
| Confirmar pago sin evidencia válida. | Política configurable por método; `Transfer`/`QR` exige referencia/evidencia. |
| Ledger inconsistente. | Escribir cargo, pago y asientos en una transacción. |
| Cerrar gasto con diferencia. | Bloqueo de cierre si `RECONCILIATION_MISMATCH`. |
| Exposición de comprobantes. | R2 privado, URL corta y control por propiedad/rol. |

## 8. Criterios de aceptación

- El reparto de un gasto cuadra exactamente con `amount_total`.
- Un pago offline no se duplica al reintentar.
- Solo Admin confirma/rechaza pagos.
- Un pago confirmado actualiza cargo y ledger en la misma transacción.
- El cierre exige conciliación correcta.
- Los recordatorios no se envían duplicados.