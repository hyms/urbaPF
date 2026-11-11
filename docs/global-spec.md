# UrbaPF - Especificación Global (v1.0.0)

## 🎯 Visión General

**UrbaPF** es una plataforma integral de coordinación vecinal diseñada para Santa Cruz, Bolivia. Digitaliza la convivencia en barrios y condominios, centralizando seguridad, comunicación y democracia participativa.

**Objetivo v1.0.0**: Baseline de producción funcional con arquitectura robusta, auditable y móvil-first.

---

## 🏛️ Modelo de Dominio (Bounded Contexts)

### 1. Identity & Access (Identidad y Acceso)
- **Entidades**: User, RefreshToken, Role, CredibilityLevel
- **Responsabilidades**: Autenticación JWT + Refresh Tokens, registro, validación vecinal (2 votos de confianza), gestión de roles, credibilidad (1-5)
- **Eventos**: `USER_REGISTERED`, `USER_VALIDATED`, `ROLE_CHANGED`, `CREDIBILITY_UPDATED`, `TOKEN_REFRESHED`, `PASSWORD_CHANGED`

### 2. Governance (Gobernanza - Votaciones)
- **Entidades**: Poll, Vote, PollOption
- **Responsabilidades**: Creación de votaciones (vecinos: 2 aprobaciones; encargados: directo), votación con firma digital SHA256, integridad inmutable, resultados
- **Reglas**: Solo Vecinos y Encargados votan. Admin no vota. Inmutabilidad en estados Active/Closed.
- **Eventos**: `POLL_CREATED`, `POLL_APPROVED`, `POLL_ACTIVATED`, `VOTE_CAST`, `POLL_CLOSED`

### 3. Communication (Comunicación - Tablón Social)
- **Entidades**: Post, Comment, PostCategory
- **Responsabilidades**: Publicaciones (anuncios, fijadas, categorías), comentarios anidados, moderación por credibilidad (Nivel 1 colapsado), aprobaciones
- **Eventos**: `POST_CREATED`, `POST_APPROVED`, `COMMENT_ADDED`, `COMMENT_HIDDEN`

### 4. Safety (Seguridad - Emergencias/Incidentes)
- **Entidades**: Alert, Incident, AlertType, IncidentType
- **Responsabilidades**: Alertas de emergencia con geolocalización (Leaflet/OSM), flujo Reportado→En Proceso→Resuelto→Cerrado, re-notificación, reputación determina routing (Nivel ≥4 → Guardias directo; <4 → Aprobación Manager)
- **Eventos**: `ALERT_CREATED`, `ALERT_APPROVED`, `ALERT_ACKNOWLEDGED`, `ALERT_RESOLVED`, `INCIDENT_CREATED`, `INCIDENT_STATUS_UPDATED`

### 5. Community (Comunidad - Condominios)
- **Entidades**: Condominium, Lot, Expense
- **Responsabilidades**: Gestión de condominios, manzanos/lotes, expensas, cuotas mensuales
- **Eventos**: `CONDOMINIUM_CREATED`, `EXPENSE_CREATED`, `EXPENSE_PAID`

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
| Ver Tablón Social privado | ❌ | ✅ | ❌ | ✅ | ✅ |

---

## 📊 Niveles de Credibilidad (1-5)

| Nivel | Nombre | Efecto en Emergencias | Efecto en Comentarios |
|-------|--------|----------------------|----------------------|
| 5 | Referente | Prioridad máxima, directo a Guardias | Máxima visibilidad |
| 4 | Confiable | Directo a Guardias | Alta visibilidad |
| 3 | Neutral | Aprobación Manager | Normal |
| 2 | Bajo | Aprobación Manager | Reducida |
| 1 | Irreverente | Aprobación Manager | Colapsado/Oculto |

---

## 🗄️ Event Sourcing - Auditoría Inmutable

### Tabla `logs_eventos` (Append-only)
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

### Reglas de Oro
1. **Nunca** actualizar estados críticos sin `INSERT INTO logs_eventos`
2. **Transacción atómica**: Operación de negocio + Evento de auditoría
3. **Hash encadenado**: `hash_verificacion = SHA256(prev_hash + current_event_json)` para integridad
4. **Prohibido**: `UPDATE`/`DELETE` en `logs_eventos`

### Eventos Obligatorios por Contexto
| Contexto | Eventos Requeridos |
|----------|-------------------|
| Identity | USER_REGISTERED, USER_VALIDATED, ROLE_CHANGED, CREDIBILITY_UPDATED, LOGIN, TOKEN_REFRESHED |
| Governance | POLL_CREATED, POLL_APPROVED, POLL_ACTIVATED, VOTE_CAST, POLL_CLOSED |
| Communication | POST_CREATED, POST_APPROVED, COMMENT_ADDED, COMMENT_HIDDEN |
| Safety | ALERT_CREATED, ALERT_APPROVED, ALERT_ACKNOWLEDGED, ALERT_RESOLVED, INCIDENT_CREATED, INCIDENT_STATUS_UPDATED |
| Community | CONDOMINIUM_CREATED, EXPENSE_CREATED, EXPENSE_PAID |

---

## 🔧 Stack Tecnológico v1.0.0

