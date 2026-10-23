# Back · Capa 02 · Aplicación

Reglas de negocio y casos de uso. El cerebro compartido: **MVC y REST entran los dos
por aquí**.

| Carpeta | Contenido |
|---|---|
| `Services/Interfaces/` | `IPersonaService`, `IProfesionService`, `ITelefonoService`, `IEstudioService` |
| `Services/Implementations/` | `PersonaService`, `ProfesionService`, `TelefonoService`, `EstudioService` |

## Reglas

- Llama **solo** a `03-AccesoDatos`, y siempre por interfaz DAO
  (`IPersonaRepository`, …). Nunca a una implementación concreta.
- **Prohibido** EF Core, `DbContext`, LINQ a base de datos y SQL. Eso vive en `03`.
- **Prohibido** cualquier dependencia de Razor, `HttpContext`, `IActionResult` o tipos
  de ASP.NET. Esta capa no sabe que existe HTTP.
- Aquí viven las reglas del laboratorio: unicidad de claves, verificación de FK y el
  **bloqueo de borrado** de persona o profesión con dependientes. El servicio decide
  el conflicto; `01-Borde` lo traduce a `409 Conflict` o a error legible en la vista.
- Registro por DI con ciclo `Scoped`.

## Por qué existe esta capa

Si la regla "no borrar persona con teléfonos" estuviera en los controladores habría
que escribirla dos veces, una en MVC y otra en REST, y podrían divergir. Al vivir aquí
se escribe una vez y los dos canales la respetan.

[← 01-Borde](../01-Borde/README.md) · [Back](../README.md) · siguiente: [03-AccesoDatos](../03-AccesoDatos/README.md)
