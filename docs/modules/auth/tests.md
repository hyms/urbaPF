# Módulo: Auth (Identity & Access)

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

### Backend (NUnit + Moq + FluentAssertions)

#### AuthDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AU-01 | Registro exitoso | email válido, contraseña válida | RegisterUser | usuario creado con rol Visitante, evento `USER_REGISTERED` insertado |
| AU-02 | Email duplicado | email ya existe | RegisterUser | error de duplicidad, ningún evento insertado |
| AU-03 | Login válido | credenciales correctas | LoginUser | JWT + Refresh Token, evento `LOGIN` insertado |
| AU-04 | Contraseña incorrecta | contraseña errónea | LoginUser | rechazo, ningún token emitido |
| AU-05 | Refresh válido | Refresh Token activo | RefreshToken | par de tokens rotado, evento `TOKEN_REFRESHED` |
| AU-06 | Refresh expirado | token expirado/revocado | RefreshToken | rechazo, forzar login completo |
| AU-07 | Validar vecino | 2 votos de Encargado | ValidateNeighbor | rol = Vecino, evento `USER_VALIDATED` |
| AU-08 | Vecino ya validado | usuario ya es Vecino | ValidateNeighbor | no-op, sin evento duplicado |
| AU-09 | Cambio de rol | solicitud de Encargado | ChangeRole | rol actualizado, evento `ROLE_CHANGED` |
| AU-10 | Sin permisos para rol | usuario sin rol de staff | ChangeRole | rechazo 403 |

#### AuthApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| AA-01 | Orquestación register | DTO válido | AuthApplicationService.Register | llama al domain service y repositorio, retorna DTO de respuesta |
| AA-02 | Orquestación login | credenciales válidas | AuthApplicationService.Login | retorna JWT + Refresh Token |
| AA-03 | Mock-First | repositorio mockeado | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)

#### Auth flows
- **AU-F-01**: Pantalla de registro valida campos y llama a `POST /api/auth/register`.
- **AU-F-02**: Pantalla de login autentica y guarda JWT + Refresh Token en store seguro (Zustand).
- **AU-F-03**: Refresh automático ante 401 con interceptor de Axios.
- **AU-F-04**: Cierre de sesión limpia store y tokens.

### Coverage Target
- Backend: >= 40% en servicios de auth.
- Frontend: >= 40% en pantallas y flujos de autenticación.
