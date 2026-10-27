# API REST — contrato del laboratorio

[← Contratos](README.md) · [SDD/LDD](../04-implementacion/03-SDD-LDD.md)

**Responsabilidad:** dentro del monolito ASP.NET Core, los controladores `Back/01-Borde/ControllersApi` atienden JSON y delegan siempre en `I*Service`. Ruta base `/api`; la aplicación también expone vistas Razor por MVC. OpenAPI 3 + Swagger UI describe y permite probar las operaciones.

**Estado de implementación:** las cinco operaciones de personas (HU-004) y las cinco de estudios (HU-011) están implementadas y verificadas contra `persona_db`. En estudios, la clave compuesta viaja en los dos segmentos de la ruta, `{idProf}/{ccPer}`, y el `PUT` solo cambia `fecha` y `univer`. Profesiones y teléfonos son todavía especificación del contrato previsto, **no evidencia de API ya implementada**. Los errores viajan como `ProblemDetails`, la forma que ya produce `[ApiController]` para un cuerpo inválido, de modo que la API no añade un estándar de error propio.

## Operaciones obligatorias (20)

| Recurso | Listado | Detalle | Alta | Edición | Baja |
|---|---|---|---|---|---|
| Personas | `GET /api/personas` | `GET /api/personas/{cc}` | `POST /api/personas` | `PUT /api/personas/{cc}` | `DELETE /api/personas/{cc}` |
| Profesiones | `GET /api/profesiones` | `GET /api/profesiones/{id}` | `POST /api/profesiones` | `PUT /api/profesiones/{id}` | `DELETE /api/profesiones/{id}` |
| Teléfonos | `GET /api/telefonos` | `GET /api/telefonos/{num}` | `POST /api/telefonos` | `PUT /api/telefonos/{num}` | `DELETE /api/telefonos/{num}` |
| Estudios | `GET /api/estudios` | `GET /api/estudios/{idProf}/{ccPer}` | `POST /api/estudios` | `PUT /api/estudios/{idProf}/{ccPer}` | `DELETE /api/estudios/{idProf}/{ccPer}` |

## Representaciones JSON (ejemplos)

```json
{ "cc": 1010101010, "nombre": "Ana", "apellido": "Gómez", "genero": "F", "edad": 28 }
```
```json
{ "id": 1, "nom": "Ingeniería de Sistemas", "des": "Diseño y desarrollo de sistemas" }
```
```json
{ "num": "3001234567", "oper": "Claro", "duenio": 1010101010 }
```
```json
{ "idProf": 1, "ccPer": 1010101010, "fecha": "2020-12-15", "univer": "Javeriana" }
```

**Claves:** persona `cc` (entero), profesión `id` (entero), teléfono `num` (cadena, preservar ceros iniciales), estudio `(idProf, ccPer)` (compuesta). JSON `idProf`/`ccPer` se mapea a SQL `id_prof`/`cc_per`. No permitir que un `PUT` cambie la identidad del recurso; si la ruta y el cuerpo difieren responder `400`.

## Respuestas y errores

| Estado | Regla |
|---|---|
| `200 OK` | Consulta exitosa; listado vacío devuelve `[]` |
| `201 Created` | Alta con `Location` que identifica la nueva entidad |
| `204 No Content` | Actualización o borrado exitoso |
| `400 Bad Request` | DTO inválido, clave inconsistente o referencias inexistentes al crear/asociar |
| `404 Not Found` | Consulta, edición o borrado de entidad inexistente |
| `409 Conflict` | Duplicado o borrado prohibido por una relación existente |
| `500 Internal Server Error` | Error inesperado sin exponer secretos ni stack trace |

Los DTO se validan en la entrada y los servicios aplican reglas de negocio. **La base de datos es la defensa definitiva de unicidad y FK**, incluso si dos solicitudes compiten. La estructura exacta del JSON de error se mantiene uniforme por el proyecto; no se añade un estándar de error no requerido sin acuerdo arquitectónico.

## Swagger y verificación

- El documento OpenAPI 3 debe incluir los 20 endpoints, sus claves, códigos y al menos un ejemplo de entrada/salida por recurso.
- Swagger UI se publica en `/swagger` en el entorno local previsto; la dirección completa depende del perfil del proyecto.
- Comprobar creación → consulta → edición → eliminación en cada recurso y casos negativos `400`, `404`, `409`.
- Cualquier cambio de ruta o propiedad requiere actualizar el contrato, las pruebas y el SDD en el mismo PR.
