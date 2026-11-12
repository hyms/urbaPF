# Auth & Session — Especificación

- Versión: 1.0.0
- Estado: propuesta para revisión
- Spec global: [`docs/global-spec.md`](../../global-spec.md)

## 1. Propósito

`auth-session` concentra identidad, credenciales, roles, credibilidad y ciclo de vida de sesiones para los dos canales de UrbaPF. El módulo responde quién es el usuario, qué puede hacer y qué dispositivo/sesión está usando.

No gestiona el perfil público del directorio; solo los campos de identidad y acceso necesarios para autenticación y autorización.

## 2. Actores y canales

| Actor | Canal | Acceso principal |
|---|---|---|
| Vecino | `MobileApp` | Login, refresh, logout, validación de otros vecinos, lectura del directorio. |
| Guardia | `MobileApp` | Login y sesión móvil; recibe alertas operativas. |
| Encargado | `WebAdmin`, `MobileApp` | Gestión administrativa y aprobaciones según caso de uso. |
| Admin | `WebAdmin` | Gestión de usuarios, roles, credibilidad y sesiones. |
| Visitante | — | No autenticado; solo recursos públicos definidos por otros módulos. |

Las sesiones de `WebAdmin` y `MobileApp` se administran por separado para un mismo usuario.

## 3. Modelo de dominio

| Entidad | Campos relevantes | Responsabilidad |
|---|---|---|
| `User` | `id`, `community_id`, `email`, `password_hash`, `role`, `credibility`, `status`, `version`, `deleted_at` | Identidad y autorización base. |
| `UserChannelSession` | `id`, `user_id`, `channel`, `device_label`, `platform`, `push_token_hash`, `last_seen_at`, `revoked_at`, `version` | Sesión activa por usuario y canal. |
| `RefreshToken` | `id`, `session_id`, `token_hash`, `family_id`, `expires_at`, `revoked_at`, `replaced_by`, `reuse_detected_at` | Rotación y detección de reuso. |
| `TrustVoucher` | `id`, `giver_user_id`, `target_user_id`, `community_id`, `created_at` | Voto de confianza para validar vecinos. |
| `PasswordChangeAudit` | `id`, `user_id`, `changed_at`, `channel` | Auditoría de cambio de contraseña. |

`User.status` admite `Active`, `PendingValidation`, `Suspended` y `Deleted`. Los usuarios eliminados mantienen su historial por integridad referencial.

## 4. Reglas de negocio

### 4.1 Registro y validación

1. El registro crea un usuario `PendingValidation` con rol `Visitante`.
2. El email se normaliza a minúsculas y debe ser único por comunidad.
3. La contraseña cumple una política mínima configurable; el hash nunca se devuelve por API.
4. Un vecino válido se convierte en `Vecino` cuando acumula **2 `TrustVoucher` válidos de vecinos distintos**.
5. Un usuario no puede validarse a sí mismo ni emitir más de un voucher por objetivo.
6. La validación y el cambio de rol generan auditoría y evento de dominio.
7. Un Admin puede suspender, reactivar o cambiar un rol; nunca puede eliminarse físicamente si tiene historial.

### 4.2 Credibilidad

1. La credibilidad es un entero entre 1 y 5.
2. El valor inicial de un vecino validado es 3.
3. Solo un Admin puede cambiarla; el cambio se audita con motivo y actor.
4. Los efectos operativos se aplican en `emergencies` y en la visibilidad de contenido, no en este módulo.

### 4.3 Autenticación

1. El login valida email, contraseña, estado y comunidad.
2. El password se verifica con PBKDF2 HMAC-SHA256, 100.000 iteraciones.
3. El login emite un access token JWT de corta vida y un refresh token de larga vida.
4. El JWT incluye `sub`, `sid`, `channel`, `community_id`, `role`, `credibility`, `iat`, `exp` y `jti`. No incluye el refresh token ni datos sensibles.
5. El refresh token se almacena hasheado; el valor opaco nunca se registra.
6. Cada uso válido rota el refresh token y emite uno nuevo.
7. Si un refresh antiguo se reutiliza, el servidor marca `TOKEN_REUSE_DETECTED`, revoca la familia completa y exige nuevo login.

