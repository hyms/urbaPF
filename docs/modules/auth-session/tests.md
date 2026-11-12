# Auth & Session — Estrategia de Pruebas

- Versión: 1.0.0
- Spec: [`spec.md`](spec.md)
- Plan: [`plan.md`](plan.md)

## 1. Alcance

Verificar identidad, credenciales, validación vecinal, rotación de tokens, una sesión activa por canal, revocación y auditoría. Los tests unitarios son Mock-First; la base de datos real solo se usa en pruebas de integración.

## 2. Distribución de pruebas

| Nivel | Herramienta sugerida | Objetivo |
|---|---|---|
| Dominio | NUnit + FluentAssertions | Reglas puras de rol, voucher y credibilidad. |
| Aplicación | NUnit + Moq | Orquestación y efectos de auditoría. |
| Integración | NUnit + PostgreSQL de prueba | Repositorios, índice único y transacciones. |
| Contrato | OpenAPI + tests de API | Headers, códigos HTTP y payload. |
| Cliente | Kotlin Test / Vitest | Refresh, storage seguro y guardas. |

## 3. Escenarios de dominio

| ID | Given | When | Then |
|---|---|---|---|
| AU-D-001 | Un usuario validado tiene 1 voucher válido | Solicita validación | Continúa `PendingValidation`. |
| AU-D-002 | Un usuario tiene 2 vouchers de vecinos distintos | Se procesa el segundo | Pasa a `Vecino` y expone `USER_VALIDATED`. |
| AU-D-003 | El emisor es el propio objetivo | Intenta validar | Rechaza con `VALIDATION_ERROR`. |
| AU-D-004 | El mismo emisor ya dio voucher | Repite el voto | No incrementa el contador. |
| AU-D-005 | La credibilidad actual es 5 | Solicita 6 | Rechaza el cambio. |
| AU-D-006 | La credibilidad actual es 1 | Solicita 0 | Rechaza el cambio. |

## 4. Escenarios de aplicación

| ID | Given | When | Then |
|---|---|---|---|
| AU-A-001 | Credenciales válidas y sin sesión móvil | Login `MobileApp` | Crea sesión, emite tokens y audita `LOGIN`. |
| AU-A-002 | Existe una sesión móvil activa | Nuevo login `MobileApp` | Revoca la anterior y su familia. |
| AU-A-003 | Existe una sesión web activa | Login `MobileApp` | Conserva la sesión `WebAdmin`. |
| AU-A-004 | Existe una sesión web activa | Nuevo login `WebAdmin` | Revoca solo la sesión web anterior. |
| AU-A-005 | Refresh válido | Solicita renovación | Rota token, audita `TOKEN_REFRESHED` y conserva `session_id`. |
| AU-A-006 | Refresh ya rotado | Se reutiliza | Emite `TOKEN_REUSE_DETECTED` y revoca la familia. |
| AU-A-007 | Usuario suspendido | Login | Rechaza con `ACCOUNT_SUSPENDED` y no crea sesión. |
| AU-A-008 | Token de canal móvil | Invoca ruta `WebAdmin` | Rechaza con `CHANNEL_FORBIDDEN`. |
| AU-A-009 | Cambio de contraseña | Se confirma | Actualiza hash, revoca familias del canal y audita. |

## 5. Escenarios de API y contrato

| ID | Escenario | Resultado esperado |
|---|---|---|
| AU-C-001 | Registro con `X-Idempotency-Key` repetida e igual payload | Respuesta original sin crear otro usuario. |
| AU-C-002 | Registro con misma clave y otro email | `409 IDEMPOTENCY_KEY_REUSED`. |
| AU-C-003 | Login sin `channel` | `400 VALIDATION_ERROR`. |
| AU-C-004 | Login con `channel=WebAdmin` desde app móvil | Tokens con claim `channel=WebAdmin`; el backend acepta la solicitud autenticada, la UI no debe usarla en móvil. |
| AU-C-005 | `GET /auth/me` sin token | `401`. |
| AU-C-006 | `GET /auth/sessions` como vecino | Solo devuelve sus propias sesiones. |
| AU-C-007 | `DELETE /auth/sessions/{id}` de otro usuario sin rol Admin | `403`. |
| AU-C-008 | Respuesta de login | No contiene `password_hash`, refresh token ni hash de token. |
| AU-C-009 | Error de validación | Content-Type `application/problem+json` y `errorCode` estable. |
| AU-C-010 | Endpoint con access token expirado | `401 TOKEN_EXPIRED` y header `WWW-Authenticate`. |

## 6. Escenarios de seguridad

| ID | Escenario | Resultado esperado |
|---|---|---|
| AU-SEC-001 | Revisar logs tras login | No aparece contraseña, JWT ni refresh token. |
| AU-SEC-002 | Access token manipulado | Firma inválida y `401`. |
| AU-SEC-003 | Token de otra comunidad | `403 NOT_COMMUNITY_MEMBER`. |
| AU-SEC-004 | Fuerza bruta de login | Rate limit activo y eventos de seguridad agregados. |
| AU-SEC-005 | Refresh token almacenado | Solo hash en base de datos; valor opaco en almacenamiento seguro del cliente. |
| AU-SEC-006 | Logout | La familia no puede rotarse de nuevo. |

## 7. Escenarios de resiliencia y cliente

| ID | Escenario | Resultado esperado |
|---|---|---|
| AU-R-001 | Red cae durante refresh | El cliente reintenta con backoff; no borra outbox. |
| AU-R-002 | App se reinicia con sesión activa | Recupera sesión desde almacenamiento seguro y renueva si toca. |
| AU-R-003 | App se reinicia sin sesión | Muestra login; conserva outbox archivado hasta confirmación. |
| AU-R-004 | Sesión revocada remotamente | El siguiente request recibe 401; el cliente limpia sesión y avisa. |
| AU-R-005 | Push token desvinculado | No llegan notificaciones del canal cerrado. |

## 8. Escenarios de sincronización

| ID | Escenario | Resultado esperado |
|---|---|---|
| AU-S-001 | Cambia rol/credibilidad del usuario actual | Aparece en el próximo delta `auth`. |
| AU-S-002 | Cambia rol de otro usuario | No se propaga al delta del usuario actual. |
| AU-S-003 | Cliente recibe `lastSyncTimestamp` repetido | No duplica perfil local. |
| AU-S-004 | Respuesta delta paginada | Watermark solo avanza con `hasMore=false`. |

## 9. Matriz de cobertura mínima

| Área | Mínimo |
|---|---:|
| Domain | 80% |
| Application | 70% |
| Infrastructure crítica | 60% |
| Global del módulo | 40% |

## 10. Criterios de aceptación de pruebas

- Ningún test unitario conecta a PostgreSQL real.
- Los escenarios AU-A-002, AU-A-003 y AU-A-004 están cubiertos por separado.
- La reutilización de refresh no puede producir dos familias activas.
- Las respuestas de autenticación se validan contra un esquema que prohíbe campos sensibles.
- Los eventos de auditoría se verifican en la misma transacción, no solo en el log.