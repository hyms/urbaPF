# Módulo: Posts (Communication - Tablón Social)

## Plan de Implementación (plan.md)

### Pasos
1. Crear `UrbaPF.Domain/Entities/Post.cs`, `Comment.cs`, `PostCategory.cs`.
2. Crear `UrbaPF.Domain/Enums/PostStatus.cs` (Pendiente, Aprobado, Publicado, Oculto).
3. Crear `UrbaPF.Domain/Interfaces/IPostRepository.cs`, `ICommentRepository.cs`, `IPostCategoryRepository.cs`.
4. Crear `UrbaPF.Domain/Services/IPostDomainService.cs`.
5. Crear `UrbaPF.Application/DTOs/PostRequestDto.cs`, `PostResponseDto.cs`, `CommentRequestDto.cs`, `CommentResponseDto.cs`, `PostFeedDto.cs`.
6. Crear `UrbaPF.Application/Services/PostApplicationService.cs`.
7. Crear `UrbaPF.Infrastructure/Repositories/PostRepository.cs`, `CommentRepository.cs` (Dapper).
8. Crear `backend/Api/Routes/PostRoutes.cs`.
9. Crear `backend/tests/Domain/PostDomainServiceTests.cs` y `backend/tests/Services/PostApplicationServiceTests.cs` (Moq).

### Dependencias
- `UrbaPF.Domain` depende de Identity (User, CredibilityLevel) por referencia de IDs.
- `UrbaPF.Application` -> `UrbaPF.Domain`

### Estimación
- ~10 horas de desarrollo + tests.

### Criterios de Aceptación
- Vecino publica en estado Pendiente (requiere aprobación).
- Encargado/Admin publica directo en estado Publicado.
- Comentarios anidados soportados.
- Moderación por credibilidad (nivel 1 colapsado).
- Eventos `POST_CREATED`, `POST_APPROVED`, `COMMENT_ADDED`, `COMMENT_HIDDEN` insertados atómicamente.

---

## Escenarios de Prueba (tests.md)

### Estrategia TDD
1. **Red**: escribir test basado en este documento que falle.
2. **Green**: implementar mínimo para pasar.
3. **Refactor**: limpiar manteniendo tests verdes.

#### Backend (NUnit + Moq + FluentAssertions)

#### PostDomainServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PP-01 | Vecino publica | usuario Vecino | CreatePost | estado Pendiente, evento `POST_CREATED` |
| PP-02 | Encargado publica directo | usuario Encargado | CreatePost | estado Publicado, evento `POST_CREATED` |
| PP-03 | Encargado aprueba | publicación Pendiente | ApprovePost | estado Publicado, evento `POST_APPROVED` |
| PP-04 | Guardia publica | usuario Guardia | CreatePost | rechazo 403 |

#### Comment tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PP-05 | Comentario normal | publicación publicada | AddComment | comentario creado, evento `COMMENT_ADDED` |
| PP-06 | Comentario anidado | comentario padre existente | AddComment | comentario con parent_id |
| PP-07 | Credibilidad 1 colapsado | usuario nivel 1 | AddComment | comentario oculto por defecto |
| PP-08 | Ocultar comentario | Encargado + comentario | HideComment | comentario oculto, evento `COMMENT_HIDDEN` |

#### Feed tests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PP-09 | Feed público | publicaciones Publicadas | GetFeed | fijadas primero, pendientes excluidas |

#### PostApplicationServiceTests
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PP-A-01 | Mock-First | repositorios mockeados | cualquier test | **no** se usa DB real |

### Frontend (Vitest + React Native Testing Library)
- **PP-F-01**: Pantalla de crear publicación (vecino: pendiente; encargado: publicado directo).
- **PP-F-02**: Flujo de aprobación de publicación por Encargado.
- **PP-F-03**: Pantalla de comentarios anidados y ocultar comentario.
- **PP-F-04**: Feed con publicaciones fijadas primero.

### Coverage Target
- Backend: >= 40% en servicios de comunicación.
- Frontend: >= 40% en pantallas y flujos de publicaciones.
