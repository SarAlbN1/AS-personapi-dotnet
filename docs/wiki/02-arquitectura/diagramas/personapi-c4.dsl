/*
 * PersonAPI · Modelo C4 (HU-016)
 *
 * Fuente en texto de los diagramas que el informe referencia en «Modelo C4»
 * (docs/informe/main.tex): c4-c1-contexto, c4-c2-contenedores y c4-c3-componentes, mas
 * tres recortes y una vista de despliegue. Se renderiza con render.sh o pegandolo en
 * https://structurizr.com/dsl
 *
 * Criterios de dibujo:
 *   - Solo se dibujan relaciones que son una llamada real entre componentes. Lo que el
 *     framework aplica por convencion (el filtro de antiforgery) y los tipos de datos que
 *     viajan entre capas (ServiceResult, DTOs, ViewModels, entidades) no son flechas: se
 *     dicen en la descripcion de quien los usa.
 *   - Descripciones de una linea. El detalle esta en el SDD, no en la caja.
 *   - persona_db es un contenedor del sistema, no un sistema externo (C4 y AD-01).
 *
 * Reglas que el modelo impone: un solo artefacto desplegable sobre una sola base;
 * los dos canales de un modulo llaman al mismo servicio; los servicios dependen de
 * interfaces DAO; ningun controlador alcanza PersonaDbContext.
 */
