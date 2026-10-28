# Back · Capa 01 · Borde HTTP

Frontera del sistema. **Único sitio que habla HTTP.** Es back: aquí no hay ni una
línea de HTML.

| Carpeta | Contenido |
|---|---|
| `Controllers/` | Controladores MVC: validan entrada, llaman al servicio y entregan un ViewModel a `Front/Views/` |
| `ControllersApi/` | Controladores REST `/api/*`: devuelven JSON y códigos de estado |
| `Dtos/` | Contrato de entrada y salida de REST, consumido por Swagger y clientes externos |

## Reglas

- Llama **solo** a `02-Aplicacion`, siempre por interfaz (`IPersonaService`, …).
- **Prohibido** inyectar o usar `PersonaDbContext`, EF Core o SQL. Regla inviolable:
  nunca acceder a `DbContext` desde un controlador.
- **Prohibido** saltar a `03-AccesoDatos` o `04-Persistencia`.
- **Prohibido** exponer entidades EF con navegaciones por HTTP. Lo que sale va en
  `Dtos/` (REST) o en `Front/ViewModels/` (Razor).
- Responsabilidad: binding, validación de **entrada** y códigos de estado. Las reglas
  de **dominio** están en `02-Aplicacion`; no duplicarlas aquí.
- MVC y REST comparten los mismos servicios. No escribir dos veces la misma regla.

## Mapeo de clases

| Entidad | Controlador MVC | Controlador REST |
|---|---|---|
| Persona | `PersonasController` | `PersonasApiController` |
| Profesion | `ProfesionesController` | `ProfesionesApiController` |
| Telefono | `TelefonosController` | `TelefonosApiController` |
| Estudio | `EstudiosController` | `EstudiosApiController` |

Los REST llevan `[ApiController]`; los MVC heredan de `Controller`.

Los REST se documentan en OpenAPI 3 con `[ProducesResponseType]` y comentarios XML; el `500` lo añade `ErroresApiConvention` (`ControllersApi/`). La convención está en [api-rest.md · Convención OpenAPI](../../../docs/wiki/03-contratos/api-rest.md#convención-openapi-hu-009).

Los MVC traducen el `ServiceResult` con `this.VistaSiFalla(...)` y quedan protegidos con
antiforgery por convención; ambas piezas están en `Controllers/` y se describen en
[Front · Convenciones compartidas](../../Front/README.md#convenciones-compartidas-hu-006).

## Por qué `Dtos/` está aquí y `ViewModels/` no

`Dtos/` es el contrato de la API: lo consumen Swagger y clientes externos, es parte
del borde HTTP. `ViewModels/` solo lo consume Razor, así que vive en
[`Front/`](../../Front/README.md).

[Back](../README.md) · [Mapa de capas](../../README.md) · siguiente: [02-Aplicacion](../02-Aplicacion/README.md)
