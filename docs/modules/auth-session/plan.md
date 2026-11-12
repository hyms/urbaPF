# Auth & Session — Plan de Implementación

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Tests: [`tests.md`](tests.md)

## 1. Dependencias

| Dependencia | Motivo |
|---|---|
| Migración de identidad | Tablas `users`, `refresh_tokens`, `trust_vouchers` y sesiones por canal. |
| `auth-session` spec | Reglas de canal, rotación y validación. |
| Contrato global de errores/idempotencia | Middleware común. |
| Módulo `directory` | Referencia de comunidad/lote para autorización. |
| Configuración de JWT/PBKDF2 | Secretos no versionados; rotación controlada. |

## 2. Entregables por capa

### Domain

- Entidades `User`, `UserChannelSession`, `RefreshToken`, `TrustVoucher`.
- Value Objects: `Email`, `PasswordPolicy`, `Channel`, `Role`, `Credibility`.
- Domain Services: `RegistrationService`, `NeighborValidationService`, `SessionLifecycleService`, `CredentialService`.
- Interfaces: `IUserRepository`, `ISessionRepository`, `IRefreshTokenRepository`, `ITrustVoucherRepository`, `IPasswordHasher`, `ITokenService`, `IAuditWriter`.
- Eventos de dominio y errores tipados.

### Application

- DTOs de registro, login, refresh, sesión, perfil mínimo y cambio de credibilidad.
- `AuthApplicationService`, `SessionApplicationService`, `UserAdministrationService`.
- Validadores de payload y política de contraseña.
- Orquestación de rotación, revocación por canal y auditoría transaccional.

### Infrastructure

- Repositorios Dapper.
- PBKDF2 HMAC-SHA256 con 100.000 iteraciones.
- Emisor/validador JWT.
- Tabla de idempotencia para registro y cambio de contraseña.
- Integración con el writer de `logs_eventos`.
- Limpieza programada de refresh tokens expirados.

### Api

- Endpoints `/api/v1/auth/*` y `/api/v1/users/*`.
- Middleware de `channel`, `community_id`, `sid` y `errorCode`.
- Rate limiting en login, refresh y registro.
- Health check para reloj/secretos (sin exponer valores).

### Mobile/Admin

- `mobile-kmp/shared`: cliente Ktor, almacenamiento seguro de refresh, estado de sesión y manejo de refresh 401.
- `admin-web`: Pinia `auth`, guardas de ruta por rol, refresh silencioso y logout de canal.
- No persistir refresh tokens en SQLDelight ni en `localStorage`.

## 3. Secuencia TDD

1. **Red — dominio**: tests de umbral de 2 vouchers, autovoucher, cambio de credibilidad 1–5 y transiciones de sesión.
2. **Green — dominio**: implementar servicios puros sin I/O.
3. **Red — Application**: tests Mock-First de login, rotación, reutilización y separación de canales.
4. **Green — Application**: orquestar repositorios, hasher y auditor.
5. **Red — Infrastructure**: repositorios Dapper y hasher contra base de prueba de integración.
6. **Green — Infrastructure**: consultas, índice único por canal y migraciones.
7. **Red — API**: tests de contrato, códigos de error, headers y autorización por canal.
8. **Green — API**: registrar endpoints y middleware.
9. **Refactor**: tipado, nombres, eliminación de duplicación y verificación de cobertura.

## 4. Tareas de implementación

| # | Tarea | Capa | Estimación |
|---:|---|---|---:|
| 1 | Modelar `User`, sesiones y refresh tokens. | Domain | 1.0 d |
| 2 | Implementar política PBKDF2 y hasher. | Infrastructure | 0.5 d |
| 3 | Implementar emisión/validación/rotación JWT. | Infrastructure | 1.0 d |
| 4 | Implementar registro y vouchers. | Application | 1.0 d |
| 5 | Implementar login/logout por canal. | Application | 1.0 d |
| 6 | Implementar revocación y reutilización. | Application/Infrastructure | 1.0 d |
| 7 | Implementar endpoints y contratos de error. | Api | 1.0 d |
| 8 | Integrar cliente KMP y admin Vue. | Clients | 1.5 d |
| 9 | Tests de integración y seguridad. | Tests | 1.0 d |

## 5. Migraciones y datos

- Añadir `channel`, `device_label`, `platform`, `push_token_hash`, `last_seen_at` a sesiones.
- Crear índice único parcial:
  - `UNIQUE (user_id, channel) WHERE revoked_at IS NULL`.
- Crear índice único de refresh por `token_hash`.
- Crear `trust_vouchers` con único `(giver_user_id, target_user_id)`.
- Backfill de usuarios existentes a `MobileApp` solo si se reconoce el dispositivo; no inventar sesiones.
- No migrar contraseñas en texto plano. Si existen hashes de otro formato, definir estrategia de rehash en el siguiente login exitoso.

## 6. Estrategia de pruebas

- Unitarias: reglas puras, política de contraseña, cálculo de expiración y permisos.
- Aplicación: orquestación con repositorios dobles.
- Integración: login, refresh, reutilización, logout y separación WebAdmin/MobileApp.
- Contrato: OpenAPI y ejemplos de request/response.
- Seguridad: tokens en logs, force browsing, channel claim, rate limit.
- Cobertura mínima global 40%; priorizar ramas de seguridad y ciclo de sesión.

## 7. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Reutilización de refresh marca legítimamente un usuario por carrera de red. | UI de recuperación con login limpio; nunca reutilizar tokens guardados en dos procesos. |
| Sesión móvil y web comparten tokens accidentalmente. | Claim `channel` obligatorio y middleware por ruta. |
| Doble login invalida la sesión que el usuario está usando. | Documentar política y devolver `session_id` en la respuesta. |
| Hashes PBKDF2 en login lentos. | Límites de rate, métricas y parámetros fijos; no reducir iteraciones sin ADR. |
| Secretos JWT en configuración versionada. | Variables/secretos externos; validación de arranque. |

## 8. Criterios de aceptación

- Un usuario con sesión móvil activa puede abrir una sesión web sin cerrar la móvil.
- Un segundo login móvil cierra el anterior y emite auditoría.
- Un refresh repetido no genera dos sesiones válidas.
- Ningún endpoint devuelve `password_hash` o refresh token.
- Todos los eventos críticos aparecen en `logs_eventos` en la misma transacción.
- Cobertura mínima cumplida y pruebas de contrato verdes.