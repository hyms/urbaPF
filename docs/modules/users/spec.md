# Módulo: Users (Usuarios y Perfiles)

## Especificación (spec.md)

### Propósito
Gestionar perfiles de usuario, niveles de credibilidad, datos de condominio/lote y preferencias de notificación.

### Bounded Context
Users (parte de Identity & Access y Community)

### Entidades y Value Objects
- `UserProfile`: perfil extendido (uuid, user_id, nombre, apellido, telefono, condominium_id, lot_number, fecha_nacimiento)
- `UserPreference`: preferencias (uuid, user_id, notificaciones_push, notificaciones_whatsapp, idioma)

### Reglas de Negocio
1. Un usuario pertenece a un condominio y lote opcional (Community).
2. El nivel de credibilidad afecta visibilidad de comentarios y routing de alertas (ver módulo Auth y Alerts).
3. Solo el propio usuario o Admin puede editar su perfil.
4. Las preferencias de notificación son independientes por canal (push / WhatsApp).
5. Soft delete: `deleted_at` se setea en lugar de borrar físicamente.

### Eventos Obligatorios (heredados de Identity & Access)
- `USER_REGISTERED`
- `ROLE_CHANGED`
- `CREDIBILITY_UPDATED`
- (no eventos exclusivos de perfil, pero los cambios de perfil se auditan si afectan estado crítico)

### Casos de Uso
- `GetUserProfile`: consultar perfil propio.
- `UpdateUserProfile`: editar datos de perfil (condominio, lote, teléfono).
- `UpdateUserPreferences`: cambiar preferencias de notificación.
- `GetUserCredibility`: consultar nivel de credibilidad y efectos aplicables.

### Edge Cases
- Usuario intentando editar perfil de otro usuario (rechazo 403).
- Condominio/lote inexistente al asignar (rechazo 400).

### API Endpoints
- `GET /api/users/me`
- `PUT /api/users/me`
- `PUT /api/users/me/preferences`
- `GET /api/users/me/credibility`

---

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

### Frontend (Vitest + React Native Testing Library)
- **US-F-01**: Pantalla de perfil de usuario.
- **US-F-02**: Edición de condominio/lote.
- **US-F-03**: Preferencias de notificación (push / WhatsApp).

### Coverage Target
- Backend: >= 40% en servicios de perfiles.
- Frontend: >= 40% en pantallas de usuario.
