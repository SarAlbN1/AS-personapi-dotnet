# EP-03 · Teléfonos

[← Backlog](../README.md) · [Persistencia](../../03-contratos/persistencia.md) · [Contrato REST](../../03-contratos/api-rest.md)

**Propósito:** entregar el CRUD completo de `telefono` por REST y Razor, llave `num` y FK `duenio` → `persona.cc`. Es el módulo que demuestra la integridad de una relación 1:N y el rechazo de referencias inválidas.

**Requisitos cubiertos:** RF-03, RF-06 en la parte de teléfono, RNF-01, RNF-02, RNF-03, RNF-06.

**Decisiones aplicadas:** AD-02, AD-03, AD-06 (FK sin cascada), AD-10.

**Componentes afectados:** `01-Presentacion/Dtos/`, `01-Presentacion/ViewModels/`, `ITelefonoService` / `TelefonoService`, `01-Presentacion/ControllersApi/TelefonosApiController`, `01-Presentacion/Controllers/TelefonosController`, `01-Presentacion/Views/Telefonos/`.

**Historias:** HU-010, HU-012 (ambas Sprint 2) · **7 puntos**

**Criterio de éxito de la épica:** los cinco endpoints de `/api/telefonos` y las cinco vistas Razor operan contra `persona_db`; un teléfono solo puede vincularse a una persona existente ([criterio de éxito 7](../../01-producto/criterios-de-exito.md)); `num` conserva ceros iniciales.

> **Paralelismo:** este módulo depende de que existan filas de `persona` (las aporta el DML de HU-002), **no** de que las pantallas o la API de personas estén terminadas. Por eso no declara dependencia de EP-01, conforme a la nota de paralelización del [SAD §5](../../02-arquitectura/02-SAD-HLD.md#5-vista-por-módulos).

---

## HU-010 · API REST de teléfonos de extremo a extremo con validación de dueño

**Épica** EP-03 · **Sprint** 2 · **Responsable** Integrante A · **Prioridad** P1 · **Puntos** 4 · **Módulo** Teléfonos

### Objetivo
Publicar las cinco operaciones REST de teléfonos validando la referencia a persona, para demostrar que la aplicación impide datos huérfanos antes de llegar a la base y que la base los impide de todos modos.

### Alcance técnico
- DTOs de teléfono con `num` como cadena (preservando ceros iniciales, [api-rest.md](../../03-contratos/api-rest.md#representaciones-json-ejemplos)).
- `ITelefonoService` / `TelefonoService`: unicidad de `num`, longitud ≤ 15, existencia de `duenio` ([SDD §7](../../04-implementacion/03-SDD-LDD.md#7-reglas-y-validaciones-por-entidad): FK inexistente ⇒ `400`).
- `TelefonosApiController` con las rutas del contrato y una única línea DI para el servicio.
- Sigue la convención OpenAPI definida en HU-009 (no bloqueante: si aún no está integrada, se aplica al cerrar HU-009).
- **No incluye:** vistas Razor (HU-012).

### Criterios de aceptación
1. Las cinco operaciones responden con `200`, `201` + `Location`, `204`, `400`, `404` y `409` según el contrato.
2. `num` se trata como cadena de hasta 15 caracteres y preserva ceros iniciales en la respuesta y en la base.
3. POST o PUT con `duenio` inexistente devuelve `400` y no escribe en la base.
4. `num` duplicado devuelve `409`; `num` de ruta distinto al del cuerpo en PUT devuelve `400`; `num` inexistente devuelve `404`.
5. `TelefonosApiController` depende solo de `ITelefonoService` y no referencia `DbContext`.
6. Las personas usadas como dueño provienen del DML de HU-002.

**Evidencia mínima:** peticiones y respuestas de cada caso, incluida la prueba de ceros iniciales, más la consulta SQL de la fila creada.

### Dependencias
**HU-003**.

### Trazabilidad
RF-03, RF-06 · RNF-02, RNF-03 · AD-03, AD-06 · [persistencia](../../03-contratos/persistencia.md#invariantes-comprobables) · [SDD §5](../../04-implementacion/03-SDD-LDD.md#5-contratos-rest-exactos-20-operaciones), §7 · reglas 2, 3, 9 · AK-04.

### Definition of Done
DoD común + endpoints probados, intento de huérfano rechazado y PR revisado.

---

## HU-012 · CRUD MVC de teléfonos con selector de persona

**Épica** EP-03 · **Sprint** 2 · **Responsable** Integrante A · **Prioridad** P1 · **Puntos** 3 · **Módulo** Teléfonos

### Objetivo
Entregar las cinco vistas Razor de teléfonos con selector de dueño, para que la relación con persona se construya sin escribir claves a mano y los errores de FK sean visibles.

### Alcance técnico
- `TelefonosController` (MVC) sobre `ITelefonoService` y el servicio de personas para poblar el selector ([SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas): `Telefonos` usa selector de persona).
- Cinco vistas y convención de errores de HU-006.
- **No incluye:** cambios en el contrato REST.

### Criterios de aceptación
1. Las cinco acciones web funcionan en navegador contra `persona_db`.
2. El selector de dueño se alimenta del servicio de personas y no permite enviar un `duenio` inexistente.
3. `num` duplicado o longitud superior a 15 se informan en el formulario sin escribir en la base.
4. Un teléfono creado desde MVC se consulta con `GET /api/telefonos/{num}` y coincide.
5. `TelefonosController` depende solo de servicios y no referencia `DbContext`.

**Evidencia mínima:** capturas de las cinco vistas y del selector poblado, más la respuesta REST cruzada.

### Dependencias
**HU-010** y **HU-006**.

### Trazabilidad
RF-03, RF-06 · RNF-01, RNF-06 · AD-02, AD-10 · [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas) · criterios de éxito 5, 7 · reglas 2, 3, 8 · AK-02.

### Definition of Done
DoD común + recorrido en navegador, comprobación cruzada por REST y PR revisado.
