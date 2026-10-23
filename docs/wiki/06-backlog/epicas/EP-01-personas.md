# EP-01 · Personas

[← Backlog](../README.md) · [Contrato REST](../../03-contratos/api-rest.md) · [SDD/LDD](../../04-implementacion/03-SDD-LDD.md)

**Propósito:** entregar el CRUD completo de `persona` por los dos canales (REST y Razor) sobre el mismo servicio, llave `cc`. Es el módulo de referencia: su recorrido demuestra la persistencia compartida exigida por el laboratorio.

**Requisitos cubiertos:** RF-01 (CRUD personas en MVC y API), RF-05 y RF-06 en la parte de persona, RNF-01, RNF-03, RNF-06.

**Decisiones aplicadas:** AD-02 (controladores separados), AD-03 (servicio por entidad), AD-10 (DTO y ViewModel), AD-06 (`NO ACTION` y `409`).

**Componentes afectados:** `01-Presentacion/Dtos/`, `01-Presentacion/ViewModels/`, `02-Aplicacion/Services/Interfaces/IPersonaService`, `02-Aplicacion/Services/Implementations/PersonaService`, `01-Presentacion/ControllersApi/PersonasApiController`, `01-Presentacion/Controllers/PersonasController`, `01-Presentacion/Views/Personas/`.

**Historias:** HU-004 (Sprint 1), HU-007 (Sprint 2) · **9 puntos**

**Criterio de éxito de la épica:** los cinco endpoints de `/api/personas` y las cinco vistas Razor operan contra `persona_db`; una persona creada en MVC se lee por REST; el borrado con dependientes devuelve `409` en API y un mensaje legible en la vista, sin modificar la base.

---

## HU-004 · API REST de personas de extremo a extremo

**Épica** EP-01 · **Sprint** 1 · **Responsable** Integrante A · **Prioridad** P0 · **Puntos** 5 · **Módulo** Personas

### Objetivo
Publicar las cinco operaciones REST de personas sobre servicio y DAO reales, para demostrar en Sprint 1 el recorrido completo `Controller → Service → DAO → EF Core → SQL Server` por el canal JSON.

### Alcance técnico
- DTOs de entrada y salida de persona ([AD-10](../../02-arquitectura/02-SAD-HLD.md#2-decisiones-arquitectónicas-definitivas)); no se exponen entidades EF con navegaciones.
- `IPersonaService` / `PersonaService` con resultados discriminados `Success`, `NotFound`, `ValidationError`, `Conflict` ([SDD §2](../../04-implementacion/03-SDD-LDD.md#2-contratos-de-aplicación-y-dao)) y las reglas de [SDD §7](../../04-implementacion/03-SDD-LDD.md#7-reglas-y-validaciones-por-entidad).
- `PersonasApiController` con `[ApiController]`, rutas de [api-rest.md](../../03-contratos/api-rest.md#operaciones-obligatorias-20) y una única línea DI para el servicio.
- Consumo de `IPersonaRepository` entregado por HU-003; **no** se reimplementa el DAO.
- **No incluye:** vistas Razor (HU-007) ni la documentación OpenAPI detallada (HU-009).

### Criterios de aceptación
1. Las cinco operaciones responden con `200`, `201` + `Location`, `204`, `400`, `404` y `409` según el contrato; el listado vacío devuelve `[]`.
2. `cc` duplicada en POST devuelve `409`; nombre o apellido ausentes, `genero` fuera de `M`/`F` o edad negativa devuelven `400`; `cc` de ruta distinta a la del cuerpo en PUT devuelve `400`.
3. GET, PUT y DELETE sobre una `cc` inexistente devuelven `404`.
4. DELETE de una persona con teléfonos o estudios devuelve `409` y la base queda sin cambios (RF-05).
5. `PersonasApiController` no referencia `DbContext` ni SQL y depende solo de `IPersonaService` (AK-01).
6. Lo creado por POST se comprueba como fila real en `persona_db`.

**Evidencia mínima:** peticiones y respuestas de cada caso positivo y negativo, más la consulta SQL que confirma la persistencia.

### Dependencias
**HU-003** (contratos DAO, contexto y DI). Las filas de prueba provienen del DML de HU-002.

### Trazabilidad
RF-01, RF-05, RF-06 · RNF-03, RNF-06 · AD-02, AD-03, AD-10 · [api-rest.md](../../03-contratos/api-rest.md) · [SDD §5](../../04-implementacion/03-SDD-LDD.md#5-contratos-rest-exactos-20-operaciones), §7 · reglas 2, 3, 4, 10, 12 · escenario SRS 10.5.

### Definition of Done
DoD común + endpoints probados con sus códigos HTTP, persistencia verificada en SQL Server y PR revisado.

---

## HU-007 · CRUD MVC de personas con errores controlados

**Épica** EP-01 · **Sprint** 2 · **Responsable** Integrante A · **Prioridad** P1 · **Puntos** 4 · **Módulo** Personas

> `P1` no significa opcional: las cinco vistas son alcance obligatorio. Indica que no bloquea a otras historias. Ver [priorización](../priorizacion.md).

### Objetivo
Entregar `Index`, `Details`, `Create`, `Edit` y `Delete` de personas en Razor sobre el mismo servicio del canal REST, para demostrar la persistencia compartida entre MVC y API.

### Alcance técnico
- `PersonasController` (MVC) sobre `IPersonaService`; ViewModels de formulario; cinco vistas de [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas).
- Uso de la convención de errores de HU-006 para validaciones y conflictos de FK.
- **No incluye:** cambios en el servicio salvo los necesarios para la presentación; cualquier regla nueva se acuerda en la issue y se refleja en SDD.

### Criterios de aceptación
1. Las cinco acciones web funcionan en navegador contra `persona_db`.
2. Un formulario inválido se vuelve a mostrar con los mensajes de error y sin escribir en la base.
3. El intento de eliminar una persona con teléfonos o estudios muestra una explicación legible y no elimina el registro.
4. Una persona creada desde MVC se consulta con `GET /api/personas/{cc}` y coincide (escenario SRS 10.1).
5. `PersonasController` depende solo de `IPersonaService` y no referencia `DbContext`.

**Evidencia mínima:** capturas de las cinco vistas, del error de validación y del borrado bloqueado, más la respuesta REST cruzada.

### Dependencias
**HU-004** (servicio y reglas de persona) y **HU-006** (layout y convención de errores).

### Trazabilidad
RF-01, RF-05, RF-06 · RNF-01, RNF-06 · AD-02, AD-03 · [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas), §7 · [criterios de éxito](../../01-producto/criterios-de-exito.md) 5, 6, 10 · reglas 2, 3, 12 · AK-02.

### Definition of Done
DoD común + recorrido en navegador, comprobación cruzada por REST y PR revisado.
