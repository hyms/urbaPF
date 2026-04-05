# UrbaPF - Sistema de Coordinación Vecinal

Plataforma integral de gestión comunitaria diseñada para centralizar la seguridad, la comunicación y la democracia participativa en barrios y condominios de Santa Cruz, Bolivia.

## 🌟 Visión de Negocio

Digitalizar la convivencia vecinal reduciendo la fricción de los grupos de WhatsApp y profesionalizando la respuesta ante emergencias y la toma de decisiones.

## 🛡️ Reglas de Gobernanza y Roles

### 1\. Jerarquía de Usuarios

-   **Visitante:** Modo lectura. Solo ve Publicaciones y Emergencias. No interactúa.
    
-   **Vecino (Validado):** Rol inicial tras registro. Puede publicar, votar y comentar.
    

    
-   **Guardia:** Rol operativo. Botón exclusivo "En Camino" para emergencias. No accede al Tablón Social privado.
    
-   **Encargado/Admin:** Moderación, aprobaciones de contenido y gestión de staff.
    

### 2\. Niveles de Credibilidad y Reputación (1-5)

-   **Nivel 5 (Referente):** Máxima prioridad en comentarios. Sus alertas de emergencia van directo a Guardias.
    
-   **Nivel >= 4 (Buena Reputación):** La alerta de emergencia llega **directamente a los guardias**.
    
-   **Nivel < 4 (Mala Reputación):** La alerta requiere **aprobación del Manager** para distribuirse a los vecinos.
    
-   **Nivel 1 (Irreverente):** Contenido colapsado/oculto por defecto.
    

## 🗳️ Sistema de Votaciones

-   **Votantes:** Solo Vecinos y Encargados. El Admin gestiona pero no vota.
    
-   **Creación:** Vecinos requieren 2 aprobaciones de Encargados; Encargados publican directo.
    
-   **Integridad:** Firma digital SHA256 (UserId + OptionId + Timestamp + Secret) para cada voto.
    
-   **Inmutabilidad:** Prohibido editar o eliminar votaciones en estado **Activa** o **Cerrada**.
    

## 🚨 Protocolo de Emergencia

-   **Ubicación:** Georeferencia obligatoria mediante Leaflet/OpenStreetMap.
    
-   **Re-notificación:** El creador del ticket puede enviar nuevamente la alerta a todos los vecinos si no hay respuesta.
    
-   **Workflow:** Reportado -> En Proceso -> Resuelto -> Cerrado.
    

## 🚀 Estado del Proyecto (v1.1.0)

-   **✅ Completado:** Autenticación (Refresh Tokens), Gestión de Usuarios, Tablón Social, Motor de Votaciones, Gestión de Condominios.
    
-   **⏳ En Desarrollo:** Módulo de Incidentes (Fase 5.2 - 5.4).
-   **Notas:** La funcionalidad de 'categorías' en publicaciones ha sido temporalmente deshabilitada y se considera para el MVP2.
    


## 📂 Estructura del Proyecto

El sistema sigue una arquitectura modular y limpia, separando responsabilidades de forma estricta:

### 🖥️ Backend (.NET 10)
-   **`Api/`**: Capa de presentación con Minimal APIs y definición de rutas.
-   **`Domain/`**: Lógica de negocio pura, entidades de dominio y reglas de gobernanza.
-   **`Infrastructure/`**: Implementación de persistencia (Dapper), migraciones, servicios externos y repositorios (e.g., `UserRepository`, `RefreshTokenRepository`).
-   **`tests/`**: Suite de pruebas unitarias organizada por servicios y dominio.

### 🎨 Frontend (Quasar)
-   **`src/pages/`**: Vistas principales de la aplicación.
-   **`src/components/`**: Componentes de UI modulares y reutilizables.
-   **`src/stores/`**: Gestión de estado global mediante Pinia.
-   **`src/boot/`**: Configuraciones de inicio (Axios/API interceptors).

## 🇧🇴 Contexto Regional

Uso de terminología local: Manzano (Mza), Lote, Vobo, Expensas, Churrasquera, Portería.

