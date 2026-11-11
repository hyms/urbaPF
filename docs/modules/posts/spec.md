# Módulo: Posts (Communication - Tablón Social)

## Especificación (spec.md)

### Propósito
Gestionar publicaciones del tablón social: anuncios, publicaciones fijadas, categorías, comentarios anidados, moderación por credibilidad y aprobaciones.

### Bounded Context
Communication

### Entidades y Value Objects
- `Post`: publicación (uuid, titulo, contenido, categoria, es_fijada, estado, creado_por, aprobado_por, fecha_creacion)
- `Comment`: comentario (uuid, post_id, parent_id, contenido, autor_id, credibilidad_nivel, oculto, fecha_creacion)
- `PostCategory`: categoría (uuid, nombre, descripcion)

### Reglas de Negocio
1. `Vecino` y `Encargado` pueden publicar. `Guardia` y `Visitante` no pueden publicar.
2. Las publicaciones de `Vecino` requieren aprobación de `Encargado`/`Admin` antes de verse públicamente (estado: Pendiente → Aprobado → Publicado).
3. `Encargado` y `Admin` publican directo (estado Publicado).
4. Comentarios anidados (hasta N niveles de profundidad).
5. Moderación por credibilidad:
   - Nivel 5: máxima visibilidad.
   - Nivel 4: alta visibilidad.
   - Nivel 3: normal.
   - Nivel 2: reducida.
   - Nivel 1: colapsado/oculto por defecto.
6. Solo `Encargado`/`Admin` pueden ocultar comentarios (`COMMENT_HIDDEN`).
7. Todo cambio de estado de post y cada comentario debe generar evento inmutable en `logs_eventos`.

### Eventos Obligatorios
- `POST_CREATED`
- `POST_APPROVED`
- `COMMENT_ADDED`
- `COMMENT_HIDDEN`

### Casos de Uso
- `CreatePost`: crear publicación (estado según rol del autor).
- `ApprovePost`: Encargado aprueba publicación pendiente.
- `PinPost`: fijar publicación en el tablón.
- `AddComment`: agregar comentario (anidado opcional).
- `HideComment`: ocultar comentario por moderación.
- `GetFeed`: consultar publicaciones activas ordenadas (fijadas primero).

### Edge Cases
- Comentario de usuario con credibilidad 1 colapsado por defecto.
- Intento de publicar como Guardia o Visitante (rechazo 403).
- Publicación sin aprobación visible solo para el autor y staff hasta ser aprobada.

### API Endpoints
- `POST /api/posts`
- `POST /api/posts/{id}/approve`
- `POST /api/posts/{id}/pin`
- `POST /api/posts/{id}/comments`
- `POST /api/posts/{id}/comments/{commentId}/hide`
- `GET /api/posts/feed`

---

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

### TDD - Red / Green / Refactor

#### CreatePost / ApprovePost
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

#### Feed
| ID | Escenario | Given | When | Then |
|----|-----------|-------|------|------|
| PP-09 | Feed público | publicaciones Publicadas | GetFeed | fijadas primero, pendientes excluidas |

### Coverage
- Backend: NUnit + Moq + FluentAssertions, cobertura >= 40%.
- Frontend: Vitest + React Native Testing Library (crear post, comentar, feed).
