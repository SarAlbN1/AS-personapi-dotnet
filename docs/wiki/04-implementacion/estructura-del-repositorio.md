# Estructura del repositorio

[← Implementación](README.md)

El proyecto es **un solo `.csproj`**. El árbol separa **`Front/`** de **`Back/`**, y
dentro de `Back/` las carpetas están numeradas por capa: el número fija la posición en
el flujo y la dirección permitida de las llamadas. Una capa solo llama a la de número
inmediatamente superior; **saltar capas está prohibido**.

`Front/` y `Back/` son **carpetas, no despliegues**: un solo artefacto y una sola base
`persona_db`. No hay microservicios ni SPA.

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
    ├── Front/                        # FRONT · lo que ve el navegador
    │   ├── Views/                    #   Razor .cshtml
    │   │   ├── Shared/
    │   │   ├── Personas/
    │   │   ├── Profesiones/
    │   │   ├── Telefonos/
    │   │   └── Estudios/
    │   ├── wwwroot/                  #   CSS, JS, imágenes
    │   └── ViewModels/               #   contrato de Razor
    └── Back/                         # BACK · lógica, cuatro capas
        ├── 01-Borde/                 # CAPA 1 · único sitio que habla HTTP
        │   ├── Controllers/          #   MVC: orquesta y entrega ViewModel
        │   ├── ControllersApi/       #   REST /api/*: JSON y códigos de estado
        │   └── Dtos/                 #   contrato de REST
        ├── 02-Aplicacion/            # CAPA 2 · reglas de negocio
        │   └── Services/
        │       ├── Interfaces/
        │       └── Implementations/
        ├── 03-AccesoDatos/           # CAPA 3 · DAO, único sitio con EF Core
        │   └── Repository/
        │       ├── Interfaces/
        │       └── Implementations/
        └── 04-Persistencia/          # CAPA 4 · Database First
            └── Entities/             #   entidades + PersonaDbContext
```

## Front y back

| | Carpetas | Patrón |
|---|---|---|
| **Front** | `Front/Views/`, `Front/wwwroot/`, `Front/ViewModels/` | MVC server-side con Razor |
| **Back** | `Back/01-Borde/`, `Back/02-Aplicacion/`, `Back/03-AccesoDatos/`, `Back/04-Persistencia/` | Capas + DAO/Repository |

Las cuatro capas de `Back/` son back, **incluida `01-Borde/`**: los controladores no
contienen HTML. Reciben la petición, validan la entrada, llaman a un servicio y
entregan un **ViewModel** a `Front/Views/`, que es quien produce el HTML. Se llama
*borde* porque es la frontera donde el sistema habla HTTP.

`Dtos/` está en el back porque es el contrato de la API REST, consumido por Swagger y
clientes externos. `ViewModels/` está en el front porque solo lo consume Razor.

## Flujo y dirección de dependencias

```text
Navegador       →  Back/01-Borde/Controllers/      ┐
                                                   ├→  02-Aplicacion → 03-AccesoDatos → 04-Persistencia → SQL Server
Swagger / REST  →  Back/01-Borde/ControllersApi/   ┘

Back/01-Borde/Controllers/  ──ViewModel en memoria──→  Front/Views/  →  HTML  →  navegador
```

No hay llamadas de red entre capas: son invocaciones de código en el mismo proceso.
Las vistas Razor **no** consumen `/api/*` por HTTP. `/api/*` existe para Swagger y
clientes REST. Son dos canales paralelos que bajan a los mismos servicios, de modo que
una regla de negocio se escribe una sola vez.

## Propiedad y límites

| Carpeta | Contenido permitido | No debe incluir |
|---|---|---|
| `Front/Views/` y `Front/wwwroot/` | Razor y estáticos | SQL, EF Core, reglas de negocio, llamadas a `/api/*` |
| `Front/ViewModels/` | Contrato que consume Razor | Entidades EF con navegaciones |
| `Back/01-Borde/ControllersApi/` | HTTP REST, binding, status codes | Consultas EF Core |
| `Back/01-Borde/Controllers/` | Orquestación MVC | HTML, SQL, reglas duplicadas |
| `Back/01-Borde/Dtos/` | Contrato de REST | Entidades EF con navegaciones |
| `Back/02-Aplicacion/Services/` | Reglas y casos de uso, vía interfaces DAO | EF Core, Razor, `HttpContext` |
| `Back/03-AccesoDatos/Repository/` | Contratos DAO e implementación EF Core | HTTP, HTML, reglas de negocio |
| `Back/04-Persistencia/Entities/` | Clases generadas Database First y contexto | Servicios o lógica de presentación |
| `scripts/` | Esquema SQL y datos de demo | Contraseñas |
| `tests/` | Pruebas unitarias y de integración | — |
| `docs/wiki/` | Requisitos, arquitectura y proceso | Estados de tareas duplicados de GitHub Projects |

Cada carpeta lleva su propio `README.md` con sus reglas y prohibiciones.

## Arquitectura: qué es y qué no es

Monolito **en capas con MVC + DAO**, decisión AD-01 del
[SAD](../02-arquitectura/02-SAD-HLD.md). **No** es onion, **no** es hexagonal y **no**
es clean architecture: en esas el dominio ocupa el centro con puertos y adaptadores y
no conoce el ORM, mientras aquí el flujo es lineal de `01` a `04` y las entidades se
generan por scaffolding EF Core. La separación se logra con **interfaces**, no con
inversión de dependencias del dominio. El árbol expresa una **separación lógica en un
solo ejecutable**.

## Consecuencias técnicas del árbol

`Views/` y `wwwroot/` dejan de estar en las rutas que ASP.NET Core busca por defecto.
Sin configuración explícita, **las vistas fallan con 500 y los estáticos con 404**.
Obligatorio en `Program.cs`:

- `WebApplicationOptions.WebRootPath = "Front/wwwroot"`.
- `RazorViewEngineOptions.ViewLocationFormats` apuntando a
  `/Front/Views/{1}/{0}.cshtml` y `/Front/Views/Shared/{0}.cshtml`.
- Scaffolding EF Core con `--output-dir Back/04-Persistencia/Entities`.

Detalle y fragmentos en [`personapi-dotnet/README.md`](../../../personapi-dotnet/README.md).
**Sin verificar:** no existen `Program.cs` ni `.csproj`; la comprobación corresponde a
la issue de arranque del proyecto.

## Discrepancia documental abierta

El [SDD](03-SDD-LDD.md) fijaba la estructura plana `Controllers/`, `Controllers/Api/`,
`Services/`, `Models/{Entities,Repository,Dtos,ViewModels}/`, `Views/` y `wwwroot/`.
Esta página adopta la separación `Front/` + `Back/` numerado, pedida por el equipo para
que la frontera entre presentación y lógica sea legible en el repositorio. **El cambio
no altera ninguna decisión arquitectónica:** mismas capas, mismas dependencias, mismo
artefacto único, mismas cuatro entidades y los mismos contratos HTTP.

El SDD es la fuente propietaria de la estructura. La actualización de su árbol se
propone en el mismo PR y **requiere revisión cruzada del equipo** antes de fusionar. Si
el equipo rechaza el árbol, se revierte aquí y en el SDD a la vez.

**Fuente de verdad:** SRS (alcance), SAD (capas), SDD (contratos y clases), DDL
(esquema), GitHub Projects (estado del trabajo).
