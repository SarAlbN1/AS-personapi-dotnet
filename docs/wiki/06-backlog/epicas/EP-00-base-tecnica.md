# EP-00 · Base técnica

[← Backlog](../README.md) · [SAD/HLD](../../02-arquitectura/02-SAD-HLD.md) · [SDD/LDD](../../04-implementacion/03-SDD-LDD.md)

**Propósito:** dejar listos los elementos compartidos por los cuatro módulos funcionales: la solución ASP.NET Core MVC, el esquema `persona_db`, las entidades y contratos DAO, y la presentación Razor común. Mientras esta épica no esté integrada, ningún CRUD puede demostrarse de extremo a extremo.

**Requisitos cubiertos:** RF-08 (datos reproducibles), RF-09 parcial (estructura entregable), RF-05/RF-06 parcial (vía convención de errores), RNF-01, RNF-02, RNF-04, RNF-05, RNF-07.

**Decisiones aplicadas:** [AD-01, AD-04, AD-05, AD-06, AD-07, AD-09, AD-10](../../02-arquitectura/02-SAD-HLD.md#2-decisiones-arquitectónicas-definitivas).

**Componentes afectados:** `personapi-dotnet.sln`, `Program.cs`, `appsettings.json`, `scripts/01_ddl.sql`, `scripts/02_dml.sql`, `Back/04-Persistencia/Entities/`, `Back/03-AccesoDatos/Repository/`, `Front/Views/Shared/`, `Front/wwwroot/`.

**Historias:** HU-001, HU-002, HU-003, HU-006 · **14 puntos**

**Criterio de éxito de la épica:** desde un clon limpio se compila la solución, se reconstruye `persona_db` con sus restricciones, un repositorio EF Core lee filas reales y el layout Razor muestra un error de validación y un conflicto. [DoR/DoD](../../05-proceso/dor-y-dod.md) aplica a las cuatro historias.

---

## HU-001 · Solución ASP.NET Core MVC .NET 10 y estructura de capas ejecutable

**Épica** EP-00 · **Sprint** 1 · **Responsable** Integrante A · **Prioridad** P0 · **Puntos** 3 · **Módulo** Base

### Objetivo
Disponer de la solución, el árbol de carpetas por capas y el arranque web con Swagger configurado, para que los demás módulos se construyan sin reorganizar el proyecto. Es la precondición física de todo el laboratorio.

### Alcance técnico
- `dotnet new sln` y `dotnet new mvc -f net10.0 --auth None` según los [comandos de referencia del SDD](../../04-implementacion/03-SDD-LDD.md#3-ddl-y-database-first); paquetes EF Core 10 y Swashbuckle.
- Árbol de carpetas de [SDD §1](../../04-implementacion/03-SDD-LDD.md#1-dependencias-y-organización-de-código) y [estructura del repositorio](../../04-implementacion/estructura-del-repositorio.md), con carpetas vacías marcadas por `.gitkeep`.
- `Program.cs`: `AddControllersWithViews`, `AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`, `MapControllers` y `MapControllerRoute`.
- **Obligatorio por la numeración de capas:** `Views/` y `wwwroot/` viven bajo
  `Back/01-Borde/` y quedan fuera de las rutas que ASP.NET Core busca por defecto.
  Sin configurarlo, las vistas fallan con 500 y los estáticos con 404. `Program.cs` debe
  fijar `WebApplicationOptions.WebRootPath = "Front/wwwroot"` y los
  `RazorViewEngineOptions.ViewLocationFormats` hacia `/Front/Views/{1}/{0}.cshtml`
  y `/Front/Views/Shared/{0}.cshtml`. Fragmentos en
  [`personapi-dotnet/README.md`](../../../../personapi-dotnet/README.md).
- `appsettings.json` con `ConnectionStrings:PersonaDb` sin credenciales; User Secrets documentado. `.gitignore` para `bin/`, `obj/`, `.vs/` y secretos.
- **Proyecto de pruebas** añadido al `.sln` (vacío). No contiene pruebas todavía: existe para que cada historia posterior añada las suyas sin inventar estructura, y para que `dotnet test` sea ejecutable desde el Sprint 1. Las pruebas de integración y E2E las construye HU-014 sobre este proyecto.
- **No incluye:** entidades, DbContext, repositorios, servicios ni vistas de módulo.

### Criterios de aceptación
1. `dotnet build` sobre `net10.0` termina sin errores desde un clon limpio.
2. La solución incluye un proyecto de pruebas registrado en el `.sln` y `dotnet test` se ejecuta sin fallos (aún sin pruebas), de modo que cada historia posterior añada las suyas sin crear estructura nueva.
3. El árbol de carpetas coincide con SDD §1 y la estructura del repositorio.
4. La aplicación arranca y `/swagger` responde con un documento OpenAPI 3 válido, aunque todavía sin operaciones.
5. `appsettings.json` versionado no contiene credenciales; la conexión se lee de `ConnectionStrings:PersonaDb`.
6. `.gitignore` excluye `bin/`, `obj/`, `.vs/` y archivos de secretos.

**Evidencia mínima:** salida de `dotnet build`, captura de `/swagger` y listado del árbol.

### Dependencias
Ninguna. Es una de las dos historias que arrancan sin bloqueo (junto con HU-002).

### Trazabilidad
RF-09 · RNF-01, RNF-04, RNF-05, RNF-07 · AD-01, AD-07, AD-09 · [SDD §1](../../04-implementacion/03-SDD-LDD.md#1-dependencias-y-organización-de-código) y [§4](../../04-implementacion/03-SDD-LDD.md#4-dependencias-en-programcs) · [reglas](../../02-arquitectura/reglas-arquitectonicas.md) 1, 6, 13 · [versiones](../../04-implementacion/versiones.md).

### Definition of Done
[DoD común](../../05-proceso/dor-y-dod.md) + compilación verde, ausencia de secretos versionados y PR revisado por otro integrante.

---

## HU-002 · Esquema `persona_db` con restricciones y datos de demostración reproducibles

**Épica** EP-00 · **Sprint** 1 · **Responsable** Integrante B · **Prioridad** P0 · **Puntos** 3 · **Módulo** Datos

### Objetivo
Entregar `scripts/01_ddl.sql` y `scripts/02_dml.sql` como origen único del esquema, para habilitar Database First y las pruebas de integridad de todo el laboratorio.

### Alcance técnico
- DDL de las cuatro tablas con los tipos de [persistencia](../../03-contratos/persistencia.md#modelo-físico) y [SDD §3](../../04-implementacion/03-SDD-LDD.md#3-ddl-y-database-first): PK `cc`, PK `id`, PK `num`, PK compuesta `(id_prof, cc_per)`, FKs con `NO ACTION` y `CHECK (genero IN ('M','F'))`.
- Conversiones MySQL→T-SQL: `ENUM`→`CHAR(1)`+CHECK, `TEXT`→`VARCHAR(MAX)`, `INT(n)`→`INT`.
- DML con datos mínimos de demostración, incluida una persona con teléfono **y** estudio para poder demostrar el `409`.
- **No incluye:** scaffolding EF Core (HU-003) ni lógica de aplicación.

### Criterios de aceptación
1. `01_ddl.sql` crea `persona_db` y las cuatro tablas con tipos, PK, FK y CHECK del contrato, incluida la PK compuesta y las FK sin cascada.
2. La ejecución del DDL sobre una instancia SQL Server 2025 Express limpia termina sin errores y el comportamiento ante reejecución queda documentado.
3. `02_dml.sql` carga datos coherentes con al menos tres filas por tabla y una persona con teléfono y estudio.
4. En SSMS fallan: `cc` duplicada, `telefono.duenio` inexistente, par `(id_prof, cc_per)` duplicado y `DELETE` de persona con dependientes.
5. Los scripts no contienen contraseñas ni cadenas de conexión con credenciales.

**Evidencia mínima:** scripts versionados, log de ejecución DDL/DML y salida de los cuatro intentos negativos.

### Dependencias
Ninguna. Se desarrolla en paralelo con HU-001 porque el esquema no depende de la solución .NET.

### Trazabilidad
RF-08, RF-05 · RNF-02, RNF-04 · AD-05, AD-06 · [persistencia](../../03-contratos/persistencia.md) · [SDD §3](../../04-implementacion/03-SDD-LDD.md#3-ddl-y-database-first) · reglas 7, 9 · AK-04.

### Definition of Done
DoD común + esquema verificado contra SQL Server real y las cuatro pruebas negativas registradas.

---

## HU-003 · EF Core Database First, DbContext, contratos DAO e inyección de dependencias

**Épica** EP-00 · **Sprint** 1 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 5 · **Módulo** Datos

### Objetivo
Generar las entidades y el contexto desde el esquema existente y publicar los contratos DAO con sus implementaciones EF Core, para que los cuatro módulos trabajen en paralelo sobre la misma persistencia sin tocar archivos centrales entre sí.

### Alcance técnico
- `dotnet ef dbcontext scaffold` con `--context PersonaDbContext --output-dir Back/04-Persistencia/Entities --no-onconfiguring`, según [SDD §3](../../04-implementacion/03-SDD-LDD.md#3-ddl-y-database-first).
- Cuatro interfaces `I*Repository` con las firmas asíncronas de [SDD §2](../../04-implementacion/03-SDD-LDD.md#2-contratos-de-aplicación-y-dao); la de estudios usa `(int idProf, int ccPer)`.
- Cuatro implementaciones EF Core en `Back/03-AccesoDatos/Repository/Implementations` sin reglas de negocio: solo acceso a datos.
- Registro DI de `PersonaDbContext` y de los cuatro repositorios con ciclo `scoped`.
- **Acuerdo de archivos compartidos:** esta historia es la única que registra repositorios en `Program.cs`; cada historia de API añade después **solo** la línea DI de su propio servicio. Así se cumple el criterio de no editar simultáneamente los mismos archivos centrales.
- **No incluye:** servicios de dominio, DTOs ni controladores.

### Criterios de aceptación
1. El comando de scaffold queda documentado y reproducible; las entidades y `PersonaDbContext` corresponden al DDL, con clave compuesta real en `estudios` y sin identificador artificial (AK-03).
2. Existen las cuatro interfaces DAO con las firmas de SDD §2 y la clave compuesta para estudios.
3. Las cuatro implementaciones residen en `Back/03-AccesoDatos/Repository/Implementations` y son el único lugar que referencia `PersonaDbContext`.
4. `PersonaDbContext` y los cuatro repositorios quedan registrados como `scoped` y la solución compila.
5. Una comprobación de integración lee filas reales de `persona_db` a través de cada repositorio.

**Evidencia mínima:** log de scaffold, salida de `dotnet build`, resultado de la lectura por repositorio y evidencia de que `Back/01-Borde/Controllers/` no referencia `DbContext`.

### Dependencias
**HU-001** (proyecto donde generar el modelo) y **HU-002** (esquema desde el cual generarlo). Ambas son bloqueos técnicos reales de Database First.

### Trazabilidad
RNF-01, RNF-02 · DR-02, DR-05 · AD-04, AD-05 · [SDD §2](../../04-implementacion/03-SDD-LDD.md#2-contratos-de-aplicación-y-dao), §3, §4 · reglas 3, 4, 5, 6, 7, 11 · AK-01, AK-03.

### Definition of Done
DoD común + persistencia verificada contra SQL Server real y revisión explícita de direcciones de dependencia.

---

## HU-006 · Capa de presentación MVC compartida: layout, navegación y errores visibles

**Épica** EP-00 · **Sprint** 1 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 3 · **Módulo** Base

### Objetivo
Entregar el layout, la navegación y la convención de mensajes de error de las vistas Razor, para que los cuatro CRUD web se construyan sin duplicar presentación ni inventar cada uno su manejo de errores.

### Alcance técnico
- `Front/Views/Shared/_Layout.cshtml`, `_ValidationScriptsPartial`, navegación a los cuatro módulos y página inicial del monolito.
- Partial o convención reutilizable para traducir los resultados `ValidationError`, `NotFound` y `Conflict` de los servicios ([SDD §2](../../04-implementacion/03-SDD-LDD.md#2-contratos-de-aplicación-y-dao)) a mensajes Razor visibles, conservando el estado del formulario, conforme a [validaciones y errores](../../03-contratos/validaciones-y-errores.md).
- Convención de antiforgery en formularios POST y base de ViewModels.
- **No incluye:** vistas de personas, profesiones, teléfonos ni estudios.

### Criterios de aceptación
1. `_Layout.cshtml` ofrece navegación a Personas, Profesiones, Teléfonos y Estudios, y la página inicial responde en el navegador; los enlaces de módulos aún sin controlador quedan documentados como pendientes.
2. Existe y está implementada una convención reutilizable para mostrar `ValidationError`, `NotFound` y `Conflict` conservando el estado del formulario.
3. Los formularios POST usan validación antiforgery por convención documentada.
4. Ni el layout ni las vistas compartidas contienen reglas de negocio, SQL ni referencias a `PersonaDbContext`.
5. Una vista de ejemplo demuestra el render de un error de validación y de un conflicto.

**Evidencia mínima:** capturas del layout y del render de error de validación y de conflicto.

### Dependencias
**HU-001**. No depende de HU-002 ni HU-003: la demostración de errores puede hacerse con un resultado simulado en la vista de ejemplo.

### Trazabilidad
RF-05, RF-06 · RNF-01 · AD-02, AD-10 · [validaciones y errores](../../03-contratos/validaciones-y-errores.md) · [SDD §6](../../04-implementacion/03-SDD-LDD.md#6-vistas-mvc-y-rutas) · reglas 2, 8, 12 · [criterio de éxito 5](../../01-producto/criterios-de-exito.md).

### Definition of Done
DoD común + flujo comprobado en navegador y convención documentada en el PR.
