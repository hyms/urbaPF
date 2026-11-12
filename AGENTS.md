# UrbaPF — Technical Master Instructions (v1.0.0)

Este archivo (`AGENTS.md`) es la **fuente de verdad técnica** de UrbaPF. Si una instrucción local contradice este documento, prevalece este documento hasta que se apruebe un ADR y se actualice la especificación global.

Versión objetivo: **1.0.0** bajo SemVer 2.0.

> Restricción activa de la migración: mientras la arquitectura esté en revisión, los agentes solo pueden crear o modificar archivos Markdown en `docs/**`, `README.md` y `AGENTS.md`. No se modifica código fuente, esquema, dependencias ni configuración de despliegue.

## 1. Orden de lectura obligatorio

Antes de escribir código, todo agente debe leer:

1. `AGENTS.md` (este archivo).
2. `README.md`.
3. `docs/global-spec.md`.
4. `docs/modules/{auth-session,governance-votes,emergencies,directory,expenses}/spec.md`.
5. `docs/modules/{...}/plan.md` y `tests.md` del módulo afectado.

Ninguna tarea de código inicia sin spec, plan y tests aprobados para el bounded context afectado.

## 2. Arquitectura objetivo

```text
backend/           # .NET 10, DDD, Dapper, FluentMigrator
mobile-kmp/        # Kotlin Multiplatform, SQLDelight, Ktor, offline-first
admin-web/         # Vue 3 + Vuetify 3 + TypeScript + Pinia + Vite
docs/              # especificación global y SDD por bounded context
```

Los bounded contexts v1.0.0 son únicamente:

- `auth-session`
- `governance-votes`
- `emergencies`
- `directory`
- `expenses`

No crear módulos, carpetas ni endpoints fuera de estos contextos sin actualizar `docs/global-spec.md` y este archivo.

## 3. Backend .NET 10 — reglas inviolables

### 3.1 Capas

```text
backend/src/UrbaPF.Domain/          # Entidades, VOs, Domain Services,
                                    # Domain Events, interfaces de repositorio.
backend/src/UrbaPF.Application/     # Application Services, DTOs, validadores.
backend/src/UrbaPF.Infrastructure/  # Dapper, FluentMigrator, R2, FCM,
                                    # WhatsApp, implementación de repositorios.
backend/src/UrbaPF.Api/             # Minimal APIs, DI, middleware.
backend/tests/                      # Unit + Integration Mock-First.
```

1. `Domain` no referencia `Application` ni `Infrastructure`.
2. `Application` referencia solo `Domain`.
3. `Infrastructure` implementa interfaces definidas en `Domain`.
4. `Api` referencia `Application` e `Infrastructure` solo para composición/DI.
5. Los Domain Services no heredan de `BaseRepository`; reciben interfaces.
6. No usar EF Core. Acceso de datos con **Dapper 2.1.72**.
7. Migraciones con **FluentMigrator 8.0.1** sobre **PostgreSQL 16+ / PostGIS**.
8. Application Services orquestan casos de uso, devuelven DTOs y no exponen entidades.
9. Sin MediatR ni CQRS en v1.0.0.
10. Los Route handlers son delgados: validan entrada, llaman al Application Service y mapean errores. No contienen reglas de negocio.
11. Toda mutación crítica escribe negocio + `logs_eventos` en la misma transacción.
12. Usar `X-Idempotency-Key` en mutaciones móviles y administrativas que crean o cambian estado.
13. Toda entidad modificable expone `version` y `updated_at`; aceptar `baseVersion` y responder `409 VERSION_CONFLICT`.
14. Usar `application/problem+json` con `errorCode` estable y `traceId`.

### 3.2 Estilo C#

- Nullable reference types habilitado; sin warnings suprimidos sin justificación.
- `async` de I/O de red/base de datos; nunca `async void`.
- Inyección de dependencias por constructor; sin service locator.
- DTOs con validación explícita; nunca confiar en el cliente.
- No registrar tokens, contraseñas, refresh tokens ni PII innecesaria.
- Nombres de casos de uso orientados a negocio; evitar `_impl`, `Helper` y `Manager` genéricos.
- Soft delete mediante `deleted_at` y `deleted_by`; nunca borrado físico de históricos.
- Auditoría append-only: prohibido `UPDATE`/`DELETE` sobre `logs_eventos`.

