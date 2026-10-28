Re-revisión de `e80d66b2`, con verificación ejecutada contra `persona_db` real.

## Verificación ejecutada

SQL Server 2025 Express en contenedor, esquema y datos desde `scripts/01_ddl.sql` y `scripts/02_dml.sql`. Cadena por `ConnectionStrings__PersonaDb`.

```text
dotnet build  -> 0 advertencias, 0 errores
dotnet test   -> 45 passed, 0 failed, 0 skipped  (las pruebas con [FactConPersonaDb] corrieron contra la base)
```

Los cinco endpoints, por HTTP contra la base:

```text
GET    /api/profesiones                          200   las tres filas del DML
GET    /api/profesiones/2                        200
GET    /api/profesiones/99                       404
POST   {"id":9101,"nom":"  Revisor  ","des":"   "} 201   -> {"id":9101,"nom":"Revisor","des":null}
POST   (id repetido)                             409
POST   (nom solo espacios)                       400
POST   (sin id)                                  400
PUT    /api/profesiones/9101 {"id":9999,...}     400   ruta != cuerpo
PUT    /api/profesiones/9101                     204
PUT    /api/profesiones/9444 (inexistente)       404
GET    /api/profesiones/9101                     200   {"nom":"Revisor 2","des":"desc"}
DELETE /api/profesiones/1                        409   «tiene 2 estudio(s) asociado(s)»
DELETE /api/profesiones/9101                     204
DELETE /api/profesiones/9101                     404
```

El recorte quedó confirmado en la respuesta: `nom` con espacios se guarda recortado y `des` solo de espacios se guarda como `NULL`.

## El cambio que pedí quedó atendido

- `ProfesionDto` declara `public const int NomLongitudMaxima = 90` y lo usa en su propio `[StringLength]`; `ProfesionService.ValidarNombre` consume `ProfesionDto.NomLongitudMaxima`. Ya no hay import de `...Services.Implementations` en el borde, así que el DTO deja de depender de una clase concreta (`4485aee8`).
- La coordinación del resultado discriminado se cerró por el otro lado: #23 adoptó `ServiceResult` y eliminó `ResultadoServicio`, así que no quedan dos tipos compitiendo en el mismo namespace.

## Observaciones de @ma-sarmiento

- Recorte de `nom` y `des`: hecho en `ff32eb5d` y `44206940`, con el mismo criterio que personas y teléfonos.
- Mensaje con `id` nulo: ya no se construye; `CreateAsync` y `UpdateAsync` cortan antes con «El id es obligatorio.».
- Pérdida de caracteres fuera de CP1252 por la colación de la instancia: no bloquea, es una propiedad del DDL de HU-002 y afecta a los cuatro módulos por igual. Si se quiere corregir, va como issue propia sobre el esquema, no en esta historia.

Retiro mi solicitud de cambios y apruebo. Aprobación registrada por asistente a petición mía, según la regla 11 de la política de integración.
