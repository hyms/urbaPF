# Módulo: Condominiums (Community - Condominios)

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
