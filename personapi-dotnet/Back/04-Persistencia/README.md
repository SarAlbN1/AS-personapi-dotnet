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

Desde `personapi-dotnet/`, con `persona_db` creada desde [`scripts/`](../../../scripts/README.md)
y `dotnet-ef` alineado con EF Core 10:

```bash
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=True" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --context PersonaDbContext --output-dir Back/04-Persistencia/Entities \
  --namespace personapi_dotnet.Back.Persistencia.Entities --no-onconfiguring
```

La cadena usa autenticación integrada; si la instancia es otra, se cambia solo
`Server`, sin escribir contraseñas en la shell ni en el repositorio.

- `--namespace` es obligatorio: sin él, el namespace se deriva de la carpeta
  (`...Back.04-Persistencia.Entities`), no es un identificador válido y no compila.
- `--no-onconfiguring` evita que la cadena quede escrita en `PersonaDbContext`; la
  conexión la aporta `Program.cs` desde `ConnectionStrings:PersonaDb`.
- Los archivos generados no se editan a mano: un cambio de esquema se regenera con
  `--force` tras modificar el DDL.

**Ejecutado en HU-003** (EF Core 10.0.14, SQL Server 2025 Express): genera `Persona`,
`Profesion`, `Telefono`, `Estudio` y `PersonaDbContext`. `Estudio` usa la clave compuesta
`HasKey(e => new { e.IdProf, e.CcPer })`, sin identificador artificial, y las tres FK
conservan sus nombres del DDL. EF Core no genera el `CHECK` de `genero`: lo garantiza la
base.

[← 03-AccesoDatos](../03-AccesoDatos/README.md) · [Back](../README.md)
