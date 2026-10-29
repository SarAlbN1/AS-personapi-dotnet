# Plan de sprints

[← Backlog](README.md) · [Priorización](priorizacion.md) · [DoR/DoD](../05-proceso/dor-y-dod.md)

Dos sprints, tres integrantes, 17 historias, **66 puntos**. Los responsables se nombran como `Integrante A`, `B` y `C`; la correspondencia con las cuentas reales de GitHub (`SarAlbN1`, `Juandi48`, `ma-sarmiento`) está en [equipo.tsv](equipo.tsv) y es la que usa el script al crear las issues. Los estados viven en GitHub Projects; esta página fija alcance, reparto y objetivos, no el progreso.

## Reparto global

| Sprint | Historias | Puntos | Milestone |
|---|---|---|---|
| Sprint 1 | 6 | 23 | `Sprint 1` |
| Sprint 2 | 11 | 43 | `Sprint 2` |
| **Total** | **17** | **66** | |

| Integrante | Issues S1 | Puntos S1 | Issues S2 | Puntos S2 | Issues total | Puntos total |
|---|---|---|---|---|---|---|
| A | 2 | 8 | 3 | 11 | 5 | 19 |
| B | 2 | 7 | 3 | 14 | 5 | 21 |
| C | 2 | 8 | 5 | 18 | 7 | 26 |

