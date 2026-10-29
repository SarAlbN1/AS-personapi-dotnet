# Priorización y secuencia de implementación

[← Backlog](README.md) · [Plan de sprints](plan-de-sprints.md) · [SRS](../01-producto/01-SRS.md)

## Qué significa cada prioridad

Solo hay dos niveles, igual que las etiquetas acordadas en [issues, etiquetas y milestones](../05-proceso/issues-etiquetas-y-milestones.md) (`priority:P0`, `priority:P1`).

| Prioridad | Definición operativa |
|---|---|
| **P0** | La historia **bloquea a otras**, cierra un [architecture killer](../01-producto/01-SRS.md#9-architecture-killers-condiciones-que-invalidan-la-solución) o condiciona la entrega. Si se retrasa, el sprint se retrasa. |
| **P1** | Alcance **igualmente obligatorio**, pero sin dependientes: puede reordenarse dentro de su sprint sin afectar a nadie más. |

**P1 no es opcional.** Las 17 historias son alcance comprometido del laboratorio: sin las vistas MVC de un módulo se incumple RF-01 a RF-04 y se activa AK-02 («solo hay vistas o solo hay API»). La prioridad ordena la ejecución, no decide qué se entrega.

| Prioridad | Historias | Puntos |
|---|---|---|
| P0 | HU-001, HU-002, HU-003, HU-004, HU-005, HU-006, HU-009, HU-011, HU-014, HU-015 | 40 |
| P1 | HU-007, HU-008, HU-010, HU-012, HU-013, HU-016, HU-017 | 26 |

### Por qué cada P0 lo es

| ID | Razón |
|---|---|
| HU-001 | Sin solución no hay dónde construir; bloquea HU-003 y HU-006. |
| HU-002 | El DDL es el origen del esquema (Database First); bloquea HU-003 y aporta los datos que usan teléfonos y estudios. |
| HU-003 | Bloquea los cuatro módulos; además cierra AK-03 al mapear la clave compuesta real. |
| HU-004 | Primer recorrido completo de capas; fija el patrón que copian los otros tres módulos y desbloquea HU-007 y HU-009. |
| HU-005 | Segundo recurso REST del Sprint 1; desbloquea HU-008 y HU-009. |
| HU-006 | Bloquea los cuatro CRUD web y fija el manejo visible de errores (RF-05, RF-06). |
| HU-009 | Cierra AK-05: sin las 20 operaciones en Swagger la API no es evaluable (RF-07). |
| HU-011 | Cierra AK-03 en el canal REST: la clave compuesta es el riesgo de modelado más citado del laboratorio; además desbloquea HU-013. |
| HU-014 | Cierra AK-01, AK-02 y AK-04 con evidencia; sin ella no hay base para el tag. |
| HU-015 | Cierra AK-06 y AK-07 y produce la entrega exigida (RF-09). |

### Por qué los P1 no bloquean

HU-007, HU-008, HU-012 y HU-013 son hojas del grafo: solo HU-014 las consume, y HU-014 es el último punto de convergencia. **HU-016 y HU-017 sí tienen un dependiente: HU-015.** No alimentan HU-014 —la verificación no espera material documental—, pero su resultado entra en lo que se entrega: los diagramas C4 y el manual de despliegue viven en `docs/informe/main.tex`, y HU-015 es quien verifica ese documento y publica el tag. Si la entrega no las esperara, `v1.0.0` podría salir sin el manual que el propio plan de sprints declara como entregable.

**Excepción declarada:** tras reasignar HU-008 y HU-012 al integrante C, **HU-012 depende de HU-010, que es de A**. Es la única dependencia entre responsables distintos del Sprint 2 y queda registrada como tal: si HU-010 se retrasa, C puede avanzar HU-008, HU-009 o HU-014 sin esperar. HU-008 no tiene ese problema: sus dos bloqueantes (HU-005, HU-006) están integrados.

## Secuencia de implementación

El orden responde a tres reglas: **lo compartido primero**, **un recorrido completo antes de replicarlo** y **verificación al final, sobre funcionalidad ya probada individualmente**.

```text
Sprint 1
  HU-001 ─┬─> HU-003 ─┬─> HU-004 ──> (desbloquea HU-007, HU-009)
  HU-002 ─┘           └─> HU-005 ──> (desbloquea HU-008, HU-009)
  HU-001 ───> HU-006 ─────────────> (desbloquea HU-007, HU-008, HU-012, HU-013)

Sprint 2
  A: HU-007 ──> HU-010 ──> (habilita HU-012, que es de C)
     HU-016 ── independiente
  B: HU-011 ──> HU-013
     HU-017 ── independiente
  C: HU-009
     HU-008 ── desbloqueada por HU-005 y HU-006, ya integradas
     HU-012 ── espera HU-010 (única dependencia entre responsables)

  HU-007, HU-008, HU-010, HU-011, HU-012, HU-013 ──> HU-014 ──> HU-015
  HU-016 y HU-017 no alimentan HU-014, pero sí HU-015: su resultado va en el informe.
```

1. **Base compartida (HU-001, HU-002).** Las dos únicas historias sin dependencias; arrancan simultáneamente.
2. **Puente a datos (HU-003).** Database First exige proyecto y esquema previos. Entrega contratos DAO para que nadie más toque persistencia.
3. **Un módulo completo por REST (HU-004).** Demuestra las cinco capas antes de replicar el patrón: si el recorrido falla, falla en un módulo, no en cuatro.
4. **Segundo módulo y presentación común (HU-005, HU-006).** Confirman que el patrón es replicable y habilitan el canal web.
5. **Canal web y recursos restantes (HU-007 a HU-013).** Dos pistas paralelas por responsable, sobre archivos disjuntos.
6. **Verificación (HU-014) y entrega (HU-015).** Integran lo ya probado; no sustituyen las pruebas de cada historia, que forman parte de sus propios criterios de aceptación.

## Decisiones de ordenación tomadas sobre la propuesta inicial

La hipótesis de distribución recibida se ajustó en cuatro puntos, sin superar las 15 issues y sin alterar ninguna decisión de SRS, SAD o SDD:

1. **Los servicios de dominio salen de la historia base.** La propuesta ponía «EF Core, contratos DAO y servicios base» en una sola issue de Sprint 1. Dejar ahí los cuatro servicios habría convertido a esa historia en propietaria de reglas de cuatro módulos y habría forzado a tres personas a editar `Back/02-Aplicacion/Services/` después. HU-003 entrega contexto, interfaces DAO e implementaciones EF Core; cada servicio de dominio pertenece a la historia REST de su entidad, que es quien conoce sus reglas.
2. **Cada historia REST es un recorrido vertical completo.** DTO, servicio, controlador y pruebas de un recurso van juntos, porque es la unidad mínima demostrable de extremo a extremo. Separar «servicio» de «controlador» habría producido issues sin utilidad comprobable.
3. **Se reordenaron los identificadores del Sprint 2** para que ninguna historia dependa de una posterior: las dos historias de convergencia (HU-014 verificación, HU-015 entrega) son las últimas, y los CRUD web de teléfonos y estudios pasaron a HU-012 y HU-013. En la numeración propuesta, «pruebas de integración E2E» era HU-012 y dependía de las vistas HU-013 y HU-014.
4. **Teléfonos y estudios no dependen de personas ni profesiones.** Sus datos de referencia los aporta el DML de HU-002. Encadenarlos a EP-01/EP-02 habría sido una dependencia artificial contraria a la nota de paralelización del [SAD §5](../02-arquitectura/02-SAD-HLD.md#5-vista-por-módulos).

También se separó, en HU-009, la dependencia **de construcción** (HU-004, HU-005) de la dependencia **de verificación** (HU-010, HU-011): el trabajo se realiza y revisa antes, y el criterio de las 20 operaciones se marca cuando esas historias se integran. Así no se inventa un bloqueo ni se cierra la historia sin evidencia.

## Ampliación documental y reasignación (HU-016, HU-017)

Decisión del equipo posterior al backlog inicial, registrada aquí para que el reparto no quede solo en el Project:

1. **Se añaden dos historias documentales.** HU-016 entrega los diagramas C4 de contexto, contenedores y componentes; HU-017 entrega el manual técnico de despliegue dentro de `docs/informe/main.tex`. Ambas son P1: obligatorias y reordenables dentro del Sprint 2, sin bloquear a la verificación; su único dependiente es HU-015, porque lo que producen va en el informe que la entrega verifica. Ninguna de las dos añade, quita ni reinterpreta un requisito: HU-016 **dibuja** lo que el SAD y el SDD ya deciden y HU-017 **documenta** el despliegue tal como el informe lo declara. Una discrepancia se reporta a la fuente propietaria, no se resuelve en el diagrama ni en el manual.
2. **Responsables.** HU-016 a A (Sara) y HU-017 a B (Juan Diego), que es quien mantiene el `.tex` y por tanto la única persona que puede escribir en él.
3. **Dos historias pasan a C (Miguel) para sostener la carga.** Se eligieron las dos de **menor prioridad** del grafo: `P1`, 3 puntos y hojas consumidas solo por HU-014 — **HU-008** (de B) y **HU-012** (de A). No se movió HU-010, que está en ejecución, ni ninguna P0.
4. **Efecto en la carga:** A 19 puntos, B 21, C 26 (antes 19/19/20). C absorbe 6 de los 8 puntos añadidos al equipo porque sus historias restantes son de verificación y documentación, no de código nuevo; los 2 restantes los asume B al pasar HU-017 de 3 a 5 puntos, por los archivos de despliegue con contenedores y las capturas reales del procedimiento.
5. **Coste aceptado:** se rompe la regla de «una entidad, un responsable en sus dos canales» en profesiones (REST de B, MVC de C) y en teléfonos (REST de A, MVC de C), y aparece una dependencia entre responsables (HU-010 → HU-012). Las dos consecuencias se documentan en el [plan de sprints](plan-de-sprints.md#acuerdos-sobre-archivos-compartidos) con el acuerdo de archivos que las hace manejables.
