# Directory — Plan de Implementación

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Tests: [`tests.md`](tests.md)

## 1. Dependencias

- `auth-session` para comunidad, rol global y sesión.
- `emergencies` para polígono/radio y destinatarios.
- `expenses` para relación lote/ficha.
- PostgreSQL + PostGIS.

## 2. Entregables

### Domain

- `Community`, `Lot`, `DirectoryEntry`, `DirectoryContact`, `CommunityRoleAssignment`.
- Políticas de visibilidad y unicidad.
- Máquina de estados de importación.
- Interfaces de repositorio y auditoría.

### Application

- DTOs de comunidad, lote, ficha, contacto, importación y filtros.
- `DirectoryApplicationService`, `CommunityApplicationService`, `DirectoryImportService`.
- Validadores de comunidad, visibilidad y campos editables por el usuario.
- Paginación por cursor y filtros por lote/tipo/estado.
- Orquestación de auditoría e idempotencia.

### Infrastructure

- Repositorios Dapper.
- PostGIS para polígono y punto de lote.
- Cifrado de contactos privados en reposo.
- Procesamiento de importación CSV/JSON.
- Búsqueda por nombre normalizado y número de lote.
- Integración con `logs_eventos`.

### Api

- Endpoints `/api/v1/communities`, `/lots`, `/directory`.
- Middleware de comunidad y visibilidad.
- Respuestas enmascaradas por defecto.
- Rate limit en importación.

### Clientes

- `mobile-kmp`: caché cifrada de directorio, búsqueda local, filtros y actualización de perfil propio.
- `admin-web`: gestión de comunidad/lotes, fichas, importación, roles y reportes.

## 3. Secuencia TDD

1. **Red**: unicidad de lote/ficha, visibilidad, transiciones de importación.
2. **Green**: dominio puro.
3. **Red**: Application con repositorios dobles y filtros.
4. **Green**: orquestación.
5. **Red**: PostGIS, cifrado, importación.
6. **Green**: Infrastructure.
7. **Red**: API y respuestas enmascaradas.
8. **Green**: endpoints.
9. **Refactor** y validación de sync delta.

## 4. Tareas

| # | Tarea | Estimación |
|---:|---|---:|
| 1 | Comunidad y lotes. | 1.0 d |
| 2 | Fichas y contactos. | 1.25 d |
| 3 | Visibilidad y enmascaramiento. | 1.0 d |
| 4 | Importación CSV/JSON. | 1.0 d |
| 5 | Delta sync móvil. | 1.0 d |
| 6 | Admin Vue de directorio. | 1.25 d |
| 7 | Tests de integración y autorización. | 1.0 d |

## 5. Migraciones

- `communities`, `lots`, `directory_entries`, `directory_contacts`, `community_role_assignments`, `directory_import_batches`.
- Índice único `(community_id, block, number)` en `lots`.
- Índice único parcial `(user_id) WHERE status='Active' AND deleted_at IS NULL`.
- Índices GIN/GIST para geometría y búsqueda de texto en `display_name`.
- Cifrado a nivel de aplicación para contactos privados; clave gestionada fuera del repositorio.
- Backfill de comunidades/lotes desde datos existentes y clasificación de fichas.

## 6. Pruebas y datos

- Unitarias: visibilidad, unicidad, normalización, estados de importación.
- Integración: PostGIS, cifrado, filtros y paginación.
- Contrato: OpenAPI, campos enmascarados, errores de comunidad.
- Seguridad: enumeración de fichas, acceso cruzado y logs.
- Cobertura mínima global 40%; dominio ≥ 80%.

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| Exposición de contactos privados. | Filtro en servidor, cifrado, pruebas de autorización y auditoría. |
| Fichas duplicadas por importación. | Validación previa, reporte y transacción por lote. |
| Búsqueda lenta por nombre. | Índice de texto y paginación obligatoria. |
| Lotes históricos inconsistentes. | Backfill con revisión humana y bloqueo de duplicados. |
| Caché móvil de datos borrados. | Tombstones, cifrado y purga en logout. |

## 8. Criterios de aceptación

- Un vecino no puede enumerar contactos privados.
- Un lote no se duplica dentro de una comunidad.
- La importación fallida no confirma cambios parciales.
- El delta sincroniza altas, cambios y tombstones sin duplicar.
- Toda mutación crítica genera auditoría y respeta visibilidad.