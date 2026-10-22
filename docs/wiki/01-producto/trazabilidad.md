# Trazabilidad de diseño

[← Producto](README.md)

| Requisito SRS | Decisión SAD | Contrato/implementación SDD | Verificación |
|---|---|---|---|
| RF-01 Personas MVC/REST | AD-02/03/04 | PersonasController, PersonasApiController, PersonaService, PersonaRepository | CRUD web + 5 endpoints |
| RF-02 Profesiones MVC/REST | AD-02/03/04 | ProfesionesController, ProfesionesApiController, ProfesionService, ProfesionRepository | CRUD web + 5 endpoints |
| RF-03 Teléfonos MVC/REST | AD-03/05/06 | TelefonoService, TelefonoRepository, FK `duenio` | FK existente, CRUD, errores |
| RF-04 Estudios MVC/REST | AD-03/05/06 | EstudioService, clave compuesta, dos FK | CRUD con `{idProf}/{ccPer}` |
| RF-05/06 Integridad y validación | AD-06/10 | DTO/ViewModel, validación y 400/404/409 | pruebas negativas y consistencia |
| RF-07 Swagger | AD-07 | OpenAPI 3 + Swagger UI | 20 operaciones visibles y ejecutables |
| RF-08 Datos reproducibles | AD-05 | `scripts/01_ddl.sql`, `02_dml.sql` | base construida desde cero |
| RF-09 Entrega | AD-01/08/09 | README, Git tag, informe | clon, ejecución y tag |

**Regla:** al crear una issue en GitHub Projects, enlazar al menos un `RF-*`, la decisión `AD-*` pertinente y un escenario de aceptación. El estado de la issue se gestiona exclusivamente en GitHub Projects.
