# Wiki | PersonAPI · Laboratorio 1

**Objetivo:** desarrollar y demostrar el monolito `personapi-dotnet` con MVC, DAO/Repository, servicios, EF Core Database First, SQL Server, vistas Razor y API REST documentada con Swagger. Esta wiki describe el diseño acordado, no certifica implementación.

## Navegación

| Sección | Consulta principal |
|---|---|
| [01 · Producto](01-producto/README.md) | SRS, alcance y criterios de éxito |
| [02 · Arquitectura](02-arquitectura/README.md) | SAD/HLD, reglas, diagramas, decisiones y calidad |
| [03 · Contratos](03-contratos/README.md) | Endpoints REST, modelo relacional y errores |
| [04 · Implementación](04-implementacion/README.md) | SDD/LDD, estructura de código, versiones y ejecución |
| [05 · Proceso](05-proceso/README.md) | Ramas, revisión, criterios de cierre y release |
| [06 · Backlog](06-backlog/README.md) | Método para derivar issues del diseño; estado vivo en GitHub Projects |

## Jerarquía documental

1. **SRS** (`01-producto/01-SRS.md`) fija el alcance funcional y los atributos esperados.
2. **SAD/HLD** (`02-arquitectura/02-SAD-HLD.md`) fija las decisiones arquitectónicas.
3. **SDD/LDD** (`04-implementacion/03-SDD-LDD.md`) fija contratos internos y diseño detallado.
4. **Contratos** (`03-contratos/`) especifican el comportamiento verificable sin contradecir SRS/SAD/SDD.
5. Resto de páginas: explicaciones, pautas de ejecución y gestión.

Si aparecen contradicciones, **no resolverlas silenciosamente**: documentarlas en la issue/PR, corregir el documento dueño de la decisión mediante revisión y actualizar las páginas derivadas. No se duplican estados de GitHub Projects en archivos de la wiki.

## Límites

Es un **único monolito desplegable** sobre una sola base SQL Server: dos interfaces HTTP (vistas Razor y API REST) que comparten servicios, DAO y EF Core. El stack adoptado está cerrado en [versiones](04-implementacion/versiones.md) y ampliarlo exige cambio aprobado del SRS y del SAD. Los archivos describen lo **diseñado**; usar evidencia técnica para afirmar lo implementado o verificado.

## Guías complementarias

- [Contratos REST](03-contratos/api-rest.md) y [persistencia](03-contratos/persistencia.md)
- [Convenciones](04-implementacion/convenciones.md), [demo](04-implementacion/runbook-demo.md) y [versiones](04-implementacion/versiones.md)
- [Proceso de colaboración](05-proceso/README.md)