## 4. Mobile KMP — reglas inviolables

Stack: **Kotlin Multiplatform**, **SQLDelight** para base local, **Ktor Client** para HTTP. Android-first, iOS-ready.

1. La UI observa SQLDelight; no llama directamente a la API desde una pantalla.
2. Toda mutación permitida se persiste primero en `pending_mutation` y luego se aplica el estado local.
3. El outbox es durable, sobrevive a reinicios y no se borra al perder red o sesión.
4. Una mutación pendiente nunca se muestra como confirmada por el servidor.
5. La sincronización es incremental: `lastSyncTimestamp` + `serverSequence`; el reloj del dispositivo nunca es la fuente de verdad.
6. Toda mutación usa un `mutationId` UUID y se envía con `X-Idempotency-Key`.
7. Reintentos con backoff exponencial y jitter; errores de validación no se reintentan automáticamente.
8. Para un mismo agregado, las mutaciones se envían en orden FIFO; distintos agregados pueden ir en paralelo.
9. Tokens en almacenamiento seguro de plataforma; nunca en logs, SQLDelight sin cifrar ni `println`.
10. Imágenes comprimidas en cliente antes de subir a Cloudflare R2; objetivo < 1 MB por imagen.
11. El dominio compartido y la lógica de sync viven en `commonMain`; no introducir APIs de Android/iOS en `shared`.
12. Los tipos de red son DTOs; convertir a modelos de dominio antes de persistir.
13. Las mutaciones fallidas quedan en `retry`, `conflict` o `rejected`; nunca se descartan silenciosamente.
14. “100% offline” significa capturar, persistir y encolar, y leer caché. No significa que FCM/WhatsApp puedan entregar sin red.
15. Una emergencia creada offline se muestra `pending_sync`, no confirmada.

## 5. Admin Web — reglas inviolables

Stack: **Vue 3 + Vuetify 3 + TypeScript + Pinia + Vite**.

1. Aplicación online-only. No implementar service worker de mutaciones ni cola offline.
2. Usar Vue Composition API con `<script setup lang="ts">`.
3. Pinia es caché de sesión/estado de UI; el servidor es la fuente de verdad persistente.
4. Vuetify es la librería de componentes; no crear un segundo sistema de diseño.
5. Axios vive en `src/services/`; las vistas no instalan clientes HTTP directamente.
6. No usar `any` en contratos; tipos de API en `src/types/`.
7. Guards de ruta por rol y canal; el backend revalida autorización.
8. Formularios con validación de cliente para UX, nunca como única barrera.
9. Errores de API se muestran con `errorCode` y acción recuperable.
10. Accesibilidad básica: labels, foco visible, contraste y navegación por teclado.

## 6. Contrato de sincronización

- Prefijo API: `/api/v1`.
- Headers móviles: `Authorization`, `X-Channel`, `X-Idempotency-Key`, `X-Client-Version`, `X-Schema-Version`, `Content-Encoding` cuando aplique.
- Push: `POST /api/v1/sync/push`.
- Pull: `GET /api/v1/sync/delta?lastSyncTimestamp=...&collections=...`.
- Estados de outbox: `pending`, `in_flight`, `retry`, `conflict`, `rejected`, `applied`.
- Resultados de push: `applied`, `duplicate`, `conflict`, `rejected`, `retryable`.
- El watermark solo avanza cuando `hasMore=false` y todos los lotes fueron persistidos.
- La idempotencia es única por `community_id + user_id + channel + idempotency_key`.
- La misma clave con otro payload es `409 IDEMPOTENCY_KEY_REUSED`.
- El servidor zanja conflictos de autenticación, autorización, estado operacional, montos y elegibilidad.
- No borrar outbox al cerrar sesión; pausar y pedir confirmación explícita.

## 7. Seguridad

