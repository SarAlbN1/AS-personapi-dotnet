# API REST — contrato del laboratorio

[← Contratos](README.md) · [SDD/LDD](../04-implementacion/03-SDD-LDD.md)

**Responsabilidad:** dentro del monolito ASP.NET Core, los controladores `Back/01-Borde/ControllersApi` atienden JSON y delegan siempre en `I*Service`. Ruta base `/api`; la aplicación también expone vistas Razor por MVC. OpenAPI 3 + Swagger UI describe y permite probar las operaciones.

**Estado de implementación:** las cinco operaciones de personas (HU-004), las cinco de profesiones (HU-005), las cinco de teléfonos (HU-010) y las cinco de estudios (HU-011) están implementadas y verificadas contra `persona_db`: las veinte del contrato. En estudios, la clave compuesta viaja en los dos segmentos de la ruta, `{idProf}/{ccPer}`, y el `PUT` solo cambia `fecha` y `univer`. En teléfonos, `num` viaja como cadena, también en la ruta y sin restricción de ruta `:int`, para no perder los ceros iniciales; su contenido se valida como dígitos, porque un número con otros caracteres crea una fila que la ruta ya no puede alcanzar. `num` es también la excepción al recorte de espacios: se exige exacto en la ruta y en el cuerpo, porque la ruta no se recorta y la clave tiene que coincidir en los dos sitios. Los errores viajan como `ProblemDetails`, la forma que ya produce `[ApiController]` para un cuerpo inválido, de modo que la API no añade un estándar de error propio.

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

## Convención OpenAPI (HU-009)

El documento OpenAPI 3 se genera a partir del código: los códigos salen de los `[ProducesResponseType]`, y los resúmenes, las descripciones y los ejemplos, de los comentarios XML (`GenerateDocumentationFile`). Los cuatro recursos la cumplen. Un controlador nuevo queda documentado sin rediseño si cumple estos seis puntos.

1. **Clase.** Lleva `[ApiController]`, `[Route("api/<recurso>")]` y `[Produces("application/json")]`. La clave de la ruta lleva su restricción de tipo (`{cc:int}`); una clave de texto, como `{num}`, no la lleva.
2. **Códigos.** Cada acción declara con `[ProducesResponseType]` al menos los códigos de esta tabla. Los errores usan `ProblemDetails`, salvo el 400, que usa `ValidationProblemDetails`: es la forma que devuelve `[ApiController]` cuando el cuerpo no valida, y cubre también el 400 del servicio.

   | Operación | Códigos mínimos |
   |---|---|
   | `GET` listado | `200` |
   | `GET` detalle | `200`, `404` |
   | `POST` | `201`, `400`, `409` |
   | `PUT` | `204`, `400`, `404` |
   | `DELETE` | `204`, `404`, y `409` si el recurso tiene dependientes (personas, profesiones) |

3. **El 500 no se declara en cada acción.** Lo añade `ErroresApiConvention` a todo `[ApiController]`. Fuera de Development, una excepción no controlada en `/api` responde `500` con un `ProblemDetails` sin traza, sin SQL y sin la cadena de conexión (`UseExceptionHandler` + `AddProblemDetails` en `Program.cs`). En Development se ve la página de diagnóstico de ASP.NET Core, que solo existe en local.
4. **Comentarios XML en la acción:**
   - un `<summary>` que diga qué hace;
   - un `<param>` por clave y por cuerpo;
   - un `<response code="...">` por cada código declarado, que explique **cuándo** ocurre (no solo «Not Found»).
5. **Ejemplos en los DTO.** Cada propiedad de los DTO de entrada y salida lleva `<example>`, con valores coherentes con `scripts/02_dml.sql`. Así Swagger UI rellena el cuerpo de prueba.
6. **Mensajes de error.** Los arma el controlador con `Problem(detail: resultado.Message, …)` a partir del `ServiceResult`. Nunca se envía el mensaje de una excepción.
7. **Solo `/api` en el documento.** `VistasFueraDeOpenApiConvention` (`Back/01-Borde/Controllers/`) oculta al explorador de API todo controlador sin `[ApiController]`. Sin ella, las acciones MVC con ruta por atributo (estudios y teléfonos llevan su clave en la URL) salían en Swagger como operaciones del contrato.

**Comprobación automática.** `tests/Unit/personapi-dotnet.Tests.Unit/OpenApi/DocumentoOpenApiTests.cs` lee `/swagger/v1/swagger.json`:
- **Exactamente 20 operaciones, todas de `/api`:** prueba `El_documento_publica_solo_las_veinte_operaciones_de_la_api`.
- **Códigos mínimos, `500` y esquema `ProblemDetails`:** los exige a **toda** operación de `/api`.
- **Resúmenes, descripciones propias y ejemplos:** los exige a los recursos listados en `RecursosDocumentados` y a los esquemas de la prueba `Los_esquemas_de_entrada_y_salida_traen_ejemplo_en_cada_propiedad`. Al documentar un recurso nuevo se añade allí.
