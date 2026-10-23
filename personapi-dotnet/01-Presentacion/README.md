# Capa 01 · Presentación

Borde del sistema. Único punto que habla HTTP. Contiene el **front** (Razor y
estáticos) y la **fachada REST**.

| Carpeta | Contenido | Front/Back |
|---|---|---|
| `Controllers/` | Controladores MVC: devuelven vistas y redirects | Borde |
| `ControllersApi/` | Controladores REST `/api/*`: devuelven JSON y códigos HTTP | Borde |
| `Views/` | Razor `.cshtml`, cuatro grupos CRUD y `Shared/` | **Front** |
| `wwwroot/` | CSS, JS, imágenes servidas al navegador | **Front** |
| `ViewModels/` | Contrato de entrada/salida de Razor | Contrato |
| `Dtos/` | Contrato de entrada/salida de REST | Contrato |

## Reglas

- Llama **solo** a `02-Aplicacion`, siempre por interfaz (`IPersonaService`, …).
- **Prohibido** inyectar o usar `PersonaDbContext`, EF Core o SQL. Regla inviolable
  del proyecto: nunca acceder a `DbContext` desde un controlador.
- **Prohibido** saltar a `03-AccesoDatos` o `04-Persistencia`.
- **Prohibido** exponer entidades EF con propiedades de navegación por HTTP. Lo que
  sale va en `Dtos/` (REST) o `ViewModels/` (Razor).
- Responsabilidad: binding, validación de **entrada**, códigos de estado. Las reglas
  de **dominio** viven en `02-Aplicacion`; no duplicarlas aquí.
- MVC y REST comparten los mismos servicios. No escribir dos veces la misma regla.

## Mapeo de clases

| Entidad | Controlador MVC | Controlador REST |
|---|---|---|
| Persona | `PersonasController` | `PersonasApiController` |
| Profesion | `ProfesionesController` | `ProfesionesApiController` |
| Telefono | `TelefonosController` | `TelefonosApiController` |
| Estudio | `EstudiosController` | `EstudiosApiController` |

Los REST llevan `[ApiController]`; los MVC heredan de `Controller`.

**Ojo:** `Views/` y `wwwroot/` ya no están en la raíz del proyecto. Requieren
`WebRootPath` y `ViewLocationFormats` configurados — ver [README de capas](../README.md).

[← Mapa de capas](../README.md) · siguiente: [02-Aplicacion](../02-Aplicacion/README.md)
