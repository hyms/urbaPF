# UrbaPF - Technical Master Instructions (v1.0.0)

Este archivo es la "fuente de verdad" técnica para el desarrollo de UrbaPF v1.0.0.

Stack: **.NET 10** (backend) + **React Native / Expo Managed** (frontend mobile-first).

---

## 🏗️ Arquitectura del Backend (.NET 10)

### Capas y dependencias (Clean Architecture + DDD ligero)

```
UrbaPF.Domain/          # Núcleo puro: Entidades, Value Objects, Domain Services,
                        # Domain Events, Repository Interfaces. Sin referencias
                        # a Application ni Infrastructure.
UrbaPF.Application/     # Orquestación: Application Services, DTOs, Validators.
                        # Referencia SOLO Domain.
UrbaPF.Infrastructure/  # Implementaciones: Repositorios (Dapper), FluentMigrator,
                        # servicios externos, Event Store (logs_eventos).
                        # Implementa interfaces de Domain.
UrbaPF.Api/             # Presentación: Minimal APIs, Routes, DI, Middleware.
                        # Referencia Application + Infrastructure (solo DI).
```

### Reglas de capas (obligatorias)

1. **Domain** no referencia ni `Application` ni `Infrastructure`.
2. **Application** referencia `Domain`, **no** `Infrastructure`.
3. **Infrastructure** implementa interfaces de `Domain`.
4. **Api** referencia `Application` + `Infrastructure` únicamente para DI.
5. Los servicios de dominio **NO** heredan de `BaseRepository`. Inyectan interfaces de repositorio (política Mock-First).
6. **No usar EF Core** para lógica de negocio. Acceso a datos con **Dapper 2.1.72**.
7. Migraciones con **FluentMigrator 8.0.1** sobre **PostgreSQL + PostGIS**.

### Application Layer (ligera)

- Application Services orquestran Domain Services.
- **Sin MediatR ni CQRS** (principio YAGNI).
- DTOs para entrada/salida de la API.

---

## 📱 Arquitectura del Frontend (React Native)

- **Framework:** React Native con **Expo Managed** (SDK 51+).
- **Ruteo:** Expo Router (file-based, `app/`).
- **Estado:** **Zustand** (stores: auth, polls, posts, alerts, offline).
- **Offline-first:** `expo-sqlite` + sync engine propio (`src/db/`).
- **HTTP:** Axios 1.14.0 con interceptores para 401 / refresh de token.
- **Mapas:** `react-native-maps` / `expo-map` (OpenStreetMap).
- **Package manager:** pnpm 9.x.
- **Tests:** Vitest 4.1.2 + React Native Testing Library.
- **TypeScript:** 6.0.2.

---

## 🧪 Calidad y Pruebas (SDD + TDD)

### SDD (Specification-Driven Development)
Cada módulo tiene en `docs/modules/{module}/`:
- `spec.md`: qué hace, reglas de negocio, casos de uso, edge cases.
- `plan.md`: pasos de implementación, dependencias, estimación.
- `tests.md`: escenarios de prueba (given/when/then), coverage targets.

### TDD ciclo
1. **Red:** escribir test que falle (basado en `tests.md`).
2. **Green:** implementar mínimo para pasar.
3. **Refactor:** limpiar manteniendo tests verdes.

### Frameworks y cobertura
- Backend: **NUnit 4.5.1** + **FluentAssertions 8.9.0** + **Moq 4.20.72**.
- **Mock-First:** prohibido depender de DB real en tests unitarios.
- Cobertura mínima: **40%** (backend y frontend).
- Frontend: Vitest 4.1.2 + TypeScript 6.0.2.

---

## 🛡️ Reglas de Oro Técnicas

1. **Seguridad:** PBKDF2 (HMAC-SHA256) con 100k iteraciones.
2. **JWT + Refresh Tokens:** acceso de corta vida, refresh de larga vida, rotación en cada uso.
3. **Soft delete:** `UPDATE {table} SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id`.
4. **Auditoría (Event Sourcing ligero):** cada acción crítica genera un evento inmutable en `logs_eventos` dentro de la misma transacción. Append-only: prohibido `UPDATE`/`DELETE` en `logs_eventos`.
5. **Hash de integridad:** `hash_verificacion = SHA256(prev_hash + current_event_json)`.
6. **Single active session:** un usuario solo puede tener una sesión activa; un nuevo login invalida el refresh token previo.
7. **S.O.L.I.D.:** Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion.
8. **DRY / YAGNI / KISS:** evitar duplicación, no implementar "por si acaso", priorizar simplicidad.

