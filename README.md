# UrbaPF - Sistema de Coordinación Vecinal (Santa Cruz)

Plataforma integral de gestión comunitaria diseñada para centralizar la seguridad, la comunicación y la democracia participativa en barrios y condominios de Santa Cruz, Bolivia.

## 🌟 Visión de Negocio

Digitalizar la convivencia vecinal reduciendo la fricción de los grupos de WhatsApp y profesionalizando la respuesta ante emergencias y la toma de decisiones.

## 🛡️ Reglas de Gobernanza y Roles

### 1\. Jerarquía de Usuarios

-   ****Visitante:**** Modo lectura. Solo ve Publicaciones y Emergencias. No interactúa.
-   ****Acceso Restringido:**** Rol inicial tras registro. Sin interacción social.
-   ****Vecino (Validado):**** Requiere ****2 Votos de Encargados****. Puede publicar, votar y comentar.
-   ****Guardia:**** Rol operativo. Botón exclusivo "En Camino" para emergencias. No vota ni accede al Tablón Social privado.
-   ****Encargado/Admin:**** Moderación, aprobaciones de contenido y gestión de staff.

### 2\. Niveles de Credibilidad (1-5)

Sistema conductual que afecta la visibilidad:

-   ****Nivel 5 (Referente):**** Máxima prioridad en comentarios.
-   ****Nivel 1 (Irreverente):**** Contenido colapsado/oculto por defecto. Requiere filtro previo para alertas.

## 🗳️ Sistema de Votaciones

### Reglas de Participación

-   ****Votantes:**** Solo ****Vecinos**** y ****Encargados**** pueden votar. El ****Admin**** crea y gestiona pero no emite voto.
-   ****Creación:****
-   -   ****Vecinos:**** Crean propuestas __Pendientes__ que requieren ****2 aprobaciones de Encargados**** para activarse.
    -   ****Encargados:**** Publican directamente (Activa o Programada).

### Alcance y Restricciones

-   ****Destinatarios:**** Las votaciones pueden ser dirigidas a todos los vecinos/encargados o a grupos específicos.
-   ****Configuración Obligatoria:**** Al crear se debe indicar: Rol mínimo para votar, Justificación, Tipo (única/múltiple), Fechas de vigencia y Restricciones específicas.

### Ciclo de Vida y Notificaciones

-   ****Vigencia:**** Período definido por `StartsAt` y `EndsAt`.
-   ****Segmentación de Avisos:**** Las notificaciones solo llegan a los usuarios que participan en esa votación específica.
-   ****Recordatorio Crítico:**** 30 min antes del cierre, se envía notificación push a todos los que ****no han votado**** todavía.
-   ****Cierre:**** Al finalizar, se anuncia automáticamente el cierre a todos los participantes.
-   ****Resultados:**** Se notifican los resultados finales únicamente al ****creador**** de la votación.

### Inmutabilidad y Control

-   ****Edición/Eliminación:**** Prohibido editar o eliminar votaciones una vez están en estado __Activa__ o __Cerrada__.
-   ****Eliminación por Manager:**** Un Encargado solo puede borrar una votación si ****nadie ha votado**** aún.

## 🚨 Protocolo de Emergencia

-   ****Ubicación:**** Georeferencia obligatoria mediante Google Maps.
-   ****Respuesta:**** Notificación persistente cada 1 min a los Encargados hasta que la alerta sea procesada.
-   ****Cierre:**** Solo el autor o el Staff pueden finalizar un incidente.

## 🚀 Estado del Proyecto y Funcionalidades

### ✅ Módulos Completados
-   ****Autenticación y Seguridad:****
    -   Registro y Login de usuarios.
    -   JWT con Refresh Tokens para persistencia en PWA/Mobile.
    -   Password Hashing robusto usando PBKDF2 (HMAC-SHA256).
-   ****Gestión de Usuarios:****
    -   Roles y jerarquías (Admin, Encargado, Vecino, Guardia).
    -   Perfil de usuario con subida de fotos a almacenamiento local (interfaz lista para S3).
    -   Validación de vecinos por votación de encargados.
-   ****Publicaciones y Tablón Social:****
    -   CRUD de publicaciones con categorías y estados.
    -   Moderación de contenido (ocultar/mostrar).
    -   Sistema de comentarios con jerarquía (respuestas).
-   ****Votaciones (Polls) - Backend:****
    -   Gobernanza comunitaria con inmutabilidad de votaciones activas.
    -   Firma digital SHA256 para cada voto (Integridad Electoral).
    -   Restricciones por rol y fecha de vigencia.

### ⏳ Próximos Pasos (Pendiente)
-   ****Votaciones (Polls) - Frontend:**** Integración completa de la UI con el servicio de firmas digitales.
-   ****Protocolo de Emergencia:**** Alertas en tiempo real con geolocalización.
-   ****Gestión de Incidentes:**** Reportes vecinales con seguimiento de estado.
-   ****Expensas y Finanzas:**** Control de pagos y cuotas mensuales.
-   ****Notificaciones Push:**** Integración final con OneSignal para recordatorios críticos.

## 🛠️ Stack Tecnológico
-   ****Backend:**** .NET 10 (Arquitectura de 3 capas), Dapper, FluentMigrator, PostgreSQL + PostGIS, NUnit + Moq.
-   ****Frontend:**** Quasar v3 (Vue 3 + TypeScript), Pinia, Axios, Vitest.

