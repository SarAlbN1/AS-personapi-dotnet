# Sustentación y demostración · PersonAPI

Material para **explicar la arquitectura y demostrar la aplicación** ante el docente. Aquí se versiona la presentación que se proyecta y el guion de apoyo.

## Qué exige la entrega y qué es complementario

| | Elemento | Fuente |
|---|---|---|
| **Exigido** | Repositorio público `personapi-dotnet`, scripts DDL y DML, README con configuración, ambiente, compilación y despliegue, código fuente, informe académico y URL del tag `v1.0.0` | [SRS §2](../wiki/01-producto/01-SRS.md) y [entrega](../wiki/05-proceso/entrega.md) |
| **Complementario** | Presentación, guion de la explicación arquitectónica y ensayo de la demo | Esta carpeta |

El alcance registrado del laboratorio **no incluye** una sustentación cronometrada como entregable. Este material existe porque la demo debe poder mostrarse y defenderse con solvencia; si el docente fija una ventana de tiempo, un formato o una asistencia obligatoria, se anota aquí la indicación y su origen antes de planificar sobre ella.

## Archivos

| Archivo | Qué es | Estado |
|---|---|---|
| `presentacion-personapi.pdf` | Presentación exportada, la que se proyecta y se entrega | Pendiente de elaborar |
| `presentacion-personapi.pptx` | Fuente editable, opcional | Opcional |

## Cómo encaja con el resto

- **Demo en vivo:** [Runbook de la demostración](../wiki/04-implementacion/runbook-demo.md) fija el recorrido y los comandos; el [plan de sustentación](../wiki/06-backlog/sustentacion.md) del backlog lo liga a las historias que lo respaldan. Esta carpeta no repite ninguno de los dos: solo guarda la presentación y marca el paso a la aplicación y a Swagger.
- **Qué se afirma sobre la arquitectura:** [SAD/HLD](../wiki/02-arquitectura/02-SAD-HLD.md) y [reglas arquitectónicas](../wiki/02-arquitectura/reglas-arquitectonicas.md). Ninguna diapositiva introduce componentes que no estén en el SAD.
- **Qué se afirma como verificado:** solo lo que tenga una fila completada en [evidencias de pruebas](../wiki/04-implementacion/pruebas/README.md) o en el PR correspondiente.
- **Diagramas:** los del SAD y del SDD, que están embebidos en Mermaid en la wiki. Si se exportan imágenes para las diapositivas, se generan de esas mismas fuentes y no se editan a mano.

## Guion de la explicación arquitectónica

Secuencia propuesta; el reparto de la palabra entre los tres integrantes se acuerda en la planificación del equipo y no se fija en esta página.

| # | Bloque | Contenido | Apoyo |
|---:|---|---|---|
| 1 | Problema y alcance | CRUD de personas, profesiones, teléfonos y estudios por dos canales; qué queda fuera | [Visión y alcance](../wiki/01-producto/vision-y-alcance.md) |
| 2 | Arquitectura | Monolito en capas: MVC/API → servicios → interfaces DAO → EF Core → SQL Server; por qué MVC y REST comparten servicios | [SAD/HLD](../wiki/02-arquitectura/02-SAD-HLD.md) |
| 3 | Patrones aplicados | MVC y DAO/Repository: dónde vive cada responsabilidad y qué prohíbe la regla de que el controlador no toque el `DbContext` | [Reglas](../wiki/02-arquitectura/reglas-arquitectonicas.md), [SDD](../wiki/04-implementacion/03-SDD-LDD.md) |
| 4 | Modelo de datos | Las cuatro tablas, la PK compuesta de `estudios` y por qué no hay borrado en cascada | [Persistencia](../wiki/03-contratos/persistencia.md) |
| 5 | Demo | Recorrido E2E por Razor y Swagger, incluido el `409` por dependencias | [Runbook](../wiki/04-implementacion/runbook-demo.md) |
| 6 | Verificación y cierre | Qué se comprobó, con qué evidencia, y qué quedó pendiente | [Evidencias](../wiki/04-implementacion/pruebas/README.md) |

## Ensayos

| Fecha | Duración | Observaciones |
|---|---|---|
| — | — | Pendiente del primer ensayo completo |
