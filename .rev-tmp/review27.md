Revisión de HU-006 sobre `35c8bd31`, con verificación ejecutada.

## Verificación ejecutada

macOS, .NET 10, SQL Server 2025 Express en contenedor con `scripts/01_ddl.sql` y `scripts/02_dml.sql`. La rama está detrás de `main`, así que la fusioné en local con `main` (`43741fb`, ya con personas y profesiones) antes de probar, sin subir nada:

```text
merge main -> sin conflictos
dotnet build -> 0 advertencias, 0 errores
dotnet test  -> 56 passed, 0 failed, 0 skipped
```

Por HTTP, con la aplicación corriendo contra `persona_db`:

```text
GET  /                      200   layout con navegación a Personas, Profesiones, Teléfonos, Estudios y /swagger
GET  /Home/Convenciones     200
GET  /Personas              404   página «No encontrado» renderizada, no la pantalla cruda de Kestrel
GET  /Home/NoExiste         404   misma página de estado
POST /Home/Convenciones con token y escenario Conflicto
                            200   alert-warning con data-mensaje="conflicto" y el mensaje del ServiceResult
POST /Home/Convenciones sin token
                            400   la convención de antiforgery bloquea el envío
GET  /api/personas          200   la API sigue respondiendo con el pipeline nuevo
```

## Observaciones

- La traducción de `ServiceResult` a vista (`ResultadoServicioVistaExtensions`, `_MensajesServicio.cshtml`) deja a los CRUD de HU-007, HU-008, HU-012 y HU-013 solo el `switch` de su propio flujo; es la plantilla que necesitábamos y la voy a usar tal cual en HU-007.
- Los enlaces de navegación a `/Personas`, `/Profesiones`, `/Telefonos` y `/Estudios` responden 404 hoy, pero con la página de estado de esta misma historia, no con un error crudo. Es el comportamiento esperado hasta que entren los CRUD.
- `Privacy.cshtml` se elimina junto con su enlace, así que no queda una ruta muerta en el menú.
- La exención de antiforgery en `Estado` y `Error` está justificada en el código: la re-ejecución conserva el verbo y ambas acciones solo muestran información.

Sin observaciones bloqueantes. Apruebo. Aprobación registrada por asistente a petición mía, según la regla 11 de la política de integración.
