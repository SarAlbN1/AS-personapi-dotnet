# EP-05 · Integración y entrega

[← Backlog](../README.md) · [Pruebas](../../04-implementacion/pruebas.md) · [Entrega](../../05-proceso/entrega.md)

**Propósito:** cerrar el laboratorio: documentar el contrato REST completo en Swagger, verificar los dos canales y la integridad real de la base, y publicar la entrega académica con el tag `v1.0.0`. Esta épica **no sustituye** las pruebas de cada historia de módulo; comprueba la integración de lo ya probado individualmente.

**Requisitos cubiertos:** RF-07 (API autodescriptiva), RF-09 (entrega Git), RNF-01, RNF-02, RNF-03, RNF-04, RNF-05, RNF-06. Cierra los *killers* AK-01 a AK-07.

**Decisiones aplicadas:** AD-07 (OpenAPI 3 + Swagger UI), AD-01, AD-08, AD-09.

**Componentes afectados:** configuración de Swagger en `Program.cs` y atributos de los controladores API, proyecto o suite de pruebas, `README.md`, `docs/wiki/04-implementacion/runbook-demo.md`, tag Git. El informe `docs/informe/main.tex` se consulta en **solo lectura**: es la fuente de verdad del entregable académico y no se modifica desde este backlog.

**Historias:** HU-009, HU-014, HU-015 (todas Sprint 2) · **12 puntos**

**Criterio de éxito de la épica:** Swagger UI expone y ejecuta las 20 operaciones; los siete escenarios de SRS §10 están ejecutados con evidencia; un clon limpio compila, conecta y corre siguiendo el README; `v1.0.0` apunta a un commit verificado.

---

## HU-009 · Contrato OpenAPI 3 y Swagger UI con las 20 operaciones documentadas

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 3 · **Módulo** Base

### Objetivo
Documentar y hacer ejecutable el contrato REST completo desde Swagger UI, para que la API sea autodescriptiva y evaluable, y cerrar el riesgo AK-05.

### Alcance técnico
- Configuración de Swashbuckle, comentarios XML, agrupación por recurso y ejemplos de esquema por DTO.
- Convención de `ProducesResponseType` y documentación de `400`/`404`/`409` por operación, aplicada a los controladores existentes (personas y profesiones) y **especificada por escrito** para los de teléfonos y estudios, de modo que HU-010 y HU-011 la cumplan sin rediseñar su controlador.
- Respuestas de error uniformes sin trazas SQL ni secretos ([validaciones y errores](../../03-contratos/validaciones-y-errores.md)).
- **No incluye:** implementar endpoints (los aportan EP-01 a EP-04).

### Criterios de aceptación
1. El documento OpenAPI 3 describe cada operación con sus claves, verbos y códigos de estado, con la convención aplicada a personas y profesiones.
2. La convención queda especificada por escrito para teléfonos y estudios.
3. Existe al menos un ejemplo de entrada y de salida por recurso.
4. Los códigos `400`, `404` y `409` están documentados por operación y las respuestas de error no exponen trazas ni secretos.
5. **Comprobación final:** las 20 operaciones aparecen y se ejecutan desde `/swagger`; se verifica una vez integradas HU-010 y HU-011.

**Evidencia mínima:** documento OpenAPI exportado con el conteo de operaciones y capturas de Swagger UI ejecutando un caso por recurso.

### Dependencias
**Bloqueantes:** HU-004, HU-005 (necesita controladores API reales sobre los que fijar la convención).
**De verificación, no de construcción:** HU-010 y HU-011. El trabajo se desarrolla y revisa antes; el criterio 5 se marca cuando esas dos historias se integran. Esta distinción se declara en la issue para no crear un bloqueo artificial ni cerrar la historia sin evidencia.

