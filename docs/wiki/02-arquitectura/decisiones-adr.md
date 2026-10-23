# Registro de decisiones arquitectónicas

[← Arquitectura](README.md)

Estas decisiones están **aprobadas a nivel de diseño**; no se deben presentar como capacidades implementadas.

| Ref. SAD | Decisión | Consecuencia / límite |
|---|---|---|
| AD-01 | Monolito ASP.NET Core MVC .NET 10 LTS | Un despliegue; sin servicios remotos internos |
| AD-02 | MVC Razor y API REST como entradas diferenciadas | Evita mezclar HTML con contratos JSON |
| AD-03 | Servicio por entidad con interfaz | Regla de negocio reutilizada en ambos canales |
| AD-04 | DAO/Repository sobre EF Core 10 | `DbContext` se encapsula detrás de interfaces |
| AD-05 | SQL Server 2025 Express y Database First | DDL precede al scaffolding |
| AD-06 | FK y eliminación `NO ACTION` | La aplicación comunica conflicto ante dependencias |
| AD-07 | Swagger UI y OpenAPI 3 | Documentación y ensayo de veinte operaciones |
| AD-08 | Ejecución local como camino verificado, sin autenticación; Docker como propuesta de despliegue del equipo (informe), sujeta al enunciado original | No es un diseño de producción |
| AD-09 | Configuración sin secretos versionados | Variable/secretos locales para conexión |
| AD-10 | DTO y ViewModels separados de EF | Más código, menor acoplamiento y ciclos JSON |

**Regla de cambio:** si una issue requiere contrariar una decisión, se propone ajuste al SAD y se revisa en PR antes de la implementación. Evitar generar diez archivos ADR independientes cuando la tabla del SAD es suficiente para esta escala.
