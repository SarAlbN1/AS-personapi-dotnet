# PersonAPI · Laboratorio 1

Aplicación monolítica ASP.NET Core MVC + servicios + DAO/Repository para gestionar **personas, profesiones, teléfonos y estudios**. El alcance de la demo es CRUD completo en vistas Razor, 20 endpoints REST documentados con Swagger/OpenAPI 3 y persistencia en SQL Server con enfoque Database First.

**Stack adoptado:** .NET 10 LTS, ASP.NET Core MVC 10, EF Core 10, SQL Server 2025 Express, SSMS 22, Visual Studio 2026 Community, REST/JSON, OpenAPI 3. Se modernizan las versiones de la guía original por decisión del equipo, manteniendo los requisitos funcionales y los patrones solicitados ([versiones](docs/wiki/04-implementacion/versiones.md)).

> Este repositorio contiene, por ahora, **documentación de diseño**: no afirma implementación, pruebas ni despliegues ejecutados.

## Arquitectura

```text
Clientes
   ├── Navegador web / Vistas Razor ──┐
   └── Swagger / Clientes REST ───────┤
                                      v
                        Controladores MVC / API
                                      v
                          Servicios de negocio
                                      v
                        Interfaces DAO / Repository
                                      v
                          Implementaciones DAO
                                      v
                            Entity Framework Core
                                      v
                        SQL Server · persona_db
```

Un único artefacto desplegable. MVC y REST son dos interfaces HTTP del mismo proceso y comparten servicios; ningún controlador accede al `DbContext`. Las capas se comunican por invocación directa, sin salto de red entre ellas.

## Documentación

- [Índice de la wiki](docs/wiki/Home.md)
- [SRS](docs/wiki/01-producto/01-SRS.md) · Requisitos, drivers y architecture killers.
- [SAD/HLD](docs/wiki/02-arquitectura/02-SAD-HLD.md) · Arquitectura y decisiones.
- [SDD/LDD](docs/wiki/04-implementacion/03-SDD-LDD.md) · Diseño de implementación.
- [Contratos REST, persistencia y errores](docs/wiki/03-contratos/README.md)
- [Reglas arquitectónicas](docs/wiki/02-arquitectura/reglas-arquitectonicas.md)
- [Proceso de colaboración](docs/wiki/05-proceso/README.md) y [CONTRIBUTING](CONTRIBUTING.md)
- [Evidencias de pruebas](docs/wiki/04-implementacion/pruebas/README.md) · [Informe académico](docs/informe/README.md) · [Sustentación](docs/sustentacion/README.md)

### Qué leer según la tarea

| Tarea | Lectura mínima |
|---|---|
| Cualquier issue | SRS, reglas arquitectónicas y la issue |
| Endpoint REST | [api-rest.md](docs/wiki/03-contratos/api-rest.md) y SDD |
| Tabla, EF Core o DAO | [persistencia.md](docs/wiki/03-contratos/persistencia.md) y SDD |
| Vista Razor | SDD y [validaciones-y-errores.md](docs/wiki/03-contratos/validaciones-y-errores.md) |
| Pruebas y PR | [pruebas.md](docs/wiki/04-implementacion/pruebas.md) y [05-proceso/](docs/wiki/05-proceso/README.md) |

## Ejecución

Consultar [configuración y ejecución local](docs/wiki/04-implementacion/ejecucion-local.md) y el [runbook de la demo](docs/wiki/04-implementacion/runbook-demo.md). Los pasos son una **guía prevista** y se actualizarán con los comandos y la evidencia reales al implementar.

## Trabajo en equipo

Tres integrantes, dos sprints. Una issue por unidad de trabajo, con un responsable, una rama y un PR revisado por otra persona. El estado del trabajo se gestiona **solo** en GitHub Projects; la wiki no lo replica. El método para derivar el backlog está en [06-backlog](docs/wiki/06-backlog/README.md); las issues definitivas aún no están creadas.

## Entrega

Repositorio público `personapi-dotnet`, scripts DDL/DML, README comprobado desde un clon limpio, código fuente, informe académico y tag `v1.0.0`. Detalle en [entrega](docs/wiki/05-proceso/entrega.md).
