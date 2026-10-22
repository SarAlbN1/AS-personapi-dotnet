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

1. Trabajar sobre una issue real de GitHub Projects, con criterios de aceptación y un único responsable.
2. Crear rama `<tipo>/LAB-NN-descripcion` desde `main` actualizado. No se escribe directamente en `main`.
3. Commits: `<tipo>(<área>): <acción breve> [LAB-NN]`.
4. Nadie edita ni sube cambios a la rama de otra persona ni corrige sus archivos sin acuerdo. Los defectos ajenos se reportan en un comentario o una issue.
5. PR con `Closes #<número real>`, pruebas, evidencia y enlaces a requisito y decisión arquitectónica.
6. Revisión cruzada: el revisor es otro integrante; el autor corrige su propio trabajo. Nadie integra su PR sin revisión.
7. Integración con squash merge; `main` siempre debe compilar.
8. Cualquier desviación del SRS, SAD o SDD se propone y se revisa **antes** de implementarla.

## Evidencias antes de declarar terminado

Un trabajo no está hecho hasta que el PR muestra: comandos ejecutados y su salida (`dotnet build` y pruebas), el caso concreto probado con resultado esperado frente a obtenido, verificación contra SQL Server real si toca persistencia, los estados HTTP comprobados si toca REST, y el recorrido en navegador si toca MVC. Las afirmaciones sin ejecución se describen como **pendientes**.

## Asistentes de desarrollo

Aplican las mismas reglas que a cualquier integrante, más las de [`CLAUDE.md`](CLAUDE.md) y [trabajo con asistentes de IA](docs/wiki/05-proceso/trabajo-con-ia.md): no introducen cambios fuera de la issue asignada, no inventan issues, ramas, pruebas, endpoints ni datos, y no declaran éxito sin ejecución. La responsabilidad sigue siendo del integrante asignado.

**Sin rastro de asistentes en el repositorio.** Los commits, mensajes de PR, documentos, informe y logs **no** llevan líneas de co-autoría, firmas, menciones de la herramienta ni marcas del tipo «generado con». El autor registrado de cada cambio es el integrante asignado, que lo verifica y responde por él. Si el curso exige declarar el uso de asistentes, se hace por el canal que indique el docente, no mediante metadatos en el repositorio.

No incluir credenciales ni datos sensibles. No duplicar en Markdown el estado de las issues: vive en GitHub Projects.