1. PBKDF2 HMAC-SHA256, 100.000 iteraciones, sal por usuario.
2. JWT de acceso de corta vida + refresh de larga vida con rotación obligatoria.
3. Reutilización de refresh revoca toda la familia y emite `TOKEN_REUSE_DETECTED`.
4. Una sesión activa por usuario **por canal**: `WebAdmin` y `MobileApp` son independientes.
5. El JWT incluye `channel`; la API rechaza tokens de un canal en rutas de otro.
6. Autorización siempre en servidor; la UI solo aplica UX.
7. Credibilidad 1–5: ≥4 enruta alertas directo a guardias; <4 requiere aprobación de Encargado.
8. Cloudflare R2 privado; URLs prefirmadas de corta vida.
9. FCM y WhatsApp Cloud API son asíncronos y quedan registrados; nunca se presentan como atención confirmada.
10. Secretos fuera del repositorio; validar su presencia al arrancar.
11. Minimizar PII en logs, payloads y notificaciones.

## 8. Auditoría y `logs_eventos`

Esquema append-only con `hash_anterior` y `hash_verificacion`:

```text
hash_verificacion = SHA256(hash_anterior || canonical_json(data_json))
```

Reglas:

1. Ninguna mutación crítica sin evento en la misma transacción.
2. Prohibido `UPDATE`/`DELETE` en `logs_eventos`.
3. El hash encadena eventos por comunidad.
4. Una cadena rota genera alerta de integridad.
5. Eventos obligatorios por contexto:

| Contexto | Eventos |
|---|---|
| `auth-session` | `USER_REGISTERED`, `USER_VALIDATED`, `ROLE_CHANGED`, `CREDIBILITY_UPDATED`, `LOGIN`, `LOGOUT`, `TOKEN_REFRESHED`, `TOKEN_REUSE_DETECTED`, `SESSION_REVOKED`, `PASSWORD_CHANGED` |
| `governance-votes` | `POLL_CREATED`, `POLL_APPROVED`, `POLL_ACTIVATED`, `POLL_CLOSED`, `VOTE_CAST`, `POLL_RESULT_FINALIZED` |
| `emergencies` | `EMERGENCY_CREATED`, `EMERGENCY_APPROVED`, `EMERGENCY_REJECTED`, `GUARD_EN_ROUTE`, `EMERGENCY_RESOLVED`, `EMERGENCY_CLOSED`, `INCIDENT_CREATED`, `INCIDENT_STATUS_UPDATED`, `NOTIFICATION_FAILED` |
| `directory` | `CONDOMINIUM_CREATED`, `LOT_CREATED`, `DIRECTORY_ENTRY_CREATED`, `DIRECTORY_ENTRY_UPDATED`, `DIRECTORY_ENTRY_DELETED`, `ROLE_ASSIGNED` |
| `expenses` | `EXPENSE_CREATED`, `EXPENSE_UPDATED`, `EXPENSE_PAID`, `PAYMENT_CONFIRMED`, `PAYMENT_REJECTED`, `EXPENSE_REMINDER_SENT` |

## 9. SDD + TDD obligatorio

### SDD

Cada bounded context contiene:

- `spec.md`: propósito, entidades, reglas, casos de uso, API, sync, errores y edge cases.
- `plan.md`: tareas, dependencias, migraciones, riesgos y estimación.
- `tests.md`: escenarios Given/When/Then, cobertura y criterios de aceptación.

Un cambio de comportamiento actualiza primero la spec y los tests; luego se implementa.

### TDD

1. **Red**: escribir test que falla según `tests.md`.
2. **Green**: implementación mínima.
3. **Refactor**: limpiar sin cambiar comportamiento.

Frameworks:

- Backend: **NUnit 4.5.1**, **FluentAssertions 8.9.0**, **Moq 4.20.72**.
- Mobile: Kotlin Test + tests compartidos; tests de outbox y sync obligatorios.
- Admin: Vitest 4 + Vue Test Utils + TypeScript.
- Cobertura mínima global: **40%**. Dominio crítico: ≥80% salvo excepción aprobada.
- Tests unitarios backend: Mock-First; prohibido depender de PostgreSQL real.
- Tests de integración: PostgreSQL/PostGIS, R2 y proveedores externos simulados o en contenedor controlado.

