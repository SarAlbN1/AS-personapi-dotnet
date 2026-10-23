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
