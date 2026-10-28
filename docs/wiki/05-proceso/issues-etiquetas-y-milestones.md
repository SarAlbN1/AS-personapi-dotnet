# Issues, etiquetas y milestones

[← Proceso](README.md) · [Backlog](../06-backlog/README.md)

## Convención de issues

- Una tarea verificable por issue, identificador interno `LAB-NN` cuando se apruebe el backlog.
- Campos: título, objetivo, descripción técnica, criterios de aceptación, componente afectado, responsable y sprint.
- Estados **solo** en GitHub Projects: `Backlog`, `In Progress`, `In Review`, `Blocked`, `Done`.
- No duplicar el estado en `docs/wiki/06-backlog/estado.md` ni en tablas paralelas.
- Vincular PR con `Closes #<número-real>`, no con identificadores supuestos.

## Transiciones de estado

El tablero refleja el trabajo real, no la intención. Cada transición se aplica cuando ocurre el hecho que la dispara, y ese hecho es comprobable en GitHub:

| De | A | Hecho que la dispara |
|---|---|---|
| `Backlog` | `In Progress` | Se empieza la issue: existe su rama y hay trabajo en curso. |
| `In Progress` | `In Review` | Se abre el PR que la cierra, con `Closes #<número>`. |
| `In Review` | `Done` | El PR se fusiona en `main`. |
| `In Review` | `In Progress` o `Backlog` | El PR se cierra sin fusionar, según siga o no en curso el trabajo. |
| cualquiera | `Blocked` | Una dependencia externa impide avanzar; al desbloquearse vuelve al estado que le corresponda. |

El estado es el del **hecho más avanzado que ya ocurrió**, no la suma de los pasos intermedios: si la rama y el PR se crean el mismo día, la issue pasa directamente a `In Review` sin haber estado en `In Progress`. La tabla nombra las transiciones habituales; saltarse una fila porque el hecho posterior ya ocurrió no es un caso que haya que informar.

Una issue no pasa a `Done` por tener el PR aprobado, fusionable o sin conflictos: solo cuando está fusionado. `Done` en el tablero no sustituye al [DoD](dor-y-dod.md): sus puntos 1 a 9 son la condición para fusionar, y el paso a `Done` es su punto 10.

**Agentes.** Un asistente automatizado puede mover los elementos del tablero entre estos estados cuando el hecho que dispara la transición ya ocurrió, también en issues de otros integrantes, porque reflejar el estado real no es apropiarse del trabajo. No cambia responsable, sprint, módulo, prioridad ni alcance, no crea issues ni las cierra a mano —el cierre automático por `Closes #<número>` al fusionar es parte de la integración, no una acción sobre la issue—, y no mueve a `Done` nada cuyo PR no esté fusionado. Si el estado real no encaja en ninguna transición, lo informa en vez de elegir una.

Esa autorización es **permanente y no se pide dos veces**: mover el tablero forma parte del acto que dispara la transición, no es una tarea aparte que haya que encargar ni confirmar caso por caso. Quien abre un PR, registra una aprobación o fusiona deja el estado al día en el mismo momento, y después dice qué movió. Pedir permiso para reflejar un hecho ya ocurrido deja el tablero desactualizado mientras se espera la respuesta, que es justo lo que estas obligaciones evitan. Los límites siguen siendo los del párrafo anterior: no se toca responsable, sprint, módulo, prioridad ni alcance, no se crean ni se cierran issues a mano, y nada pasa a `Done` sin su PR fusionado.

## Obligaciones de actualización del tablero

Mover el tablero **no es opcional ni es tarea de quien pase por ahí**: cada hecho tiene un responsable nombrado y un momento exacto. Un tablero desactualizado hace que el equipo planifique sobre datos falsos, así que estas cuatro obligaciones se cumplen siempre.

