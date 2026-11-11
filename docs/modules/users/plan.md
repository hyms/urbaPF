# Módulo: Users (Usuarios y Perfiles)

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/UserProfile.cs`, `UserPreference.cs`.
2. Crear `UrbaPF.Domain/Interfaces/IUserProfileRepository.cs`, `IUserPreferenceRepository.cs`.
3. Crear `UrbaPF.Domain/Services/IUserProfileDomainService.cs`.
4. Crear `UrbaPF.Application/DTOs/UserProfileDto.cs`, `UserPreferenceDto.cs`.
5. Crear `UrbaPF.Application/Services/UserProfileApplicationService.cs`.
6. Crear `UrbaPF.Infrastructure/Repositories/UserProfileRepository.cs`, `UserPreferenceRepository.cs` (Dapper).
7. Crear `backend/Api/Routes/UserRoutes.cs`.
8. Crear `backend/tests/Domain/UserProfileDomainServiceTests.cs` y `backend/tests/Services/UserProfileApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Identity (User, Condominium) por referencia de IDs.
- `UrbaPF.Application` -> `UrbaPF.Domain`

### Estimación
- ~6 horas de desarrollo + tests.

### Criterios de Aceptación
- Usuario consulta y edita su propio perfil.
- Admin puede editar cualquier perfil.
- Preferencias de notificación por canal independientes.
- Soft delete con `deleted_at`.

---

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

#### Backend (NUnit + Moq + FluentAssertions)

#### UserProfileDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| US-01 | Consultar perfil propio | usuario autenticado | GetUserProfile | retorna datos del perfil |
| US-02 | Editar perfil propio | datos válidos | UpdateUserProfile | perfil actualizado |
| US-03 | Editar perfil ajeno | usuario sin rol Admin | UpdateUserProfile | rechazo 403 |
| US-04 | Condominio inexistente | condominium_id no existe | UpdateUserProfile | rechazo 400 |

#### UserPreference tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| US-05 | Cambiar preferencias | push=true, whatsapp=false | UpdateUserPreferences | preferencias guardadas por canal |

#### UserProfileApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| US-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **US-F-01**: Pantalla de perfil de usuario.
- **US-F-02**: Edición de condominio/lote.
- **US-F-03**: Preferencias de notificación (push / WhatsApp).

### Coverage Target
- Backend: >= 40% en servicios de perfiles.
- Frontend: >= 40% en pantallas de usuario.
