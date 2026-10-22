# Reglas arquitectónicas obligatorias

[← Arquitectura](README.md)

1. Una solución ASP.NET Core MVC `.NET 10` y **un artefacto desplegable**.
2. MVC y REST son interfaces HTTP separadas dentro del mismo proceso; comparten servicios.
3. Ningún controlador MVC/API accede a `PersonaDbContext` ni realiza SQL.
4. Servicios de aplicación dependen de contratos `I*Repository`, no de clases concretas de EF Core.
5. Las implementaciones DAO/Repository encapsulan el acceso mediante EF Core.
6. `DbContext` es una dependencia de persistencia, registrada por DI con ciclo de vida scoped.
7. Entidades provienen de **Database First**; cambios de esquema se realizan primero en DDL, después se regenera/ajusta el mapeo bajo control de versiones.
8. DTOs de API y ViewModels MVC evitan publicar directamente entidades EF con propiedades de navegación.
9. La base **única** `persona_db` protege PK, FK y CHECK; no usar `ON DELETE CASCADE` para las relaciones del laboratorio.
10. Persona/profesión con dependientes **no se eliminan**; se comunica `409 Conflict` en API y un error legible en MVC.
11. Estudios se identifica con **dos claves** (`id_prof`, `cc_per`) en persistencia y rutas REST.
12. No copiar lógica de negocio entre controladores; la validación de entrada y las reglas del dominio son responsabilidades diferenciadas.
13. No incluir cadenas de conexión con credenciales, secretos ni contraseñas en commits.
14. Toda tecnología o componente fuera de [versiones](../04-implementacion/versiones.md) requiere un cambio formal y aprobado del alcance en SRS y SAD **antes** de implementarlo.

## Lista de revisión arquitectónica de un PR

- ¿El cambio respeta las direcciones de dependencia?
- ¿El contrato afecta a MVC y API de forma consistente?
- ¿La FK o clave compuesta sigue protegida en SQL?
- ¿Hay pruebas de éxito, fallos y persistencia?
- ¿El comportamiento y los documentos derivados están sincronizados?

Una infracción bloquea la aprobación del PR. Si el diseño debe cambiar, se revisa primero el SAD/SDD, no se modifica en silencio.
