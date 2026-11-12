# Expenses — Especificación

- Versión: 1.0.0
- Estado: propuesta para revisión
- Spec global: [`docs/global-spec.md`](../../global-spec.md)

## 1. Propósito

`expenses` gestiona gastos comunes, cargos por lote, pagos, verificación y conciliación. El Admin define gastos y confirma pagos; los vecinos consultan su estado y pueden registrar pagos offline con evidencia. El módulo nunca permite que el cliente fije por sí mismo el estado contable definitivo.

## 2. Actores y canales

| Actor | Canal | Capacidad |
|---|---|---|
| Vecino | `MobileApp` | Ver cargos, registrar pago y evidencia, ver estado. |
| Encargado | `WebAdmin` | Ver cargos y conciliación; no confirma pagos por sí solo si la política exige Admin. |
| Admin | `WebAdmin` | CRUD de gastos, cargos, confirmación/rechazo de pagos, reportes. |
| Guardia | `MobileApp` | Sin acceso a finanzas. |
| Visitante | — | Sin acceso. |

## 3. Modelo de dominio

| Entidad | Campos relevantes |
|---|---|
| `Expense` | `id`, `community_id`, `name`, `description`, `period_month`, `amount_total`, `currency`, `status`, `created_by`, `version`, `deleted_at` |
| `ExpenseCharge` | `id`, `expense_id`, `lot_id`, `amount_due`, `status`, `due_at`, `version` |
| `Payment` | `id`, `charge_id`, `amount`, `currency`, `method`, `status`, `paid_at`, `verified_by`, `rejection_reason`, `client_mutation_id`, `version` |
| `PaymentEvidence` | `id`, `payment_id`, `r2_object_key`, `mime_type`, `size_bytes`, `compressed_in_client`, `created_at` |
| `ExpenseNotification` | `id`, `expense_id`, `channel`, `recipient_hash`, `status`, `sent_at` |
| `ExpenseLedgerEntry` | `id`, `expense_id`, `charge_id?`, `debit`, `credit`, `currency`, `created_at` |

Estados de `Expense`: `Draft`, `Published`, `Closed`, `Cancelled`.
Estados de `ExpenseCharge`: `Pending`, `PartiallyPaid`, `Paid`, `Overdue`, `Waived`.
Estados de `Payment`: `PendingVerification`, `Confirmed`, `Rejected`, `Cancelled`.

## 4. Reglas de negocio

### 4.1 Gastos y cargos

1. Solo Admin crea, edita, publica o cierra un gasto.
2. Un gasto obligatorio tiene `name`, `period_month`, `amount_total`, `currency` y fecha límite.
3. La moneda v1.0.0 es `BOB`; no se admiten conversiones automáticas.
4. Al publicar un gasto, el sistema genera un cargo por lote activo. El reparto por defecto es `amount_total / lotes_activos`, redondeado a 2 decimales y ajustado para cuadrar exactamente el total.
5. Un gasto `Published` no cambia su monto ni sus cargos; cualquier corrección requiere un ajuste o cierre y nuevo gasto.
6. Un gasto `Closed` no acepta pagos nuevos.
7. Un cargo solo puede pertenecer a un gasto y a un lote.

### 4.2 Pagos

1. Un vecino puede registrar un pago para un cargo propio o de un lote que administra.
2. El pago incluye `amount`, `method` (`Transfer`, `QR`, `Cash`, `Other`), `paid_at` y evidencia opcional.
3. El monto del pago no puede superar el saldo pendiente del cargo.
4. Un pago se crea en `PendingVerification` y nunca cambia el estado del cargo hasta su confirmación.
5. Un pago confirmado actualiza el cargo a `Paid` o `PartiallyPaid` y genera asiento(s) en `ExpenseLedgerEntry`.
6. Un pago rechazado conserva la evidencia y el motivo; no altera saldo.
7. Un pago solo puede confirmarse/rechazarse una vez; un segundo intento es idempotente o `409`.
8. El pago de un cargo cerrado se rechaza.
9. La conciliación diaria compara `ExpenseCharge.amount_due` con la suma de pagos confirmados y reporta diferencias.

