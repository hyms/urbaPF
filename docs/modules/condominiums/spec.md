# Módulo: Condominiums (Community - Condominios)

## Especificación (spec.md)

### Propósito
Gestionar condominios, manzanos/lotes y expensas: creación de condominios, asignación de lotes, cuotas mensuales y pagos.

### Bounded Context
Community

### Entidades y Value Objects
- `Condominium`: condominio (uuid, nombre, direccion, telefono, fecha_creacion, creado_por)
- `Lot`: lote/manzano (uuid, condominium_id, numero_manzana, numero_lote, propietario_id, estado)
- `Expense`: expensa (uuid, condominium_id, concepto, monto, mes_año, fecha_vencimiento, estado)
- `ExpensePayment`: pago de expensa (uuid, expense_id, user_id, monto_pagado, fecha_pago, comprobante_url)

### Reglas de Negocio
1. Un condominio tiene múltiples lotes (manzanas/lotes).
2. Un lote pertenece a un propietario (User) y a un condominio.
3. Las expensas son mensuales por condominio con monto, concepto y fecha de vencimiento.
4. Un pago de expensa vincula un usuario, una expensa y un monto pagado.
5. Soft delete en condominios, lotes y expensas (`deleted_at`).
6. Solo `Admin` puede crear/gestionar condominios y lotes.

### Eventos Obligatorios
- `CONDOMINIUM_CREATED`
- `EXPENSE_CREATED`
- `EXPENSE_PAID`

### Casos de Uso
- `CreateCondominium`: crear condominio (Admin).
- `CreateLot`: crear lote/manzano dentro de un condominio (Admin).
- `AssignLotOwner`: asignar propietario a un lote (Admin).
- `CreateExpense`: crear expensa mensual (Admin/Encargado).
- `PayExpense`: registrar pago de expensa (usuario propietario).
- `GetCondominiumSummary`: resumen de condominio (lotes, expensas pendientes, pagos).

### Edge Cases
- Lote duplicado (mismo manzano+lote en un condominio).
- Pago de expensa ya pagada (rechazo, expensa cerrada).
- Usuario no propietario intentando pagar expensa de otro lote (rechazo 403).

### API Endpoints
- `POST /api/condominiums`
- `POST /api/condominiums/{id}/lots`
- `POST /api/lots/{id}/owner`
- `POST /api/expenses`
- `POST /api/expenses/{id}/pay`
- `GET /api/condominiums/{id}/summary`

---

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/Condominium.cs`, `Lot.cs`, `Expense.cs`, `ExpensePayment.cs`.
2. Crear `UrbaPF.Domain/Interfaces/ICondominiumRepository.cs`, `ILotRepository.cs`, `IExpenseRepository.cs`, `IExpensePaymentRepository.cs`.
3. Crear `UrbaPF.Domain/Services/ICondominiumDomainService.cs`.
4. Crear `UrbaPF.Application/DTOs/CondominiumDto.cs`, `LotDto.cs`, `ExpenseDto.cs`, `ExpensePaymentDto.cs`.
5. Crear `UrbaPF.Application/Services/CondominiumApplicationService.cs`.
6. Crear `UrbaPF.Infrastructure/Repositories/CondominiumRepository.cs`, `LotRepository.cs`, `ExpenseRepository.cs` (Dapper).
7. Crear `backend/Api/Routes/CondominiumRoutes.cs`, `ExpenseRoutes.cs`.
8. Crear `backend/tests/Domain/CondominiumDomainServiceTests.cs` y `backend/tests/Services/CondominiumApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Identity (User) por referencia de IDs de propietario.
- `UrbaPF.Application` -> `UrbaPF.Domain`

### Estimación
- ~8 horas de desarrollo + tests.

### Criterios de Aceptación
- Admin crea condominio, lote y asigna propietario.
- Expensa mensual con monto, concepto, vencimiento.
- Pago de expensa registra monto, fecha y comprobante.
- Eventos `CONDOMINIUM_CREATED`, `EXPENSE_CREATED`, `EXPENSE_PAID` insertados atómicamente.
- Soft delete con `deleted_at`.

---

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

#### Backend (NUnit + Moq + FluentAssertions)

#### CondominiumDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| CO-01 | Crear condominio | Admin + datos válidos | CreateCondominium | condominio creado, evento `CONDOMINIUM_CREATED` |
| CO-02 | Crear lote | condominio existente | CreateLot | lote asociado al condominio |
| CO-03 | Lote duplicado | mismo manzano+lote en condominio | CreateLot | rechazo, lote duplicado |
| CO-04 | Asignar propietario | Admin + lote sin dueño | AssignLotOwner | propietario asignado al lote |
| CO-05 | Crear expensa | Admin/Encargado + condominio | CreateExpense | expensa con monto, concepto, vencimiento |
| CO-06 | Pagar expensa | propietario + expensa pendiente | PayExpense | expensa marcada pagada, evento `EXPENSE_PAID` |
| CO-07 | Pagar expensa ya pagada | expensa cerrada | PayExpense | rechazo |
| CO-08 | Pagar expensa ajena | usuario no propietario | PayExpense | rechazo 403 |

#### CondominiumApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| CO-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **CO-F-01**: Pantalla de gestión de condominios (Admin).
- **CO-F-02**: Asignación de lotes y propietarios.
- **CO-F-03**: Creación y pago de expensas.
- **CO-F-04**: Resumen de condominio (pendientes, pagados).

### Coverage Target
- Backend: >= 40% en servicios de comunidad.
- Frontend: >= 40% en pantallas de condominios y expensas.
