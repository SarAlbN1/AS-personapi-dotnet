# EP-04 · Estudios

[← Backlog](../README.md) · [Persistencia](../../03-contratos/persistencia.md) · [Contrato REST](../../03-contratos/api-rest.md)

**Propósito:** entregar el CRUD completo de `estudios` por REST y Razor con **clave primaria compuesta** `(id_prof, cc_per)` y dos FKs. Es el módulo de mayor riesgo arquitectónico: modelar esta entidad con un identificador ficticio es un *architecture killer* explícito (AK-03).

**Requisitos cubiertos:** RF-04, RF-06 en la parte de estudio, RNF-01, RNF-02, RNF-03, RNF-06.

**Decisiones aplicadas:** AD-02, AD-03, AD-05, AD-06, AD-10; [regla arquitectónica 11](../../02-arquitectura/reglas-arquitectonicas.md).

**Componentes afectados:** `Back/01-Borde/Dtos/`, `Front/ViewModels/`, `IEstudioService` / `EstudioService`, `Back/01-Borde/ControllersApi/EstudiosApiController`, `Back/01-Borde/Controllers/EstudiosController`, `Front/Views/Estudios/`.

**Historias:** HU-011, HU-013 (ambas Sprint 2) · **9 puntos**

**Criterio de éxito de la épica:** las rutas `/api/estudios/{idProf}/{ccPer}` y las cinco vistas Razor operan contra `persona_db`; un estudio solo existe con persona y profesión existentes; la pareja identificadora no cambia por una actualización ([criterio de éxito 8](../../01-producto/criterios-de-exito.md)).

> **Paralelismo:** requiere filas de `persona` y `profesion` del DML de HU-002, no las pantallas de EP-01 ni EP-02.

---

## HU-011 · API REST de estudios de extremo a extremo con clave compuesta

**Épica** EP-04 · **Sprint** 2 · **Responsable** Integrante B · **Prioridad** P0 · **Puntos** 5 · **Módulo** Estudios

### Objetivo
Publicar las cinco operaciones REST de estudios identificadas por `(idProf, ccPer)`, para demostrar que la clave compuesta se respeta en rutas, servicio y persistencia, y cerrar el riesgo AK-03.

### Alcance técnico
- DTOs con mapeo `idProf`/`ccPer` ⇄ `id_prof`/`cc_per` ([api-rest.md](../../03-contratos/api-rest.md#representaciones-json-ejemplos)).
- `IEstudioService` / `EstudioService`: existencia de ambas FKs, unicidad del par, validación de `fecha` y `univer` ([SDD §7](../../04-implementacion/03-SDD-LDD.md#7-reglas-y-validaciones-por-entidad)).
- `EstudiosApiController` con rutas de dos segmentos de clave y una única línea DI para el servicio.
- Sigue la convención OpenAPI de HU-009 (no bloqueante).
- **No incluye:** vistas Razor (HU-013).

### Criterios de aceptación
1. Las cinco operaciones responden con `200`, `201` + `Location`, `204`, `400`, `404` y `409` según el contrato; el listado vacío devuelve `[]`.
2. Las rutas de detalle, edición y baja usan `/api/estudios/{idProf}/{ccPer}` y nunca un identificador artificial.
3. POST con profesión o persona inexistente devuelve `400` y no escribe en la base.
4. El par `(idProf, ccPer)` duplicado devuelve `409`; el par inexistente devuelve `404`.
5. PUT modifica solo `fecha` y `univer`; un intento de cambiar el par identificador devuelve `400`.
6. `fecha` inválida devuelve `400`; los valores nulos permitidos por el DDL se aceptan.
7. `EstudiosApiController` depende solo de `IEstudioService` y no referencia `DbContext`.

**Evidencia mínima:** peticiones y respuestas de cada caso, incluido el intento de cambiar la clave, más la consulta SQL filtrada por las dos columnas.

### Dependencias
**HU-003** (la interfaz DAO de estudios ya expone la clave `(int idProf, int ccPer)`).

### Trazabilidad
RF-04, RF-06 · RNF-02, RNF-03 · AD-03, AD-05, AD-06 · AK-03 · [persistencia](../../03-contratos/persistencia.md#modelo-físico) · [SDD §5](../../04-implementacion/03-SDD-LDD.md#5-contratos-rest-exactos-20-operaciones), §7 · regla 11 · escenario SRS 10.4.

### Definition of Done
DoD común + operaciones probadas por las dos claves, persistencia verificada y PR revisado.

---

## HU-013 · CRUD MVC de estudios con selectores y clave compuesta

**Épica** EP-04 · **Sprint** 2 · **Responsable** Integrante B · **Prioridad** P1 · **Puntos** 4 · **Módulo** Estudios

### Objetivo
Entregar las cinco vistas Razor de estudios con selectores de persona y profesión, para demostrar el CRUD web de una entidad con clave compuesta y relaciones obligatorias.

### Alcance técnico
- `EstudiosController` (MVC) sobre `IEstudioService` más los servicios de persona y profesión para los selectores ([SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas)).
- Rutas MVC que transportan ambas claves en `Details`, `Edit` y `Delete`.
- Convención de errores de HU-006 para par duplicado y referencias inválidas.
- **No incluye:** cambios en el contrato REST.

### Criterios de aceptación
1. Las cinco acciones web funcionan en navegador y las rutas de detalle, edición y baja transportan ambas claves.
2. Los selectores de persona y profesión se alimentan de los servicios y no permiten referencias inexistentes.
3. La edición no cambia el par identificador y el intento de crear un par existente se informa en el formulario.
4. Un estudio creado desde MVC se consulta con `GET /api/estudios/{idProf}/{ccPer}` y coincide (escenario SRS 10.4).
5. `EstudiosController` depende solo de servicios y no referencia `DbContext`.

**Evidencia mínima:** capturas de las cinco vistas mostrando ambas claves en la URL y la respuesta REST cruzada.

### Dependencias
**HU-011** y **HU-006**.

### Trazabilidad
RF-04, RF-06 · RNF-01, RNF-06 · AD-02, AD-10 · AK-02, AK-03 · [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas) · criterios de éxito 5, 8 · reglas 2, 3, 8, 11.

### Definition of Done
DoD común + recorrido en navegador por la clave compuesta, comprobación cruzada por REST y PR revisado.
