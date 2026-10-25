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

Una issue no pasa a `Done` por tener el PR aprobado, fusionable o sin conflictos: solo cuando está fusionado. `Done` en el tablero no sustituye al [DoD](dor-y-dod.md), que sigue siendo la condición para fusionar.

**Agentes.** Un asistente automatizado puede mover los elementos del tablero entre estos estados cuando el hecho que dispara la transición ya ocurrió, también en issues de otros integrantes, porque reflejar el estado real no es apropiarse del trabajo. No cambia responsable, sprint, módulo, prioridad ni alcance, no crea ni cierra issues, y no mueve a `Done` nada cuyo PR no esté fusionado. Si el estado real no encaja en ninguna transición, lo informa en vez de elegir una.

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
- Cantidad igual no garantiza esfuerzo igual. Validar tamaño, riesgos y carga de revisión antes de asignar definitivamente.
- Dependencias: fijar contratos compartidos temprano; no bloquear un módulo hasta que otro esté terminado si puede desarrollarse con datos de prueba y contratos estables.

## Ready / Done

Aplicar [DoR/DoD](dor-y-dod.md) a todos los issues, incluidas documentación, pruebas y entrega. GitHub es la fuente viva de responsables, estados y sprints.
