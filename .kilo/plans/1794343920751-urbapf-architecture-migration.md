# UrbaPF Architecture Migration Plan

## 🎯 Goal
Migrate UrbaPF to a production-ready v1.0.0 with:
- **Backend**: .NET 10 + DDD + Lightweight Application Layer + SDD/TDD
- **Frontend**: React Native (Expo Managed) + Zustand + Custom SQLite Offline-First
- **Docs**: `docs/` with global spec + module specs
- **README**: Badges, Mermaid diagram, Architectural Decisions, Quick Start

---

## 📋 Decisions Summary

| Area | Decision |
|------|----------|
| RN Framework | Expo (Managed) |
| State Management | Zustand |
| Backend App Layer | Lightweight Application Services (no MediatR) |
| Offline Strategy | Custom SQLite + Sync Engine |
| Versioning | SemVer 2.0 → v1.0.0 |
| Testing | NUnit + FluentAssertions (Backend), Vitest (Frontend), Mock-First |

---

## 🏗️ Backend Architecture (DDD + Lightweight App Layer)

### Layer Structure
```
backend/
├── src/
│   ├── UrbaPF.Domain/           # Pure domain: Entities, Value Objects, Domain Services, Domain Events, Repository Interfaces
│   ├── UrbaPF.Application/      # Application Services, DTOs, Commands/Queries (lightweight), Validators
│   ├── UrbaPF.Infrastructure/   # Dapper Repositories, FluentMigrator, External Services, Event Store
│   ├── UrbaPF.Api/              # Minimal APIs, Route registration, DI config
│   └── UrbaPF.Tests/            # Unit + Integration tests (Mock-First)
└── UrbaPF.sln
```

### Key Refactoring Tasks
1. **Create `UrbaPF.Application` project** - New project for Application Services, DTOs, Validators
2. **Move DTOs** from `Api/DTOs` → `Application/DTOs`
3. **Extract Application Services** from Infrastructure Services (AuthService, UserService, PollService, etc.)
   - These orchestrate Domain Services + Repositories
   - Return DTOs, not Entities
4. **Domain Layer cleanup**
   - Entities stay pure (no infrastructure deps)
   - Domain Services contain business rules
   - Repository interfaces in Domain
5. **Infrastructure Layer**
   - Implement Repository interfaces
   - Event Sourcing: `AuditRepository` writes to `logs_eventos`
   - FluentMigrator migrations
6. **API Layer**
   - Thin Minimal API endpoints
   - Call Application Services only
   - No business logic in routes

### Application Service Pattern (Lightweight)
```csharp
// Application/DTOs/CreatePollDto.cs
public record CreatePollDto(Guid CommunityId, string Question, List<string> Options);

// Application/Services/PollApplicationService.cs
public class PollApplicationService
{
    private readonly IPollRepository _pollRepo;
    private readonly PollDomainService _pollDomain;
    private readonly IAuditService _audit;

    public async Task<PollDto> CreateAsync(CreatePollDto dto, Guid userId)
    {
        var poll = _pollDomain.CreatePoll(dto.CommunityId, dto.Question, dto.Options, userId);
        await _pollRepo.AddAsync(poll);
        await _audit.LogAsync("POLL_CREATED", poll.Id, new { dto.Question });
        return poll.ToDto();
    }
}
```

---

## 📱 Frontend Architecture (React Native + Expo)

### Structure
```
frontend/
├── app/                       # Expo Router (file-based routing)
│   ├── (auth)/                # Auth stack
│   ├── (tabs)/                # Main tabs stack
│   │   ├── index.tsx          # Home/Feed
│   │   ├── alerts.tsx         # Emergencies
│   │   ├── polls.tsx          # Votaciones
│   │   ├── posts.tsx          # Tablón Social
│   │   └── profile.tsx        # Perfil
│   └── _layout.tsx
├── src/
│   ├── api/                   # Axios instance + interceptors
│   ├── store/                 # Zustand stores (auth, polls, posts, alerts, offline)
│   ├── hooks/                 # Custom hooks (useOfflineSync, useAuth, etc.)
│   ├── components/            # Reusable UI components
│   ├── utils/                 # Helpers (crypto, date, validation)
│   ├── types/                 # TypeScript types (shared with backend DTOs)
│   └── db/                    # SQLite offline engine
│       ├── schema.ts          # WatermelonDB-like schema
│       ├── sync-engine.ts     # Custom sync logic
│       └── migrations/
├── assets/
├── package.json
├── app.json
├── tsconfig.json
└── eslint.config.js
```

### Key Implementation Tasks
1. **Initialize Expo project** - `npx create-expo-app@latest frontend --template blank-typescript`
2. **Install dependencies**:
   - `expo-router`, `expo-sqlite`, `zustand`, `axios`, `@react-native-async-storage/async-storage`
   - `react-native-mmkv` (fast sync storage)
   - `expo-secure-store` (tokens)
   - `react-native-get-random-values`, `uuid` (crypto for vote signatures)
3. **Configure Axios** with interceptors for 401/refresh token flow
4. **Zustand stores**:
   - `useAuthStore` - tokens, user, refresh logic
   - `usePollsStore` - polls, votes, optimistic updates
   - `usePostsStore` - posts, comments
   - `useAlertsStore` - emergencies, geolocation
   - `useOfflineStore` - pending mutations, sync status
5. **SQLite Offline Engine**:
   - Schema mirrors backend tables (polls, votes, posts, comments, alerts)
   - Write-ahead log for pending mutations
   - Background sync on reconnect
   - Conflict resolution: server wins for votes, client wins for drafts
6. **Expo Router navigation** with auth guards
7. **Leaflet/Map** - `react-native-maps` or `expo-map` for emergencies

---

