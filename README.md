# PersonAPI · Laboratorio 1

Aplicación monolítica ASP.NET Core MVC + servicios + DAO/Repository para gestionar **personas, profesiones, teléfonos y estudios**. El alcance de la demo es CRUD completo en vistas Razor, 20 endpoints REST documentados con Swagger/OpenAPI 3 y persistencia en SQL Server con enfoque Database First.

**Stack adoptado:** .NET 10 LTS, ASP.NET Core MVC 10, EF Core 10, SQL Server 2025 Express, SSMS 22, Visual Studio 2026 Community, REST/JSON, OpenAPI 3. Se modernizan las versiones de la guía original por decisión del equipo, manteniendo los requisitos funcionales y los patrones solicitados ([versiones](docs/wiki/04-implementacion/versiones.md)).

> Estado real: la solución arranca y compila (HU-001) y el esquema `persona_db` se crea con sus restricciones y datos de demostración desde `scripts/` (HU-002); las entidades EF Core, `PersonaDbContext` y los cuatro repositorios se generan y leen la base real (HU-003); las cinco operaciones de `/api/personas` (HU-004), las cinco de `/api/profesiones` (HU-005) y las cinco de `/api/estudios`, identificadas por el par `(idProf, ccPer)` (HU-011), responden contra la base con sus códigos 200, 201, 204, 400, 404 y 409. El recurso REST de teléfonos y todos los CRUD de Razor **están diseñados y aún no implementados**; este repositorio no afirma despliegues ejecutados.

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

```bash
dotnet build
dotnet test
dotnet run --project personapi-dotnet/personapi-dotnet.csproj
```

La aplicación responde en la URL que publique Kestrel: `/` para las vistas MVC y `/swagger` para la interfaz de la API. La cadena de conexión se lee de `ConnectionStrings:PersonaDb` y **no se versiona**: se define por User Secrets o variable de entorno.

```bash
dotnet user-secrets init --project personapi-dotnet/personapi-dotnet.csproj
dotnet user-secrets set "ConnectionStrings:PersonaDb" "<cadena-local>" --project personapi-dotnet/personapi-dotnet.csproj
# alternativa equivalente sin User Secrets
export ConnectionStrings__PersonaDb="<cadena-local>"
```

Detalle en [configuración y ejecución local](docs/wiki/04-implementacion/ejecucion-local.md) y el [runbook de la demo](docs/wiki/04-implementacion/runbook-demo.md). La creación del esquema está verificada en HU-002 y la conexión de la aplicación a `persona_db` por `ConnectionStrings:PersonaDb`, en HU-003; los recorridos CRUD siguen siendo una **guía prevista** hasta sus historias.

## Trabajo en equipo

Tres integrantes, dos sprints. Una issue por unidad de trabajo, con un responsable, una rama y un PR revisado por otra persona. El estado del trabajo se gestiona **solo** en GitHub Projects; la wiki no lo replica. El método para derivar el backlog está en [06-backlog](docs/wiki/06-backlog/README.md); las issues definitivas aún no están creadas.

## Entrega

Repositorio público `personapi-dotnet`, scripts DDL/DML, README comprobado desde un clon limpio, código fuente, informe académico y tag `v1.0.0`. Detalle en [entrega](docs/wiki/05-proceso/entrega.md).
