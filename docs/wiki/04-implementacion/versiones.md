# Versiones y compatibilidad

[← Implementación](README.md)

**Línea base aprobada para PersonAPI (21 de octubre de 2026).** Se prioriza la versión estable vigente, sin `preview`. Las versiones **mayores** se fijan aquí; la versión exacta de cada paquete se fija y registra en el proyecto al instalarlo.

| Componente | Línea adoptada | Política de implementación |
|---|---|---|
| SDK y runtime | **.NET 10 LTS** | `TargetFramework` = `net10.0` |
| Framework | **ASP.NET Core MVC 10** | Controladores MVC y Razor, también API Controllers |
| ORM | **Entity Framework Core 10** | Paquetes Core, SqlServer, Tools, Design con versiones 10.x compatibles |
| Motor de BD | **SQL Server 2025 Express** | Instancia local Express; `persona_db` |
| IDE | **Visual Studio 2026 Community** | Workload ASP.NET y web; herramientas de datos |
| Administración SQL | **SSMS 22** | Conexión al motor SQL Server |
| Contrato HTTP | REST / JSON | API en `/api/*`, vistas Razor en rutas MVC |
| Especificación de API | OpenAPI 3 + Swagger UI | No confundir Swagger UI con la versión de OpenAPI |
| SCM | Git + GitHub | Repo público `personapi-dotnet`, entrega por tag `v1.0.0` |

**Compatibilidad:** EF Core 10 requiere una línea .NET compatible; paquetes NuGet de EF Core 10 deben estar alineados en versión mayor y restaurar sin conflictos. El .NET SDK y el IDE deben soportar `net10.0`. Registrar versiones exactas del SDK (`dotnet --info`), herramientas y paquetes usados en el informe al realizar la implementación.

**Restricciones:** no instalar .NET 6/7 ni SQL 2019/2022 solo por aparecer en el texto histórico del taller; la decisión adoptada por el equipo es actualizar herramientas manteniendo el alcance docente. No introducir autenticación ni infraestructura adicional como requisito obligatorio. No usar contraseñas reales en ejemplos y no versionar secretos.

**Docker y Docker Compose** forman parte de la propuesta de despliegue documentada en el [informe](../../informe/main.tex), con las imágenes `mcr.microsoft.com/dotnet/sdk:10.0`, `mcr.microsoft.com/dotnet/aspnet:10.0` y `mcr.microsoft.com/mssql/server:2025-latest`. No son requisito para cerrar una historia: el criterio de aceptación se verifica con ejecución local. Su exigibilidad como entregable depende del enunciado original del laboratorio, no de esta página.

**Fuente:** [SRS](../01-producto/01-SRS.md), [SAD](../02-arquitectura/02-SAD-HLD.md) y [SDD](03-SDD-LDD.md). Ante una actualización técnica posterior debe existir PR, justificación, verificación de compatibilidad y actualización coordinada de esta tabla.
