# EP-02 · Profesiones

[← Backlog](../README.md) · [Contrato REST](../../03-contratos/api-rest.md) · [SDD/LDD](../../04-implementacion/03-SDD-LDD.md)

**Propósito:** entregar el CRUD completo de `profesion` por REST y Razor sobre el mismo servicio, llave `id`. Es el módulo con menos restricciones de entrada, por lo que se usa para validar la simetría del patrón entre canales y el conflicto de borrado por estudios asociados.

**Requisitos cubiertos:** RF-02, RF-05 y RF-06 en la parte de profesión, RNF-01, RNF-03, RNF-06.

**Decisiones aplicadas:** AD-02, AD-03, AD-06, AD-10.

**Componentes afectados:** `Models/Dtos/`, `Models/ViewModels/`, `IProfesionService` / `ProfesionService`, `Controllers/Api/ProfesionesApiController`, `Controllers/ProfesionesController`, `Views/Profesiones/`.

**Historias:** HU-005 (Sprint 1), HU-008 (Sprint 2) · **7 puntos**

**Criterio de éxito de la épica:** los cinco endpoints de `/api/profesiones` y las cinco vistas Razor operan contra `persona_db`; una profesión creada por REST se visualiza en MVC; el borrado de una profesión con estudios devuelve `409` y un mensaje legible.

---

## HU-005 · API REST de profesiones de extremo a extremo

**Épica** EP-02 · **Sprint** 1 · **Responsable** Integrante B · **Prioridad** P0 · **Puntos** 4 · **Módulo** Profesiones

### Objetivo
Publicar las cinco operaciones REST de profesiones sobre servicio y DAO reales, para completar en Sprint 1 el segundo recurso del canal JSON y dejar probado el patrón que seguirán teléfonos y estudios.

### Alcance técnico
- DTOs de profesión, `IProfesionService` / `ProfesionService` con resultados discriminados y reglas de [SDD §7](../../04-implementacion/03-SDD-LDD.md#7-reglas-y-validaciones-por-entidad).
- `ProfesionesApiController` con las rutas de [api-rest.md](../../03-contratos/api-rest.md#operaciones-obligatorias-20) y una única línea DI para el servicio.
- Tratamiento de `des` como `VARCHAR(MAX)` opcional.
- **No incluye:** vistas Razor (HU-008) ni documentación OpenAPI detallada (HU-009).

### Criterios de aceptación
1. Las cinco operaciones responden con `200`, `201` + `Location`, `204`, `400`, `404` y `409` según el contrato.
2. `id` duplicado en POST devuelve `409` y `nom` ausente devuelve `400`.
3. `id` de ruta distinto al del cuerpo en PUT devuelve `400`; una profesión inexistente devuelve `404`.
4. DELETE de una profesión asociada a estudios devuelve `409` y la base queda sin cambios.
5. `des` admite texto largo y valor nulo sin romper el contrato.
6. `ProfesionesApiController` depende solo de `IProfesionService` y no referencia `DbContext`.

**Evidencia mínima:** peticiones y respuestas de cada caso positivo y negativo, más la consulta SQL que confirma la persistencia.

### Dependencias
**HU-003**. Las filas de prueba provienen del DML de HU-002.

### Trazabilidad
RF-02, RF-05, RF-06 · RNF-03, RNF-06 · AD-02, AD-03, AD-10 · [api-rest.md](../../03-contratos/api-rest.md) · [SDD §5](../../04-implementacion/03-SDD-LDD.md#5-contratos-rest-exactos-20-operaciones), §7 · reglas 2, 3, 4, 10, 12.

### Definition of Done
DoD común + endpoints probados con sus códigos HTTP, persistencia verificada y PR revisado.

---

## HU-008 · CRUD MVC de profesiones con errores controlados

**Épica** EP-02 · **Sprint** 2 · **Responsable** Integrante B · **Prioridad** P1 · **Puntos** 3 · **Módulo** Profesiones

### Objetivo
Entregar las cinco vistas Razor de profesiones sobre el servicio compartido, para cerrar el segundo CRUD dual y comprobar el sentido inverso de la persistencia compartida (crear por REST, ver en MVC).

### Alcance técnico
- `ProfesionesController` (MVC) sobre `IProfesionService`, ViewModels y cinco vistas de [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas).
- Convención de errores de HU-006 para duplicados y conflictos de borrado.
- **No incluye:** cambios de contrato REST.

### Criterios de aceptación
1. Las cinco acciones web funcionan en navegador contra `persona_db`.
2. Un `id` duplicado o un `nom` ausente se informan en el formulario sin escribir en la base.
3. El intento de eliminar una profesión con estudios asociados muestra una explicación legible y no elimina el registro.
4. Una profesión creada por REST se visualiza en la vista `Index` de MVC (escenario SRS 10.2).
5. `ProfesionesController` depende solo de `IProfesionService` y no referencia `DbContext`.

**Evidencia mínima:** capturas de las cinco vistas y del borrado bloqueado, más la evidencia de la creación cruzada por REST.

### Dependencias
**HU-005** y **HU-006**.

### Trazabilidad
RF-02, RF-05, RF-06 · RNF-01, RNF-06 · AD-02, AD-03 · [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas), §7 · criterios de éxito 5, 6 · reglas 2, 3, 12 · AK-02.

### Definition of Done
DoD común + recorrido en navegador, comprobación cruzada por REST y PR revisado.