---

## 🗄️ Sistema de Auditoría (Event Sourcing Ligero)

### Tabla `logs_eventos` (append-only)
```sql
CREATE TABLE logs_eventos (
    id BigSerial PRIMARY KEY,
    uuid UUID UNIQUE DEFAULT gen_random_uuid(),
    fecha TIMESTAMPTZ DEFAULT NOW(),
    usuario_id UUID NOT NULL,
    comunidad_id UUID NOT NULL,
    tipo_evento VARCHAR(100) NOT NULL,
    entidad_id UUID NOT NULL,
    data_json JSONB NOT NULL,
    hash_verificacion TEXT
);
```

### Reglas
1. Nunca actualizar estados críticos sin `INSERT INTO logs_eventos`.
2. Transacción atómica: operación de negocio + evento de auditoría.
3. Hash encadenado para integridad; si se rompe la cadena, alerta de integridad.
4. Prohibido `UPDATE`/`DELETE` en `logs_eventos`.

### Eventos obligatorios por contexto
| Contexto | Eventos |
|----------|---------|
| Identity | USER_REGISTERED, USER_VALIDATED, ROLE_CHANGED, CREDIBILITY_UPDATED, LOGIN, TOKEN_REFRESHED, PASSWORD_CHANGED |
| Governance | POLL_CREATED, POLL_APPROVED, POLL_ACTIVATED, VOTE_CAST, POLL_CLOSED |
| Communication | POST_CREATED, POST_APPROVED, COMMENT_ADDED, COMMENT_HIDDEN |
| Safety | ALERT_CREATED, ALERT_APPROVED, ALERT_ACKNOWLEDGED, ALERT_RESOLVED, INCIDENT_CREATED, INCIDENT_STATUS_UPDATED |
| Community | CONDOMINIUM_CREATED, EXPENSE_CREATED, EXPENSE_PAID |

---

## 🔐 Matriz de Roles y Permisos

| Acción | Visitante | Vecino | Guardia | Encargado | Admin |
|--------|-----------|--------|---------|-----------|-------|
| Ver publicaciones | ✅ | ✅ | ✅ | ✅ | ✅ |
| Ver emergencias | ✅ | ✅ | ✅ | ✅ | ✅ |
| Publicar | ❌ | ✅ | ❌ | ✅ | ✅ |
| Votar | ❌ | ✅ | ❌ | ✅ | ❌ |
| Crear votación | ❌ | 2 aprobaciones | ❌ | Directo | Gestiona |
| Botón "En Camino" | ❌ | ❌ | ✅ | ❌ | ❌ |
| Aprobar contenido | ❌ | ❌ | ❌ | ✅ | ✅ |
| Gestionar staff | ❌ | ❌ | ❌ | ❌ | ✅ |

### Credibilidad (1-5)
- ≥4: alertas de emergencia directo a Guardias.
- <4: alertas requieren aprobación de Manager.
- 1: comentarios colapsados/ocultos por defecto.

---

## 🚀 Quick Start (para agentes)

### Backend
```bash
cd backend
dotnet restore
dotnet run --project src/UrbaPF.Api
# API en http://localhost:5000
```

### Frontend
```bash
cd frontend
pnpm install
pnpm dev
```

### Tests
```bash
# Backend
cd backend
dotnet test --collect:"XPlat Code Coverage"

# Frontend
cd frontend
pnpm test --coverage
```

---

## 📂 Documentación asociada

- `docs/global-spec.md`: especificación global (visión, bounded contexts, stack, principios, roadmap).
- `docs/modules/{auth,polls,posts,alerts,users,condominiums,incidents}/`: spec.md, plan.md, tests.md por módulo (SDD).
- `README.md`: visión de negocio, gobernanza, decisiones de arquitectura, quick start, estructura.
