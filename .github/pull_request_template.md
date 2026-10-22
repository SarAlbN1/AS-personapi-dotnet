# Pull request · PersonAPI

## Issue

Closes #<numero-real> · LAB-NN

<!-- No usar `Closes #NN` si todavía no existe un número real de issue.
     Rama: `<tipo>/LAB-<NN>-<slug>` desde `main` actualizado.
     Commits: `<tipo>(<área>): <acción imperativa en español> [LAB-NN]`. -->

## Cambio realizado

<!-- Qué cambia y por qué, en pocas líneas. -->

-

## Contratos / arquitectura afectados

- Tablas, rutas, interfaces, capas o vistas:
- SRS/SAD/SDD/ADR actualizado cuando corresponda:

## Criterios de aceptación de la issue

<!-- Copiar los criterios de la issue y marcar solo los comprobados. -->

- [ ]
- [ ]

## Verificación

Comandos ejecutados y su resultado real (no declarar una prueba que no se ejecutó):

```text
dotnet build
dotnet test
```

<!-- Para endpoints: método, ruta, cuerpo, código de estado esperado y obtenido.
     Para vistas: acción, recorrido y resultado. Adjuntar captura o salida. -->

- [ ] `dotnet build` sin errores.
- [ ] Pruebas del módulo y casos negativos ejecutadas.
- [ ] Evidencia de operación E2E (si aplica).

## Verificación arquitectónica

- [ ] Se respetan las capas MVC → Service → DAO; ninguna capa salta a la siguiente.
- [ ] Ningún controlador (MVC o API) usa `PersonaDbContext` ni SQL directo; EF Core queda en `Models/Repository`.
- [ ] Servicios dependen de interfaces DAO, no de implementaciones.
- [ ] Rutas, verbos y códigos de estado coinciden con `docs/wiki/03-contratos/api-rest.md`.
- [ ] No se exponen entidades EF con navegaciones como contrato HTTP; se usan DTO o ViewModels.
- [ ] Integridad de datos: PK, FK y CHECK del DDL se respetan; sin borrado en cascada de personas o profesiones con dependencias; conflictos devuelven `409`.
- [ ] Database First intacto: el DDL de `scripts/` sigue siendo el origen del esquema, sin migraciones Code First y sin editar a mano el código regenerable de `Models/Entities`.
- [ ] Sin secretos en el diff: ninguna contraseña, token, certificado privado, cadena de conexión con credenciales ni archivo `.env`.
- [ ] Sin archivos generados ni dependencias fuera del alcance aprobado (ni Kafka, microservicios, API Gateway, autenticación o PostgreSQL).
- [ ] El cambio se limita al alcance de la issue; sin refactores ni archivos de otras personas.

## Documentación

- [ ] Documentos de la wiki afectados actualizados en este mismo PR, o
- [ ] No aplica: el cambio no altera requisitos, arquitectura ni contratos.

## Revisión cruzada obligatoria

Revisor solicitado: @<otro-integrante>

- [ ] Revisión solicitada a otro integrante con permiso de escritura; el autor no aprueba ni integra su propio PR.
- [ ] Revisado contra criterios de aceptación y `docs/wiki/02-arquitectura/reglas-arquitectonicas.md`, no solo estilo.
- [ ] Conversaciones de revisión resueltas; si llegaron commits nuevos tras la aprobación, se pidió otra.
- [ ] Hay una aprobación válida registrada en el PR. Que GitHub lo marque como fusionable no cuenta como aprobación.
- [ ] Integración por squash merge, borrando la rama tras integrar; `main` siempre compila.

<!-- `main` está protegida: PR obligatoria, 1 aprobación, conversaciones resueltas, historia
     lineal, sin excepciones para administradores, sin force-push ni eliminación.
     Política completa: docs/wiki/05-proceso/pull-requests-y-releases.md -->

## Resultado

-
