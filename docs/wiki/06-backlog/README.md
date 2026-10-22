# 06 · Backlog

[← Índice de la wiki](../Home.md)

Esta sección define el **alcance, el reparto y los criterios** del backlog de PersonAPI: 15 historias derivadas de [SRS](../01-producto/01-SRS.md) → [SAD](../02-arquitectura/02-SAD-HLD.md) → [SDD](../04-implementacion/03-SDD-LDD.md) → [contratos](../03-contratos/README.md). **El estado del trabajo no vive aquí:** responsables reales, estados y progreso se gestionan exclusivamente en GitHub Projects.

> Las 15 issues **todavía no están creadas** en GitHub. Crearlas es una acción posterior y explícita del equipo, con el backlog aprobado. Ver [publicación](#publicación-en-github).

## Páginas

| Página | Contenido |
|---|---|
| [backlog.tsv](backlog.tsv) | Registro estructurado de las 15 historias, pensado para crear las issues con GitHub CLI |
| [plan-de-sprints.md](plan-de-sprints.md) | Reparto por sprint e integrante, objetivos, entregables, dependencias y acuerdos sobre archivos compartidos |
| [priorizacion.md](priorizacion.md) | Significado de P0/P1, secuencia de implementación y ajustes hechos a la propuesta inicial |
| [sustentacion.md](sustentacion.md) | Guion de demostración: arquitectura, funcionalidad, persistencia, errores y entrega |
| [plantilla-issue.md](plantilla-issue.md) | Estructura obligatoria al crear la issue real |
| [crear-issues.sh](crear-issues.sh) | Creación reproducible de labels, milestones e issues desde el TSV, con `dry-run` por defecto |

## Épicas

| Épica | Alcance | Historias | Puntos |
|---|---|---|---|
| [EP-00 · Base técnica](epicas/EP-00-base-tecnica.md) | Solución .NET 10, esquema `persona_db`, Database First, contratos DAO, DI y presentación Razor común | HU-001, HU-002, HU-003, HU-006 | 14 |
| [EP-01 · Personas](epicas/EP-01-personas.md) | CRUD dual de `persona`, llave `cc` | HU-004, HU-007 | 9 |
| [EP-02 · Profesiones](epicas/EP-02-profesiones.md) | CRUD dual de `profesion`, llave `id` | HU-005, HU-008 | 7 |
| [EP-03 · Teléfonos](epicas/EP-03-telefonos.md) | CRUD dual de `telefono`, llave `num` y FK `duenio` | HU-010, HU-012 | 7 |
| [EP-04 · Estudios](epicas/EP-04-estudios.md) | CRUD dual de `estudios`, clave compuesta `(id_prof, cc_per)` | HU-011, HU-013 | 9 |
| [EP-05 · Integración y entrega](epicas/EP-05-integracion-y-entrega.md) | OpenAPI 3 y Swagger, verificación integral y entrega con `v1.0.0` | HU-009, HU-014, HU-015 | 12 |

**Total: 15 historias · 58 puntos · 2 sprints · 3 integrantes.** Los responsables son temporales (`A`, `B`, `C`) hasta que el equipo asigne los usuarios reales de GitHub; en los documentos no se inventan nombres ni cuentas.

## Alcance del backlog

Cubre lo exigido por el laboratorio y nada más:

- **Infraestructura y arquitectura:** solución MVC, `persona_db`, DDL y DML, EF Core Database First, entidades y `DbContext`, interfaces DAO e implementaciones, servicios de negocio, inyección de dependencias y estructura compartida de la interfaz.
- **Funcionalidad:** crear, listar, consultar por identificador, actualizar y eliminar las cuatro entidades, **por MVC y por REST** (20 endpoints + 20 acciones web).
- **Validaciones:** identificadores duplicados, campos obligatorios, identificadores inexistentes, referencias inválidas, restricciones de eliminación, integridad referencial, códigos HTTP y errores visibles en Razor.
- **Integración y entrega:** Swagger/OpenAPI 3, pruebas CRUD por entidad, pruebas de persistencia real, pruebas de relaciones, demostración E2E, README verificado, revisión arquitectónica, tag `v1.0.0` y material académico.

Queda **fuera**, conforme a [SRS §2](../01-producto/01-SRS.md#2-alcance-confirmado) y [AD-08](../02-arquitectura/02-SAD-HLD.md#2-decisiones-arquitectónicas-definitivas): autenticación, Docker, CI, microservicios, broker, gateway y nube. Ninguna historia los introduce.

## Reglas de gestión

1. Una issue = una unidad de trabajo verificable = una rama = un PR.
2. Cada issue enlaza al menos un `RF-*`/`RNF-*` del SRS, la decisión `AD-*` del SAD y el contrato o sección del SDD que concreta.
3. Los estados (`Backlog`, `In Progress`, `In Review`, `Blocked`, `Done`) se registran **solo** en GitHub Projects; no se replican en Markdown. No existe `estado.md`.
4. Una issue no entra en ejecución sin cumplir el [DoR](../05-proceso/dor-y-dod.md) ni se cierra sin cumplir el DoD con evidencia.
5. Las dependencias se declaran solo ante un bloqueo técnico real; se enuncian por ID de historia.
6. Los defectos hallados fuera de la issue propia se reportan en comentarios o en una issue nueva; no se corrigen en la rama de otra persona.
7. **El informe académico se escribe en LaTeX y `docs/informe/main.tex` es su fuente de verdad: no se toca.** Ninguna historia de este backlog edita el `.tex`, ni lo regenera, ni lo sustituye por Markdown, ni versiona un PDF como si fuera la fuente. HU-015 **verifica y cita** su contenido; cualquier ajuste de redacción se solicita a quien mantiene el documento, que lo aplica en el `.tex`. La misma regla rige para cualquier persona o asistente automatizado.

## Configuración de GitHub Projects

**Project:** `PersonAPI - Laboratorio MVC DAO` ([bootstrap](../05-proceso/bootstrap-repositorio.md)).

| Campo | Valores |
|---|---|
| Status | `Backlog`, `In Progress`, `In Review`, `Blocked`, `Done` |
| Sprint | `1`, `2` |
| Module | `Base`, `Datos`, `Personas`, `Profesiones`, `Telefonos`, `Estudios`, `Pruebas`, `Entrega` |
| Priority | `P0`, `P1` |
| Assignee | integrante real |
| Points | entero (escala 3, 4, 5) |

**Milestones:** `Sprint 1`, `Sprint 2`.

**Etiquetas** (familias ya acordadas en [issues, etiquetas y milestones](../05-proceso/issues-etiquetas-y-milestones.md)): `area:repo`, `area:core`, `area:database`, `area:personas`, `area:profesiones`, `area:telefonos`, `area:estudios`, `area:api`, `area:mvc`, `area:qa`, `area:testing`, `area:docs`, `priority:P0`, `priority:P1`. No se añade más taxonomía: con 15 issues, el módulo y la prioridad ya son campos del Project.

> **Conflicto documental pendiente de acuerdo.** [bootstrap-repositorio.md](../05-proceso/bootstrap-repositorio.md) y la [plantilla de issue](plantilla-issue.md) nombran el campo `Module` como `Core, Database, Personas, Profesiones, Telefonos, Estudios, API, QA, Deploy`. Este backlog usa el conjunto `Base, Datos, Personas, Profesiones, Telefonos, Estudios, Pruebas, Entrega`, que es el aprobado para la planificación. Son dos nomenclaturas del mismo alcance; **no se resuelve en silencio**: se unifica en el PR que apruebe este backlog, actualizando la página dueña de la decisión de proceso. Las etiquetas `area:*` no cambian.

## Publicación en GitHub

`backlog.tsv` es la fuente estructurada; [`crear-issues.sh`](crear-issues.sh) la consume:

```bash
# Simulación (por defecto): no escribe nada en GitHub
./docs/wiki/06-backlog/crear-issues.sh --repo OWNER/personapi-dotnet

# Creación real: requiere autorización explícita del equipo
./docs/wiki/06-backlog/crear-issues.sh --repo OWNER/personapi-dotnet --apply
```

El script valida el TSV (15 filas, IDs únicos, reparto 6/9, 2+3 por integrante), crea las etiquetas y los milestones que falten, **omite** las issues cuyo `HU-###` ya exista en el repositorio y las asocia a su milestone y etiquetas. No crea el Project ni mueve tarjetas de estado: la incorporación al tablero y la asignación de personas se hacen en GitHub, donde vive el estado. Sin `--apply` no ejecuta ninguna acción de escritura.
