# Estructura del repositorio

[← Implementación](README.md)

El proyecto es **un solo `.csproj`**. Las carpetas de primer nivel del proyecto están
numeradas por **capa**: el número fija la posición en el flujo y la dirección
permitida de las llamadas. Una capa solo llama a la de número inmediatamente
superior; **saltar capas está prohibido**.

## Organización

```text
personapi-dotnet/
├── README.md
├── CONTRIBUTING.md
├── CLAUDE.md
├── .github/
│   ├── ISSUE_TEMPLATE/
│   └── pull_request_template.md
├── docs/
│   ├── wiki/
│   │   ├── Home.md
│   │   ├── 01-producto/
│   │   ├── 02-arquitectura/
│   │   ├── 03-contratos/
│   │   ├── 04-implementacion/
│   │   ├── 05-proceso/
│   │   └── 06-backlog/
│   └── informe/                      # entregable académico
├── scripts/
│   ├── 01_ddl.sql
│   └── 02_dml.sql
├── tests/
│   ├── Unit/
│   └── Integration/
├── personapi-dotnet.sln
└── personapi-dotnet/
    ├── personapi-dotnet.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── README.md                     # mapa de capas y diagrama
    ├── 01-Presentacion/              # CAPA 1 · borde HTTP + FRONT
    │   ├── Controllers/              #   MVC: vistas y redirects
    │   ├── ControllersApi/           #   REST /api/*: JSON y códigos HTTP
    │   ├── Views/                    #   FRONT · Razor
    │   │   ├── Shared/
    │   │   ├── Personas/
    │   │   ├── Profesiones/
    │   │   ├── Telefonos/
    │   │   └── Estudios/
    │   ├── wwwroot/                  #   FRONT · CSS, JS, imágenes
    │   ├── ViewModels/               #   contrato de Razor
    │   └── Dtos/                     #   contrato de REST
    ├── 02-Aplicacion/                # CAPA 2 · reglas de negocio
    │   └── Services/
    │       ├── Interfaces/
    │       └── Implementations/
    ├── 03-AccesoDatos/               # CAPA 3 · DAO, único sitio con EF Core
    │   └── Repository/
    │       ├── Interfaces/
    │       └── Implementations/
    └── 04-Persistencia/              # CAPA 4 · Database First
        └── Entities/                 #   entidades + PersonaDbContext
```

## Flujo y dirección de dependencias

```text
Navegador        →  01-Presentacion/Controllers/      ┐
                                                      ├→  02-Aplicacion  →  03-AccesoDatos  →  04-Persistencia  →  SQL Server
Swagger / REST   →  01-Presentacion/ControllersApi/   ┘
```

No hay llamadas de red entre capas: son invocaciones de código en el mismo proceso.
Las vistas Razor **no** consumen `/api/*` por HTTP; reciben un ViewModel en memoria.

## Front y back

El front **no** es una aplicación aparte: es la capa `01`, renderizada en el servidor.

| | Carpetas | Patrón |
|---|---|---|
| **Front** | `01-Presentacion/Views/`, `01-Presentacion/wwwroot/` | MVC server-side con Razor |
| **Back** | `01-Presentacion/ControllersApi/`, capas `02`, `03`, `04` | Capas + DAO/Repository |

## Propiedad y límites

| Carpeta | Contenido permitido | No debe incluir |
|---|---|---|
| `01-Presentacion/ControllersApi/` | HTTP REST, binding, status codes | Consultas EF Core |
| `01-Presentacion/Controllers/` y `Views/` | Orquestación MVC y presentación Razor | SQL o reglas duplicadas |
| `01-Presentacion/Dtos/` y `ViewModels/` | Contratos de salida | Entidades EF con navegaciones |
| `02-Aplicacion/Services/` | Reglas y casos de uso, vía interfaces DAO | EF Core, Razor, `HttpContext` |
| `03-AccesoDatos/Repository/` | Contratos DAO e implementación EF Core | HTTP, HTML, reglas de negocio |
| `04-Persistencia/Entities/` | Clases generadas Database First y contexto | Servicios o lógica de presentación |
| `scripts/` | Esquema SQL y datos de demo | Contraseñas |
| `tests/` | Pruebas unitarias y de integración | — |
| `docs/wiki/` | Requisitos, arquitectura y proceso | Estados de tareas duplicados de GitHub Projects |

Cada carpeta de capa lleva su propio `README.md` con sus reglas y prohibiciones.

## Arquitectura: qué es y qué no es

Monolito **en capas con MVC + DAO**, decisión AD-01 del
[SAD](../02-arquitectura/02-SAD-HLD.md). **No** es onion, **no** es hexagonal y
**no** es clean architecture: en esas el dominio ocupa el centro con puertos y
adaptadores y no conoce el ORM, mientras aquí el flujo es lineal de `01` a `04` y las
entidades se generan por scaffolding EF Core. La separación se logra con
**interfaces**, no con inversión de dependencias del dominio. La numeración expresa
una **separación lógica en un solo ejecutable**, no microservicios ni varias bases.

## Consecuencias técnicas de la numeración

`Views/` y `wwwroot/` dejan de estar en las rutas que ASP.NET Core busca por defecto.
Sin configuración explícita, **las vistas fallan con 500 y los estáticos con 404**.
Obligatorio en `Program.cs`:

- `WebApplicationOptions.WebRootPath = "01-Presentacion/wwwroot"`.
- `RazorViewEngineOptions.ViewLocationFormats` apuntando a
  `/01-Presentacion/Views/{1}/{0}.cshtml` y `/01-Presentacion/Views/Shared/{0}.cshtml`.
- Scaffolding EF Core con `--output-dir 04-Persistencia/Entities`.

Detalle y fragmentos en [`personapi-dotnet/README.md`](../../../personapi-dotnet/README.md).
**Sin verificar:** no existen `Program.cs` ni `.csproj`; la comprobación corresponde a
la issue de arranque del proyecto.

## Discrepancia documental abierta

El [SDD](03-SDD-LDD.md) fijaba la estructura plana `Controllers/`, `Controllers/Api/`,
`Services/`, `Models/{Entities,Repository,Dtos,ViewModels}/`, `Views/` y `wwwroot/`.
Esta página adopta la numeración por capas pedida por el equipo para hacer legible la
separación en el repositorio. **El cambio no altera ninguna decisión arquitectónica**:
mismas capas, mismas dependencias, mismo artefacto único, mismas cuatro entidades.

El SDD es la fuente propietaria de la estructura. La actualización de su árbol se
propone en el mismo PR y **requiere revisión cruzada del equipo** antes de fusionar.
Si el equipo rechaza la numeración, se revierte aquí y en el SDD a la vez.

**Fuente de verdad:** SRS (alcance), SAD (capas), SDD (contratos y clases), DDL
(esquema), GitHub Projects (estado del trabajo).