### 4.4 Una sesión activa por canal

1. La unicidad se aplica a `(user_id, channel)` entre sesiones no revocadas.
2. Un nuevo login `MobileApp` revoca la sesión móvil anterior y su familia de refresh.
3. Un nuevo login `WebAdmin` revoca únicamente la sesión `WebAdmin` anterior.
4. Un logout revoca la sesión actual y su familia; no afecta al otro canal.
5. Un Admin puede revocar una sesión `WebAdmin` o `MobileApp` de otro usuario, dejando registro de auditoría.
6. Los access tokens ya emitidos siguen vigentes hasta su expiración corta; la revocación inmediata se logra con listas de comprobación de `sid` para operaciones críticas.

### 4.5 Dispositivos y push

- El cliente registra `platform`, `device_label` y hash del token FCM al iniciar sesión.
- Un token FCM se asocia como máximo a una sesión activa por dispositivo.
- Al cerrar sesión o revocar, el token se desvincula para evitar notificaciones a un dispositivo que ya no tiene acceso.

## 5. Casos de uso

| ID | Caso de uso | Actor | Resultado |
|---|---|---|---|
| AU-01 | Registrarse | Visitante | Usuario `PendingValidation` y evento `USER_REGISTERED`. |
| AU-02 | Validar vecino | Vecino | Segundo voucher promueve a `Vecino` y emite `USER_VALIDATED`. |
| AU-03 | Iniciar sesión móvil | Vecino/Guardia | Sesión `MobileApp` activa; sesiones móviles previas revocadas. |
| AU-04 | Iniciar sesión web | Encargado/Admin | Sesión `WebAdmin` activa; sesión móvil intacta. |
| AU-05 | Renovar token | Cliente autenticado | Nuevo access/refresh; familia rotada. |
| AU-06 | Cerrar sesión | Cliente autenticado | Familia revocada y sesión marcada. |
| AU-07 | Cambiar contraseña | Usuario autenticado | Hash actualizado; otras sesiones del mismo canal revocadas. |
| AU-08 | Cambiar rol | Admin | `ROLE_CHANGED` auditado. |
| AU-09 | Cambiar credibilidad | Admin | `CREDIBILITY_UPDATED` auditado. |
| AU-10 | Revocar sesión | Admin/Usuario | `SESSION_REVOKED` auditado. |

## 6. Contrato API

Todas las rutas usan `/api/v1` y `application/json`, salvo carga de evidencias.

| Método | Ruta | Autenticación | Propósito |
|---|---|---|---|
| POST | `/auth/register` | No | Crear usuario; requiere `X-Idempotency-Key`. |
| POST | `/auth/login` | No | Crear sesión del canal indicado por `channel`. |
| POST | `/auth/refresh` | Refresh | Rotar tokens. |
| POST | `/auth/logout` | Access | Revocar familia de la sesión actual. |
| GET | `/auth/me` | Access | Devuelve identidad, rol, credibilidad y sesión. |
| GET | `/auth/sessions` | Access/Admin | Listar sesiones visibles. |
| DELETE | `/auth/sessions/{id}` | Access/Admin | Revocar sesión autorizada. |
| POST | `/users/{id}/trust-vouchers` | Vecino/Encargado | Emitir voucher de confianza. |
| PATCH | `/users/{id}/role` | Admin | Cambiar rol. |
| PATCH | `/users/{id}/credibility` | Admin | Cambiar credibilidad. |
| POST | `/users/{id}/password` | Usuario/Admin | Cambiar contraseña. |

Reglas de contrato:

- `channel` solo acepta `WebAdmin` o `MobileApp`.
- Un token `MobileApp` no puede invocar rutas administrativas, aunque el rol sea compatible.
- El login no usa `X-Idempotency-Key`: cada login es una nueva rotación de sesión intencional.
- `register` y `password` sí la usan para evitar duplicados por reintento.
- Los errores usan `application/problem+json` y códigos estables.

## 7. Sincronización y offline

