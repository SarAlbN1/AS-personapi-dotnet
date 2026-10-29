# Git: ramas y commits

[← Proceso](README.md)

- Rama estable `main`, sin commits directos después del bootstrap.
- Una issue = una rama corta = un PR. Formato: `<tipo>/LAB-<NN>-<slug>`.
- Tipos: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `build`.
- Ejemplos: `feat/LAB-04-api-personas`, `docs/LAB-01-arquitectura`, `test/LAB-12-e2e`.
- Commit: `<tipo>(<área>): <acción imperativa en español> [LAB-NN]`.
- Áreas: `core`, `database`, `personas`, `profesiones`, `telefonos`, `estudios`, `api`, `mvc`, `qa`, `docs`, `repo`.
- Ejemplo: `feat(personas): agrega consulta por cédula [LAB-04]`.
- Integración **solo** por PR con aprobación de otro integrante y *squash merge*. `main` está protegida: sin push directo, sin force-push y sin eliminación. La política completa está en [pull requests, revisión y versiones](pull-requests-y-releases.md#protección-de-main-y-revisión-cruzada-obligatoria). No reescribir `main` ni tocar ramas ajenas.
- Si un cambio requiere modificar contratos/arquitectura, se documenta y revisa **antes** de implementarlo.

Los identificadores `LAB-NN` son la convención de rama para las issues del backlog, hoy 17 (`HU-001` a `HU-017`): `LAB-NN` corresponde al número de historia, no al número de issue de GitHub.
