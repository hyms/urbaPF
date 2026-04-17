# 📖 AGENTS & SKILLS: Fuente de Verdad (UrbaPF v0.2)

Este documento es la instrucción definitiva para el Agente de IA. Define las reglas operativas, técnicas y de negocio para garantizar la coherencia del proyecto y evitar desviaciones.

---

## 1. 🌟 Visión de Negocio
Digitalizar la convivencia vecinal en Santa Cruz, Bolivia, eliminando el ruido de los grupos de WhatsApp. Foco en:
- **Transparencia Financiera:** Control absoluto de fondos comunes.
- **Seguridad Profesional:** Respuesta geolocalizada ante emergencias.
- **Democracia Participativa:** Votaciones inmutables y transparentes.

---

## 2. 🧠 Perfil Técnico y Stack (v0.2)

- **Arquitectura:** Clean Architecture de 3 capas (Api, Domain, Infrastructure).
- **Backend:** .NET 10 Minimal APIs.
- **Data Access:** **Dapper 2.1.72** para lectura/escritura (Prioritario). Usar `Result<T>` para flujos.
- **Migraciones:** **FluentMigrator 8.0.1**.
- **Frontend:** Quasar v2 (Vue 3) + TypeScript + Vite 8. Composition API con `<script setup>`.
- **Estilo Visual:** "Industrial", bordes `rounded-lg`, Mobile-First.
- **Base de Datos:** PostgreSQL + PostGIS.
- **Notificaciones:** Firebase Cloud Messaging (FCM) (Reemplaza OneSignal).

---

## 3. 🛡️ Matriz de Roles y Permisos

| Módulo        | Visitante/Restringido | Vecino (Validado) | Encargado/Admin | Guardia |
|---------------|-----------------------|-------------------|-----------------|---------|
| **Directorio**| Ver Nombre/Mza       | Ver Nombre/Mza    | Ver Todo (+Tel) | Sin Acceso |
| **Gastos**    | Ver Resumen          | Ver Resumen       | CRUD Completo   | Sin Acceso |
| **Tablón**    | Lectura              | Publicar/Comentar | Moderar/Borrar  | Ver Alertas |
| **Emergencia**| Recibir Alerta       | Activar Alerta    | Gestionar       | "En Camino" |

### 🔒 Flujo de Validación de Vecino
Todo usuario nuevo inicia como **Restringido**. Para subir a **Vecino** requiere:
1. **2 Votos de confianza** de usuarios tipo Encargado.
2. **1 Validación final** del Administrador.

---

## 4. 🧠 Reglas de Negocio Críticas

### A. Privacidad en Directorio
- El campo `phone` NUNCA debe viajar en el JSON si el usuario autenticado tiene rol `Vecino` o inferior.
- Los vecinos solo ven `FullName`, `StreetAddress` y `LotNumber`.

### B. Transparencia Financiera (Control de Gastos)
- Dashboard debe mostrar widget: `Saldo Caja`, `Total Egresos Mes` y `Top 3 Gastos`.
- Todo registro de gasto (`expense_reports`) debe vincularse a un `usuario_id` responsable.
- Adjuntos: Permitir carga de comprobantes digitales (imágenes/PDF).

### C. Sistema de Reputación (Niveles 1-5)
- **Nivel 5 (Referente):** Badge dorado, comentarios prioritarios (ordenar por `CredibilidadLevel DESC`).
- **Nivel >= 4:** La alerta de emergencia llega **directamente** a los guardias.
- **Nivel < 4:** La alerta requiere **aprobación** del Encargado antes de ser masiva.
- **Nivel 1 (Irreverente):** Contenido colapsado/oculto por defecto.

### D. Multi-tenancy
- Todas las tablas de negocio deben tener `comunidad_id`.
- Todo query debe filtrar estrictamente por `comunidad_id` para garantizar el aislamiento entre barrios.

---

## 🏗️ Guía de Estilo y Estándares

### 📝 Nomenclatura
- **Código:** Nomenclatura técnica en **Inglés** (Variables, Métodos, Tablas).
- **Interfaz:** Textos y mensajes en **Español** (Santa Cruz, BO).
- **Terminología Local:** Manzano (Mza), Lote, Expensas, Vobo, Churrasquera, Portería.

### 🏛️ Arquitectura Backend
1. **Presentation:** Endpoints ligeros. JWT + Refresh Tokens.
2. **Domain:** Entidades puras y Records. Inyectar interfaces (No heredar de BaseRepository).
3. **Infrastructure:** Dapper, FluentMigrator, Integración FCM y PostGIS.
4. **Audit:** Prohibido actualizar estados críticos sin generar un evento en `logs_eventos`.

### 🧪 Calidad
- **Mock-First:** Usar Moq. Prohibido depender de DB real en tests unitarios.
- **Frameworks:** NUnit, FluentAssertions, Vitest.
- **Cobertura:** Mínimo 40%.

---

## 🛡️ Reglas de Oro Técnicas
1. **Seguridad:** PBKDF2 (HMAC-SHA256) con 100k iteraciones.
2. **Soft Delete:** Usar columna `deleted_at`. `UPDATE {table} SET deleted_at = NOW() WHERE id = @Id`.
3. **Inmutabilidad:** Votos protegidos por firma digital SHA256 (UserId + OptionId + Timestamp + Secret). No se editan votaciones Activas/Cerradas.
4. **FCM Topics:** Implementar `comunidad_{id}_alertas` y `comunidad_{id}_avisos`.

---

## 📜 Sistema de Auditoría (Event Sourcing Ligero)
La tabla `logs_eventos` es **Append-only** (Inmutable).

```sql
CREATE TABLE logs_eventos (
    id BigSerial PRIMARY KEY,
    uuid UUID UNIQUE DEFAULT gen_random_uuid(),
    fecha TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    usuario_id UUID NOT NULL,
    comunidad_id UUID NOT NULL,
    tipo_evento VARCHAR(100) NOT NULL, -- Ej: 'VECINO_VALIDADO', 'EGRESO_REGISTRADO'
    entidad_id UUID NOT NULL,
    data_json JSONB NOT NULL,
    hash_verificacion TEXT -- Integridad encadenada
);
```

### 🧩 Principios S.O.I.D.
- **S (Single Responsibility):** Servicios pequeños.
- **O (Open/Closed):** Extensibilidad sin modificar core.
- **I (Interface Segregation):** Interfaces específicas.
- **D (DRY):** No repetir lógica de negocio.
- **YAGNI & KISS:** Simplicidad sobre complejidad innecesaria.

---
**🚫 Restricción Final:** NO usar EF Core para lógica de negocio (solo permitido para migraciones si se requiere, aunque se prefiere FluentMigrator).