### 4.3 Idempotencia y offline

1. El móvil puede registrar un pago sin red: primero persiste `pending_mutation`, luego muestra estado `pending_sync`.
2. El servidor recibe `mutationId`, `chargeId`, `amount`, `method`, `paidAt` y `evidence`.
3. La idempotencia evita duplicar el pago si el mismo comprobante se reintenta.
4. El cliente nunca marca un pago como `Confirmed`; solo el servidor puede hacerlo.
5. Si el cargo cambió o se cerró, el servidor rechaza o devuelve conflicto.
6. Un pago rechazado por saldo insuficiente se muestra con corrección sugerida.

### 4.4 Evidencias

1. La evidencia se comprime en el dispositivo antes de subirla.
2. El cliente solicita una URL prefirmada a R2 y luego confirma la clave del objeto.
3. El tamaño máximo recomendado es 1 MB por imagen y 5 MB por PDF.
4. La evidencia se asocia al pago solo después de que el objeto existe.
5. No se aceptan URLs de R2 persistentes en respuestas públicas.

### 4.5 Notificaciones

- Recordatorios de cargo pendiente/vencido se envían por FCM o WhatsApp según preferencia.
- Un recordatorio es idempotente por `expense_id + charge_id + day + channel`.
- Un pago confirmado notifica al vecino; un rechazo notifica con motivo.

## 5. Casos de uso

| ID | Caso | Actor | Resultado |
|---|---|---|---|
| EX-01 | Crear gasto | Admin | `EXPENSE_CREATED`; estado `Draft`. |
| EX-02 | Publicar gasto | Admin | Cargos por lote y `Published`. |
| EX-03 | Editar gasto publicado | Admin | Rechazado; requiere ajuste/cierre. |
| EX-04 | Consultar cargo propio | Vecino | Saldo, vencimiento y pagos visibles. |
| EX-05 | Registrar pago online | Vecino | `PendingVerification`. |
| EX-06 | Registrar pago offline | Vecino | `pending_sync`; luego confirmación o rechazo. |
| EX-07 | Confirmar pago | Admin | Cargo actualizado y ledger generado. |
| EX-08 | Rechazar pago | Admin | `Rejected` con motivo. |
| EX-09 | Reintentar pago | Vecino | Sin duplicar por idempotencia. |
| EX-10 | Cerrar gasto | Admin | Sin nuevos pagos; conciliación final. |
| EX-11 | Enviar recordatorio | Sistema/Admin | Notificación idempotente y auditada. |

## 6. Contrato API

| Método | Ruta | Permiso | Propósito |
|---|---|---|---|
| GET | `/expenses` | Admin/Encargado | Listar gastos. |
| POST | `/expenses` | Admin | Crear gasto. |
| GET | `/expenses/{id}` | Admin/Encargado | Detalle y cargos. |
| PATCH | `/expenses/{id}` | Admin | Editar solo `Draft`. |
| POST | `/expenses/{id}/publish` | Admin | Publicar y generar cargos. |
| POST | `/expenses/{id}/close` | Admin | Cerrar gasto. |
| GET | `/expenses/me/charges` | Vecino | Cargos propios/administrados. |
| GET | `/charges/{id}` | Propietario/Admin | Detalle del cargo. |
| POST | `/charges/{id}/payments` | Vecino/Admin | Registrar pago; idempotencia obligatoria. |
| POST | `/payments/{id}/evidence` | Propietario/Admin | Solicitar URL prefirmada. |
| GET | `/payments` | Admin/Encargado | Listar pagos por filtro. |
| POST | `/payments/{id}/confirm` | Admin | Confirmar pago. |
| POST | `/payments/{id}/reject` | Admin | Rechazar con motivo. |
| GET | `/expenses/{id}/reconciliation` | Admin | Diferencias y estado contable. |

Headers:

- `X-Idempotency-Key` obligatoria en creación/edición de gasto, publicación, cierre, pago, confirmación y rechazo.
- `X-Client-Version` y `X-Schema-Version` recomendadas en clientes móviles.
- `baseVersion` obligatoria en PATCH de gasto y pago.

## 7. Sincronización

