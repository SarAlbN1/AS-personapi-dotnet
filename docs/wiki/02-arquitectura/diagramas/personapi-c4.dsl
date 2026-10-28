/*
 * PersonAPI · Modelo C4 (HU-016)
 *
 * Fuente en texto de los tres diagramas que el informe académico referencia en
 * «Modelo C4» (docs/informe/main.tex): c4-c1-contexto, c4-c2-contenedores y
 * c4-c3-componentes. Se renderiza en https://structurizr.com/dsl (playground) o
 * con la CLI de Structurizr; ver README.md de esta carpeta.
 *
 * Reglas que cumple este modelo:
 *   - Un solo artefacto desplegable y una sola base persona_db (AD-01, regla 1).
 *   - MVC y REST comparten servicios; no se duplica lógica (AD-02, regla 2).
 *   - Los servicios dependen de interfaces DAO (AD-03/AD-04, reglas 3 y 4).
 *   - EF Core y PersonaDbContext solo viven en las implementaciones de repositorio:
 *     ningún controlador tiene una flecha hacia el contexto (AK-01, regla 3).
 *   - No se dibuja ningún elemento que el SAD, el SDD o el informe no declaren, ni
 *     tecnología fuera de 04-implementacion/versiones.md.
 *
 * Estado del código: la etiqueta "Pendiente" marca lo que el diseño declara y
 * todavía no está en main; su historia aparece en la descripción. Verificado contra
 * main 7d144fb (28-10-2026).
 */
