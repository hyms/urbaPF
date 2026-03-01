UrbaPF - Sistema de Coordinación Vecinal

Plataforma de gestión comunitaria y seguridad ciudadana diseñada específicamente para barrios y condominios en Santa Cruz, Bolivia. El sistema reemplaza la fragmentación de los grupos de chat tradicionales por una estructura de gobernanza digital robusta y democrática.

🚀 Visión del Proyecto

UrbaPF no es solo una app de reportes; es un sistema de identidad donde la participación social y política del vecino está validada por la propia comunidad, garantizando que solo residentes reales tomen decisiones sobre su entorno.

🛠 Stack Técnico

Backend: .NET 8.0 LTS (Minimal APIs) con Clean Architecture (3 Capas).

Frontend: Quasar Framework (Vue 3 + TypeScript) para despliegue multiplataforma (Web, Android, iOS).

Base de Datos: PostgreSQL + PostGIS (Geolocalización avanzada).

ORM: Dapper (Performance) y EF Core (Migraciones).

Principios de Diseño: YAGNI, DRY, SOLID (S-O).

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