- Los tokens, hashes de refresh y sesiones de terceros **nunca** se sincronizan al dispositivo.
- El cliente puede cachear `GET /auth/me` y los campos de identidad del propio usuario en SQLDelight.
- La colección delta `auth` expone únicamente `me` y cambios de rol/credibilidad del usuario actual.
- Si el access token expira durante un sync, el cliente intenta un refresh; si falla, pausa el outbox y muestra “sesión expirada” sin borrar mutaciones.
- Una mutación rechazada por `401` no se reintenta automáticamente.
- El logout no elimina el outbox pendiente; lo marca y solicita confirmación antes de descartarlo.

## 8. Eventos de auditoría

| Evento | Cuándo | Payload mínimo |
|---|---|---|
| `USER_REGISTERED` | Registro exitoso. | `user_id`, `community_id`, `email_normalizado`. |
| `USER_VALIDATED` | Segundo voucher válido. | `user_id`, `voucher_ids`. |
| `ROLE_CHANGED` | Cambio de rol. | `from`, `to`, `actor_id`, `motivo`. |
| `CREDIBILITY_UPDATED` | Cambio de credibilidad. | `from`, `to`, `motivo`. |
| `LOGIN` | Sesión creada. | `session_id`, `channel`, `platform`; nunca token. |
| `LOGOUT` | Logout/revocación. | `session_id`, `channel`, `motivo`. |
| `TOKEN_REFRESHED` | Rotación válida. | `session_id`, `family_id`. |
| `TOKEN_REUSE_DETECTED` | Reutilización de refresh. | `session_id`, `family_id`, `ip_hash`. |
| `SESSION_REVOKED` | Revocación administrativa. | `session_id`, `actor_id`, `motivo`. |
| `PASSWORD_CHANGED` | Cambio de contraseña. | `user_id`, `channel`. |

## 9. Errores

| Código HTTP | `errorCode` | Causa |
|---:|---|---|
| 400 | `VALIDATION_ERROR` | Payload o política de contraseña inválida. |
| 401 | `INVALID_CREDENTIALS` | Email/contraseña incorrectos. |
| 401 | `TOKEN_EXPIRED` | Access token expirado. |
| 401 | `TOKEN_REUSE_DETECTED` | Familia revocada por reutilización. |
| 403 | `CHANNEL_FORBIDDEN` | Canal incompatible con la ruta. |
| 403 | `NOT_COMMUNITY_MEMBER` | Usuario de otra comunidad. |
| 404 | `USER_NOT_FOUND` | Usuario inexistente o eliminado. |
| 409 | `IDEMPOTENCY_KEY_REUSED` | Misma clave con payload distinto. |
| 409 | `SESSION_ALREADY_ACTIVE` | Condición de carrera en login. |
| 423 | `ACCOUNT_SUSPENDED` | Usuario suspendido. |

## 10. Edge cases

- Doble clic en login: la segunda solicitud puede ganar y revocar la primera; el cliente debe usar la sesión confirmada por la última respuesta.
- Refresh perdido por caída de red: el cliente reintenta el mismo refresh; si el servidor ya lo rotó, recibe `TOKEN_REUSE_DETECTED` y debe limpiar la sesión local de forma controlada.
- Cambio de contraseña con otras sesiones activas: se revocan todas las familias del canal donde ocurre el cambio.
- Voucher emitido por un usuario suspendido: no cuenta.
- Usuario de otra comunidad intentando emitir voucher: `403`.
- Email con mayúsculas/espacios: se normaliza antes de buscar y persistir.
- Usuario eliminado con historial: se conserva por auditoría, pero no puede autenticarse.

## 11. Fuera de alcance

- Perfil público, foto, dirección y contactos del directorio.
- Autorización concreta de votaciones, emergencias o expensas.
- Recuperación por SMS/WhatsApp y verificación telefónica.
- MFA, SSO o WebAuthn en v1.0.0.

## 12. Trazabilidad

- Contratos transversales: [`docs/global-spec.md`](../../global-spec.md)
- Plan: [`plan.md`](plan.md)
- Tests: [`tests.md`](tests.md)