Colecciones delta:

| Colección | Contenido | Permiso |
|---|---|---|
| `expenses.myCharges` | Cargos del usuario y estado de pago. | Vecino/Admin. |
| `expenses.myPayments` | Pagos del usuario y evidencia. | Vecino/Admin. |
| `expenses.operational` | Gastos/cargos para Encargado/Admin. | Roles administrativos. |
| `expenses.deleted` | Tombstones de gastos/pagos. | Según permiso. |

Reglas:

- Un pago offline se encola con `chargeId`, `amount`, `method`, `paidAt`, `evidenceKey` e `idempotencyKey`.
- El cliente no descarga la contabilidad completa de la comunidad.
- El watermark avanza solo tras persistir todos los lotes.
- Un pago `Confirmed` no se puede revertir desde móvil; solo Admin mediante ajuste auditable.
- La evidencia sube antes o después del push según conectividad; el pago queda `pending_sync` hasta que ambas partes estén confirmadas.

## 8. Eventos de auditoría

| Evento | Payload mínimo |
|---|---|
| `EXPENSE_CREATED` | `expense_id`, `amount_total`, `period_month`. |
| `EXPENSE_UPDATED` | `expense_id`, `campos`, `actor_id`. |
| `EXPENSE_PAID` | `charge_id`, `payment_id`, `amount`, `method`. |
| `PAYMENT_CONFIRMED` | `payment_id`, `charge_id`, `verified_by`. |
| `PAYMENT_REJECTED` | `payment_id`, `motivo`, `actor_id`. |
| `EXPENSE_REMINDER_SENT` | `expense_id`, `charge_id?`, `channel`. |

## 9. Errores

| HTTP | `errorCode` | Causa |
|---:|---|---|
| 400 | `VALIDATION_ERROR` | Monto, fecha o método inválido. |
| 403 | `EXPENSE_ADMIN_FORBIDDEN` | Actor sin permiso administrativo. |
| 403 | `CHARGE_ACCESS_DENIED` | Cargo de otro lote/usuario. |
| 404 | `EXPENSE_NOT_FOUND` | Gasto inexistente o cancelado. |
| 404 | `CHARGE_NOT_FOUND` | Cargo inexistente. |
| 409 | `EXPENSE_NOT_DRAFT` | Edición de gasto publicado. |
| 409 | `EXPENSE_CLOSED` | Pago sobre gasto cerrado. |
| 409 | `PAYMENT_ALREADY_VERIFIED` | Confirmación/rechazo repetido. |
| 409 | `PAYMENT_AMOUNT_EXCEEDS_BALANCE` | Monto mayor al saldo. |
| 409 | `VERSION_CONFLICT` | `baseVersion` desactualizada. |
| 409 | `IDEMPOTENCY_KEY_REUSED` | Clave repetida con otro payload. |
| 413 | `EVIDENCE_TOO_LARGE` | Archivo fuera de límite. |
| 422 | `RECONCILIATION_MISMATCH` | La conciliación no cuadra al cerrar. |

## 10. Edge cases

- Reparto con decimales: el último cargo absorbe el residuo para cuadrar el total.
- Dos pagos simultáneos por el saldo: uno confirma, el otro pasa a `PartiallyPaid` o rechazo según saldo.
- Pago offline con monto incorrecto: el servidor valida contra el cargo y rechaza.
- Evidencia duplicada: se reutiliza el objeto R2 y se asocia al pago idempotente.
- Gasto cancelado después de pagos confirmados: no se borran asientos; se genera ajuste auditable.
- Cambio de moneda: no soportado en v1.0.0.
- Admin intenta confirmar un pago ya rechazado: `409 PAYMENT_ALREADY_VERIFIED`.

## 11. Fuera de alcance

- Pasarela de pago automática y liquidación bancaria.
- Contabilidad general/exportación contable certificada.
- Multimoneda y presupuestos multi-anuales.
- Fraude avanzado o scoring crediticio.

## 12. Trazabilidad

- Global: [`docs/global-spec.md`](../../global-spec.md)
- Plan: [`plan.md`](plan.md)
- Tests: [`tests.md`](tests.md)