| Capa | Tecnología | Versión |
|------|------------|---------|
| Backend Runtime | .NET | 10.0 |
| API Style | Minimal APIs | - |
| ORM/Data Access | Dapper | 2.1.72 |
| Migrations | FluentMigrator | 8.0.1 |
| Database | PostgreSQL + PostGIS | 16+ |
| Auth | JWT + Refresh Tokens | PBKDF2 100k iter |
| Testing | NUnit + FluentAssertions + Moq | 4.5.1 / 8.9.0 / 4.20.72 |
| Frontend Framework | React Native (Expo Managed) | SDK 51+ |
| State Management | Zustand | 4.x |
| Navigation | Expo Router | 4.x |
| Offline DB | Expo SQLite / expo-sqlite | 14.x |
| HTTP Client | Axios | 1.14.0 |
| Maps | react-native-maps / expo-map | - |
| Testing | Vitest + React Native Testing Library | 4.1.2 |
| Package Manager | pnpm | 9.x |
| CI/CD | GitHub Actions | - |
| File Storage | Cloudflare R2 (S3-compatible) | - |
| Notifications | WhatsApp Business API / Push | - |

---

## 📐 Principios Arquitectónicos

### Backend: Clean Architecture + DDD (Ligero)
```
src/
├── UrbaPF.Domain/          # Núcleo puro: Entidades, VOs, Domain Services, Domain Events, Repository Interfaces
├── UrbaPF.Application/     # Orquestación: Application Services, DTOs, Validators, Commands/Queries (light)
├── UrbaPF.Infrastructure/  # Implementaciones: Repositories (Dapper), Migrations, External Services, Event Store
├── UrbaPF.Api/             # Presentation: Minimal APIs, Routes, DI, Middleware
└── UrbaPF.Tests/           # Unit + Integration (Mock-First)
```

**Reglas DDD**:
- Domain **no** referencia Infrastructure ni Application
- Application referencia Domain, **no** Infrastructure
- Infrastructure implementa interfaces de Domain
- Api referencia Application + Infrastructure (solo para DI)

### Frontend: Mobile-First + Offline-First
```
frontend/
├── app/                    # Expo Router (file-based routing)
│   ├── (auth)/             # Auth stack
│   ├── (tabs)/             # Main tabs
│   └── _layout.tsx
├── src/
│   ├── api/                # Axios + interceptors (401/refresh)
│   ├── store/              # Zustand stores (auth, polls, posts, alerts, offline)
│   ├── hooks/              # Custom hooks (useOfflineSync, useAuth, etc.)
│   ├── components/         # Reusable UI
│   ├── utils/              # Crypto, date, validation
│   ├── types/              # TypeScript types (shared with backend DTOs)
│   └── db/                 # SQLite offline engine
│       ├── schema.ts
│       ├── sync-engine.ts
│       └── migrations/
```

---

## 🧪 Estrategia de Testing (SDD + TDD)

### Specification-Driven Development (SDD)
Cada módulo tiene en `docs/modules/{module}/`:
- `spec.md`: Qué hace, reglas de negocio, casos de uso, edge cases
- `plan.md`: Pasos de implementación, dependencias, estimación
- `tests.md`: Escenarios de prueba (given/when/then), coverage targets

### Test-Driven Development (TDD)
1. **Red**: Escribir test que falle (basado en `tests.md`)
2. **Green**: Implementar mínimo para pasar
3. **Refactor**: Limpiar manteniendo tests verdes

### Cobertura Mínima: 40%
- Backend: NUnit + Moq (Mock-First: **nunca** DB real en unit tests)
- Frontend: Vitest + React Native Testing Library

---

## 🚀 Quick Start (v1.0.0)

### Prerrequisitos
- .NET 10 SDK
- Node.js 20+ / pnpm 9+
- PostgreSQL 16+ con PostGIS
- Expo CLI (`npm i -g @expo/cli`)

### Backend
```bash
cd backend
dotnet restore
dotnet run --project src/UrbaPF.Api
# API en http://localhost:5000
# Swagger en http://localhost:5000/swagger
```

### Frontend
```bash
cd frontend
pnpm install
pnpm dev
# Expo DevTools en terminal, app en dispositivo/emulador
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

## 📦 Versionado SemVer 2.0

| Versión | Significado |
|---------|-------------|
| 1.0.0 | Primera release de producción funcional |
| 1.x.x | Features backwards-compatible |
| 2.0.0 | Breaking changes |

**Reglas**:
- `MAJOR`: Cambios incompatibles en API pública o esquema de BD
- `MINOR`: Nueva funcionalidad compatible
- `PATCH`: Bug fixes compatibles
- Tags Git: `v{MAJOR}.{MINOR}.{PATCH}`

---

## 📋 Roadmap Post-v1.0.0

### v1.1.0 - Notificaciones Push
- OneSignal / Expo Push integration
- Centro de notificaciones in-app
- Migración BD: tabla `notifications`

### v1.2.0 - Mensajería Directa
- Chat 1:1 y grupos
- SignalR / WebSockets
- Cifrado end-to-end opcional

### v2.0.0 - Multi-Comunidad & Marketplace
- Usuario en múltiples condominios
- Marketplace vecinal (trueque, servicios)
- Breaking: modelo de datos usuarios