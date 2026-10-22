# SRS | Especificación de requisitos de software

**Sistema:** `personapi-dotnet`  
**Laboratorio:** Implementación de monolito con MVC y DAO

> **Versión documental:** 2.0 | **Fecha:** 20-10-2026 | **Estado:** diseño aprobado como línea base de implementación, aún no implementado.  
> **Origen:** guía y diapositiva del Laboratorio 1; las versiones técnicas se actualizan por decisión del equipo.

## 1. Propósito y objetivo
Implementar una aplicación monolítica que permita realizar CRUD de **persona, profesion, telefono y estudios** mediante **vistas web MVC (Razor)** y **API REST**, documentada y comprobable desde **Swagger UI**, sobre SQL Server. La persistencia debe emplear Entity Framework Core (Database First) y el acceso a datos debe encapsularse mediante interfaces e implementaciones DAO/Repository.

## 2. Alcance confirmado
**Incluye:** CRUD web y REST de cuatro entidades; base de datos `persona_db`; SQL DDL y DML; validaciones; patrón MVC; patrón DAO; Swagger/OpenAPI; repositorio público `personapi-dotnet`; README, código, tag Git y documento de entrega con portada, marco conceptual, diseño, procedimiento, conclusiones/lecciones y referencias.

**No incluye:** autenticación, perfiles, aplicación móvil, microservicios, nube, colas ni integración con servicios externos. **Docker no es necesario para el alcance base** y queda excluido de esta línea base para evitar infraestructura adicional no solicitada. El despliegue se hará como proceso ASP.NET Core local con SQL Server Express local.

## 3. Línea base técnica cerrada
| ID | Decisión | Valor adoptado |
|---|---|---|
| DT-01 | Plataforma | .NET 10 LTS / ASP.NET Core MVC 10 |
| DT-02 | ORM | Entity Framework Core 10, proveedor SQL Server y herramientas de diseño |
| DT-03 | SGBD | Microsoft SQL Server 2025 Express |
| DT-04 | Herramientas | Visual Studio 2026 Community y SQL Server Management Studio 22 |
| DT-05 | API | Controladores REST ASP.NET Core, JSON, Swagger UI con OpenAPI 3 |
| DT-06 | Enfoque de datos | Database First (`Scaffold-DbContext`) |
| DT-07 | Arquitectura | Un solo proceso y proyecto web, separación lógica MVC → servicios → DAO → EF Core |
| DT-08 | Seguridad demo | Sin autenticación según guía; acceso restringido a desarrollo local, sin secretos en Git |
| DT-09 | Entrega | Repositorio público `personapi-dotnet`, tag `v1.0.0` al aprobar pruebas |

Las versiones .NET 6/7 y SQL Server 2019/2022 de la guía se reemplazan expresamente por las versiones estables vigentes. **Esto es una adaptación del equipo, no una afirmación de que la guía exigía esas versiones.** No quedan decisiones tecnológicas abiertas dentro de este diseño.

## 4. Actores
| Actor | Necesidad |
|---|---|
| Usuario de la interfaz | Crear, consultar, modificar y eliminar registros desde Razor. |
| Consumidor REST / evaluador | Ejecutar CRUD vía Swagger o cliente HTTP. |
| Equipo de desarrollo | Configurar entorno, reproducir la base, compilar, verificar y entregar el tag. |

## 5. Modelo de datos
| Entidad | Campos | Llaves y relaciones |
|---|---|---|
| `persona` | `cc INT`, `nombre VARCHAR(45)`, `apellido VARCHAR(45)`, `genero CHAR(1)`, `edad INT NULL` | PK `cc`; `genero` restringido a `M` o `F` según modelo original. |
| `profesion` | `id INT`, `nom VARCHAR(90)`, `des VARCHAR(MAX) NULL` | PK `id`. |
| `telefono` | `num VARCHAR(15)`, `oper VARCHAR(45)`, `duenio INT` | PK `num`; FK `duenio` → `persona.cc`. |
| `estudios` | `id_prof INT`, `cc_per INT`, `fecha DATE NULL`, `univer VARCHAR(50) NULL` | PK compuesta (`id_prof`, `cc_per`); FKs a `profesion.id` y `persona.cc`. |

**Conversión SQL:** el diagrama original usa tipos MySQL (`ENUM`, `TEXT`, `INT(n)`); el DDL de SQL Server emplea `CHAR(1)` + `CHECK`, `VARCHAR(MAX)` e `INT`. Las FKs tienen eliminación `NO ACTION`, sin borrado en cascada. Se mantiene el modelo académico, sin agregar nuevos campos.

