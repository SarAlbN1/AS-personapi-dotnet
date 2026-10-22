# Estándares de documentación

[← Proceso](README.md)

| Documento | Propósito y contenido mínimo |
|---|---|
| `README.md` | Objetivo, stack, ejecución, pruebas, estructura, enlaces a la wiki y tag de entrega |
| `docs/wiki/01-producto/01-SRS.md` | Alcance, requisitos, drivers y architecture killers |
| `docs/wiki/02-arquitectura/02-SAD-HLD.md` | Estilo, capas, relaciones, flujos y decisiones |
| `docs/wiki/04-implementacion/03-SDD-LDD.md` | Interfaces, clases, rutas, vistas, validaciones y diseño de implementación |
| `docs/wiki/03-contratos/api-rest.md` | 20 operaciones, DTO, respuestas y ejemplos |
| `docs/wiki/03-contratos/persistencia.md` | Tablas, PK/FK, esquema, integridad y Database First |
| `docs/wiki/04-implementacion/runbook-demo.md` | Preparación, recorrido E2E, negativos y recuperación |
| `docs/wiki/04-implementacion/pruebas/` | Resultados medidos: fecha, entorno, comando y valor observado |
| `docs/informe/` | Portada, marco conceptual, diseño, procedimiento, conclusiones/lecciones, referencias; estructura en `estructura-del-informe.md` |
| `docs/sustentacion/` | Presentación y guion de la explicación arquitectónica; distingue lo exigido de lo complementario |
| `CONTRIBUTING.md` | Reglas breves de colaboración y PR |

**Reglas:** enlazar en lugar de duplicar, usar rutas relativas comprobables, diferenciar «diseñado» de «implementado», actualizar documentación en el PR que modifica su contrato y no convertir wiki en espejo de GitHub Projects. Solo documentar tecnologías que existen en PersonAPI.
