# Front

Lo que termina en el navegador. Renderizado **en el servidor** con Razor, dentro del
mismo ejecutable que el back. **No es una aplicación aparte:** no hay `npm`, no hay
SPA, no hay segundo despliegue.

| Carpeta | Contenido |
|---|---|
| `Views/` | Razor `.cshtml`. Cuatro grupos CRUD (`Personas`, `Profesiones`, `Telefonos`, `Estudios`) y `Shared/` con el layout |
| `wwwroot/` | CSS, JS e imágenes servidos al navegador |
| `ViewModels/` | Contrato que consume Razor: lo que el controlador entrega a la vista |

## Reglas

- Las vistas **reciben** un ViewModel ya resuelto. No llaman a servicios, no llaman a
  repositorios y no consultan la base.
- **Prohibido** `PersonaDbContext`, EF Core y SQL. Regla inviolable del proyecto.
- **Prohibido** consumir `/api/*` por HTTP desde las vistas. El ViewModel llega en
  memoria desde `Back/01-Borde/Controllers/`. `/api/*` es para Swagger y clientes REST.
- **Prohibido** recibir entidades EF con propiedades de navegación. Lo que llega a la
  vista va en `ViewModels/`.
- Los mensajes de error visibles cumplen
  [validaciones y errores](../../docs/wiki/03-contratos/validaciones-y-errores.md).

## Convenciones compartidas (HU-006)

Los cuatro CRUD web usan lo mismo y no inventan cada uno su presentación.

**Layout y navegación.** `Views/Shared/_Layout.cshtml` enlaza Personas, Profesiones,
Teléfonos y Estudios por ruta (`/Personas`, …) y Swagger. Mientras un módulo no tenga
controlador, su enlace responde con la vista `Estado` (404):

| Módulo | Historia que lo implementa |
|---|---|
| Personas | HU-007 |
| Profesiones | HU-008 |
| Teléfonos | HU-012 |
| Estudios | HU-013 |

**Personas es la referencia de los otros tres.** `Back/01-Borde/Controllers/PersonasController.cs`
consume el mismo `IPersonaService` que el controlador REST, los ViewModels
(`PersonaFormViewModel`, `PersonaDetalleViewModel`, `PersonaListadoViewModel`) llevan solo
las anotaciones de forma, y `Views/Personas/_CamposPersona.cshtml` es el formulario que
comparten `Create` y `Edit`, con la clave bloqueada en edición.

**Errores del servicio.** El controlador traduce el `ServiceResult` con
`this.VistaSiFalla(resultado, modelo)` (`Back/01-Borde/Controllers/ResultadoServicioVistaExtensions.cs`)
y la vista incluye `<partial name="_MensajesServicio" />` encima del formulario:

| Resultado | Respuesta | Qué ve el usuario |
|---|---|---|
| `ValidationError` | misma vista, 400 | el formulario tal como lo envió y el mensaje en un recuadro rojo |
| `Conflict` | misma vista, 409 | el formulario tal como lo envió y un aviso amarillo |
| `NotFound` | vista `Estado`, 404 | «No encontrado» con el mensaje del servicio |
| `Success` | redirección (POST-Redirect-GET) | el destino muestra un aviso verde una sola vez |

```csharp
[HttpPost]
public async Task<IActionResult> Create(PersonaFormViewModel modelo)
{
    if (!ModelState.IsValid) return View(modelo);     // forma: anotaciones del ViewModel
    var resultado = await _personas.CreateAsync(...); // dominio: el servicio decide
    return this.VistaSiFalla(resultado, modelo)
        ?? this.RedirigirConExito("Persona creada.", nameof(Index));
}
```

Los errores de cada campo se pintan con `asp-validation-for` junto al campo; el partial
solo muestra los de nivel modelo, el conflicto y el éxito. Para un formulario de
borrado, `modelo` es el ViewModel de la confirmación, y el 409 se muestra sobre ella.

En `Edit`, la clave de la ruta lleva `[FromRoute]` (`Edit([FromRoute] int id, … modelo)`):
el formulario también envía `Id` y el binding lo leería antes que la ruta, así que un id
manipulado editaría otro registro en vez de llegar al servicio como discrepancia (400).
`ProfesionesController` (HU-008) es el primer CRUD que aplica la convención completa.

**Antiforgery.** Todo POST, PUT, PATCH o DELETE de un controlador MVC exige el token por
convención (`AntiforgeryEnVistasConvention`, registrada en `Program.cs`). El tag helper
`<form method="post">` lo inserta solo: **no** hace falta `[ValidateAntiForgeryToken]` ni
`@Html.AntiForgeryToken()`. Un POST sin token responde 400 con la vista `Estado`. Los
controladores `[ApiController]` quedan fuera.

**ViewModels.** Uno por formulario, en `ViewModels/`, con las anotaciones de **forma**
(obligatorio, longitud) y los mensajes en español. Las reglas de **dominio** (claves
duplicadas, dependencias, referencias) las decide el servicio y llegan por
`VistaSiFalla`; no se repiten en el ViewModel.

**Clave compuesta (estudios).** `EstudiosController` (HU-013) lleva las dos mitades de la
clave en la ruta de `Details`, `Edit` y `Delete`, con rutas por atributo:
`/Estudios/Edit/{idProf}/{ccPer}`. Las vistas las generan con `asp-route-idProf` y
`asp-route-ccPer`. Los selectores de persona y profesión se llenan con `IPersonaService` e
`IProfesionService`, y el servicio de estudios vuelve a comprobar las dos referencias si el
formulario llega manipulado. En `Edit`, el par de la ruta lleva `[FromRoute]` y, al repintar,
el formulario vuelve a tomarlo de la ruta y lo quita de `ModelState`: un par manipulado da
400 y el siguiente «Guardar» sigue apuntando al estudio original.

**Clave de texto (teléfonos).** `TelefonosController` (HU-012) lleva el número en la ruta
como cadena y sin restricción de tipo (`/Telefonos/Edit/{num}`), para no perder los ceros
iniciales. El selector de dueño se llena con `IPersonaService`, y el servicio de teléfonos
vuelve a comprobar que el dueño existe si el formulario llega manipulado. El número no se
recorta, así que su forma (obligatorio, hasta 15 dígitos, solo dígitos) se valida en el
ViewModel y se informa junto al campo; el número repetido llega del servicio como conflicto
(409). En `Edit` se aplica lo mismo que en estudios: `[FromRoute]` y, al repintar, el número
de la ruta.

**Ejemplo.** `/Home/Convenciones` recorre los cuatro resultados con un resultado de
servicio **simulado**. Es la referencia visual de la convención, no un módulo del dominio.

## Quién lo alimenta

```text
Back/01-Borde/Controllers/PersonasController.cs
        │  ViewModel en memoria
        ▼
Front/Views/Personas/Index.cshtml  →  HTML  →  navegador
```

## Por qué los Controllers no están aquí

No contienen HTML. Orquestan: validan entrada, llaman al servicio y entregan el
ViewModel. Esa es lógica de borde, y vive en
[`Back/01-Borde/`](../Back/01-Borde/README.md).

**Ojo:** `Views/` y `wwwroot/` no están en la raíz del proyecto. Requieren
`WebRootPath` y `ViewLocationFormats` configurados — ver [mapa de capas](../README.md).

[← Mapa de capas](../README.md) · [Back](../Back/README.md)