### Trazabilidad
RF-07 · RNF-03 · AD-07 · AK-05 · [api-rest.md](../../03-contratos/api-rest.md#swagger-y-verificación) · [SDD §4](../../04-implementacion/03-SDD-LDD.md#4-dependencias-en-programcs), §5 · criterio de éxito 4.

### Definition of Done
DoD común + conteo de 20 operaciones comprobado en el documento OpenAPI y PR revisado.

---

## HU-014 · Verificación integral: integridad SQL, CRUD dual y recorrido E2E

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 5 · **Módulo** Pruebas

### Objetivo
Ejecutar y registrar la verificación de los dos canales, la integridad real de la base y los siete escenarios de aceptación, para sostener la demostración con evidencia y no con afirmaciones.

### Alcance técnico
- Pirámide de [pruebas.md](../../04-implementacion/pruebas.md): reglas de servicio con DAOs controlables, integración contra SQL Server real, comprobación de los 20 endpoints, de las 20 acciones web y de los escenarios E2E.
- Escenarios 1 a 7 de [SRS §10](../../01-producto/01-SRS.md#10-escenarios-mínimos-de-aceptación), incluido el borrado bloqueado y la limpieza ordenada de dependientes.
- Comprobación arquitectónica automatizable: ningún archivo fuera de `Back/03-AccesoDatos/Repository/Implementations` referencia `PersonaDbContext`.
- Actualización de [runbook-demo.md](../../04-implementacion/runbook-demo.md) con el recorrido realmente ejecutado.
- Construcción de las pruebas sobre el proyecto de pruebas creado en HU-001; no se crea una estructura de pruebas paralela.
- **No incluye:** corregir defectos de módulos ajenos. Se reportan como issue o comentario según [CLAUDE.md](../../../../CLAUDE.md) y el [proceso](../../05-proceso/pull-requests-y-releases.md); el responsable del módulo los corrige en su rama, regla de propiedad que se aplica durante toda la historia aunque no sea un criterio de aceptación.

### Criterios de aceptación
1. Los escenarios 1 a 7 de SRS §10 se ejecutan contra `persona_db` real y su resultado queda registrado.
2. La matriz negativa se ejecuta para los cuatro recursos: clave duplicada `409`, referencia inválida `400`, clave ausente `404` y borrado bloqueado `409`.
3. Las pruebas de integración demuestran que las restricciones de SQL Server impiden duplicados y huérfanos, no solo con repositorios simulados.
4. Se recorren y registran las 20 operaciones REST y las 20 acciones web (cinco por entidad) con su resultado.
5. **Comprobación arquitectónica completa**, con evidencia por cada punto:
   - ningún archivo fuera de las implementaciones DAO referencia `PersonaDbContext` ni ejecuta SQL (AK-01, regla 3);
   - el controlador MVC y el controlador API de cada entidad consumen **el mismo** `I*Service`, sin reglas duplicadas (AD-02, AD-03, regla 2 y 12, [criterio de éxito 12](../../01-producto/criterios-de-exito.md));
   - las respuestas HTTP y las vistas usan DTO y ViewModels, sin exponer entidades EF con navegaciones (AD-10, regla 8);
   - la solución sigue siendo **un único artefacto desplegable** sobre una sola base `persona_db` (AD-01, regla 1).
6. `runbook-demo.md` queda actualizado con el recorrido ejecutado.

**Evidencia mínima:** salida de compilación y pruebas, matriz de casos con resultado esperado frente al obtenido y runbook actualizado.

### Dependencias
**HU-007, HU-008, HU-010, HU-011, HU-012, HU-013.** Es el punto de convergencia del Sprint 2 por naturaleza: no puede verificarse un CRUD dual que no existe. Mitigación acordada: la suite se construye de forma incremental a medida que cada módulo se integra, en lugar de esperar al final del sprint. El conteo de Swagger se toma de HU-009.

### Trazabilidad
RNF-01, RNF-02, RNF-06 · AD-01, AD-02, AD-03, AD-10 · AK-01, AK-02, AK-03, AK-04 · [pruebas.md](../../04-implementacion/pruebas.md) · [SRS §10](../../01-producto/01-SRS.md#10-escenarios-mínimos-de-aceptación) · [SDD §9](../../04-implementacion/03-SDD-LDD.md#9-comprobación-de-implementación) · reglas 3, 9, 10, 11 · criterios de éxito 6, 10, 11, 13.

### Definition of Done
DoD común + evidencia de persistencia real, matriz de casos completa y PR revisado.

---

## HU-015 · Cierre de entrega: clon limpio verificado, revisión arquitectónica, informe y tag `v1.0.0`

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 4 · **Módulo** Entrega

### Objetivo
Verificar la reproducibilidad desde un clon limpio, cerrar la revisión arquitectónica y publicar la entrega académica exigida por el laboratorio.

### Alcance técnico
- README con restauración, configuración, compilación, ejecución y scripts, **verificado ejecutándolo** sobre un clon limpio y una base creada desde cero (AK-06).
- Recorrido de la [lista de revisión arquitectónica](../../02-arquitectura/reglas-arquitectonicas.md#lista-de-revisión-arquitectónica-de-un-pr) por módulo y cierre documentado de AK-01 a AK-07.
- Comprobación de ausencia de secretos en el historial (AK-07).
- **Verificación** del informe académico, cuya fuente de verdad es `docs/informe/main.tex` (LaTeX, compilado en Overleaf): se comprueba que contiene portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones, y referencias, con las versiones resueltas (`dotnet --info`, paquetes) de [versiones](../../04-implementacion/versiones.md). **El `.tex` no se toca en esta historia:** si falta contenido, se solicita a quien mantiene el documento y esa persona lo redacta en el `.tex`; no se crea un informe paralelo en Markdown ni se versiona el PDF como fuente.
- Tag `v1.0.0` conforme a [entrega.md](../../05-proceso/entrega.md) y [releases](../../05-proceso/pull-requests-y-releases.md).
- **No incluye:** nueva funcionalidad, ni edición de `docs/informe/main.tex`. Si aparece un defecto, se abre issue; el tag no se publica sobre un estado no verificado.

### Criterios de aceptación
1. Los pasos del README se verifican ejecutándolos sobre un clon limpio y una base creada desde cero.
2. La lista de revisión arquitectónica se recorre por módulo y cada killer AK-01 a AK-07 queda cerrado con evidencia.
3. Se comprueba que no hay secretos ni cadenas con credenciales en el historial del repositorio.
4. Se verifica que `docs/informe/main.tex` contiene portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones, y referencias, con las versiones resueltas, y que el PDF entregado se compila de ese `.tex` sin haberlo modificado desde el backlog.
5. `v1.0.0` se crea sobre un commit integrado y verificado de `main` y su URL queda registrada, solo después de la evidencia de HU-014.
6. Las páginas de la wiki afectadas se actualizan en el mismo PR.

**Evidencia mínima:** log de la instalación desde clon limpio, lista de revisión firmada, verificación de ausencia de secretos y URL del tag.

### Dependencias
**HU-009** (conteo de Swagger para el criterio 4 del laboratorio) y **HU-014** (evidencia de verificación). Es la última historia del backlog por definición.

### Trazabilidad
RF-09 · RNF-04, RNF-05 · AK-06, AK-07 · AD-01, AD-08, AD-09 · [entrega.md](../../05-proceso/entrega.md) · [versiones](../../04-implementacion/versiones.md) · criterios de éxito 1, 3, 14, 15.

### Definition of Done
DoD común + reproducibilidad comprobada en entorno limpio, informe entregado y tag publicado tras la verificación.
