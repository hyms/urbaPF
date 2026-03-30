# UrbaPF - Sistema de Coordinación Vecinal (Santa Cruz)

Plataforma integral de gestión comunitaria diseñada para centralizar la seguridad, la comunicación y la democracia participativa en barrios y condominios de Santa Cruz, Bolivia.

## 🌟 Visión de Negocio

Digitalizar la convivencia vecinal reduciendo la fricción de los grupos de WhatsApp y profesionalizando la respuesta ante emergencias y la toma de decisiones.

## 🛡️ Reglas de Gobernanza y Roles

### 1. Jerarquía de Usuarios

-   **Visitante:** Modo lectura. Solo ve Publicaciones y Emergencias. No interactúa.
-   **Acceso Restringido:** Rol inicial tras registro. Sin interacción social.
-   **Vecino (Validado):** Requiere **2 Votos de Encargados**. Puede publicar, votar y comentar.
-   **Guardia:** Rol operativo. Botón exclusivo "En Camino" para emergencias. No vota ni accede al Tablón Social privado.
-   **Encargado/Admin:** Moderación, aprobaciones de contenido y gestión de staff.

### 2. Niveles de Credibilidad (1-5)

Sistema conductual que afecta la visibilidad:

-   **Nivel 5 (Referente):** Máxima prioridad en comentarios.
-   **Nivel 1 (Irreverente):** Contenido colapsado/oculto por defecto.

### 3. Sistema de Reputación para Emergencias

-   **Buena reputación (nivel >= 4):** La alerta de emergencia llega **directamente a los guardias**.
-   **Mala reputación (nivel < 4):** La alerta requiere **aprobación del Manager** para distribuirse a los vecinos.
-   **Re-notificación:** El creador del ticket puede "re-notificar" para enviar nuevamente la alerta a todos los vecinos.

## 🗳️ Sistema de Votaciones

### Reglas de Participación

-   **Votantes:** Solo **Vecinos** y **Encargados** pueden votar. El **Admin** crea y gestiona pero no emite voto.
-   **Creación:**
    -   **Vecinos:** Crean propuestas __Pendientes__ que requieren **2 aprobaciones de Encargados** para activarse.
    -   **Encargados:** Publican directamente (Activa o Programada).

### Inmutabilidad y Control

-   **Edición/Eliminación:** Prohibido editar o eliminar votaciones una vez están en estado __Activa__ o __Cerrada__.
-   **Integridad:** Cada voto genera una firma digital SHA256 (UserId + OptionId + Timestamp + Secret) para auditoría posterior.

## 🚀 Estado del Proyecto (v1.1.0)

### ✅ Módulos Completados
-   **Autenticación y Seguridad:**
    -   Registro y Login con flujo de **Refresh Tokens**.
    -   Password Hashing con **PBKDF2 (HMAC-SHA256)** @ 100k iteraciones.
-   **Gestión de Usuarios:**
    -   Perfiles con subida de fotos (Storage Local escalable a S3).
    -   Lógica de validación por votos de confianza.
-   **Tablón Social (Posts):**
    -   CRUD completo con categorías y estados.
    -   Comentarios jerárquicos y moderación (Soft Delete).
-   **Votaciones (Backend + Frontend):**
    -   Motor de gobernanza con inmutabilidad y firmas digitales.
    -   UI con tabs (Activas, Finalizadas, Mis Propuestas).
    -   Visualización de resultados con barras de progreso.
-   **Gestión de Condominios:**
    -   Selección de condominio activo.
    -   CRUD de condominios.

### ⏳ Módulo de Incidentes y Emergencias (En Desarrollo)

**Fase 5.1: Infraestructura** (Completada)
-   Almacenamiento multimedia local.
-   Componentes base para Incidentes y Alertas.

**Fase 5.2: Reporte de Incidentes** (Pendiente)
-   GPS automático con opción de edición manual.
-   Integración con Leaflet/OpenStreetMap.
-   Límite de 3 archivos por incidente (preparado para video).

**Fase 5.3: Protocolo de Emergencia** (Pendiente)
-   Botón de pánico (3 segundos de presión).
-   Sistema de reputación aplicado a alertas.
-   Aprobación por Manager antes de broadcast global.

**Fase 5.4: Dashboard de Seguridad** (Pendiente)
-   Mapa en tiempo real de incidentes y alertas.
-   Workflow de resolución (Reportado -> En Proceso -> Resuelto -> Cerrado).

### 🛠️ Stack Tecnológico Actualizado

### Backend (.NET 10.0)
-   **Core:** .NET 10.0 (Native AOT compatible).
-   **ORM/Data:** Dapper **2.1.72**, Npgsql **10.0.2**, FluentMigrator **8.0.1**.
-   **Seguridad:** Microsoft.AspNetCore.Authentication.JwtBearer **10.0.5**, System.IdentityModel.Tokens.Jwt **8.17.0**.
-   **Tests:** NUnit **4.5.1**, Moq **4.20.72**, FluentAssertions **8.9.0**.

### Frontend (Quasar v2)
-   **Core:** Vue **3.5.31**, Vite **8.0.3**.
-   **State:** Pinia **3.0.4**.
-   **Router:** Vue Router **5.0.4**.
-   **Language:** TypeScript **6.0.2**.
-   **Tests:** Vitest **4.1.2**.
-   **API:** Axios **1.14.0**.
-   **Maps:** Leaflet + OpenStreetMap.

## 📂 Estructura del Proyecto

El sistema sigue una arquitectura modular y limpia, separando responsabilidades de forma estricta:

### 🖥️ Backend (.NET 10)
-   **`Api/`**: Capa de presentación con Minimal APIs y definición de rutas.
-   **`Domain/`**: Lógica de negocio pura, entidades de dominio y reglas de gobernanza.
-   **`Infrastructure/`**: Implementación de persistencia (Dapper), migraciones y servicios externos.
-   **`tests/`**: Suite de pruebas unitarias organizada por servicios y dominio.

### 🎨 Frontend (Quasar)
-   **`src/pages/`**: Vistas principales de la aplicación.
-   **`src/components/`**: Componentes de UI modulares y reutilizables.
-   **`src/stores/`**: Gestión de estado global mediante Pinia.
-   **`src/boot/`**: Configuraciones de inicio (Axios/API interceptors).
