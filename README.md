UrbaPF - Sistema de Coordinación Vecinal

Plataforma de gestión comunitaria y seguridad ciudadana diseñada específicamente para barrios y condominios en Santa Cruz, Bolivia. El sistema reemplaza la fragmentación de los grupos de chat tradicionales por una estructura de gobernanza digital robusta y democrática.

🚀 Visión del Proyecto

UrbaPF no es solo una app de reportes; es un sistema de identidad donde la participación social y política del vecino está validada por la propia comunidad, garantizando que solo residentes reales tomen decisiones sobre su entorno.

🛠 Stack Técnico

Backend: .NET 10.0 LTS (Minimal APIs) con Clean Architecture (3 Capas).

Frontend: Quasar Framework (Vue 3 + TypeScript) para despliegue multiplataforma (Web, Android, iOS).

Base de Datos: PostgreSQL + PostGIS (Geolocalización avanzada).

ORM: Dapper (Performance) y FluentMigrator (Migraciones).

Principios de Diseño: YAGNI, DRY, SOLID (S-O).

---

## ✅ Checklist de Desarrollo

### Backend - Módulos API

| Módulo | Endpoint Base | Estado | Descripción |
|--------|--------------|--------|-------------|
| Auth | `/api/auth` | ✅ Completo | Login, Register, JWT |
| Users | `/api/users` | ✅ Completo | CRUD, foto, contraseña |
| Condominiums | `/api/condominiums` | ✅ Completo | CRUD de condominios |
| Posts | `/api/posts` | ✅ Completo | Tablón de anuncios |
| Comments | `/api/comments` | ✅ Completo | Comentarios en posts |
| Incidents | `/api/incidents` | ✅ Completo | Reporte de incidentes |
| Polls | `/api/polls` | ✅ Completo | Votaciones |
| Votes | `/api/votes` | ✅ Completo | Votos en encuestas |
| Alerts | `/api/alerts` | ✅ Completo | Alertas de seguridad |
| Expenses | `/api/expenses` | ✅ Completo | Gastos del condo |
| **Vobo** | `/api/vobos` | ❌ Pendiente | Validación de identidad |
| **Credibilidad** | `/api/credibilidad` | ❌ Pendiente | Sistema de reputación |

### Frontend - Páginas

| Página | Ruta | Estado | Descripción |
|--------|------|--------|-------------|
| Login | `/login` | ✅ Completo | Autenticación |
| Register | `/register` | ✅ Completo | Registro de usuarios |
| Dashboard | `/` | ✅ Completo | Panel principal |
| Users | `/users` | ✅ Completo | Gestión de vecinos |
| Condominiums | `/condominiums` | ✅ Completo | Admin de condominios |
| Posts | `/posts` | ✅ Completo | Tablón de anuncios |
| Incidents | `/incidents` | ✅ Completo | Reporte de incidentes |
| Polls | `/polls` | ✅ Completo | Votaciones |
| Alerts | `/alerts` | ✅ Completo | Alertas de seguridad |
| Settings | `/settings` | ✅ Completo | Configuración |
| Expenses | `/expenses` | ❌ Pendiente | Gastos del condo |

### Frontend - Stores (Pinia)

| Store | Estado |
|-------|--------|
| auth.js | ✅ Completo |
| user.js | ✅ Completo |
| condominium.js | ✅ Completo |
| post.js | ✅ Completo |
| incident.js | ✅ Completo |
| poll.js | ✅ Completo |
| alert.js | ✅ Completo |
| expense.js | ❌ Pendiente |

### Frontend - Componentes

| Componente | Estado |
|------------|--------|
| PostItem.vue | ✅ Completo |
| IncidentItem.vue | ✅ Completo |
| PollItem.vue | ✅ Completo |
| AlertItem.vue | ✅ Completo |
| CondoItem.vue | ✅ Completo |
| ExpenseItem.vue | ❌ Pendiente |

### Features de Gobernanza

| Feature | Estado |
|---------|--------|
| Validación de Identidad (Vobo) | ❌ Pendiente |
| Niveles de Credibilidad (1-5) | ❌ Pendiente |
| Colapsado de contenido Nivel 1 | ❌ Pendiente |
| Respuesta de Emergencia (Guard) | ❌ Pendiente |

### Configuración y Herramientas

| Herramienta | Estado |
|-------------|--------|
| TypeScript | ✅ Configurado |
| ESLint | ✅ Configurado |
| Prettier | ✅ Configurado |
| pnpm | ✅ Configurado |
| Vitest (Tests) | ❌ Pendiente |
| Docker | ⚙️ Configuración existente |

🛡️ Reglas de Gobernanza (SCZ logic)

El proyecto implementa un modelo de "Confianza Escalonada":

Validación de Identidad (Vobos):

Un nuevo usuario inicia como Acceso Restringido.

Para ascender a Vecino, requiere el Vobo (Visto Bueno) de 2 Encargados y la aprobación final del Administrador.

Niveles de Credibilidad (1 a 5):

Los comentarios en el tablón se ordenan por nivel de reputación.

Los usuarios de Nivel 1 (Irreverentes) tienen su contenido colapsado para reducir el "ruido" en la comunidad.

Respuesta de Emergencia:

Rol de Guardia con capacidad exclusiva para marcar incidentes como "En Camino".

🏗️ Estructura del Repositorio

/backend/Api: Capa de presentación y contratos.

/backend/Domain: Entidades, interfaces de dominio y lógica de negocio.

/backend/Infrastructure: Persistencia, base de datos y servicios externos.

/frontend: Aplicación Quasar (Vue.js).

🐳 Despliegue con Docker

El proyecto está optimizado para ejecutarse en contenedores Linux, separando el entorno de desarrollo (Hot Reload) y el de producción (optimizado para bajos recursos en VPS).
