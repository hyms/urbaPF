# Módulo: Users (Usuarios y Perfiles)

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

### Backend (NUnit + Moq + FluentAssertions)

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
