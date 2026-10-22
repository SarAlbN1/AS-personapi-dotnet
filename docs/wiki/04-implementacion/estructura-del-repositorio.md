# Estructura del repositorio

[← Implementación](README.md)

## Organización prevista

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
│   └── informe/               # entregable académico
├── scripts/
│   ├── 01_ddl.sql
│   └── 02_dml.sql
├── personapi-dotnet.sln
└── personapi-dotnet/
    ├── personapi-dotnet.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/
    │   ├── PersonasController.cs
    │   ├── ProfesionesController.cs
    │   ├── TelefonosController.cs
    │   ├── EstudiosController.cs
    │   └── Api/                # cuatro *ApiController
    ├── Services/
    │   ├── Interfaces/
    │   └── Implementations/
    ├── Models/
    │   ├── Entities/          # scaffold: entidades y DbContext
    │   └── Repository/
    │       ├── Interfaces/
    │       └── Implementations/
    ├── Views/                  # Razor: cuatro grupos CRUD y Shared
    └── wwwroot/
```

## Propiedad y límites

| Carpeta | Contenido permitido | No debe incluir |
|---|---|---|
| `Controllers/Api/` | HTTP REST, binding, status codes | Consultas EF Core |
| `Controllers/` y `Views/` | Orquestación MVC y presentación Razor | SQL o reglas duplicadas |
| `Services/` | Reglas y casos de uso, vía interfaces DAO | Dependencias de Razor |
| `Models/Repository/` | Contratos DAO e implementación EF Core | HTTP, HTML y vistas |
| `Models/Entities/` | Clases generadas Database First y contexto | Servicios o lógica de presentación |
| `scripts/` | Esquema SQL y datos de demo | Contraseñas |
| `docs/wiki/` | Requisitos, arquitectura y proceso | Estados de tareas duplicados de GitHub Projects |

**Fuente de verdad:** SRS (alcance), SAD (capas), SDD (contratos y clases), DDL (esquema), GitHub Projects (estado del trabajo). La estructura expresa una **separación lógica en un solo ejecutable**, no microservicios ni múltiples bases.
