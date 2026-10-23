# CLAUDE.md · Reglas de trabajo para PersonAPI

Este repositorio es un **monolito MVC + servicios + DAO + EF Core + SQL Server**, desplegable como un solo artefacto. La documentación vive en [`docs/wiki/Home.md`](docs/wiki/Home.md).

## Jerarquía

El orden de autoridad es **enunciado del laboratorio → wiki → este archivo → informe**.

0. **Enunciado original del laboratorio** (documento del docente, no versionado en este repositorio): fija los requisitos y entregables académicos. Es la autoridad máxima. Ningún documento de la wiki, de este archivo ni del informe puede añadir, quitar ni reinterpretar un requisito del enunciado. Si una afirmación depende de un dato del enunciado que no está disponible, se deja **condicionada** y se pide el documento; no se convierte una suposición en requisito.
1. **Wiki** `docs/wiki/`: especificación, arquitectura, diseño, contratos y decisiones aprobadas del equipo, en este orden interno:
   1. SRS: `docs/wiki/01-producto/01-SRS.md` (alcance y criterios).
   2. SAD: `docs/wiki/02-arquitectura/02-SAD-HLD.md` (decisiones y capas).
   3. SDD: `docs/wiki/04-implementacion/03-SDD-LDD.md` (clases, interfaces, vistas y endpoints).
   4. Contratos `docs/wiki/03-contratos/`; reglas `docs/wiki/02-arquitectura/reglas-arquitectonicas.md`.
   5. Proceso `docs/wiki/05-proceso/`; backlog **vivo** en GitHub Projects.
   6. `docs/wiki/06-backlog/` contiene el backlog documentado (15 historias, 6 épicas, priorización, plan de sprints, guion de sustentación, plantilla de issue, `equipo.tsv` y `crear-issues.sh`). Es material **derivado** de SRS, SAD y SDD: no decide arquitectura, alcance ni contratos. El estado del trabajo vive en GitHub Projects. No publicar, crear ni asignar issues sin autorización explícita.
2. **Este archivo (`CLAUDE.md`)**: instrucciones operativas del asistente, derivadas del enunciado y de la wiki. **No tiene autoridad para crear ni modificar requisitos ni decisiones arquitectónicas**, y no puede imponer reglas que contradigan el enunciado o la wiki. Si choca con alguno de ellos, se corrige este archivo.
3. **Informe académico** `docs/informe/`: presenta y desarrolla el trabajo; debe ser coherente con el enunciado y con las decisiones vigentes, y no es fuente de decisiones arquitectónicas. **Sus contenidos no se reescriben para resolver una contradicción**: la contradicción se reporta y la corrige quien mantiene el documento propietario. La única intervención admitida sin esa revisión es retirar credenciales o datos sensibles antes de publicar.

Ante conflictos, no adivinar y no editar por iniciativa propia: documentar el conflicto en issue/PR, proponer la corrección a la fuente propietaria y pedir revisión. Esto vale también para el informe y para cualquier documento entregado por una persona del equipo. La documentación describe el diseño; no afirmar implementación ni pruebas inexistentes.

## Lectura según tarea

| Tarea | Lectura mínima |
|---|---|
| Cualquier issue | SRS, reglas arquitectónicas, issue y contrato afectado |
| Endpoint REST | `03-contratos/api-rest.md`, SDD |
| Tabla/EF/DAO | `03-contratos/persistencia.md`, SDD |
| Vista MVC | SDD y `03-contratos/validaciones-y-errores.md` |
| Versiones y estructura | `04-implementacion/versiones.md`, `estructura-del-repositorio.md` |
| Pruebas/PR | `04-implementacion/pruebas.md`, `05-proceso/` |

## Reglas inviolables

