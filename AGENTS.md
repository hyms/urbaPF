# UrbaPF - Technical Master Instructions (v15.0)

Este archivo es la "fuente de verdad" técnica para el desarrollo en .NET 10 y Quasar v2.

## 🏗️ Arquitectura del Backend (.NET 10)

1.  **Presentation Layer (Minimal APIs):** Endpoints ligeros, JWT + Refresh Tokens.
    
2.  **Domain Layer:** Entidades puras y Records. Servicios inyectan interfaces de repositorio. Los servicios **NO** deben heredar de `BaseRepository`. Deben inyectar interfaces para facilitar la política Mock-First.
    
3.  **Infrastructure Layer:** **Dapper 2.1.72**, **FluentMigrator 8.0.1**, PostgreSQL + PostGIS.
    
4.  **Audit Strategy (Event Sourcing):** Prohibido actualizar estados críticos sin generar un evento en la tabla `logs_eventos`. El historial es inmutable (Append-only).
    

## 🧪 Calidad y Pruebas

-   **Mock-First:** Prohibido depender de DB real en tests unitarios. Usar Moq.
    
-   **Backend:** NUnit 4.5.1 + FluentAssertions 8.9.0.
    
-   **Frontend:** Vitest 4.1.2 + TypeScript 6.0.2.
    
-   **Cobertura:** Mínimo 40%.
    

## 🎨 Frontend Guidelines (Quasar v2 + Vite 8)

-   **Package Manager:** pnpm.
    
-   **API Client:** Axios 1.14.0 con interceptores para 401/Refresh.
    
-   **Maps:** Leaflet + OpenStreetMap.
    

## 🛡️ Reglas de Oro Técnicas

1.  **Seguridad:** PBKDF2 (HMAC-SHA256) con 100k iteraciones.
    
2.  **Soft Delete:** `UPDATE {table} SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id`.
    
3.  **Auditoría:** Cada acción de gobernanza (voto, cambio de rol, aprobación) debe disparar un evento inmutable.
    

## 🧩 Principios de Diseño y Arquitectura

El desarrollo debe adherirse a los siguientes principios para garantizar mantenibilidad, escalabilidad y legibilidad:

- **S.O.I.:**
    - **S (Single Responsibility):** Cada servicio/repositorio tiene una única responsabilidad.
    - **O (Open/Closed):** El código es abierto a extensiones pero cerrado a modificaciones.
    - **I (Interface Segregation):** Interfaces pequeñas y específicas (como `IRefreshTokenRepository`).
- **DRY (Don't Repeat Yourself):** Evitar la duplicación de lógica de negocio o acceso a datos. Extraer componentes compartidos.
- **YAGNI (You Ain't Gonna Need It):** No implementar funcionalidades ni estructuras "por si acaso". Desarrollar solo lo necesario para el requerimiento actual.
- **KISS (Keep It Simple, Stupid):** Priorizar la simplicidad en la solución sobre la complejidad innecesaria.


## 🚫 Restricciones

-   **NO** usar EF Core para lógica de negocio.



# Sistema de Auditoría mediante Event Sourcing (UrbaPF)

Para garantizar la máxima transparencia en Santa Cruz, UrbaPF utiliza un modelo de **Event Sourcing Ligero**. No se auditan "cambios", se registran "hechos".

## 1\. Esquema de la Tabla de Eventos (Inmutable)

Esta tabla es **Append-only**. No se permiten `UPDATE` ni `DELETE`.

```
CREATE TABLE logs_eventos (
    id BigSerial PRIMARY KEY, -- Orden secuencial físico
    uuid UUID UNIQUE DEFAULT gen_random_uuid(),
    fecha TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    usuario_id UUID NOT NULL, -- Quién disparó el evento
    comunidad_id UUID NOT NULL,
    tipo_evento VARCHAR(100) NOT NULL, -- Ej: 'VECINO_VALIDADO', 'VOTO_REGISTRADO'
    entidad_id UUID NOT NULL, -- ID del objeto afectado
    data_json JSONB NOT NULL, -- Detalles del evento (Payload)
    hash_verificacion TEXT -- Hash del evento anterior + actual (Integridad)
);
```

## 2\. Lógica de Aplicación (C#)

Cada vez que un `Encargado` valida a un vecino, en lugar de solo hacer un update, el flujo es:

1.  Iniciar Transacción.
    
2.  `INSERT INTO votos_validacion ...`
    
3.  `INSERT INTO logs_eventos (tipo_evento, data_json) VALUES ('VOTO_CONFIANZA_EMITIDO', '{"candidato": "...", "votos_actuales": 1}')`
    
4.  Confirmar Transacción.
    

## 3\. Ventajas para la Auditoría Vecinal

-   **Reconstrucción:** Podemos saber exactamente en qué segundo un usuario pasó de "Restringido" a "Vecino" y quiénes fueron los dos encargados involucrados.
    
-   **Prueba de No-Repudio:** Al usar un `hash_verificacion` que encadena eventos, si alguien borra un registro de la base de datos, la cadena se rompe y el sistema lanza una alerta de integridad.
    
-   **Transparencia en Votaciones:** Los vecinos pueden ver un "Libro de Actas Digital" derivado directamente de estos eventos.
