# Diagramas de arquitectura

[← Arquitectura](../README.md) · [SAD](../02-SAD-HLD.md) · [SDD](../../04-implementacion/03-SDD-LDD.md)

Fuentes **en texto** de los diagramas C4 (HU-016). La fuente es el `.dsl`: las imágenes
son material derivado y se regeneran a partir de él.

| Archivo | Contenido |
|---|---|
| [`personapi-c4.dsl`](personapi-c4.dsl) | Modelo C4 completo en Structurizr DSL: contexto, contenedores, componentes y despliegue |

## Qué dibuja cada vista

| Clave de la vista | Nivel | Qué muestra |
|---|---|---|
| `c4-c1-contexto` | C4 1 · Contexto | El usuario, el monolito PersonAPI y SQL Server `persona_db` como único almacén |
| `c4-c2-contenedores` | C4 2 · Contenedores | La aplicación ASP.NET Core MVC 10 como **único artefacto desplegable**, sus dos canales (vistas Razor y API REST) y la base `persona_db` |
| `c4-c3-componentes` | C4 3 · Componentes | Dentro del contenedor web: controladores MVC y API, vistas y ViewModels, DTOs, Swagger, servicios, interfaces DAO, repositorios EF Core, `PersonaDbContext` y entidades |
| `c4-despliegue` | Despliegue | Los tres servicios de `docker-compose.yml` (`personapi`, `sqlserver`, `db-init`) en la red `personapi-net` |

## Cómo renderizar

**Playground (sin instalar nada).** Abrir <https://structurizr.com/dsl>, pegar el
contenido de `personapi-c4.dsl` y elegir la vista en el selector. Exportar cada diagrama
como PNG.

**CLI (reproducible).** Con la [CLI de Structurizr](https://docs.structurizr.com/cli):

```bash
structurizr-cli export -workspace docs/wiki/02-arquitectura/diagramas/personapi-c4.dsl \
  -format png -output docs/informe/diagramas
```

### Nombres de archivo que espera el informe

`docs/informe/main.tex`, en la sección «Modelo C4», referencia exactamente estos tres
archivos; las imágenes exportadas se renombran a estos nombres si el exportador añade
prefijos:

```text
docs/informe/diagramas/c4-c1-contexto.png
docs/informe/diagramas/c4-c2-contenedores.png
docs/informe/diagramas/c4-c3-componentes.png
```

Mientras un archivo no exista, el informe pinta el marco de «pendiente» en su lugar y
compila igual, así que la ausencia se ve en el PDF en vez de pasar inadvertida.

## Lo que el modelo garantiza

- **Un solo contenedor desplegable y una sola base** (AD-01, regla 1).
- **MVC y REST comparten servicios:** los dos controladores apuntan al mismo componente
  de servicios, así que el diagrama no permite afirmar lógica duplicada (AD-02, regla 2).
- **Los servicios dependen de interfaces DAO,** no de implementaciones (AD-03, AD-04).
- **Ningún controlador tiene una flecha hacia `PersonaDbContext`:** el único camino a los
  datos pasa por los repositorios EF Core (AK-01, regla 3). Si alguien añadiera esa
  dependencia en el código, el diagrama deja de corresponder y la diferencia se reporta.
- **Nada fuera de `versiones.md`:** .NET 10, ASP.NET Core MVC 10, EF Core 10, SQL Server
  2025 Express, Swagger/OpenAPI 3 y, para el despliegue, las imágenes que ya declara
  `docker-compose.yml`.

## Correspondencia con el código

Comprobada contra `main` en `7d144fb` (28-10-2026). El modelo dibuja las **capas**, que
son estables; el estado por módulo se escribe en la descripción de cada componente, para
que el diagrama no afirme lo que todavía no existe.

| Componente del diagrama | Código real |
|---|---|
| Controladores MVC | `Back/01-Borde/Controllers/`: `HomeController`, `AntiforgeryEnVistasConvention`, `ResultadoServicioVistaExtensions`. Los CRUD de módulo llegan con HU-007, HU-008, HU-012 y HU-013 |
| Vistas Razor y ViewModels | `Front/Views/` (layout, `Home`, `Shared`) y `Front/ViewModels/` |
| Controladores API REST | `PersonasApiController`, `ProfesionesApiController`; teléfonos y estudios llegan con HU-010 y HU-011 |
| DTOs | `Back/01-Borde/Dtos/`: `PersonaDto`, `PersonaEntradaDto`, `ProfesionDto` |
| Swagger / OpenAPI 3 | Configuración de Swashbuckle en `Program.cs`; la documentación completa es HU-009 |
| Servicios de dominio | `IPersonaService`/`PersonaService`, `IProfesionService`/`ProfesionService` y `ServiceResult` |
| Interfaces DAO | las cuatro de `Back/03-AccesoDatos/Repository/Interfaces/` |
| Repositorios EF Core | las cuatro de `Back/03-AccesoDatos/Repository/Implementations/`, único lugar que referencia el contexto |
| `PersonaDbContext` y entidades | `Back/04-Persistencia/Entities/` |
| `persona_db` | `scripts/01_ddl.sql` y `scripts/02_dml.sql` |
| `personapi`, `sqlserver`, `db-init` | servicios de `docker-compose.yml`, red `personapi-net`, puertos 8080 y 1433 |

## Diferencias encontradas al dibujar

Se reportan, no se corrigen desde aquí: el documento propietario lo corrige quien lo
mantiene ([jerarquía de CLAUDE.md](../../../../CLAUDE.md)).

1. **`docs/informe/main.tex`, «Estructura del proyecto»** lista como existentes
   `ProfesionesController.cs`, `TelefonosController.cs`, `EstudiosController.cs`,
   `TelefonosApiController.cs`, `EstudiosApiController.cs` y las carpetas de vistas de los
   cuatro módulos. En `main` todavía no existen: son el árbol de destino, no el actual. El
   diagrama no los dibuja como implementados.
2. **Alcance de Docker.** El informe presenta el despliegue con Docker Compose y los
   archivos (`Dockerfile`, `docker-compose.yml`, `.env.example`) están versionados y
   coinciden con la figura; el [README del backlog](../../06-backlog/README.md) mantiene
   que ninguna historia lo toma como criterio de aceptación. La vista de despliegue se
   incluye porque el informe la declara, y describe lo que dicen esos archivos, sin
   afirmar que se haya ejecutado en una entrega.