## 10. Estándares por tecnología

### Backend

- Minimal APIs con `TypedResults`/`Results` y mapeo explícito.
- Validadores en Application; no duplicar reglas en Api.
- Repositorios con SQL parametrizado; prohibido concatenar SQL.
- Migraciones con nombre descriptivo y rollback documentado.
- Índices y constraints acompañan la migración.
- No exponer entidades de dominio por la API.

### Kotlin Multiplatform

- Estructura por feature dentro de `commonMain`; no paquetes por capa.
- Modelos de dominio sin dependencias de plataforma.
- SQLDelight: queries tipadas, migraciones versionadas y transacciones para sync.
- Ktor: cliente único, interceptor de auth, manejo de 401/refresh y timeout.
- Coroutines: `Dispatchers.IO` para I/O; no bloquear el hilo principal.
- Errores: `sealed` result/exception por caso de uso; nunca fallar silenciosamente.
- No usar `printStackTrace`; usar logging estructurado sin datos sensibles.

### Vue 3 + Vuetify

- `script setup lang="ts"` y Composition API.
- Pinia por dominio; no stores monolíticos.
- Componentes presentacionales sin Axios ni reglas de negocio.
- Vuetify: `v-data-table`, `v-form`, `v-snackbar`, `v-dialog`; respetar densidad y breakpoints.
- Rutas con `meta.roles`; guardas en `router`.
- Sin SSR, SEO ni i18n en v1.0.0.

## 11. Git, ramas y entregas

- Permanecer en la rama asignada por la sesión; no crear ramas por iniciativa.
- No hacer commit, push ni PR sin solicitud explícita.
- Antes de cualquier commit, `git status` y `git diff` deben mostrar solo archivos previstos.
- No versionar secretos, `.env`, llaves R2, credenciales WhatsApp ni tokens.
- Commits convencionales: `docs:`, `feat:`, `fix:`, `refactor:`, `test:`, `chore:`.
- Cada cambio de contrato se refleja en `docs/global-spec.md` y en el `spec.md` del módulo.
- Un cambio incompatible exige `MAJOR`; no disfrazarlo como `PATCH`.

## 12. Definition of Done

Una tarea está terminada solo si:

- Se cumplen las capas y dependencias.
- La spec y los tests fueron actualizados antes del código.
- Los tests automáticos pasan y la cobertura mínima se cumple.
- La mutación crítica es transaccional y auditada.
- La idempotencia y la sincronización fueron probadas.
- No hay secretos, PII ni tokens en logs.
- La documentación no contradice a `docs/global-spec.md`.
- No se modificaron archivos fuera del alcance autorizado.

## 13. Prohibiciones inmediatas

- Modificar código fuente durante la fase de revisión documental.
- Introducir React Native, Expo, Zustand o Axios para el cliente móvil.
- Mantener Quasar como framework del admin destino.
- Usar EF Core, MediatR o CQRS.
- Implementar sincronización sin outbox, idempotencia o watermark.
- Implementar una sesión única global que mezcle WebAdmin y MobileApp.
- Escribir `logs_eventos` con UPDATE/DELETE.
- Confirmar una notificación externa como atención recibida.
- Añadir bounded contexts fuera de los cinco aprobados.

## 14. Documentación asociada

- [`README.md`](README.md): visión, diagrama, decisiones, quick start.
- [`docs/global-spec.md`](docs/global-spec.md): arquitectura, sync, seguridad, roadmap.
- `docs/modules/auth-session/`: identidad, sesiones y credibilidad.
- `docs/modules/governance-votes/`: votaciones y firmas.
- `docs/modules/emergencies/`: alertas, guardias, incidentes y notificaciones.
- `docs/modules/directory/`: comunidad, lotes, fichas y visibilidad.
- `docs/modules/expenses/`: gastos, cargos, pagos y conciliación.