workspace "PersonAPI" "Monolito MVC + REST sobre SQL Server del Laboratorio 1 de Arquitectura de Software." {

    !identifiers hierarchical

    model {
        usuario = person "Usuario del laboratorio" "Consulta y mantiene personas, profesiones, teléfonos y estudios desde el navegador." "Persona"

        clienteRest = softwareSystem "Cliente REST" "Swagger UI y Postman: consumen el canal JSON para probar y evaluar el contrato." "Externo"

        personapi = softwareSystem "PersonAPI" "Gestiona las cuatro entidades del dominio por dos canales —vistas Razor y API REST— sobre una sola base de datos." {

            web = container "Aplicación PersonAPI" "Único artefacto desplegable: sirve las vistas Razor y la API REST desde el mismo proceso, con Swagger para documentar el contrato." "ASP.NET Core MVC 10 (.NET 10)" {

                mvc = component "Controladores MVC" "Traducen la petición del navegador a llamadas al servicio y devuelven vistas. Implementados: Home y la convención compartida de errores; personas y profesiones en PR abierto, teléfonos y estudios en HU-012 y HU-013." "Back/01-Borde/Controllers"
                vistas = component "Vistas Razor y ViewModels" "Renderizado en el servidor. Reciben un ViewModel ya resuelto: no consultan servicios ni la base, y no reciben entidades EF." "Front/Views, Front/ViewModels"
                api = component "Controladores API REST" "Exponen /api/* y traducen el resultado del servicio a códigos 200, 201, 204, 400, 404 y 409. Implementados: personas y profesiones; teléfonos y estudios en PR abierto." "Back/01-Borde/ControllersApi"
                dtos = component "DTOs" "Contrato de entrada y salida del canal JSON: planos, sin navegaciones EF." "Back/01-Borde/Dtos"
                swagger = component "Swagger / OpenAPI 3" "Documenta y permite ejecutar las operaciones REST desde /swagger." "Swashbuckle"

                servicios = component "Servicios de dominio" "Reglas de SDD §7: clave no duplicada, referencia existente, borrado bloqueado con dependencias. Devuelven resultados discriminados (ServiceResult) que el borde traduce a HTTP o a mensaje visible. Implementados: persona y profesión." "Back/02-Aplicacion/Services"

                daoInterfaces = component "Interfaces DAO" "IPersonaRepository, IProfesionRepository, ITelefonoRepository, IEstudioRepository: el contrato de persistencia del que dependen los servicios." "Back/03-AccesoDatos/Repository/Interfaces"
                daoEf = component "Repositorios EF Core" "Las cuatro implementaciones de las interfaces DAO. Único lugar del sistema que referencia PersonaDbContext y ejecuta consultas." "Back/03-AccesoDatos/Repository/Implementations"

                contexto = component "PersonaDbContext" "Contexto generado por scaffolding desde el esquema existente (Database First)." "EF Core 10"
                entidades = component "Entidades" "Persona, Profesion, Telefono y Estudio, con la clave compuesta (id_prof, cc_per) de estudios." "Back/04-Persistencia/Entities"
            }

            db = container "persona_db" "Cuatro tablas con sus llaves primarias, foráneas y restricciones; origen del esquema según el DDL versionado en scripts/." "SQL Server 2025 Express" "Base de datos"
        }

        # --- Nivel 1 y 2: quién usa qué -------------------------------------------
        usuario -> personapi "Consulta y mantiene los datos del dominio"
        clienteRest -> personapi "Prueba y documenta el contrato REST"

        usuario -> personapi.web "Usa las vistas HTML" "HTTPS"
        clienteRest -> personapi.web "Consume /api/* y /swagger" "HTTPS/JSON"
        personapi.web -> personapi.db "Lee y escribe con EF Core" "ADO.NET / TDS 1433"

        # --- Nivel 3: dependencias dentro del contenedor web ----------------------
        usuario -> personapi.web.mvc "Envía formularios y navega" "HTTPS"
        clienteRest -> personapi.web.api "Llama a las operaciones REST" "HTTPS/JSON"
        clienteRest -> personapi.web.swagger "Explora el contrato" "HTTPS"

        personapi.web.swagger -> personapi.web.api "Describe y ejecuta sus operaciones"
        personapi.web.mvc -> personapi.web.vistas "Renderiza con un ViewModel"
        personapi.web.api -> personapi.web.dtos "Recibe y devuelve"

        personapi.web.mvc -> personapi.web.servicios "Invoca las reglas de negocio"
        personapi.web.api -> personapi.web.servicios "Invoca las mismas reglas de negocio"

        personapi.web.servicios -> personapi.web.daoInterfaces "Depende del contrato de persistencia"
        personapi.web.daoEf -> personapi.web.daoInterfaces "Implementa"
        personapi.web.daoEf -> personapi.web.contexto "Consulta y persiste con LINQ"
        personapi.web.contexto -> personapi.web.entidades "Materializa y rastrea"
        personapi.web.contexto -> personapi.db "Ejecuta SQL" "ADO.NET / TDS 1433"

        # --- Despliegue declarado en el informe (docker-compose.yml) --------------
        deploymentEnvironment "Docker Compose" {
            equipo = deploymentNode "Equipo del laboratorio" "Donde se ejecuta la demostración" "macOS o Windows con Docker Desktop" {
                motor = deploymentNode "Docker Engine · red personapi-net" "Los tres servicios comparten una red interna" "Docker Compose v2" {
                    nodoApp = deploymentNode "personapi" "Publica el puerto 8080 del host" "mcr.microsoft.com/dotnet/aspnet:10.0" {
                        containerInstance personapi.web
                    }
                    nodoSql = deploymentNode "sqlserver" "Publica el puerto 1433; los datos viven en el volumen sqldata" "mcr.microsoft.com/mssql/server:2025-latest" {
                        containerInstance personapi.db
                    }
                    init = infrastructureNode "db-init" "Ejecuta 01_ddl.sql y después 02_dml.sql con sqlcmd y termina; la aplicación arranca cuando este servicio acaba bien." "mssql-tools18"
                }
            }

            init -> nodoSql "Crea el esquema y carga los datos de demostración" "TDS 1433"
        }
    }

    views {
        systemContext personapi "c4-c1-contexto" "C4 nivel 1 · Contexto: el usuario, el monolito PersonAPI y SQL Server persona_db como único almacén." {
            include *
            autolayout tb
        }

        container personapi "c4-c2-contenedores" "C4 nivel 2 · Contenedores: la aplicación ASP.NET Core MVC 10 como único artefacto desplegable, con sus dos canales (vistas Razor y API REST), y la base persona_db." {
            include *
            autolayout tb
        }

        component personapi.web "c4-c3-componentes" "C4 nivel 3 · Componentes del contenedor web: controladores MVC y API, servicios, interfaces DAO e implementaciones EF Core. Ningún controlador alcanza el contexto de datos." {
            include *
            autolayout tb
        }

        deployment personapi "Docker Compose" "c4-despliegue" "Vista de despliegue con Docker Compose: personapi, sqlserver y db-init en la red personapi-net." {
            include *
            autolayout lr
        }

        styles {
            element "Person" {
                shape person
                background #0b5394
                color #ffffff
            }
            element "Externo" {
                background #7f8c8d
                color #ffffff
            }
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "Container" {
                background #438dd5
                color #ffffff
            }
            element "Base de datos" {
                shape cylinder
                background #2e6da4
                color #ffffff
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            element "Infrastructure Node" {
                background #ffffff
                color #000000
            }
        }
    }
}
