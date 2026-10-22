# Pull request · PersonAPI

Closes #

## Resumen de cambios

<!-- Qué cambia y por qué, en pocas líneas. Capas tocadas: MVC / Service / DAO / SQL / docs. -->

## Criterios de aceptación de la issue

<!-- Copiar los criterios de la issue y marcar solo los comprobados. -->

- [ ]
- [ ]

## Pruebas realizadas

Comandos ejecutados y su resultado real (no declarar una prueba que no se ejecutó):

```text
dotnet build
dotnet test
```

<!-- Para endpoints: método, ruta, cuerpo, código de estado esperado y obtenido.
     Para vistas: acción, recorrido y resultado. Adjuntar captura o salida. -->

- [ ] `dotnet build` sin errores.
- [ ] Pruebas aplicables ejecutadas y en verde.
- [ ] Recorrido manual o E2E realizado cuando la issue lo exige.

## Verificación arquitectónica

- [ ] Se respetan las capas MVC → Service → DAO; ninguna capa salta a la siguiente.
- [ ] Ningún controlador (MVC o API) usa `PersonaDbContext` ni SQL directo; EF Core queda en `Models/Repository`.
- [ ] Servicios dependen de interfaces DAO, no de implementaciones.
- [ ] Rutas, verbos y códigos de estado coinciden con `docs/wiki/03-contratos/api-rest.md`.
- [ ] No se exponen entidades EF con navegaciones como contrato HTTP; se usan DTO o ViewModels.
- [ ] Integridad de datos: PK, FK y CHECK del DDL se respetan; sin borrado en cascada de personas o profesiones con dependencias; conflictos devuelven `409`.
- [ ] Database First intacto: sin migraciones Code First y sin editar a mano el código regenerable de `Models/Entities`.
- [ ] Sin secretos, cadenas con contraseñas ni archivos `.env` en el diff.
- [ ] El cambio se limita al alcance de la issue; sin refactores ni archivos ajenos.
- [ ] Sin Kafka, microservicios, API Gateway, autenticación ni PostgreSQL (fuera del alcance aprobado).

## Documentación

- [ ] Documentos de la wiki afectados actualizados en este mismo PR, o
- [ ] No aplica: el cambio no altera requisitos, arquitectura ni contratos.

## Revisión

- [ ] Revisión solicitada a otro integrante; el autor no aprueba su propio PR.
- [ ] Integración por squash.
