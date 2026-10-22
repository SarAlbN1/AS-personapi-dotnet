# Plan de sprints

[← Backlog](README.md) · [Priorización](priorizacion.md) · [DoR/DoD](../05-proceso/dor-y-dod.md)

Dos sprints, tres integrantes, 15 historias, **58 puntos**. Los responsables son temporales (`Integrante A`, `B`, `C`) hasta que el equipo asigne los usuarios reales de GitHub. Los estados viven en GitHub Projects; esta página fija alcance, reparto y objetivos, no el progreso.

## Reparto global

| Sprint | Historias | Puntos | Milestone |
|---|---|---|---|
| Sprint 1 | 6 | 23 | `Sprint 1` |
| Sprint 2 | 9 | 35 | `Sprint 2` |
| **Total** | **15** | **58** | |

| Integrante | Issues S1 | Puntos S1 | Issues S2 | Puntos S2 | Issues total | Puntos total |
|---|---|---|---|---|---|---|
| A | 2 | 8 | 3 | 11 | 5 | 19 |
| B | 2 | 7 | 3 | 12 | 5 | 19 |
| C | 2 | 8 | 3 | 12 | 5 | 20 |

**Diferencias de carga y justificación.** El rango es de 19 a 20 puntos, bajo un punto de diferencia (1,7 %). En Sprint 1, B queda con 7 puntos frente a 8 porque profesiones (HU-005) es la entidad con menos restricciones de entrada: no tiene FK ni clave compuesta. Se compensa en Sprint 2, donde B asume estudios (HU-011 + HU-013, 9 puntos), el módulo de mayor riesgo arquitectónico por la clave compuesta (AK-03). C concentra la base técnica de datos (HU-003, 5 puntos) y, en Sprint 2, la integración y la entrega (HU-009, HU-014, HU-015), trabajo de verificación y documentación que no produce funcionalidad nueva pero sí depende de todo lo demás; por eso se le reservan las historias de menor volumen de código y se acepta el punto adicional.

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
| HU-008 | CRUD MVC de profesiones con errores controlados | EP-02 | Profesiones | P1 | 3 | B | HU-005, HU-006 |
| HU-009 | Contrato OpenAPI 3 y Swagger UI con 20 operaciones | EP-05 | Base | P0 | 3 | C | HU-004, HU-005 |
| HU-010 | API REST de teléfonos con validación de dueño | EP-03 | Teléfonos | P1 | 4 | A | HU-003 |
| HU-011 | API REST de estudios con clave compuesta | EP-04 | Estudios | P0 | 5 | B | HU-003 |
| HU-012 | CRUD MVC de teléfonos con selector de persona | EP-03 | Teléfonos | P1 | 3 | A | HU-010, HU-006 |
| HU-013 | CRUD MVC de estudios con selectores y clave compuesta | EP-04 | Estudios | P1 | 4 | B | HU-011, HU-006 |
| HU-014 | Verificación integral: integridad SQL, CRUD dual y E2E | EP-05 | Pruebas | P0 | 5 | C | HU-007, HU-008, HU-010, HU-011, HU-012, HU-013 |
| HU-015 | Cierre de entrega: clon limpio, revisión, informe y tag | EP-05 | Entrega | P0 | 4 | C | HU-009, HU-014 |

**Entregables verificables al cierre:** 20 endpoints REST y 20 acciones web (cinco por entidad) operativas; Swagger con las 20 operaciones ejecutables; matriz de casos negativos ejecutada; informe académico; tag `v1.0.0` con su URL.

**Secuencia interna.** A y B abren el sprint con pistas independientes: A avanza personas MVC (HU-007) y teléfonos REST (HU-010); B avanza profesiones MVC (HU-008) y estudios REST (HU-011); ambas pistas cierran con su CRUD web (HU-012, HU-013). C empieza por HU-009, que está desbloqueada desde el día 1 del sprint por HU-004/HU-005, construye la suite de HU-014 de forma incremental a medida que cada módulo se integra, y cierra con HU-015.

**Riesgo del sprint:** HU-014 y HU-015 convergen al final. Es una dependencia técnica real, no artificial: no se verifica un CRUD dual inexistente ni se etiqueta una entrega sin evidencia. Mitigación: la suite se construye incrementalmente y la lista de revisión de HU-015 se va llenando por módulo conforme se integran los PR, en lugar de dejarla para el último día.

## Reparto por módulo

| Módulo | Historias | Responsable |
|---|---|---|
| Base | HU-001, HU-006, HU-009 | A, C, C |
| Datos | HU-002, HU-003 | B, C |
| Personas | HU-004, HU-007 | A |
| Profesiones | HU-005, HU-008 | B |
| Teléfonos | HU-010, HU-012 | A |
| Estudios | HU-011, HU-013 | B |
| Pruebas | HU-014 | C |
| Entrega | HU-015 | C |

Cada entidad tiene **un único responsable** en sus dos canales: evita que dos personas editen el mismo servicio, DTO o vista, y permite que quien construyó la API conozca las reglas al construir la vista.

## Acuerdos sobre archivos compartidos

`Program.cs`, el layout y los scripts SQL son los únicos puntos de contención. Para cumplir el criterio de no editar simultáneamente los mismos archivos centrales:

1. **HU-001** crea `Program.cs` con el arranque y la configuración de Swagger.
2. **HU-003** añade el registro del `DbContext` y de los cuatro repositorios.
3. Cada historia de API añade **solo** la línea DI de su propio servicio (`AddScoped<IXService, XService>()`).
4. **HU-006** es la única propietaria del layout y de la convención de errores; los módulos la consumen sin modificarla. Si necesitan un cambio, lo piden en la issue de HU-006 o en una issue nueva, no lo editan por su cuenta.
5. **HU-002** es la única propietaria de los scripts SQL; un cambio de esquema se propone primero en el DDL y se coordina con HU-003 ([regla 7](../02-arquitectura/reglas-arquitectonicas.md)).
