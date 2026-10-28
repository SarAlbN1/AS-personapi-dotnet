# 06 · Backlog

[← Índice de la wiki](../Home.md)

Esta sección define el **alcance, el reparto y los criterios** del backlog de PersonAPI: 17 historias derivadas de [SRS](../01-producto/01-SRS.md) → [SAD](../02-arquitectura/02-SAD-HLD.md) → [SDD](../04-implementacion/03-SDD-LDD.md) → [contratos](../03-contratos/README.md). **El estado del trabajo no vive aquí:** responsables reales, estados y progreso se gestionan exclusivamente en GitHub Projects.

> Las 17 issues están creadas en GitHub (`#2` a `#16` para HU-001 a HU-015; HU-016 y HU-017 se añadieron después, con autorización explícita del equipo) e incorporadas al Project. Cualquier alta posterior sigue requiriendo esa autorización. Ver [publicación](#publicación-en-github).

## Páginas

| Página | Contenido |
|---|---|
| [backlog.tsv](backlog.tsv) | Registro estructurado de las 17 historias, pensado para crear las issues con GitHub CLI |
| [plan-de-sprints.md](plan-de-sprints.md) | Reparto por sprint e integrante, objetivos, entregables, dependencias y acuerdos sobre archivos compartidos |
| [priorizacion.md](priorizacion.md) | Significado de P0/P1, secuencia de implementación y ajustes hechos a la propuesta inicial |
| [sustentacion.md](sustentacion.md) | Guion de demostración: arquitectura, funcionalidad, persistencia, errores y entrega |
| [equipo.tsv](equipo.tsv) | Mapeo de los marcadores `A`/`B`/`C` del plan de sprints al usuario real de GitHub; es lo único que `crear-issues.sh` usa para asignar una issue |
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
| [EP-05 · Integración y entrega](epicas/EP-05-integracion-y-entrega.md) | OpenAPI 3 y Swagger, verificación integral, documentación de arquitectura y despliegue, y entrega con `v1.0.0` | HU-009, HU-014, HU-015, HU-016, HU-017 | 18 |

**Total: 17 historias · 64 puntos · 2 sprints · 3 integrantes.** Los responsables se escriben como marcadores `A`, `B`, `C` en [backlog.tsv](backlog.tsv) y se traducen a cuentas reales en [equipo.tsv](equipo.tsv): tres columnas (`marcador`, `nombre`, `github`) que `crear-issues.sh` lee para añadir `--assignee`. Los tres usuarios están registrados: **A = Sara (`SarAlbN1`), B = Juan Diego (`Juandi48`), C = Miguel (`ma-sarmiento`)**. Una fila con la columna `github` vacía crearía la issue sin asignar y el script lo advierte; no se inventan cuentas ni se reparten responsabilidades en los documentos.

**Condición operativa previa a publicar las issues:** GitHub solo asigna una issue a quien es colaborador del repositorio, y solo cuenta como aprobación la revisión de alguien con permiso de escritura. Las invitaciones de `Juandi48` y `ma-sarmiento` deben estar **aceptadas** antes de ejecutar `crear-issues.sh --apply`; registrarlos aquí no sustituye esa aceptación.

## Alcance del backlog

Cubre lo exigido por el laboratorio y nada más:

- **Infraestructura y arquitectura:** solución MVC, `persona_db`, DDL y DML, EF Core Database First, entidades y `DbContext`, interfaces DAO e implementaciones, servicios de negocio, inyección de dependencias y estructura compartida de la interfaz.
- **Funcionalidad:** crear, listar, consultar por identificador, actualizar y eliminar las cuatro entidades, **por MVC y por REST** (20 endpoints + 20 acciones web).
- **Validaciones:** identificadores duplicados, campos obligatorios, identificadores inexistentes, referencias inválidas, restricciones de eliminación, integridad referencial, códigos HTTP y errores visibles en Razor.
- **Integración y entrega:** Swagger/OpenAPI 3, pruebas CRUD por entidad, pruebas de persistencia real, pruebas de relaciones, demostración E2E, README verificado, revisión arquitectónica, diagramas C4 de contexto, contenedores y componentes, manual técnico de despliegue en el informe, tag `v1.0.0` y material académico.

Queda **fuera**, conforme a [SRS §2](../01-producto/01-SRS.md#2-alcance-confirmado) y [AD-08](../02-arquitectura/02-SAD-HLD.md#2-decisiones-arquitectónicas-definitivas): autenticación, CI, microservicios, broker, gateway y nube. Ninguna historia los introduce.

**Docker** es un caso aparte: se conserva como propuesta de despliegue del equipo en el [informe](../../informe/main.tex), pero **ninguna de las 17 historias lo toma como criterio de aceptación**, porque los criterios se verifican con ejecución local. HU-017 **documenta** el despliegue tal como el informe lo declara, sin añadir ni retirar tecnologías respecto de ese documento, y solo presenta como verificado lo que ejecuta. Si el enunciado original lo exige como entregable, se resuelve en la etapa de entrega sin alterar el reparto de historias ni los puntos.

## Reglas de gestión

1. Una issue = una unidad de trabajo verificable = una rama = un PR.
2. Cada issue enlaza al menos un `RF-*`/`RNF-*` del SRS, la decisión `AD-*` del SAD y el contrato o sección del SDD que concreta.
3. Los estados (`Backlog`, `In Progress`, `In Review`, `Blocked`, `Done`) se registran **solo** en GitHub Projects; no se replican en Markdown. No existe `estado.md`.
4. Una issue no entra en ejecución sin cumplir el [DoR](../05-proceso/dor-y-dod.md) ni se cierra sin cumplir el DoD con evidencia.
5. Las dependencias se declaran solo ante un bloqueo técnico real; se enuncian por ID de historia.
6. Los defectos hallados fuera de la issue propia se reportan en comentarios o en una issue nueva; no se corrigen en la rama de otra persona.
7. **El informe académico se escribe en LaTeX y `docs/informe/main.tex` es su fuente de verdad.** Ninguna historia lo regenera, lo sustituye por Markdown ni versiona un PDF como si fuera la fuente, y **solo quien mantiene el documento escribe en él**: la única historia que lo edita es **HU-017** (manual técnico de despliegue), asignada precisamente a esa persona. HU-015 **verifica y cita** su contenido; cualquier ajuste de redacción se solicita a quien mantiene el documento, que lo aplica en el `.tex`. La misma regla rige para cualquier persona o asistente automatizado.

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

**Etiquetas** (familias ya acordadas en [issues, etiquetas y milestones](../05-proceso/issues-etiquetas-y-milestones.md)): `area:repo`, `area:core`, `area:database`, `area:personas`, `area:profesiones`, `area:telefonos`, `area:estudios`, `area:api`, `area:mvc`, `area:qa`, `area:testing`, `area:docs`, `priority:P0`, `priority:P1`. No se añade más taxonomía: con 17 issues, el módulo y la prioridad ya son campos del Project.

> **Taxonomía `Module` unificada.** El campo usa un único conjunto de ocho valores: `Base`, `Datos`, `Personas`, `Profesiones`, `Telefonos`, `Estudios`, `Pruebas`, `Entrega`, idéntico a la columna `modulo` de [backlog.tsv](backlog.tsv) y replicado en [bootstrap-repositorio.md](../05-proceso/bootstrap-repositorio.md) y en la [plantilla de issue](plantilla-issue.md). Es un campo de GitHub Projects, no una etiqueta: las etiquetas `area:*` siguen siendo la dimensión transversal (`area:api`, `area:mvc`, `area:docs`) y no cambian. Sin acentos, para que el valor del campo, la etiqueta y el TSV coincidan carácter a carácter.

## Publicación en GitHub

`backlog.tsv` es la fuente estructurada; [`crear-issues.sh`](crear-issues.sh) la consume:

```bash
# Simulación (por defecto): no escribe nada en GitHub
./docs/wiki/06-backlog/crear-issues.sh --repo OWNER/personapi-dotnet

# Creación real: requiere autorización explícita del equipo
./docs/wiki/06-backlog/crear-issues.sh --repo OWNER/personapi-dotnet --apply
```

El script valida el TSV (17 filas, IDs únicos, reparto 6/11 por sprint, 2 issues por integrante en Sprint 1 y 3/3/5 en Sprint 2, `modulo` dentro de la taxonomía `Module`), resuelve los responsables con [equipo.tsv](equipo.tsv), crea las etiquetas y los milestones que falten, **omite** las issues cuyo `HU-###` ya exista en el repositorio y las asocia a su milestone y etiquetas. No crea el Project ni mueve tarjetas de estado: la incorporación al tablero y la asignación de personas se hacen en GitHub, donde vive el estado. Sin `--apply` no ejecuta ninguna acción de escritura.
