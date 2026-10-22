# Informe académico · PersonAPI

Documento académico que acompaña la entrega del Laboratorio 1 de `personapi-dotnet`: monolito ASP.NET Core MVC con patrones MVC y DAO sobre SQL Server.

| Ruta | Contenido |
|---|---|
| `main.tex` | Fuente LaTeX del informe |
| `diagramas/` | Imágenes exportadas de los diagramas del SAD y del SDD cuando se necesiten fuera de la wiki |
| `figuras/` | Capturas y gráficas propias del informe |

## Jerarquía documental

El informe **deriva** de la wiki; no decide por su cuenta. El orden de autoridad es el de [`CLAUDE.md`](../../CLAUDE.md) y [Home de la wiki](../wiki/Home.md):

**SRS → SAD/HLD → SDD/LDD → contratos y reglas → proceso → informe.**

Consecuencias prácticas:

1. El informe **cita** requisitos (`RF-*`, `RNF-*`) y decisiones (`AD-*`); no introduce requisitos ni decisiones nuevas.
2. Si el informe y la wiki discrepan, se corrige el informe, salvo que la discrepancia revele un error en el documento dueño de la decisión; en ese caso se propone el cambio al SRS, SAD o SDD mediante PR revisado y después se actualiza el informe.
3. El informe no declara implementado ni verificado nada que no tenga evidencia en un PR o en [evidencias de pruebas](../wiki/04-implementacion/pruebas/README.md).

## Contenido exigido

Portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones aprendidas, y referencias ([SRS §2](../wiki/01-producto/01-SRS.md) y [entrega](../wiki/05-proceso/entrega.md)). Cada sección se redacta desde la fuente que le corresponde en la wiki:

| Sección | Fuente |
|---|---|
| Marco conceptual | [SAD §1-§2](../wiki/02-arquitectura/02-SAD-HLD.md) y [reglas arquitectónicas](../wiki/02-arquitectura/reglas-arquitectonicas.md) |
| Diseño: requisitos | [SRS §6-§9](../wiki/01-producto/01-SRS.md) |
| Diseño: arquitectura y decisiones | [SAD/HLD](../wiki/02-arquitectura/02-SAD-HLD.md) y [decisiones ADR](../wiki/02-arquitectura/decisiones-adr.md) |
| Diseño: modelo de datos | [Persistencia](../wiki/03-contratos/persistencia.md) |
| Diseño: clases, capas y vistas | [SDD/LDD](../wiki/04-implementacion/03-SDD-LDD.md) |
| Diseño: contrato de la API | [API REST](../wiki/03-contratos/api-rest.md) |
| Procedimiento | [Ejecución local](../wiki/04-implementacion/ejecucion-local.md), [SDD §3-§4](../wiki/04-implementacion/03-SDD-LDD.md), [versiones](../wiki/04-implementacion/versiones.md) y [runbook](../wiki/04-implementacion/runbook-demo.md) |
| Conclusiones y lecciones | Observaciones reales del desarrollo; sin generalidades |
| Trazabilidad | [Tabla de trazabilidad](../wiki/01-producto/trazabilidad.md) |

Los diagramas se toman de las fuentes de la wiki. Si se exportan como imagen, se exportan de esa misma fuente y se guardan en `diagramas/`.
