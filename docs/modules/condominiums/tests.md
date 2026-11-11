# Módulo: Condominiums (Community - Condominios)

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

### Backend (NUnit + Moq + FluentAssertions)

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