| Hecho | Quién lo registra | Cuándo | Resultado en el Project |
|---|---|---|---|
| Se empieza la historia | el responsable de la issue | **antes del primer commit** de su rama | `Backlog` → `In Progress` |
| Se abre el PR con `Closes #<número>` | quien abre el PR | al abrirlo | `In Progress` → `In Review` |
| El PR se fusiona tras una aprobación válida | **quien fusiona** | en el mismo momento de la fusión, no al final del día | `In Review` → `Done` |
| La fusión deja sin dependencias a otras issues | **quien fusiona** | inmediatamente después, en la misma sesión | las desbloqueadas salen de `Blocked` |

1. **Al empezar una historia se cambia el estado.** Quien la toma la pasa a `In Progress` antes de su primer commit, para que nadie más la crea libre. Si abre la rama y el PR el mismo día, va directo a `In Review`: el estado es el del hecho más avanzado.
2. **Al fusionar se pasa a `Done`, obligatoriamente.** La fusión y el cambio de estado son el mismo acto. No se deja para después, no se delega en el autor si fusionó otra persona, y no se da por hecho que el cierre automático de la issue por `Closes #<número>` mueva la tarjeta: cerrar la issue y mover el estado son cosas distintas. Un PR fusionado con su issue fuera de `Done` es trabajo incompleto, no una cuestión de forma.
3. **Después de fusionar se revisa qué queda desbloqueado.** Quien fusiona consulta la columna `dependencias` de [backlog.tsv](../06-backlog/backlog.tsv), busca las issues que declaraban esa historia como bloqueante y, para cada una que ya no tenga ninguna dependencia pendiente, la saca de `Blocked`: a `Backlog` si nadie la ha empezado, o a `In Progress` si su responsable ya tiene rama. Si quedan otras dependencias sin fusionar, se queda en `Blocked` y se dice cuáles en un comentario de la issue.
4. **Se avisa a quien se desbloquea.** El barrido anterior se acompaña de un comentario en la issue desbloqueada o un mensaje al responsable: el cambio de columna no sirve si la persona no se entera el mismo día.

Nada de esto se registra en Markdown: el estado vive en el Project ([regla de no duplicar](#convención-de-issues)). Lo que vive en la wiki es esta obligación, no el estado.

## Etiquetas

Usar etiquetas existentes cuando sirvan (`enhancement`, `bug`, `documentation`) y crear solo las necesarias:

| Familia | Etiquetas sugeridas |
|---|---|
| Área | `area:core`, `area:database`, `area:personas`, `area:profesiones`, `area:telefonos`, `area:estudios`, `area:api`, `area:mvc`, `area:qa`, `area:docs`, `area:repo` |
| Prioridad | `priority:P0`, `priority:P1` |
| Tipo adicional | `chore`, `test`, `adr` (solo si son necesarias) |

`Blocked` debe ser un **estado del Project**, no una segunda etiqueta obligatoria. `Module` es un campo simple y coincide con la clasificación aprobada en [bootstrap](bootstrap-repositorio.md).

## Milestones y reparto

- Milestones: `Sprint 1` y `Sprint 2`.
- Plan inicial: 15 issues, 3 integrantes, 5 issues por persona; en Sprint 1, 2 por persona y en Sprint 2, 3 por persona.
- Estado vigente: 17 issues tras añadir HU-016 y HU-017 y reasignar HU-008 y HU-012; el reparto por persona es 5/5/7 y la justificación está en la [priorización](../06-backlog/priorizacion.md#ampliación-documental-y-reasignación-hu-016-hu-017).
- Cantidad igual no garantiza esfuerzo igual. Validar tamaño, riesgos y carga de revisión antes de asignar definitivamente.
- Dependencias: fijar contratos compartidos temprano; no bloquear un módulo hasta que otro esté terminado si puede desarrollarse con datos de prueba y contratos estables.

## Ready / Done

Aplicar [DoR/DoD](dor-y-dod.md) a todos los issues, incluidas documentación, pruebas y entrega. GitHub es la fuente viva de responsables, estados y sprints.
