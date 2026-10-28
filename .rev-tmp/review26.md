Revisión de HU-011 sobre `96136135`, con verificación ejecutada contra `persona_db` real.

## Verificación ejecutada

SQL Server 2025 Express en contenedor, esquema y datos desde `scripts/`. Cadena por `ConnectionStrings__PersonaDb`.

```text
dotnet build -> 0 advertencias, 0 errores
dotnet test  -> 42 passed, 0 failed, 0 skipped
```

Las cinco operaciones, por HTTP contra la base:

```text
GET    /api/estudios                                  200   las tres filas del DML
GET    /api/estudios/1/1010101010                     200
GET    /api/estudios/3/1010101010 (inexistente)       404
POST   {"idProf":3,"ccPer":1010101010,"fecha":"2020-05-10","univer":"  Nacional  "}
                                                      201   -> univer "Nacional"
POST   (par repetido)                                 409
POST   (idProf 999)                                   400   «No existe una profesión con id 999.»
POST   (ccPer 999)                                    400   «No existe una persona con cédula 999.»
POST   (sin ccPer)                                    400   errors.CcPer
POST   (fecha "2020-02-30")                           400   no enlaza el DateOnly
PUT    /api/estudios/3/1010101010 con par distinto    400   «el par identificador no se puede cambiar»
PUT    /api/estudios/3/1010101010                     204
GET    tras el PUT                                    200   fecha y univer actualizadas
PUT    /api/estudios/2/999 (inexistente)              404
DELETE /api/estudios/3/1010101010                     204
DELETE (repetido)                                     404
```

## Bien

- La clave compuesta viaja como `{idProf}/{ccPer}` con restricción `:int` en los dos segmentos, sin id artificial, como fija el SDD.
- Referencia inexistente → `400` y par duplicado → `409`: es la distinción que pide `api-rest.md`, y las FK `NO ACTION` quedan como defensa final.
- `ServiceResult`, `ProblemDetails` y el recorte de `univer` siguen el mismo criterio que personas, profesiones y teléfonos; `univer` solo de espacios se guarda como `NULL`.
- `DateOnly?` deja que `[ApiController]` rechace una fecha imposible antes del servicio.

## Lo único pendiente

La rama está desactualizada y **en conflicto con `main`**: ya entraron #23 (personas), #22 (profesiones) y #27 (presentación MVC), y chocan `README.md`, `docs/wiki/03-contratos/api-rest.md` y `personapi-dotnet/Program.cs` (la línea de registro del servicio). Actualízala con `main` y resuelve esos tres, cuidando que la línea de estado del README y la de `api-rest.md` sumen estudios sin borrar personas ni profesiones.

No apruebo todavía solo por eso: en cuanto la rama esté al día la apruebo y la fusiono, el contenido ya quedó verificado.
