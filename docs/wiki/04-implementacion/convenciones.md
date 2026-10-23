# Convenciones de implementación

[← Implementación](README.md) · [Reglas arquitectónicas](../02-arquitectura/reglas-arquitectonicas.md)

## C# y capas

- Clases y métodos en `PascalCase`, variables y parámetros `camelCase`, campos privados `_camelCase`, interfaces prefijadas `I`.
- Asincronía para operaciones de BD: `Task`, `async`/`await`, sufijo `Async`; evitar `.Result` o `.Wait()`.
- Una clase por responsabilidad, inyección de dependencias en constructor y registros `Scoped` para servicios/DAO/DbContext.
- Controladores MVC y API llaman a los mismos `I*Service`. Solo los DAO conocen `PersonaDbContext`.
- DTO o ViewModel para entradas y salidas, evitando referencias circulares y exposición accidental de propiedades de navegación.
- Entidades de `Back/04-Persistencia/Entities` provienen de Database First; no editar manualmente código regenerable para implementar reglas de negocio.

## HTTP y MVC

- REST en `/api/{recurso}` con GET, POST, PUT, DELETE. Estándar de respuestas en [api-rest.md](../03-contratos/api-rest.md).
- Formulario Razor GET para mostrar y POST para guardar. Validar `ModelState`, proteger formularios de modificación con antiforgery en MVC.
- Las vistas relacionadas (teléfonos, estudios) usan desplegables de personas/profesiones en vez de pedir manualmente FK.
- Nunca incorporar SQL ni `DbContext` dentro de controladores o vistas.

## Datos, seguridad y observabilidad

- SQL Server es fuente del estado; usar restricciones PK/FK/CHECK como garantía de integridad.
- Configuración sensible por variables de entorno o secretos locales; no subir contraseñas a Git.
- Mostrar mensajes de error entendibles y registrar diagnóstico sin secretos ni trazas completas en HTTP.
- Mantener formato uniforme de errores API, sin inventar endpoints ajenos al contrato.

## Cambio coordinado

Un cambio del esquema requiere actualizar DDL → entidades/DbContext → DAO → servicios → DTO/API/MVC → pruebas → wiki. Un cambio en rutas implica actualizar Swagger/contratos y pruebas en el mismo PR.

## Alcance técnico

Lo que este taller implementa: un proyecto web ASP.NET Core MVC 10 con vistas Razor y controladores API, servicios de aplicación, DAO sobre EF Core 10 y una base SQL Server `persona_db` generada desde el DDL, documentada con Swagger/OpenAPI 3. Las capas se comunican por invocación directa dentro del mismo proceso. El stack está cerrado en [versiones](versiones.md); incorporar otra tecnología exige cambio aprobado del SRS y del SAD.
