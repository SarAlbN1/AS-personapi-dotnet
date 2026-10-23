# Cómo contribuir · PersonAPI

PersonAPI es un **monolito ASP.NET Core MVC + servicios + DAO + EF Core + SQL Server**. La [wiki](docs/wiki/Home.md) contiene requisitos, arquitectura y contratos. El [proceso completo](docs/wiki/05-proceso/README.md) y el [DoR/DoD](docs/wiki/05-proceso/dor-y-dod.md) son obligatorios.

## Qué leer antes de tocar algo

| Tarea | Lectura mínima |
|---|---|
| Cualquier issue | [SRS](docs/wiki/01-producto/01-SRS.md), [reglas arquitectónicas](docs/wiki/02-arquitectura/reglas-arquitectonicas.md) y la issue |
| Endpoint REST | [Contrato REST](docs/wiki/03-contratos/api-rest.md) y [SDD](docs/wiki/04-implementacion/03-SDD-LDD.md) |
| Tabla, EF Core o DAO | [Persistencia](docs/wiki/03-contratos/persistencia.md) y SDD |
| Vista Razor | SDD y [validaciones y errores](docs/wiki/03-contratos/validaciones-y-errores.md) |
| Estructura o versiones | [Estructura del repositorio](docs/wiki/04-implementacion/estructura-del-repositorio.md) y [versiones](docs/wiki/04-implementacion/versiones.md) |
| Pruebas y PR | [Pruebas](docs/wiki/04-implementacion/pruebas.md) y [proceso](docs/wiki/05-proceso/README.md) |

## Reglas arquitectónicas que todo cambio debe cumplir

1. Un solo proyecto desplegable y una sola base `persona_db`.
2. MVC y REST comparten los mismos `I*Service`; ningún controlador accede a `PersonaDbContext` ni escribe SQL.
3. Los servicios dependen de interfaces `I*Repository`; EF Core queda dentro de las implementaciones DAO.
4. Database First: el DDL versionado es el origen del esquema, incluida la PK compuesta de `estudios`.
5. Sin borrado en cascada: persona o profesión con dependencias responde `409` en REST y mensaje legible en MVC.
6. DTO y ViewModels en los límites HTTP; no se publican entidades EF con navegaciones.
7. El stack es el de [versiones](docs/wiki/04-implementacion/versiones.md): ASP.NET Core MVC 10, EF Core 10, SQL Server 2025 Express y Swagger/OpenAPI 3. Incorporar otra tecnología exige cambio aprobado del SRS y del SAD. Sin secretos versionados.

Las reglas completas y la lista de revisión de PR están en [reglas arquitectónicas](docs/wiki/02-arquitectura/reglas-arquitectonicas.md).

## Flujo de trabajo (tres integrantes)

`main` está **protegida en GitHub**: exige pull request, una aprobación de otra persona, resolución de conversaciones e historia lineal, sin excepciones para administradores y sin force-push ni eliminación. La política completa y su estado verificado están en [pull requests, revisión y versiones](docs/wiki/05-proceso/pull-requests-y-releases.md#protección-de-main-y-revisión-cruzada-obligatoria); aquí no se repite.

1. Trabajar sobre una issue real de GitHub Projects, con criterios de aceptación y un único responsable.
2. Crear rama `<tipo>/LAB-NN-descripcion` desde `main` actualizado. **No se escribe directamente en `main`.**
3. Commits: `<tipo>(<área>): <acción breve> [LAB-NN]`.
4. Nadie edita ni sube cambios a la rama de otra persona ni corrige sus archivos sin acuerdo. Los defectos ajenos se reportan en un comentario o una issue.
5. PR con `Closes #<número real>`, pruebas, evidencia y enlaces a requisito y decisión arquitectónica.
6. **Revisión cruzada obligatoria:** el revisor es otro integrante con permiso de escritura; el autor no aprueba ni integra su propio PR, ni usa permisos de administrador para omitir la revisión. Si llegan commits nuevos que invalidan la aprobación, se pide otra.
7. Antes de integrar: conversaciones resueltas y aprobación válida. Que GitHub muestre el PR como fusionable **no** equivale a estar aprobado.
8. Integración con squash merge; `main` siempre debe compilar.
9. Cualquier desviación del SRS, SAD o SDD se propone y se revisa **antes** de implementarla.

El equipo son tres integrantes, registrados con sus cuentas reales en [`equipo.tsv`](docs/wiki/06-backlog/equipo.tsv): Sara (`SarAlbN1`), Juan Diego (`Juandi48`) y Miguel (`ma-sarmiento`).

## Evidencias antes de declarar terminado

Un trabajo no está hecho hasta que el PR muestra: comandos ejecutados y su salida (`dotnet build` y pruebas), el caso concreto probado con resultado esperado frente a obtenido, verificación contra SQL Server real si toca persistencia, los estados HTTP comprobados si toca REST, y el recorrido en navegador si toca MVC. Las afirmaciones sin ejecución se describen como **pendientes**.

## Asistentes de desarrollo

Aplican las mismas reglas que a cualquier integrante, más las de [`CLAUDE.md`](CLAUDE.md) y [trabajo con asistentes de IA](docs/wiki/05-proceso/trabajo-con-ia.md): no introducen cambios fuera de la issue asignada, no inventan issues, ramas, pruebas, endpoints ni datos, y no declaran éxito sin ejecución. La responsabilidad sigue siendo del integrante asignado.

**Sin rastro de asistentes en el repositorio.** Los commits, mensajes de PR, documentos, informe y logs **no** llevan líneas de co-autoría, firmas, menciones de la herramienta ni marcas del tipo «generado con». El autor registrado de cada cambio es el integrante asignado, que lo verifica y responde por él. Si el curso exige declarar el uso de asistentes, se hace por el canal que indique el docente, no mediante metadatos en el repositorio.

No incluir credenciales ni datos sensibles. No duplicar en Markdown el estado de las issues: vive en GitHub Projects.
