# Back · Capa 04 · Persistencia

Modelo de datos generado desde la base. Fondo del flujo.

| Carpeta | Contenido |
|---|---|
| `Entities/` | `Persona`, `Profesion`, `Telefono`, `Estudio` y `PersonaDbContext`, generados por scaffolding |

## Reglas

- **Database First.** El DDL de [`scripts/`](../../../scripts/README.md) es el origen
  del esquema. Las entidades se **generan** desde la base, nunca al contrario.
- **Prohibido** migraciones Code First.
- **Prohibido** escribir lógica de servicio o de presentación en estas clases.
- **Prohibido** exponer estas entidades por HTTP: tienen propiedades de navegación. Lo
  que sale va en `Back/01-Borde/Dtos/` o en `Front/ViewModels/`.
- `PersonaDbContext` se registra por DI con ciclo `Scoped`.
- Un cambio de esquema se propaga en el mismo PR: DDL → entidades → DAO → servicios →
  DTO/API/MVC → pruebas → wiki.

## Comando de scaffolding

```bash
dotnet ef dbcontext scaffold "<cadena-local-sin-credenciales>" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --context PersonaDbContext --output-dir Back/04-Persistencia/Entities --no-onconfiguring
```

**Sin ejecutar.** No hay base creada ni entidades generadas; corresponde a su issue.

[← 03-AccesoDatos](../03-AccesoDatos/README.md) · [Back](../README.md)
