# UrbaPF - Sistema de Coordinación Vecinal

Sistema integral de gestión, seguridad y transparencia financiera para barrios y condominios en Santa Cruz, Bolivia. Diseñado para optimizar la comunicación entre vecinos, encargados y guardias, eliminando el ruido de los grupos de chat convencionales.

## 🌟 Visión de Negocio

Digitalizar la convivencia vecinal reduciendo la fricción en la comunicación, profesionalizando la respuesta ante emergencias y garantizando la transparencia absoluta en el manejo de fondos comunes.

## 🚀 Stack Tecnológico

-   **Frontend:** Quasar Framework v2 (Vue 3) + TypeScript + Vite.
    
-   **Backend:** .NET 10 Minimal APIs + Dapper (Consultas) + FluentMigrator (Migraciones).
    
-   **Base de Datos:** PostgreSQL + PostGIS (Geolocalización).
    
-   **Notificaciones:** Firebase Cloud Messaging (FCM).
    
-   **Infraestructura:** Docker (Producción) + VPS Linux.
    

## 🛡️ Reglas de Gobernanza y Roles

### 1\. Jerarquía de Usuarios y Permisos

-   **Visitante / Restringido:** Modo lectura. Solo ve avisos oficiales y recibe alertas. Requiere validación para interactuar.
    
-   **Vecino (Validado):** Puede publicar, comentar, votar y reportar incidencias.
    
-   **Guardia:** Rol operativo. Botón exclusivo "En Camino" para emergencias. Sin acceso al Tablón Social privado.
    
-   **Encargado / Administrador:** Moderación, aprobación de vecinos, gestión de staff y control financiero.
    

### 2\. Niveles de Credibilidad (Reputación 1-5)

-   **Nivel 5 (Referente):** Comentarios prioritarios y badge dorado.
    
-   **Nivel >= 4:** Alertas de emergencia se notifican **directamente** a los guardias.
    
-   **Nivel < 4:** Las alertas requieren validación del Encargado antes de ser masivas.
    
-   **Nivel 1 (Irreverente):** Contenido colapsado por defecto; requiere un clic extra para visualizarse.
    

### 3\. Flujo de Validación de Identidad

Todo usuario nuevo inicia como **Restringido**. Para subir a **Vecino** requiere:

-   **2 Votos de confianza** de usuarios tipo Encargado.
    
-   **1 Validación final** del Administrador.
    

## 📦 Módulos Principales (v0.2)

### 🗂️ Directorio Vecinal

Directorio telefónico y de ubicación.

-   **Vecinos:** Ven nombres y direcciones.
    
-   **Encargados:** Tienen acceso exclusivo al número de teléfono y detalle de perfil.
    

### 💰 Control de Gastos

Módulo de transparencia financiera.

-   **Dashboard:** Cuadro resumen con Saldo de Caja, Gastos del Mes y últimos 3 egresos visible para todos.
    
-   **Gestión:** CRUD completo para Encargados con carga de comprobantes digitales.
    

### 🚨 Respuesta de Emergencia

-   **Botón de Pánico:** Alerta masiva con geolocalización (PostGIS).
    
-   **Seguimiento:** Los vecinos ven en tiempo real qué guardia está atendiendo el incidente.
    

### 🗳️ Gobernanza y Votaciones

-   **Inmutabilidad:** Votos protegidos por firma digital SHA256.
    
-   **Tipos:** Consultivas (opinión) y Obligatorias (asambleas).
    

## 📂 Estructura del Proyecto

### 🖥️ Backend (.NET 10)

-   **`Api/`**: Minimal APIs, JWT Auth y Endpoints de negocio.
    
-   **`Domain/`**: Entidades puras y lógica de roles/reputación.
    
-   **`Infrastructure/`**: Repositorios Dapper, integración con Firebase y PostGIS.
    

### 🎨 Frontend (Quasar)

-   **`src/pages/`**: Vistas de Dashboard, Directorio y Gastos.
    
-   **`src/components/`**: UI Industrial con componentes `rounded-lg`.
    
-   **`src/stores/`**: Gestión de estado con Pinia.
    

## 🛠️ Instalación y Desarrollo

```
# Clonar repositorio y levantar entorno completo
docker-compose up --build
```

## 📱 Validación UX/UI Mobile (Indicios de Calidad)

Para garantizar que el sistema es apto para el uso en calle y por personas de todas las edades, se aplican los siguientes criterios:

- **Touch Targets:** Todos los elementos interactivos tienen un tamaño mínimo de 44x44px para evitar errores de pulsación.

- **Thumb Zone Design:** Las acciones críticas (Botón de Emergencia, Reportar) están ubicadas en la zona de alcance natural del pulgar (parte inferior de la pantalla).

- **Optimización de Carga:** Uso de `q-img` con placeholders y lazy-loading para minimizar el consumo de datos móviles en zonas con baja señal.

- **Feedback Háptico y Visual:** Confirmación inmediata en pantalla tras acciones críticas y estados de carga (skeletons) para evitar múltiples clics por impaciencia.

- **Legibilidad:** Uso de fuentes Sans-Serif con un tamaño base de 16px y contrastes altos para lectura bajo el sol cruceño.

- **Navegación Adaptativa (Menú Mobile):** En móviles se utiliza una Bottom Navigation Bar para las 4 secciones principales. El menú lateral (Drawer) se reserva exclusivamente para configuraciones de perfil y cambio de condominio, liberando espacio visual en la pantalla principal.

## 📋 Reglas de Negocio v0.2

### 1. Selección Automática de Condominio

-   Cuando existe un único condominio en el sistema, se selecciona automáticamente al iniciar sesión.
-   El usuario no necesita seleccionar manualmente el condominio.
-   Si hay más de uno, el usuario puede elegir desde el menú.

### 2. Página de Detalle del Condominio

-   **Encabezado:** Nombre, dirección y cuota mensual.
-   **Descripción:** Texto libre sobre el condominio.
-   **Reglas y Normas:** Lista de reglas mostradas una por línea con checkmarks.
-   **Equipo de Gestión:** Lista de encargados y administradores con foto, nombre, rol y badge de validación.
-   **Ubicación:**
    -   Coordenadas GPS (latitud/longitud) editables por administradores.
    -   Mapa interactivo (Leaflet) centrado en las coordenadas.
    -   Valor por defecto: `-17.607406, -63.097274` (Santa Cruz, Bolivia).
    -   Botón "Obtener ubicación actual" para usar geolocalización del navegador.

### 3. Control de Gastos - Permisos

-   **Administradores/Encargados:**
    -   Pueden crear nuevos registros de gasto.
    -   Pueden editar registros existentes.
    -   Pueden eliminar registros.
    -   Ver botón "Nuevo Registro" y acciones de editar/eliminar en la tabla.
-   **Vecinos/Guardias/Restringidos:**
    -   Solo modo lectura.
    -   Pueden ver el resumen (Saldo en Caja, Egresos del Mes, Top 3 Gastos).
    -   Pueden ver la tabla completa de registros sin opciones de edición.
    -   No ven el botón de crear ni las acciones en la tabla.