workspace "PersonAPI" "Monolito MVC + REST sobre SQL Server del Laboratorio 1 de Arquitectura de Software." {

    !identifiers hierarchical

    model {
        usuario = person "Usuario del laboratorio" "Mantiene las cuatro entidades desde el navegador." "Persona"

        clienteRest = softwareSystem "Navegador con Swagger UI, o Postman" "Consume el canal JSON para probar el contrato." "Externo"

        personapi = softwareSystem "PersonAPI" "CRUD de las cuatro entidades por dos canales sobre una sola base." {

            web = container "Aplicación PersonAPI" "Vistas Razor y API REST en el mismo proceso." "ASP.NET Core MVC 10 (.NET 10)" {

                # --- Capa 1 · Borde web ---
                home = component "HomeController" "Página inicial y vista Estado." "Back/01-Borde/Controllers"
                mvcPersonas = component "PersonasController" "Cinco acciones web de personas." "Back/01-Borde/Controllers"
                mvcProfesiones = component "ProfesionesController" "Cinco acciones web de profesiones." "Back/01-Borde/Controllers"
                mvcTelefonos = component "TelefonosController" "Cinco acciones web de teléfonos." "Back/01-Borde/Controllers"
                mvcEstudios = component "EstudiosController" "Cinco acciones web de estudios." "Back/01-Borde/Controllers"
                conv = component "ResultadoServicioVistaExtensions" "Traduce el ServiceResult a vista y código (HU-006)." "Back/01-Borde/Controllers"
                antiforgery = component "AntiforgeryEnVistasConvention" "Filtro: exige el token en los POST web." "Back/01-Borde/Controllers"

                # --- Capa 1 · Borde JSON ---
                apiPersonas = component "PersonasApiController" "Cinco operaciones de /api/personas." "Back/01-Borde/ControllersApi"
                apiProfesiones = component "ProfesionesApiController" "Cinco operaciones de /api/profesiones." "Back/01-Borde/ControllersApi"
                apiTelefonos = component "TelefonosApiController" "Cinco operaciones de /api/telefonos." "Back/01-Borde/ControllersApi"
                apiEstudios = component "EstudiosApiController" "Cinco operaciones de /api/estudios/{idProf}/{ccPer}." "Back/01-Borde/ControllersApi"
                swagger = component "Swagger / OpenAPI 3" "Documenta y ejecuta las 20 operaciones." "Swashbuckle"

                # --- Capa 2 · Aplicación ---
                svcPersona = component "PersonaService" "Cédula única, género M/F, borrado bloqueado." "IPersonaService"
                svcProfesion = component "ProfesionService" "Id único, nombre obligatorio, borrado bloqueado." "IProfesionService"
                svcTelefono = component "TelefonoService" "num único de 15, dueño existente." "ITelefonoService"
                svcEstudio = component "EstudioService" "Par (idProf, ccPer) único; no se modifica." "IEstudioService"

                # --- Capa 3 · Acceso a datos ---
                daoPersona = component "IPersonaRepository · PersonaRepository" "Persistencia de persona." "EF Core"
                daoProfesion = component "IProfesionRepository · ProfesionRepository" "Persistencia de profesión." "EF Core"
                daoTelefono = component "ITelefonoRepository · TelefonoRepository" "Persistencia de teléfono." "EF Core"
                daoEstudio = component "IEstudioRepository · EstudioRepository" "Persistencia de estudio, clave compuesta." "EF Core"

                # --- Capa 4 · Persistencia ---
                contexto = component "PersonaDbContext" "Único punto que abre conexiones a la base." "EF Core 10"

                # --- Composición (vista aparte) ---
                arranque = component "Program.cs" "Registra contexto, repositorios y servicios (SDD §4)." "ASP.NET Core"
            }

            db = container "persona_db" "Cuatro tablas; el DDL de scripts/ es el origen." "SQL Server 2025 Express" "Base de datos"
        }

        # ===== Nivel 1 =====
        usuario -> personapi "Mantiene los datos"
        clienteRest -> personapi "Prueba el contrato REST"

        # ===== Nivel 2 =====
        usuario -> personapi.web "Vistas HTML" "HTTPS"
        clienteRest -> personapi.web "/api/* y /swagger" "HTTPS/JSON"
        personapi.web -> personapi.db "Lee y escribe" "EF Core · TDS 1433"

        # ===== Nivel 3 · entrada =====
        usuario -> personapi.web.home "Abre" "HTTPS"
        usuario -> personapi.web.mvcPersonas "Usa" "HTTPS"
        usuario -> personapi.web.mvcProfesiones "Usa" "HTTPS"
        usuario -> personapi.web.mvcTelefonos "Usa" "HTTPS"
        usuario -> personapi.web.mvcEstudios "Usa" "HTTPS"

        clienteRest -> personapi.web.swagger "Explora" "HTTPS"
        clienteRest -> personapi.web.apiPersonas "Llama" "HTTPS/JSON"
        clienteRest -> personapi.web.apiProfesiones "Llama" "HTTPS/JSON"
        clienteRest -> personapi.web.apiTelefonos "Llama" "HTTPS/JSON"
        clienteRest -> personapi.web.apiEstudios "Llama" "HTTPS/JSON"

        # ===== Nivel 3 · los dos canales llaman al mismo servicio =====
        personapi.web.mvcPersonas -> personapi.web.svcPersona "Usa"
        personapi.web.apiPersonas -> personapi.web.svcPersona "Usa"
        personapi.web.mvcProfesiones -> personapi.web.svcProfesion "Usa"
        personapi.web.apiProfesiones -> personapi.web.svcProfesion "Usa"
        personapi.web.mvcTelefonos -> personapi.web.svcTelefono "Usa"
        personapi.web.apiTelefonos -> personapi.web.svcTelefono "Usa"
        personapi.web.mvcEstudios -> personapi.web.svcEstudio "Usa"
        personapi.web.apiEstudios -> personapi.web.svcEstudio "Usa"

        # Selectores: la vista pide las claves al servicio dueño.
        personapi.web.mvcTelefonos -> personapi.web.svcPersona "Selector"
        personapi.web.mvcEstudios -> personapi.web.svcPersona "Selector"
        personapi.web.mvcEstudios -> personapi.web.svcProfesion "Selector"

        # ===== Nivel 3 · servicios a interfaces DAO =====
        personapi.web.svcPersona -> personapi.web.daoPersona "Usa"
        personapi.web.svcProfesion -> personapi.web.daoProfesion "Usa"
        personapi.web.svcTelefono -> personapi.web.daoTelefono "Usa"
        personapi.web.svcEstudio -> personapi.web.daoEstudio "Usa"

        # Entre módulos, solo lo que exige una regla de SDD §7.
        personapi.web.svcPersona -> personapi.web.daoTelefono "Dependientes"
        personapi.web.svcPersona -> personapi.web.daoEstudio "Dependientes"
        personapi.web.svcProfesion -> personapi.web.daoEstudio "Dependientes"
        personapi.web.svcTelefono -> personapi.web.daoPersona "Valida dueño"
        personapi.web.svcEstudio -> personapi.web.daoPersona "Valida persona"
        personapi.web.svcEstudio -> personapi.web.daoProfesion "Valida profesión"

        # ===== Nivel 3 · datos =====
        personapi.web.daoPersona -> personapi.web.contexto "LINQ"
        personapi.web.daoProfesion -> personapi.web.contexto "LINQ"
        personapi.web.daoTelefono -> personapi.web.contexto "LINQ"
        personapi.web.daoEstudio -> personapi.web.contexto "LINQ"
        personapi.web.contexto -> personapi.db "SQL" "ADO.NET / TDS 1433"

        # ===== Borde web · convención de errores (recorte del canal MVC) =====
        personapi.web.home -> personapi.web.conv "Traduce"
        personapi.web.mvcPersonas -> personapi.web.conv "Traduce"
        personapi.web.mvcProfesiones -> personapi.web.conv "Traduce"
        personapi.web.mvcTelefonos -> personapi.web.conv "Traduce"
        personapi.web.mvcEstudios -> personapi.web.conv "Traduce"

        # ===== Composición de SDD §4 (vista aparte) =====
        personapi.web.arranque -> personapi.web.contexto "Registra scoped"
        personapi.web.arranque -> personapi.web.daoPersona "Registra"
        personapi.web.arranque -> personapi.web.daoProfesion "Registra"
        personapi.web.arranque -> personapi.web.daoTelefono "Registra"
        personapi.web.arranque -> personapi.web.daoEstudio "Registra"
        personapi.web.arranque -> personapi.web.svcPersona "Registra"
        personapi.web.arranque -> personapi.web.svcProfesion "Registra"
        personapi.web.arranque -> personapi.web.svcTelefono "Registra"
        personapi.web.arranque -> personapi.web.svcEstudio "Registra"
        personapi.web.arranque -> personapi.web.antiforgery "Registra la convención"
        personapi.web.arranque -> personapi.web.swagger "Configura"

        # ===== Despliegue declarado en el informe (docker-compose.yml) =====
        deploymentEnvironment "Docker Compose" {
            equipo = deploymentNode "Equipo del laboratorio" "docker compose up --build -d" "macOS o Windows · Docker Desktop" {
                motor = deploymentNode "Docker Compose · red personapi-net" "Tres servicios en una red interna; solo 8080 y 1433 salen al host" "docker-compose.yml" {
                    nodoApp = deploymentNode "personapi" "build: . · Dockerfile de dos etapas (sdk:10.0 compila, aspnet:10.0 ejecuta) · la cadena llega por ConnectionStrings__PersonaDb" "dotnet personapi-dotnet.dll · 8080 → http://localhost:8080 y /swagger" {
                        containerInstance personapi.web
                    }
                    nodoSql = deploymentNode "sqlserver" "MSSQL_PID=Express · contraseña de sa desde .env, no versionada" "mssql/server:2025-latest · 1433 · volumen sqldata" {
                        containerInstance personapi.db
                    }
                    init = infrastructureNode "db-init" "Ejecuta 01_ddl.sql y después 02_dml.sql con sqlcmd y termina; la aplicación no arranca hasta que acaba bien." "sqlcmd · /scripts montado solo lectura"
                }
            }

            equipo.motor.init -> equipo.motor.nodoSql "Crea persona_db y carga el DML" "TDS 1433"
        }
    }

    views {
        systemContext personapi "c4-c1-contexto" "C4 nivel 1 · Contexto: quién usa PersonAPI y por qué canal." {
            include *
            autolayout tb 450 400
        }

        container personapi "c4-c2-contenedores" "C4 nivel 2 · Contenedores: la aplicación ASP.NET Core MVC 10 como único artefacto desplegable y la base persona_db." {
            include *
            autolayout tb 450 400
        }

        component personapi.web "c4-c3-componentes" "C4 nivel 3 · Componentes del contenedor web: los dos canales de cada módulo sobre el mismo servicio, las interfaces DAO y el contexto." {
            include *
            # Fuera de esta vista: la convención de errores y el antiforgery son del canal
            # web y van en su recorte; Program.cs tiene vista propia.
            exclude personapi.web.arranque
            exclude personapi.web.conv
            exclude personapi.web.antiforgery
            autolayout tb 500 450
        }

        component personapi.web "c4-c3-componentes-canal-web" "C4 nivel 3 · Recorte del canal MVC: controladores web, la convención de errores y el antiforgery." {
            include personapi.web.home personapi.web.mvcPersonas personapi.web.mvcProfesiones personapi.web.mvcTelefonos personapi.web.mvcEstudios
            include personapi.web.conv personapi.web.antiforgery
            include personapi.web.svcPersona personapi.web.svcProfesion personapi.web.svcTelefono personapi.web.svcEstudio
            include usuario
            autolayout tb 500 450
        }

        component personapi.web "c4-c3-componentes-canal-json" "C4 nivel 3 · Recorte del canal REST: controladores de API y Swagger sobre los mismos servicios." {
            include personapi.web.apiPersonas personapi.web.apiProfesiones personapi.web.apiTelefonos personapi.web.apiEstudios
            include personapi.web.swagger
            include personapi.web.svcPersona personapi.web.svcProfesion personapi.web.svcTelefono personapi.web.svcEstudio
            include clienteRest
            autolayout tb 500 450
        }

        component personapi.web "c4-c3-componentes-arranque" "C4 nivel 3 · Recorte de la composición: qué registra Program.cs (SDD §4)." {
            include personapi.web.arranque
            include personapi.web.svcPersona personapi.web.svcProfesion personapi.web.svcTelefono personapi.web.svcEstudio
            include personapi.web.daoPersona personapi.web.daoProfesion personapi.web.daoTelefono personapi.web.daoEstudio
            include personapi.web.contexto personapi.web.antiforgery personapi.web.swagger
            autolayout tb 500 450
        }

        deployment personapi "Docker Compose" "c4-despliegue" "Despliegue con Docker Compose: personapi, sqlserver y db-init en la red personapi-net." {
            include *
            autolayout lr 450 400
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

    configuration {
        scope softwaresystem
    }
}
