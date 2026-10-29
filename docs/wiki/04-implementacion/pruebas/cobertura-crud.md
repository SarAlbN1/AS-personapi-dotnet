# Cobertura CRUD comprobada

[← Evidencias](README.md) · [Contrato REST](../../03-contratos/api-rest.md) · [Runbook de la demo](../runbook-demo.md)

Los [criterios de éxito](../../01-producto/criterios-de-exito.md) 4 y 5 exigen las **cinco operaciones REST** y las **cinco acciones Razor** por recurso. Esta página las registra una por una. **Las 20 operaciones REST están ejecutadas. De las acciones Razor, profesiones y estudios están en `main`; personas y teléfonos siguen pendientes de integrar.**

Columna `Resultado`: código HTTP observado en REST, o el comportamiento observado en la vista. Columna `Evidencia`: salida, captura o PR donde consta.

**Recorrido R1.** Se ejecutó el 2026-10-28 con la aplicación en `Development`, contra `persona_db` y sobre `8434ffa`, que incluye `main` en `05c02ac`. Las peticiones salieron por HTTP con un script desde fuera de la aplicación. Usó claves propias (persona `916000001`, profesión `9161` y teléfono `9160000001`) y las borró al terminar, así que la base volvió a quedar con 3 filas por tabla. Las 39 comprobaciones dieron el código esperado. Las mismas operaciones las cubren de forma automática `Integracion/*ApiPersonaDbTests.cs` y `Integracion/MatrizNegativaHttpPersonaDbTests.cs`.

## API REST (20 operaciones)

| Recurso | Operación | Ruta | Esperado | Resultado | Evidencia |
|---|---|---|---|---|---|
| Personas | Listar | `GET /api/personas` | `200` | `200`, 3 filas | R1 |
| Personas | Detalle | `GET /api/personas/{cc}` | `200` / `404` | `200`; `404` tras la baja | R1 |
| Personas | Alta | `POST /api/personas` | `201` + `Location` | `201`, `Location /api/personas/916000001` | R1 |
| Personas | Edición | `PUT /api/personas/{cc}` | `204` | `204`; el detalle devuelve el apellido editado | R1 |
| Personas | Baja | `DELETE /api/personas/{cc}` | `204` / `409` | `409` con teléfono y estudio; `204` sin ellos | R1 |
| Profesiones | Listar | `GET /api/profesiones` | `200` | `200`, 3 filas | R1 |
| Profesiones | Detalle | `GET /api/profesiones/{id}` | `200` / `404` | `200`; `404` con id 9169 | R1 |
| Profesiones | Alta | `POST /api/profesiones` | `201` + `Location` | `201`, `Location /api/profesiones/9161` | R1 |
| Profesiones | Edición | `PUT /api/profesiones/{id}` | `204` | `204` | R1 |
| Profesiones | Baja | `DELETE /api/profesiones/{id}` | `204` / `409` | `409` con estudios; `204` sin ellos | R1 |
| Teléfonos | Listar | `GET /api/telefonos` | `200` | `200`, 3 filas | R1 |
| Teléfonos | Detalle | `GET /api/telefonos/{num}` | `200` / `404` | `200`; `404` con 9160000099 | R1 |
| Teléfonos | Alta | `POST /api/telefonos` | `201` + `Location` | `201`, `Location /api/telefonos/9160000001` | R1 |
| Teléfonos | Edición | `PUT /api/telefonos/{num}` | `204` | `204` | R1 |
| Teléfonos | Baja | `DELETE /api/telefonos/{num}` | `204` | `204` | R1 |
| Estudios | Listar | `GET /api/estudios` | `200` | `200`, 3 filas | R1 |
| Estudios | Detalle | `GET /api/estudios/{idProf}/{ccPer}` | `200` / `404` | `200`; `404` con 9169/916000099 | R1 |
| Estudios | Alta | `POST /api/estudios` | `201` + `Location` | `201`, `Location /api/estudios/9161/916000001` | R1 |
| Estudios | Edición | `PUT /api/estudios/{idProf}/{ccPer}` | `204` | `204` | R1 |
| Estudios | Baja | `DELETE /api/estudios/{idProf}/{ccPer}` | `204` | `204` | R1 |

## Vistas Razor (20 acciones)

| Recurso | Index | Details | Create | Edit | Delete | Evidencia |
|---|---|---|---|---|---|---|
| Personas | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | HU-007, PR #35 sin integrar |
| Profesiones | Funciona | Funciona | Funciona | Funciona | Funciona, y el 409 se ve sobre la confirmación | PR #32: capturas en navegador y `ProfesionesMvcPersonaDbTests` |
| Teléfonos | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | HU-012, PR #39 sin integrar |
| Estudios | Funciona | Funciona | Funciona | Funciona | Funciona | PR #38: capturas en navegador y `EstudiosMvcPersonaDbTests` |

En `Telefonos` y `Estudios` la comprobación incluye el **selector de FK** (persona, y persona + profesión): una vista que exige teclear la llave a mano no cumple el criterio. En estudios, el selector se comprueba en `Los_selectores_ofrecen_las_profesiones_y_personas_de_los_servicios` y se ve en la captura del alta del PR #38.

## Casos negativos

| Caso | Esperado | Resultado | Evidencia |
|---|---|---|---|
| Alta duplicada en cada recurso | `409` | `409` en los cuatro | R1 y `MatrizNegativaHttpPersonaDbTests` |
| Identificador inexistente en detalle, edición y baja | `404` | `404` en los detalles de los cuatro recursos, en la edición de profesión y en la baja de teléfono | R1; los demás verbos y recursos, en `MatrizNegativaHttpPersonaDbTests` |
| Teléfono con `duenio` inexistente | `400` | `400` | R1 y `MatrizNegativaHttpPersonaDbTests` |
| Estudio con persona o profesión inexistente | `400` | `400` en los dos casos | R1 y `MatrizNegativaHttpPersonaDbTests` |
| Clave de ruta distinta de la del cuerpo en `PUT` | `400` | `400` en personas | R1 |
| Borrado de persona con teléfonos o estudios | `409` y dependientes intactos | `409`; la persona y sus dependientes siguen | R1 y `EscenariosSrsPersonaDbTests` (escenarios 5 y 6) |
| Borrado de profesión con estudios | `409` | `409` | R1 |
| Borrado ordenado: dependientes y después la persona | `204` y luego `404` | `204` en teléfono, estudio y persona; después `404` | R1 y `EscenariosSrsPersonaDbTests` |
| Recuento de operaciones en el documento OpenAPI 3 | 20 | 20, `openapi 3.0.4` | R1 |