**Diferencias de carga y justificación.** A queda en 19 puntos, B en 21 y C en 26. El desequilibrio es deliberado y acordado: al añadirse HU-016 (diagramas C4, a A) y HU-017 (manual de despliegue en el informe, a B), el equipo pasó a C las dos historias de **menor prioridad** del Sprint 2 —`P1`, 3 puntos, hojas del grafo— una de cada persona: **HU-008** (antes de B) y **HU-012** (antes de A). Ver la [justificación completa](priorizacion.md#ampliación-documental-y-reasignación-hu-016-hu-017). Los 7 puntos que separan a A de C corresponden a historias de verificación y presentación, no de código nuevo. B sube de 19 a 21 porque HU-017 pasó de 3 a 5 puntos al incorporar los archivos de despliegue con contenedores que el informe ya declara y las capturas reales del procedimiento. En Sprint 1, B queda con 7 puntos frente a 8 porque profesiones (HU-005) es la entidad con menos restricciones de entrada: no tiene FK ni clave compuesta. Se compensa en Sprint 2, donde B asume estudios (HU-011 + HU-013, 9 puntos), el módulo de mayor riesgo arquitectónico por la clave compuesta (AK-03). C concentra la base técnica de datos (HU-003, 5 puntos) y, en Sprint 2, la integración y la entrega (HU-009, HU-014, HU-015), trabajo de verificación y documentación que no produce funcionalidad nueva pero sí depende de todo lo demás; por eso se le reservan las historias de menor volumen de código y se acepta el punto adicional.

## Sprint 1 — base técnica y primer canal REST

**Objetivo:** desde un clon limpio se compila la solución, se reconstruye `persona_db` con sus restricciones y dos recursos (`personas`, `profesiones`) completan su CRUD REST contra la base real. El layout Razor queda listo para que los cuatro CRUD web del Sprint 2 arranquen en paralelo.

| ID | Entregable | Épica | Módulo | Prioridad | Puntos | Responsable | Depende de |
|---|---|---|---|---|---|---|---|
| HU-001 | Solución ASP.NET Core MVC .NET 10 y estructura de capas ejecutable | EP-00 | Base | P0 | 3 | A | — |
| HU-002 | Esquema `persona_db` con restricciones y DML reproducible | EP-00 | Datos | P0 | 3 | B | — |
| HU-003 | EF Core Database First, DbContext, contratos DAO y DI | EP-00 | Datos | P0 | 5 | C | HU-001, HU-002 |
| HU-004 | API REST de personas de extremo a extremo | EP-01 | Personas | P0 | 5 | A | HU-003 |
| HU-005 | API REST de profesiones de extremo a extremo | EP-02 | Profesiones | P0 | 4 | B | HU-003 |
| HU-006 | Layout MVC compartido, navegación y errores visibles | EP-00 | Base | P0 | 3 | C | HU-001 |

**Entregables verificables al cierre:** solución que compila; `persona_db` creada por DDL con datos del DML; entidades y repositorios generados desde el esquema; 10 de los 20 endpoints operativos con sus códigos `200/201/204/400/404/409`; layout navegable con render de errores. Corte interno opcional `v0.1.0` ([releases](../05-proceso/pull-requests-y-releases.md)).

**Secuencia interna.** Día 1: A inicia HU-001 y B inicia HU-002 en paralelo (ninguna depende de la otra). En cuanto ambas se integran, C ejecuta el scaffold de HU-003; mientras espera, C puede avanzar HU-006, que solo necesita HU-001. Integrada HU-003, A y B trabajan HU-004 y HU-005 en paralelo sobre archivos distintos.

**Riesgo del sprint:** HU-003 es el cuello de botella real: tres historias dependen de ella. Mitigación: es la primera prioridad de C, su alcance excluye servicios y DTOs (que pertenecen a cada módulo), y HU-006 le da trabajo útil mientras HU-001/HU-002 se integran.

## Sprint 2 — CRUD dual completo, verificación y entrega

**Objetivo:** las cuatro entidades operan por los dos canales, Swagger expone y ejecuta las 20 operaciones, los siete escenarios de aceptación están ejecutados con evidencia y la entrega académica queda publicada con `v1.0.0`.

| ID | Entregable | Épica | Módulo | Prioridad | Puntos | Responsable | Depende de |
|---|---|---|---|---|---|---|---|
| HU-007 | CRUD MVC de personas con errores controlados | EP-01 | Personas | P1 | 4 | A | HU-004, HU-006 |
| HU-008 | CRUD MVC de profesiones con errores controlados | EP-02 | Profesiones | P1 | 3 | C | HU-005, HU-006 |
| HU-009 | Contrato OpenAPI 3 y Swagger UI con 20 operaciones | EP-05 | Base | P0 | 3 | C | HU-004, HU-005 |
| HU-010 | API REST de teléfonos con validación de dueño | EP-03 | Teléfonos | P1 | 4 | A | HU-003 |
| HU-011 | API REST de estudios con clave compuesta | EP-04 | Estudios | P0 | 5 | B | HU-003 |
| HU-012 | CRUD MVC de teléfonos con selector de persona | EP-03 | Teléfonos | P1 | 3 | C | HU-010, HU-006 |
| HU-013 | CRUD MVC de estudios con selectores y clave compuesta | EP-04 | Estudios | P1 | 4 | B | HU-011, HU-006 |
| HU-014 | Verificación integral: integridad SQL, CRUD dual y E2E | EP-05 | Pruebas | P0 | 5 | C | HU-007, HU-008, HU-010, HU-011, HU-012, HU-013 |
| HU-015 | Cierre de entrega: clon limpio, revisión, informe y tag | EP-05 | Entrega | P0 | 4 | C | HU-009, HU-014 |
| HU-016 | Diagramas C4: contexto, contenedores y componentes | EP-05 | Entrega | P1 | 3 | A | HU-001, HU-003 |
| HU-017 | Manual técnico de despliegue en el informe | EP-05 | Entrega | P1 | 5 | B | HU-001, HU-002 |

**Entregables verificables al cierre:** 20 endpoints REST y 20 acciones web (cinco por entidad) operativas; Swagger con las 20 operaciones ejecutables; matriz de casos negativos ejecutada; diagramas C4 de contexto, contenedores y componentes enlazados desde la wiki; manual técnico de despliegue en el informe, con sus pasos ejecutados en entorno limpio; informe académico; tag `v1.0.0` con su URL.

**Secuencia interna.** A avanza personas MVC (HU-007) y teléfonos REST (HU-010) y cierra con los diagramas C4 (HU-016), que puede dibujar en paralelo porque solo necesita estructura y capas ya integradas. B avanza estudios REST (HU-011) y su CRUD web (HU-013) y redacta el manual de despliegue (HU-017) en el `.tex` que mantiene. C toma las dos historias reasignadas y las de convergencia: empieza por HU-009 (desbloqueada desde el día 1 por HU-004/HU-005) y HU-008 (sus bloqueantes ya están integrados), construye la suite de HU-014 de forma incremental a medida que cada módulo se integra, hace HU-012 cuando A integra HU-010 —única espera entre responsables— y cierra con HU-015.

**Riesgo del sprint:** HU-014 y HU-015 convergen al final. Es una dependencia técnica real, no artificial: no se verifica un CRUD dual inexistente ni se etiqueta una entrega sin evidencia. Mitigación: la suite se construye incrementalmente y la lista de revisión de HU-015 se va llenando por módulo conforme se integran los PR, en lugar de dejarla para el último día.

## Reparto por módulo

| Módulo | Historias | Responsable |
|---|---|---|
| Base | HU-001, HU-006, HU-009 | A, C, C |
| Datos | HU-002, HU-003 | B, C |
| Personas | HU-004, HU-007 | A |
| Profesiones | HU-005, HU-008 | B, C |
| Teléfonos | HU-010, HU-012 | A, C |
| Estudios | HU-011, HU-013 | B |
| Pruebas | HU-014 | C |
| Entrega | HU-015, HU-016, HU-017 | C, A, B |

La regla de partida era **un único responsable por entidad en sus dos canales**, para que nadie editara el mismo servicio, DTO o vista y para que quien construyó la API conociera las reglas al construir la vista. Se mantiene en personas y estudios. **Profesiones y teléfonos son la excepción acordada** al reasignar HU-008 y HU-012 a C: el canal REST y el canal web de esas dos entidades tienen responsables distintos. Consecuencia real y su acuerdo en el punto 6 de la sección siguiente.

## Acuerdos sobre archivos compartidos

`Program.cs`, el layout y los scripts SQL son los únicos puntos de contención. Para cumplir el criterio de no editar simultáneamente los mismos archivos centrales:

1. **HU-001** crea `Program.cs` con el arranque y la configuración de Swagger.
2. **HU-003** añade el registro del `DbContext` y de los cuatro repositorios.
3. Cada historia de API añade **solo** la línea DI de su propio servicio (`AddScoped<IXService, XService>()`).
4. **HU-006** es la única propietaria del layout y de la convención de errores; los módulos la consumen sin modificarla. Si necesitan un cambio, lo piden en la issue de HU-006 o en una issue nueva, no lo editan por su cuenta.
5. **HU-002** es la única propietaria de los scripts SQL; un cambio de esquema se propone primero en el DDL y se coordina con HU-003 ([regla 7](../02-arquitectura/reglas-arquitectonicas.md)).
6. **Profesiones y teléfonos con responsable distinto por canal.** HU-008 y HU-012 (web) son de C; HU-005 y HU-010 (REST) son de B y de A. El servicio y los DTO de cada entidad los fija la historia REST y la historia web **los consume sin modificarlos**: si la vista necesita un método nuevo en `I*Service`, se pide en la issue de la historia REST o en una issue nueva, no se edita la rama de otra persona. HU-012 además espera la integración de HU-010; mientras tanto C avanza HU-008, HU-009 o HU-014.
7. **`docs/informe/main.tex` tiene un solo propietario.** HU-017 es la única historia que escribe en el `.tex` y está asignada a quien lo mantiene; HU-015 solo lo verifica y cita. Las fuentes de los diagramas de HU-016 viven en `docs/wiki/02-arquitectura/diagramas/`, que crea esa historia y nadie más edita.
