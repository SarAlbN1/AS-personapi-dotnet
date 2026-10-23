# Capa 03 · Acceso a datos (DAO)

Patrón **DAO/Repository**. Única capa que ejecuta consultas. Aísla EF Core del resto
del sistema.

| Carpeta | Contenido |
|---|---|
| `Repository/Interfaces/` | Contratos DAO: `IPersonaRepository`, `IProfesionRepository`, `ITelefonoRepository`, `IEstudioRepository` |
| `Repository/Implementations/` | `PersonaRepository`, `ProfesionRepository`, `TelefonoRepository`, `EstudioRepository` con EF Core |

## Reglas

- Llama **solo** a `04-Persistencia` (`PersonaDbContext` y entidades).
- **Único sitio del repositorio donde se permite EF Core.** Ninguna otra capa lo toca.
- **Prohibido** HTTP, HTML, vistas y códigos de estado.
- **Prohibido** reglas de negocio. El repositorio lee y escribe; no decide si un
  borrado es legítimo. Esa decisión está en `02-Aplicacion`.
- `EstudioRepository` opera con **clave compuesta** `(id_prof, cc_per)`.
- Sin `ON DELETE CASCADE` en las relaciones del laboratorio.

## Por qué la interfaz importa

`02-Aplicacion` depende de `IPersonaRepository`, no de `PersonaRepository`. Eso
permite probar las reglas de negocio con dobles de prueba, sin SQL Server levantado
— es el nivel 1 de [pruebas.md](../../docs/wiki/04-implementacion/pruebas.md).

[← 02-Aplicacion](../02-Aplicacion/README.md) · [Mapa de capas](../README.md) · siguiente: [04-Persistencia](../04-Persistencia/README.md)
