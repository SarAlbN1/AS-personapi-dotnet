# scripts

Scripts SQL de `persona_db`. Ubicación fijada por el
[SDD](../docs/wiki/04-implementacion/03-SDD-LDD.md) y por la
[estructura del repositorio](../docs/wiki/04-implementacion/estructura-del-repositorio.md).

| Archivo | Estado | Contenido previsto |
|---|---|---|
| `01_ddl.sql` | **pendiente** | Crear `persona_db` y las tablas `persona`, `profesion`, `telefono`, `estudios` con PK, FK (`NO ACTION`), CHECK de `genero` e índices. PK compuesta de `estudios` = `(id_prof, cc_per)`. |
| `02_dml.sql` | **pendiente** | Datos de demostración reproducibles e idempotentes. |

Ambos scripts están **reservados a su propia issue** del backlog; no se crean aquí
para no fabricar un esquema sin revisión. El esquema autorizado está descrito en
[persistencia.md](../docs/wiki/03-contratos/persistencia.md) y en la sección 3 del SDD.

## Reglas

- Database First: el DDL es el origen del esquema. Las entidades EF Core se generan
  por scaffolding a partir de estos scripts, nunca al contrario.
- Sin migraciones Code First, sin Flyway y sin PostgreSQL.
- Sin contraseñas ni credenciales dentro de los scripts.
- Un cambio de esquema se propaga en el mismo PR: DDL → entidades/DbContext → DAO →
  servicios → DTO/API/MVC → pruebas → wiki.
