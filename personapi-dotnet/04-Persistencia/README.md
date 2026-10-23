# Capa 04 · Persistencia

Modelo de datos generado desde la base. Fondo del flujo.

| Carpeta | Contenido |
|---|---|
| `Entities/` | `Persona`, `Profesion`, `Telefono`, `Estudio` y `PersonaDbContext`, generados por scaffolding |

## Reglas

- **Database First.** El DDL de [`scripts/`](../../scripts/README.md) es el origen del
  esquema. Las entidades se **generan** a partir de la base, nunca al contrario.
- **Prohibido** migraciones Code First.
- **Prohibido** escribir lógica de servicio o de presentación en estas clases.
- **Prohibido** exponer estas entidades por HTTP: tienen propiedades de navegación.
  Lo que sale por HTTP va en `Dtos/` o `ViewModels/` de `01-Presentacion`.
- `PersonaDbContext` se registra por DI con ciclo `Scoped`.
- Un cambio de esquema se propaga en el mismo PR: DDL → entidades → DAO → servicios
  → DTO/API/MVC → pruebas → wiki.

## Comando de scaffolding

```bash
dotnet ef dbcontext scaffold "<cadena-local-sin-credenciales>" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --context PersonaDbContext --output-dir 04-Persistencia/Entities --no-onconfiguring
```

La ruta `--output-dir` cambió con la numeración de capas. El SDD documenta
`Models/Entities`; ver la nota de discrepancia en
[estructura del repositorio](../../docs/wiki/04-implementacion/estructura-del-repositorio.md).

**Sin ejecutar.** No hay base creada ni entidades generadas; corresponde a su issue.

[← 03-AccesoDatos](../03-AccesoDatos/README.md) · [Mapa de capas](../README.md)
