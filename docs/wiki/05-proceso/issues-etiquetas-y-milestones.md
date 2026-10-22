# Issues, etiquetas y milestones

[← Proceso](README.md) · [Backlog](../06-backlog/README.md)

## Convención de issues

- Una tarea verificable por issue, identificador interno `LAB-NN` cuando se apruebe el backlog.
- Campos: título, objetivo, descripción técnica, criterios de aceptación, componente afectado, responsable y sprint.
- Estados **solo** en GitHub Projects: `Backlog`, `In Progress`, `In Review`, `Blocked`, `Done`.
- No duplicar el estado en `docs/wiki/06-backlog/estado.md` ni en tablas paralelas.
- Vincular PR con `Closes #<número-real>`, no con identificadores supuestos.

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
