# Cobertura CRUD comprobada

[← Evidencias](README.md) · [Contrato REST](../../03-contratos/api-rest.md) · [Runbook de la demo](../runbook-demo.md)

Los [criterios de éxito](../../01-producto/criterios-de-exito.md) 4 y 5 exigen las **cinco operaciones REST** y las **cinco acciones Razor** por recurso. Esta página las registra una por una. **Ninguna fila está ejecutada todavía.**

Columna `Resultado`: código HTTP observado en REST, o el comportamiento observado en la vista. Columna `Evidencia`: salida, captura o PR donde consta.

## API REST (20 operaciones)

| Recurso | Operación | Ruta | Esperado | Resultado | Evidencia |
|---|---|---|---|---|---|
| Personas | Listar | `GET /api/personas` | `200` | Pendiente | — |
| Personas | Detalle | `GET /api/personas/{cc}` | `200` / `404` | Pendiente | — |
| Personas | Alta | `POST /api/personas` | `201` + `Location` | Pendiente | — |
| Personas | Edición | `PUT /api/personas/{cc}` | `204` | Pendiente | — |
| Personas | Baja | `DELETE /api/personas/{cc}` | `204` / `409` | Pendiente | — |
| Profesiones | Listar | `GET /api/profesiones` | `200` | Pendiente | — |
| Profesiones | Detalle | `GET /api/profesiones/{id}` | `200` / `404` | Pendiente | — |
| Profesiones | Alta | `POST /api/profesiones` | `201` + `Location` | Pendiente | — |
| Profesiones | Edición | `PUT /api/profesiones/{id}` | `204` | Pendiente | — |
| Profesiones | Baja | `DELETE /api/profesiones/{id}` | `204` / `409` | Pendiente | — |
| Teléfonos | Listar | `GET /api/telefonos` | `200` | Pendiente | — |
| Teléfonos | Detalle | `GET /api/telefonos/{num}` | `200` / `404` | Pendiente | — |
| Teléfonos | Alta | `POST /api/telefonos` | `201` + `Location` | Pendiente | — |
| Teléfonos | Edición | `PUT /api/telefonos/{num}` | `204` | Pendiente | — |
| Teléfonos | Baja | `DELETE /api/telefonos/{num}` | `204` | Pendiente | — |
| Estudios | Listar | `GET /api/estudios` | `200` | Pendiente | — |
| Estudios | Detalle | `GET /api/estudios/{idProf}/{ccPer}` | `200` / `404` | Pendiente | — |
| Estudios | Alta | `POST /api/estudios` | `201` + `Location` | Pendiente | — |
| Estudios | Edición | `PUT /api/estudios/{idProf}/{ccPer}` | `204` | Pendiente | — |
| Estudios | Baja | `DELETE /api/estudios/{idProf}/{ccPer}` | `204` | Pendiente | — |

## Vistas Razor (20 acciones)

| Recurso | Index | Details | Create | Edit | Delete |
|---|---|---|---|---|---|
| Personas | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |
| Profesiones | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |
| Teléfonos | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |
| Estudios | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |

En `Telefonos` y `Estudios` la comprobación incluye el **selector de FK** (persona, y persona + profesión): una vista que exige teclear la llave a mano no cumple el criterio.

## Casos negativos

| Caso | Esperado | Resultado | Evidencia |
|---|---|---|---|
| Alta duplicada en cada recurso | `409` | Pendiente | — |
| Identificador inexistente en detalle, edición y baja | `404` | Pendiente | — |
| Teléfono con `duenio` inexistente | `400` | Pendiente | — |
| Estudio con persona o profesión inexistente | `400` | Pendiente | — |
| Clave de ruta distinta de la del cuerpo en `PUT` | `400` | Pendiente | — |
| Borrado de persona con teléfonos o estudios | `409` y dependientes intactos | Pendiente | — |
| Borrado de profesión con estudios | `409` | Pendiente | — |
| Borrado ordenado: dependientes y después la persona | `204` y luego `404` | Pendiente | — |
| Recuento de operaciones en el documento OpenAPI 3 | 20 | Pendiente | — |
