# Módulo: Auth (Identity & Access)

## Especificación (spec.md)

### Propósito
Gestionar el ciclo de vida de identidad y acceso de los usuarios de UrbaPF: registro, autenticación JWT + Refresh Tokens, validación vecinal, gestión de roles y niveles de credibilidad.

### Bounded Context
Identity & Access

### Entidades y Value Objects
- `User`: identidad del usuario (uuid, email, password_hash, salt, role, credibility_level, created_at, deleted_at)
- `RefreshToken`: token de renovación (uuid, user_id, token, expires_at, revoked_at)
- `Role`: catálogo de roles (Visitante, Vecino, Guardia, Encargado, Admin)
- `CredibilityLevel`: nivel 1-5 con efectos en emergencias y comentarios

### Reglas de Negocio
1. El rol por defecto al registrarse es `Visitante`.
2. Un `Visitante` solo puede leer publicaciones y emergencias (modo lectura).
3. Para pasar a `Vecino` se requieren **2 votos de confianza** de `Encargado` (registrados en `logs_eventos` como `VOTO_CONFIANZA_EMITIDO`).
4. Solo `Encargado` y `Admin` pueden cambiar roles de otros usuarios.
5. La credibilidad inicia en 1 y se ajusta según comportamiento validado (máximo 5).
6. Credibilidad >= 4: alertas de emergencia van directo a Guardias. Credibilidad < 4: requieren aprobación de Manager.
7. Credibilidad 1: comentarios colapsados/ocultos por defecto.
8. PBKDF2 (HMAC-SHA256) con 100k iteraciones para hashing de contraseñas.
9. JWT de acceso de corta vida + Refresh Token de larga vida, ambos rotados en cada refresh.
10. Todo cambio crítico (registro, validación, cambio de rol, cambio de credencial, refresh) debe generar un evento inmutable en `logs_eventos` dentro de la misma transacción.

### Eventos Obligatorios
- `USER_REGISTERED`
- `USER_VALIDATED`
- `ROLE_CHANGED`
- `CREDIBILITY_UPDATED`
- `LOGIN`
- `TOKEN_REFRESHED`
- `PASSWORD_CHANGED`

### Casos de Uso
- `RegisterUser`: crear usuario con rol Visitante, hashear contraseña, insertar evento `USER_REGISTERED`.
- `LoginUser`: validar credenciales, emitir JWT + Refresh Token, insertar evento `LOGIN`.
- `RefreshToken`: rotar par de tokens, insertar evento `TOKEN_REFRESHED`.
- `ValidateNeighbor`: registrar 2 votos de confianza, cambiar rol a Vecino, insertar evento `USER_VALIDATED`.
- `ChangeRole`: cambiar rol de usuario, insertar evento `ROLE_CHANGED`.
- `UpdateCredibility`: ajustar nivel 1-5, insertar evento `CREDIBILITY_UPDATED`.
- `ChangePassword`: re-hashear contraseña, insertar evento `PASSWORD_CHANGED`.

### Edge Cases
- Intento de login con contraseña incorrecta (bloqueo tras N intentos fallidos).
- Refresh token expirado o revocado (forzar login completo).
- Intento de validar un usuario ya Vecino (no-op, no generar evento duplicado).
- Usuario con credibilidad 1 intentando publicar (comentario colapsado).

### API Endpoints
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/validate-neighbor`
- `POST /api/auth/change-role`
- `POST /api/auth/change-password`

---

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/User.cs`, `RefreshToken.cs`, `Role.cs`, `CredibilityLevel.cs`.
2. Crear `UrbaPF.Domain/Services/IAuthDomainService.cs` y su implementación en Domain.
3. Crear `UrbaPF.Domain/Interfaces/IUserRepository.cs`, `IRefreshTokenRepository.cs`, `IRoleRepository.cs`.
4. Crear `UrbaPF.Application/DTOs/AuthRequestDto.cs`, `AuthResponseDto.cs`.
5. Crear `UrbaPF.Application/Services/AuthApplicationService.cs` (orquestador sin lógica de negocio pura).
6. Crear `UrbaPF.Infrastructure/Repositories/UserRepository.cs`, `RefreshTokenRepository.cs` (Dapper).
7. Crear `UrbaPF.Infrastructure/Services/PasswordHasher.cs` (PBKDF2 100k), `JwtTokenService.cs`.
8. Crear `backend/Api/Routes/AuthRoutes.cs` (Minimal APIs).
9. Crear `backend/Infrastructure/Migrations/M014_auth_indexes.sql` (FluentMigrator) si faltan índices.
10. Crear `backend/tests/Domain/AuthDomainServiceTests.cs` y `backend/tests/Services/AuthApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` (puro, sin dependencias externas de infraestructura)
- `UrbaPF.Application` -> `UrbaPF.Domain`
- `UrbaPF.Infrastructure` -> `UrbaPF.Domain` + `UrbaPF.Application`
- `UrbaPF.Api` -> `UrbaPF.Application` + `UrbaPF.Infrastructure` (solo DI)

### Estimación
- ~8 horas de desarrollo + tests.

### Criterios de Aceptación
- Registro crea usuario con rol Visitante y evento `USER_REGISTERED`.
- Login retorna JWT válido y Refresh Token, evento `LOGIN`.
- Refresh rota tokens, evento `TOKEN_REFRESHED`.
- 2 votos de Encargado pasan usuario a Vecino, evento `USER_VALIDATED`.
- Contraseñas hasheadas con PBKDF2 100k.
- Tests unitarios con Moq (sin DB real), cobertura >= 40%.

---

## Escenarios de Prueba (tests.md)

### TDD - Red / Green / Refactor

#### RegisterUser
- **Given** email y contraseña válidos **When** se registra **Then** se crea usuario con rol Visitante y se inserta `USER_REGISTERED`.
- **Given** email ya existente **When** se registra **Then** lanza error de duplicidad, sin evento.

#### LoginUser
- **Given** credenciales válidas **When** se hace login **Then** retorna JWT + Refresh Token e inserta `LOGIN`.
- **Given** contraseña incorrecta **When** se hace login **Then** rechaza con error, sin emitir token.

#### RefreshToken
- **Given** Refresh Token válido y no revocado **When** se refresca **Then** rota par de tokens e inserta `TOKEN_REFRESHED`.
- **Given** Refresh Token expirado/revocado **When** se refresca **Then** rechaza, obliga a login completo.

#### ValidateNeighbor
- **Given** usuario Visitante con 2 votos de Encargado **When** se valida **Then** rol cambia a Vecino e inserta `USER_VALIDATED`.
- **Given** usuario ya Vecino **When** se intenta validar **Then** no-op, sin evento duplicado.

#### ChangeRole / ChangePassword / UpdateCredibility
- **Given** solicitud de Encargado/Admin **When** cambia rol/credencial/credibilidad **Then** se actualiza e inserta evento correspondiente (`ROLE_CHANGED`, `PASSWORD_CHANGED`, `CREDIBILITY_UPDATED`).
- **Given** usuario sin permisos **When** intenta cambiar rol **Then** rechaza con 403.

### Coverage
- Backend: NUnit + Moq + FluentAssertions, cobertura >= 40% en servicios de auth.
- Frontend: Vitest + React Native Testing Library (login, register, refresh flows).