- Un único monolito .NET 10, una base SQL Server `persona_db`, cuatro entidades.
- MVC y REST comparten servicios; nunca acceder a `DbContext` desde controladores.
- Servicios dependen de interfaces DAO; EF Core queda en repositorios.
- Database First: DDL es el origen de esquema, incluida PK compuesta de estudios.
- Sin eliminación en cascada de personas/profesiones con dependencias; conflictos controlados.
- No exponer entidades EF con navegaciones como contratos HTTP; DTO/ViewModels.
- API documentada: 20 operaciones de CRUD + Swagger/OpenAPI 3.
- Stack cerrado en `04-implementacion/versiones.md`: ASP.NET Core MVC 10, EF Core 10, SQL Server 2025 Express, Swagger/OpenAPI 3. Ampliarlo exige cambio aprobado del SRS y del SAD.
- Sin secretos versionados: la cadena de conexión se lee de configuración local.
- No modificar nada fuera del alcance de la issue asignada, ni documentar como implementado lo que solo está diseñado.
- **Sin rastro de IA en el repositorio:** ningún commit, mensaje de PR, documento, informe ni log incluye co-autoría, firma, mención de asistentes automatizados ni marcas de generación automática. El autor registrado es el integrante asignado. La declaración académica del uso de asistentes, si el docente la exige, se hace por su canal, no en metadatos del repositorio.

## Propiedad y colaboración

- Una issue: un responsable, una rama y un PR, con revisión de otra persona.
- No editar ni hacer push en ramas de otras personas ni corregir sus archivos sin asignación/acuerdo.
- Reportar defectos encontrados fuera de la issue propia en comentarios/issue.
- Antes de `Done`, demostrar pruebas, contratos y criterios; actualizar documentos afectados.
- GitHub Projects es la fuente de estado; no duplicar el backlog en Markdown.
- Equipo e identidades reales: [`docs/wiki/06-backlog/equipo.tsv`](docs/wiki/06-backlog/equipo.tsv). Sara (`SarAlbN1`), Juan Diego (`Juandi48`), Miguel (`ma-sarmiento`). No inventar cuentas ni reasignar responsables.

## Reglas obligatorias para Claude Code y cualquier agente

`main` está protegida en GitHub. Antes de modificar archivos, leer [CONTRIBUTING](CONTRIBUTING.md) y la [política de integración](docs/wiki/05-proceso/pull-requests-y-releases.md#protección-de-main-y-revisión-cruzada-obligatoria), que es la fuente única de estas reglas.

- **No escribir en `main`.** Ningún commit ni push directo, ni siquiera para un cambio trivial: se trabaja en rama y se integra por PR.
- **No aprobar un PR propio ni uno escrito por un agente.** La aprobación la da una persona distinta del autor, con permiso de escritura.
- **No fusionar sin una aprobación humana externa válida** registrada en el PR. Que GitHub indique `MERGEABLE`, `CLEAN` o ausencia de conflictos **no** es una aprobación, y no se presenta como tal en ningún informe.
- **No usar permisos administrativos para eludir las protecciones**, ni activar auto-merge, ni hacer force-push, ni borrar `main`.
- **No deshabilitar, relajar ni convertir a Ruleset** las reglas de protección de GitHub sin autorización expresa del equipo.
- **No sobrescribir ramas, commits ni worktrees de otros integrantes**, ni reescribir historia publicada.
- Respetar la revisión cruzada y **reportar resultados reales**: lo ejecutado se describe con su salida; lo no ejecutado se declara pendiente.
- Si una regla de GitHub impide una acción pedida, informarlo y detenerse; no buscar una vía alternativa para sortearla.

## Protocolo de implementación

1. Leer issue, requisitos SRS, decisiones SAD y diseño SDD relevante.
2. Confirmar contratos/llaves y alcance antes de modificar.
3. Implementar en capas y con pruebas, solo dentro de lo asignado.
4. Ejecutar build, tests aplicables, y verificar recorrido real si la issue es E2E.
5. Abrir PR con evidencia y `Closes #<número>`, recibir revisión y fusionar con squash.

**No afirmar éxito sin ejecución.** No inventar ramas, PR, pruebas, endpoints ni datos. Ante ambigüedad de diseño que afecte interfaz, tabla o alcance, solicitar revisión dentro del proyecto en vez de decidir silenciosamente.
