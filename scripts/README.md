# scripts

Scripts SQL de `persona_db`. Ubicación fijada por el
[SDD](../docs/wiki/04-implementacion/03-SDD-LDD.md) y por la
[estructura del repositorio](../docs/wiki/04-implementacion/estructura-del-repositorio.md).

| Archivo | Contenido |
|---|---|
| `01_ddl.sql` | Crea `persona_db` (propietario `sa`) y las tablas `persona`, `profesion`, `telefono`, `estudios` con PK, FK (`NO ACTION`), CHECK de `genero` e índices sobre las FK. PK compuesta de `estudios` = `(id_prof, cc_per)`. |
| `02_dml.sql` | Datos de demostración: tres filas por tabla; las tres personas tienen teléfono y estudio. |

## Ejecución

En orden, DDL y luego DML, desde SSMS (abrir y ejecutar cada archivo) o con `sqlcmd`
y autenticación integrada. Las comillas dobles conservan la barra invertida del nombre de
instancia en bash, PowerShell y cmd:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i scripts/01_ddl.sql
sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i scripts/02_dml.sql
```

**Reejecución.** Ambos scripts son idempotentes: el DDL crea la base y cada tabla solo
si no existen y no altera las existentes; el DML inserta cada fila solo si su clave no
existe, sin duplicar ni sobrescribir. Para volver al estado inicial exacto, o para
aplicar un cambio de esquema, se elimina la base (`DROP DATABASE persona_db;`) y se
ejecutan de nuevo `01` y `02`.

## Reglas

- Database First: el DDL es el origen del esquema. Las entidades EF Core se generan
  por scaffolding a partir de estos scripts, nunca al contrario.
- Sin migraciones Code First, sin Flyway y sin PostgreSQL.
- Sin contraseñas ni credenciales dentro de los scripts.
- Un cambio de esquema se propaga en el mismo PR: DDL → entidades/DbContext → DAO →
  servicios → DTO/API/MVC → pruebas → wiki.