## 6. Requisitos funcionales
| ID | Requisito | Aceptación verificable |
|---|---|---|
| RF-01 | CRUD personas en MVC y API | Listar, consultar por `cc`, crear, editar, eliminar; cambios persistidos. |
| RF-02 | CRUD profesiones en MVC y API | Cinco operaciones por ambos canales; cambios persistidos. |
| RF-03 | CRUD teléfonos en MVC y API | Cinco operaciones; `duenio` debe existir. |
| RF-04 | CRUD estudios en MVC y API | Cinco operaciones; identifica por (`id_prof`, `cc_per`) y exige ambas FKs existentes. |
| RF-05 | Integridad referencial | No borrar persona/profesión con dependencias; REST devuelve `409`, web muestra explicación. |
| RF-06 | Validaciones de entrada | Rechazar identificadores duplicados, formatos inválidos, FK inexistente y claves de ruta discordantes. |
| RF-07 | API autodescriptiva | 20 operaciones (5 por recurso) publicadas y ejecutables desde Swagger UI con OpenAPI 3. |
| RF-08 | Datos reproducibles | DDL crea base y tablas; DML agrega datos de prueba válidos. |
| RF-09 | Entrega Git | README reproducible, código, scripts, informe académico y URL del tag de entrega. |

## 7. Requisitos no funcionales
| ID | Atributo | Criterio comprobable |
|---|---|---|
| RNF-01 | Mantenibilidad | Controladores sin acceso directo a `DbContext`; reglas de negocio compartidas en servicios; repositorios implementan interfaces. |
| RNF-02 | Correctitud | Las relaciones están protegidas por PK, FK, CHECK y validación del lado servidor. |
| RNF-03 | Interoperabilidad | REST usa JSON, rutas convencionales, métodos HTTP y códigos 200/201/204/400/404/409. |
| RNF-04 | Reproducibilidad | Desde un clon y SQL Server 2025 Express, el README permite ejecutar DDL/DML, restaurar dependencias y arrancar. |
| RNF-05 | Seguridad mínima | No hay credenciales de base en commits; configuración local fuera del control de versiones. |
| RNF-06 | Verificabilidad | Casos positivos y negativos de los cuatro recursos probados en MVC y REST con registros reales. |
| RNF-07 | Compatibilidad | Componentes alineados con .NET 10 y EF Core 10; no combinar versiones mayores incompatibles. |

## 8. Drivers arquitectónicos
| ID | Driver | Requisitos que satisface | Impacto de diseño |
|---|---|---|---|
| DR-01 | CRUD por dos canales | RF-01 a RF-04 | MVC y API comparten servicios. |
| DR-02 | Uso explícito de MVC y DAO | RNF-01 | Capas lógicas e inversión de dependencias mediante interfaces. |
| DR-03 | Monolito requerido | Objetivo | Unidad única de ejecución y despliegue. |
| DR-04 | Relaciones de datos y clave compuesta | RF-03 a RF-06 | SQL Server con restricciones y servicios con validaciones. |
| DR-05 | Partir de esquema existente | RF-08 | EF Core Database First. |
| DR-06 | Evaluación de API | RF-07 | Swagger UI + contratos REST estables. |
| DR-07 | Entrega académica reproducible | RF-09, RNF-04 | Scripts, README y tag Git. |

## 9. Architecture killers (condiciones que invalidan la solución)
Un *killer* no es una preferencia: es una falla que impide cumplir el objetivo del laboratorio.

| ID | Killer | Prevención / prueba |
|---|---|---|
| AK-01 | Controlador MVC/API consulta `DbContext` directamente | Revisar dependencias y pruebas de arquitectura. |
| AK-02 | Solo hay vistas o solo hay API | Verificar ambos canales para los cuatro módulos. |
| AK-03 | `estudios` modelado con ID ficticio y no con PK compuesta | Validar rutas, mapeo EF y operaciones CRUD por dos llaves. |
| AK-04 | SQL permite datos huérfanos o borrado cascada no solicitado | Verificar FKs y DELETE conflictivo. |
| AK-05 | Swagger no ofrece los 20 endpoints | Inspección interactiva de OpenAPI. |
| AK-06 | Clon limpio no compila o no conecta con la base | Prueba de instalación sobre entorno nuevo. |
| AK-07 | Secretos en repositorio público | Verificación antes de cada merge y antes del tag. |

## 10. Escenarios mínimos de aceptación
1. Crear persona por MVC y verla mediante GET REST.
2. Crear profesión por REST y verla mediante MVC.
3. Asociar teléfono a persona y consultar la relación.
4. Vincular persona/profesión por estudio, consultar por PK compuesta.
5. Intentar eliminar una persona con registros asociados: `409` en REST y error visible en MVC.
6. Eliminar primero dependientes y luego la persona: operación exitosa.
7. Clonar repositorio, reconstruir base, compilar, abrir Swagger y comprobar los 20 endpoints.

## 11. Trazabilidad de entrega
El SRS define **qué** se necesita; el SAD describe la **arquitectura HLD**; el SDD concreta el **diseño LDD**. El backlog se construirá después a partir de estas tres fuentes; no forma parte de esta entrega.

## 12. Referencias
- Guía y diapositiva del Laboratorio 1, suministradas por el usuario.
- .NET Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy
- SQL Server 2025: https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2025
- Visual Studio 2026 compatibility: https://learn.microsoft.com/en-us/visualstudio/releases/2026/compatibility