## 📚 Documentation Structure

```
docs/
├── global-spec.md                    # High-level project goals, domain model, bounded contexts
├── modules/
│   ├── auth/
│   │   ├── spec.md                   # Auth flows, tokens, roles
│   │   ├── plan.md                   # Implementation steps
│   │   └── tests.md                  # Test scenarios
│   ├── polls/
│   │   ├── spec.md
│   │   ├── plan.md
│   │   └── tests.md
│   ├── posts/
│   │   ├── spec.md
│   │   ├── plan.md
│   │   └── tests.md
│   ├── alerts/
│   │   ├── spec.md
│   │   ├── plan.md
│   │   └── tests.md
│   ├── users/
│   │   ├── spec.md
│   │   ├── plan.md
│   │   └── tests.md
│   └── condominiums/
│       ├── spec.md
│       ├── plan.md
│       └── tests.md
└── architecture/
    ├── decisions.md                  # ADRs
    └── data-model.md                 # ERD, event store schema
```

### Global Spec Content
- Vision, domain model (User, Condominium, Poll, Post, Alert, Vote)
- Bounded contexts: Identity, Governance, Communication, Safety
- Event Sourcing rules
- Role matrix & credibility levels
- API contract overview

---

## 📖 README.md Rewrite

### Structure
```markdown
# UrbaPF

[![Build](https://img.shields.io/github/actions/workflow/status/.../ci.yml?label=Build)]()
[![Coverage](https://img.shields.io/codecov/c/github/...?label=Coverage)]()
[![Tech Stack](https://img.shields.io/badge/Stack-React%20Native%20%7C%20.NET%2010-blue)]()
[![License](https://img.shields.io/badge/License-MIT-green)]()

## Mermaid Architecture Diagram
```mermaid
graph TD
    A[React Native App\n(Expo + Zustand)] -->|HTTPS/REST + JWT| B[.NET 10 Minimal API]
    B --> C[PostgreSQL + PostGIS]
    B --> D[Cloudflare R2\nFile Storage]
    B --> E[WhatsApp Business API\nNotifications]
    B --> F[Event Store\nlogs_eventos]
```

## Decisiones Clave de Arquitectura
- **Clean Architecture + DDD**: Separation of Domain/Application/Infrastructure enables testability and mock-first.
- **Specification-Driven Development**: Every module has spec/plan/tests before code.
- **SemVer 1.0.0**: Production baseline, strict versioning from now on.
- **Event Sourcing Audit**: Immutable `logs_eventos` for governance transparency.
- **Offline-First Mobile**: Custom SQLite sync for unreliable connectivity in Santa Cruz.

## Quick Start
```bash
# Backend
cd backend
dotnet restore
dotnet run --project UrbaPF.Api

# Frontend
cd frontend
pnpm install
pnpm dev
```

## Tests
```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
pnpm test
```
```

---

## 🤖 agents.md Update

### New Rules to Add
1. **DDD Boundaries**: Domain = pure, Application = orchestration, Infrastructure = impl
2. **SDD Workflow**: Spec → Plan → Test → Code (red-green-refactor)
3. **React Native Conventions**: Expo Router, Zustand stores, custom hooks for data
4. **Offline-First**: All mutations go through sync engine; UI reads from local SQLite
5. **Single Active Session**: One Cloud Agent per workspace; stacked PRs for parallel work
6. **Security**: PBKDF2 100k, JWT + Refresh, vote signatures SHA256
7. **Audit**: Every governance action → `logs_eventos` (append-only)

---

## ✅ Implementation Order

### Phase 1: Backend Restructure (DDD + App Layer)
1. Create `UrbaPF.Application` project + references
2. Move DTOs → Application
3. Create Application Services (Auth, User, Poll, Post, Alert, Condominium)
4. Update DI in `Program.cs` to register Application Services
5. Refactor Routes to call Application Services
6. Run tests to verify

### Phase 2: Documentation
1. Create `docs/` structure
2. Write `global-spec.md`
3. Write module specs/plans/tests for each bounded context

### Phase 3: React Native Migration
1. Backup current `frontend/` → `frontend-web-archive/`
2. Initialize Expo project in `frontend/`
3. Configure TypeScript, ESLint, Prettier
4. Set up Axios, Zustand, SQLite, Expo Router
5. Implement Auth flow (login, register, token refresh)
6. Build core screens: Home, Polls, Posts, Alerts, Profile
7. Implement offline sync engine

### Phase 4: README + agents.md
1. Rewrite `README.md` with badges, Mermaid, decisions, quick start
2. Update `agents.md` with new rules

### Phase 5: Validation
1. Backend: `dotnet test` → 40%+ coverage
2. Frontend: `pnpm test` → 40%+ coverage
3. Integration: Run both, test auth + poll creation + offline sync
4. Tag v1.0.0

---

## ⚠️ Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Expo Managed limits native modules | Use Dev Client if needed; evaluate early |
| Custom SQLite sync complexity | Start simple: write-ahead log + background sync; iterate |
| DDD refactor breaks tests | Mock-First: tests use interfaces, not implementations |
| Frontend-backend contract drift | Shared TypeScript types generated from backend DTOs |
| Event store performance | Partition by `comunidad_id`; index on `tipo_evento`, `fecha` |

---

## 📦 Deliverables

- [ ] Restructured backend with Application layer
- [ ] `docs/global-spec.md` + 6 module specs
- [ ] React Native app (Expo) with auth, polls, posts, alerts, offline sync
- [ ] Updated `README.md` (badges, Mermaid, decisions, quick start)
- [ ] Updated `agents.md` (DDD, SDD, TDD, RN, security rules)
- [ ] All tests passing, 40%+ coverage
- [ ] Git tag `v1.0.0`