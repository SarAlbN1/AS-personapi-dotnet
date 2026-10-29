# Plan de sustentación

[← Backlog](README.md) · [Runbook de demo](../04-implementacion/runbook-demo.md) · [Criterios de éxito](../01-producto/criterios-de-exito.md)

Guion para demostrar PersonAPI ante el docente. Cubre funcionalidad, arquitectura, persistencia, manejo de errores y entrega. **No es evidencia por sí mismo:** cada paso se ejecuta en vivo o con la evidencia registrada en HU-014, y el recorrido detallado se mantiene en el [runbook de demo](../04-implementacion/runbook-demo.md).

## Preparación

| Paso | Qué se muestra | Historia que lo respalda |
|---|---|---|
| Clon limpio del repositorio público | Reproducibilidad, nombre `personapi-dotnet` | HU-015 |
| `01_ddl.sql` y `02_dml.sql` en SSMS | `persona_db` creada desde cero con PK, FK y CHECK | HU-002 |
| `dotnet build` y arranque | Un solo proceso, un artefacto | HU-001 |
| `dotnet --info` y versiones de paquetes | Línea base de [versiones](../04-implementacion/versiones.md) | HU-015 |

## Bloque 1 · Arquitectura (antes de la funcionalidad)

Se explica el flujo con el [SAD §4](../02-arquitectura/02-SAD-HLD.md#4-vista-lógica-y-reglas-de-dependencia) y se demuestra en el código:

1. `PersonasApiController` y `PersonasController` dependen de `IPersonaService`: dos presentaciones, una lógica (AD-02, AD-03).
2. `PersonaService` depende de `IPersonaRepository`, no de EF Core (AD-04).
3. Solo `Back/03-AccesoDatos/Repository/Implementations` referencia `PersonaDbContext`: se muestra la búsqueda que lo comprueba (AK-01).
4. `estudios` se mapea con clave compuesta, sin identificador artificial (AK-03).
5. DTOs y ViewModels evitan exponer entidades con navegaciones (AD-10).

**Respalda:** HU-003, HU-004, HU-011, HU-014 · **Evidencia:** estructura de carpetas, registro DI de `Program.cs` y salida de la comprobación arquitectónica.

## Bloque 2 · Funcionalidad por los dos canales

Un recorrido por entidad, alternando canal para demostrar persistencia compartida:

| Orden | Demostración | Canal | Escenario SRS | Historia |
|---|---|---|---|---|
| 1 | Crear persona en Razor y leerla con `GET /api/personas/{cc}` | MVC → REST | 10.1 | HU-007, HU-004 |
| 2 | Crear profesión por REST y verla en la vista `Index` | REST → MVC | 10.2 | HU-005, HU-008 |
| 3 | Asignar teléfono a la persona creada y consultar la relación | MVC y REST | 10.3 | HU-010, HU-012 |
| 4 | Vincular persona y profesión como estudio y consultar por `/api/estudios/{idProf}/{ccPer}` | REST y MVC | 10.4 | HU-011, HU-013 |
| 5 | Recorrer `Index`, `Details`, `Create`, `Edit` y `Delete` de una entidad en Razor | MVC | criterio 5 | HU-007 |

**Cierre del bloque:** Swagger UI con las **20 operaciones** desplegadas y una ejecución en vivo por recurso (RF-07, AK-05) — HU-009.

## Bloque 3 · Persistencia real

1. Tras cada escritura de la demo, consulta `SELECT` en SSMS sobre la tabla afectada: la aplicación no mantiene estado en memoria.
2. Mostrar que MVC y REST leen la misma fila (criterio de éxito 6).
3. Mostrar el intento de inserción inválida directamente en SQL: `cc` duplicada y `telefono.duenio` inexistente fallan en el motor, no solo en la aplicación (RNF-02).

**Respalda:** HU-002, HU-003, HU-014.

## Bloque 4 · Manejo de errores e integridad

| Caso | Resultado esperado | Donde se muestra |
|---|---|---|
| POST con `cc` ya existente | `409` en REST, mensaje en el formulario | HU-004, HU-007 |
| POST de teléfono con `duenio` inexistente | `400`, sin escritura | HU-010 |
| POST de estudio con profesión inexistente | `400`, sin escritura | HU-011 |
| Par `(idProf, ccPer)` duplicado | `409` | HU-011, HU-013 |
| `GET`/`PUT`/`DELETE` con clave inexistente | `404` | los cuatro módulos |
| `PUT` con clave de ruta distinta a la del cuerpo | `400` | los cuatro módulos |
| **Eliminar persona con teléfono y estudio** | `409` en REST y explicación legible en Razor, base sin cambios | HU-004, HU-007 (escenario 10.5) |
| Eliminar primero los dependientes y luego la persona | operación exitosa | escenario 10.6 |
| Error inesperado | respuesta controlada sin trazas SQL ni secretos | HU-009 |

Este bloque es el que demuestra RF-05, RF-06 y AK-04; se ejecuta en vivo porque es el más fácil de falsear con capturas.

## Bloque 5 · Proceso y entrega

1. GitHub Projects como fuente de estado: 17 issues, dos milestones, campos `Sprint`, `Module`, `Priority`, `Points`.
2. Trazabilidad de una issue cualquiera a su `RF-*`, su `AD-*` y su sección del SDD.
3. Un PR real: issue vinculada con `Closes #<número>`, evidencia de pruebas y revisión cruzada de otro integrante.
4. Lista de revisión arquitectónica cerrada y los siete killers con evidencia.
5. Diagramas C4 de contexto, contenedores y componentes como apoyo del Bloque 1 (HU-016).
6. Repositorio público, README verificado en clon limpio, scripts, informe académico compilado desde `docs/informe/main.tex` —fuente de verdad en LaTeX, escrita solo por quien la mantiene e incluido el manual técnico de despliegue de HU-017— y **URL del tag `v1.0.0`**.

**Respalda:** HU-014, HU-015, HU-016, HU-017 · **Referencias:** [entrega](../05-proceso/entrega.md), [pull requests y releases](../05-proceso/pull-requests-y-releases.md).

## Reparto de la exposición

| Integrante | Bloques | Coherencia con su trabajo |
|---|---|---|
| A | Bloque 2 personas y teléfonos | Propietario de EP-01 y EP-03 |
| B | Bloque 2 profesiones y estudios, Bloque 4 | Propietario de EP-02 y EP-04, incluida la clave compuesta |
| C | Bloques 1, 3 y 5 | Propietario de la base de datos, Swagger, verificación y entrega |

Cada integrante expone lo que implementó. Los tres deben poder responder sobre el flujo completo de capas, porque es la pregunta central del laboratorio.

## Qué no se demuestra

No hay autenticación, CI, microservicios, broker ni despliegue en nube: están **fuera del alcance** aprobado en [SRS §2](../01-producto/01-SRS.md#2-alcance-confirmado) y [AD-08](../02-arquitectura/02-SAD-HLD.md#2-decisiones-arquitectónicas-definitivas). Si se preguntan, se responde con la decisión documentada, no improvisando una implementación.

Si preguntan por **Docker**: la propuesta de despliegue con Dockerfile y Docker Compose está desarrollada en el informe y se explica desde ahí. Lo que se demuestra en vivo es la ejecución local, que es la que tiene evidencia. No se afirma que los contenedores estén probados si no se ejecutaron.
