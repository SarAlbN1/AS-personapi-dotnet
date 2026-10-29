/*
 * PersonAPI · Modelo C4 (HU-016)
 *
 * Fuente en texto de los diagramas que el informe académico referencia en «Modelo C4»
 * (docs/informe/main.tex): c4-c1-contexto, c4-c2-contenedores y c4-c3-componentes, más
 * una vista de despliegue y dos recortes por canal. Se renderiza en
 * https://structurizr.com/dsl (playground) o con la CLI; ver README.md de esta carpeta.
 *
 * ALCANCE: la arquitectura del sistema, con los cuatro módulos del dominio en los dos
 * canales, tal como la deciden el SRS, el SAD, el SDD y el informe. Los diagramas
 * describen el diseño y no marcan el avance: el estado de cada historia vive en GitHub
 * Projects, y la correspondencia entre cada elemento y su código o su historia está en el
 * README de esta carpeta.
 *
 * Reglas que el modelo impone:
 *   - Un solo artefacto desplegable y una sola base persona_db (AD-01, regla 1).
 *   - MVC y REST comparten el servicio de cada entidad: los dos controladores de un
 *     módulo apuntan al mismo servicio (AD-02, reglas 2 y 12).
 *   - Los servicios dependen de interfaces DAO, nunca de implementaciones (AD-03, AD-04).
 *   - EF Core y PersonaDbContext solo viven en las implementaciones de repositorio:
 *     ningún controlador tiene una flecha hacia el contexto (AK-01, regla 3).
 *   - Las dependencias entre módulos son las que exigen las reglas de SDD §7: borrado
 *     bloqueado y validación de referencias, no acoplamiento accidental.
 *   - Nada fuera de 04-implementacion/versiones.md.
 */
workspace "PersonAPI" "Monolito MVC + REST sobre SQL Server del Laboratorio 1 de Arquitectura de Software." {

    !identifiers hierarchical

    model {
        usuario = person "Usuario del laboratorio" "Consulta y mantiene personas, profesiones, teléfonos y estudios desde el navegador." "Persona"

        clienteRest = softwareSystem "Navegador con Swagger UI, o Postman" "Consume el canal JSON para probar y evaluar el contrato. Swagger UI se sirve desde la propia aplicación; aquí representa a quien la usa." "Externo"

        personapi = softwareSystem "PersonAPI" "Gestiona las cuatro entidades del dominio por dos canales —vistas Razor y API REST— sobre una sola base de datos." {

            web = container "Aplicación PersonAPI" "Único artefacto desplegable: sirve las vistas Razor y la API REST desde el mismo proceso, con Swagger para documentar el contrato." "ASP.NET Core MVC 10 (.NET 10)" {

                # ---- Capa 1 · Borde: canal web ------------------------------------
                home = component "HomeController y convención de errores" "Página inicial, vista Estado y la convención compartida que traduce ValidationError, Conflict y NotFound a lo que ve el usuario, sin repetirla en cada módulo." "Back/01-Borde/Controllers"
                mvcPersonas = component "PersonasController" "Index, Details, Create, Edit y Delete de personas en Razor." "Back/01-Borde/Controllers"
                mvcProfesiones = component "ProfesionesController" "Las cinco acciones web de profesiones." "Back/01-Borde/Controllers"
                mvcTelefonos = component "TelefonosController" "Las cinco acciones web de teléfonos, con selector de persona dueña." "Back/01-Borde/Controllers"
                mvcEstudios = component "EstudiosController" "Las cinco acciones web de estudios, con selectores de persona y profesión y la clave compuesta en la ruta." "Back/01-Borde/Controllers"
                vistas = component "Vistas Razor y ViewModels" "Layout compartido y los cuatro grupos de vistas (Personas, Profesiones, Telefonos, Estudios). Reciben un ViewModel ya resuelto: no consultan servicios ni la base y no reciben entidades EF." "Front/Views, Front/ViewModels"

                # ---- Capa 1 · Borde: canal JSON -----------------------------------
                apiPersonas = component "PersonasApiController" "Las cinco operaciones de /api/personas." "Back/01-Borde/ControllersApi"
                apiProfesiones = component "ProfesionesApiController" "Las cinco operaciones de /api/profesiones." "Back/01-Borde/ControllersApi"
                apiTelefonos = component "TelefonosApiController" "Las cinco operaciones de /api/telefonos, con num como cadena para conservar los ceros iniciales." "Back/01-Borde/ControllersApi"
                apiEstudios = component "EstudiosApiController" "Las cinco operaciones de /api/estudios/{idProf}/{ccPer}, identificadas por la clave compuesta." "Back/01-Borde/ControllersApi"
                dtos = component "DTOs" "Contrato de entrada y salida del canal JSON: planos, sin navegaciones EF (AD-10, regla 8)." "Back/01-Borde/Dtos"
                swagger = component "Swagger / OpenAPI 3" "Documenta y ejecuta las 20 operaciones REST desde /swagger." "Swashbuckle"

                # ---- Capa 2 · Aplicación: un servicio por entidad -----------------
                svcPersona = component "PersonaService" "Reglas de persona: cédula única, género M/F, edad no negativa y borrado bloqueado si tiene teléfonos o estudios." "IPersonaService"
                svcProfesion = component "ProfesionService" "Reglas de profesión: id único, nombre obligatorio y borrado bloqueado si tiene estudios." "IProfesionService"
                svcTelefono = component "TelefonoService" "Reglas de teléfono: num único de hasta 15 caracteres y persona dueña existente." "ITelefonoService"
                svcEstudio = component "EstudioService" "Reglas de estudio: par (idProf, ccPer) único y existente en ambas tablas; el par identificador no se modifica." "IEstudioService"

                # ---- Capa 3 · Acceso a datos --------------------------------------
                daoPersona = component "IPersonaRepository · PersonaRepository" "Contrato de persistencia de persona y su implementación EF Core." "Back/03-AccesoDatos/Repository"
                daoProfesion = component "IProfesionRepository · ProfesionRepository" "Contrato de persistencia de profesión y su implementación EF Core." "Back/03-AccesoDatos/Repository"
                daoTelefono = component "ITelefonoRepository · TelefonoRepository" "Contrato de persistencia de teléfono y su implementación EF Core." "Back/03-AccesoDatos/Repository"
                daoEstudio = component "IEstudioRepository · EstudioRepository" "Contrato de persistencia de estudio, con clave (idProf, ccPer), y su implementación EF Core." "Back/03-AccesoDatos/Repository"

                # ---- Capa 4 · Persistencia ----------------------------------------
                contexto = component "PersonaDbContext" "Contexto generado por scaffolding desde el esquema existente (Database First). Único punto del proceso que abre conexiones a la base; solo lo referencian las implementaciones de repositorio." "EF Core 10"
                entidades = component "Entidades" "Persona, Profesion, Telefono y Estudio, con la clave compuesta (id_prof, cc_per) de estudios y sin identificador artificial." "Back/04-Persistencia/Entities"
            }

        }

        db = softwareSystem "persona_db" "SQL Server 2025 Express. Única base del sistema: cuatro tablas con sus llaves primarias, foráneas y restricciones CHECK; el DDL de scripts/ es el origen del esquema." "Base de datos"

        # ===== Nivel 1 ==========================================================
        usuario -> personapi "Consulta y mantiene los datos del dominio"
        clienteRest -> personapi "Prueba y documenta el contrato REST"
        personapi -> db "Lee y escribe" "ADO.NET / TDS 1433"

        # ===== Nivel 2 ==========================================================
        usuario -> personapi.web "Usa las vistas HTML" "HTTPS"
        clienteRest -> personapi.web "Consume /api/* y /swagger" "HTTPS/JSON"
        personapi.web -> db "Lee y escribe con EF Core" "ADO.NET / TDS 1433"

        # ===== Nivel 3 · entrada ================================================
        usuario -> personapi.web.home "Abre la página inicial" "HTTPS"
        usuario -> personapi.web.mvcPersonas "Navega y envía formularios" "HTTPS"
        usuario -> personapi.web.mvcProfesiones "Navega y envía formularios" "HTTPS"
        usuario -> personapi.web.mvcTelefonos "Navega y envía formularios" "HTTPS"
        usuario -> personapi.web.mvcEstudios "Navega y envía formularios" "HTTPS"

        clienteRest -> personapi.web.swagger "Explora y ejecuta el contrato" "HTTPS"
        clienteRest -> personapi.web.apiPersonas "Llama a /api/personas" "HTTPS/JSON"
        clienteRest -> personapi.web.apiProfesiones "Llama a /api/profesiones" "HTTPS/JSON"
        clienteRest -> personapi.web.apiTelefonos "Llama a /api/telefonos" "HTTPS/JSON"
        clienteRest -> personapi.web.apiEstudios "Llama a /api/estudios" "HTTPS/JSON"

        # ===== Nivel 3 · borde a presentación y contrato ========================
        personapi.web.home -> personapi.web.vistas "Renderiza"
        personapi.web.mvcPersonas -> personapi.web.vistas "Renderiza con un ViewModel"
        personapi.web.mvcProfesiones -> personapi.web.vistas "Renderiza con un ViewModel"
        personapi.web.mvcTelefonos -> personapi.web.vistas "Renderiza con un ViewModel"
        personapi.web.mvcEstudios -> personapi.web.vistas "Renderiza con un ViewModel"

        personapi.web.apiPersonas -> personapi.web.dtos "Recibe y devuelve"
        personapi.web.apiProfesiones -> personapi.web.dtos "Recibe y devuelve"
        personapi.web.apiTelefonos -> personapi.web.dtos "Recibe y devuelve"
        personapi.web.apiEstudios -> personapi.web.dtos "Recibe y devuelve"
        personapi.web.swagger -> personapi.web.dtos "Publica sus esquemas"

        # ===== Nivel 3 · los dos canales comparten el mismo servicio ============
        personapi.web.mvcPersonas -> personapi.web.svcPersona "Invoca las reglas de negocio"
        personapi.web.apiPersonas -> personapi.web.svcPersona "Invoca las mismas reglas"
        personapi.web.mvcProfesiones -> personapi.web.svcProfesion "Invoca las reglas de negocio"
        personapi.web.apiProfesiones -> personapi.web.svcProfesion "Invoca las mismas reglas"
        personapi.web.mvcTelefonos -> personapi.web.svcTelefono "Invoca las reglas de negocio"
        personapi.web.apiTelefonos -> personapi.web.svcTelefono "Invoca las mismas reglas"
        personapi.web.mvcEstudios -> personapi.web.svcEstudio "Invoca las reglas de negocio"
        personapi.web.apiEstudios -> personapi.web.svcEstudio "Invoca las mismas reglas"

        # Selectores de las vistas: la vista no inventa claves, las pide al servicio dueño.
        personapi.web.mvcTelefonos -> personapi.web.svcPersona "Puebla el selector de persona dueña"
        personapi.web.mvcEstudios -> personapi.web.svcPersona "Puebla el selector de persona"
        personapi.web.mvcEstudios -> personapi.web.svcProfesion "Puebla el selector de profesión"

        # ===== Nivel 3 · servicios a interfaces DAO =============================
        personapi.web.svcPersona -> personapi.web.daoPersona "Depende del contrato de persistencia"
        personapi.web.svcProfesion -> personapi.web.daoProfesion "Depende del contrato de persistencia"
        personapi.web.svcTelefono -> personapi.web.daoTelefono "Depende del contrato de persistencia"
        personapi.web.svcEstudio -> personapi.web.daoEstudio "Depende del contrato de persistencia"

        # Dependencias entre módulos exigidas por las reglas de SDD §7.
        personapi.web.svcPersona -> personapi.web.daoTelefono "Comprueba dependientes antes de borrar"
        personapi.web.svcPersona -> personapi.web.daoEstudio "Comprueba dependientes antes de borrar"
        personapi.web.svcProfesion -> personapi.web.daoEstudio "Comprueba dependientes antes de borrar"
        personapi.web.svcTelefono -> personapi.web.daoPersona "Valida que la persona dueña exista"
        personapi.web.svcEstudio -> personapi.web.daoPersona "Valida que la persona exista"
        personapi.web.svcEstudio -> personapi.web.daoProfesion "Valida que la profesión exista"

        # ===== Nivel 3 · acceso a datos a persistencia ==========================
        personapi.web.daoPersona -> personapi.web.contexto "Consulta y persiste con LINQ"
        personapi.web.daoProfesion -> personapi.web.contexto "Consulta y persiste con LINQ"
        personapi.web.daoTelefono -> personapi.web.contexto "Consulta y persiste con LINQ"
        personapi.web.daoEstudio -> personapi.web.contexto "Consulta y persiste con LINQ"
        personapi.web.contexto -> personapi.web.entidades "Materializa y rastrea"
        personapi.web.contexto -> db "Ejecuta SQL" "ADO.NET / TDS 1433"

        # ===== Despliegue declarado en el informe (docker-compose.yml) ==========
        deploymentEnvironment "Docker Compose" {
            equipo = deploymentNode "Equipo del laboratorio" "Donde se ejecuta la demostración" "macOS o Windows con Docker Desktop" {
                motor = deploymentNode "Docker Engine · red personapi-net" "Los tres servicios comparten una red interna" "Docker Compose" {
                    nodoApp = deploymentNode "personapi" "Publica el puerto 8080 del host" "mcr.microsoft.com/dotnet/aspnet:10.0 · puerto 8080" {
                        containerInstance personapi.web
                    }
                    nodoSql = deploymentNode "sqlserver" "Los datos viven en el volumen sqldata" "mcr.microsoft.com/mssql/server:2025-latest · puerto 1433" {
                        softwareSystemInstance db
                    }
                    init = infrastructureNode "db-init" "Ejecuta 01_ddl.sql y después 02_dml.sql con el sqlcmd de /opt/mssql-tools18 y termina; la aplicación arranca cuando este servicio acaba bien." "mcr.microsoft.com/mssql/server:2025-latest"
                }
            }

            equipo.motor.init -> equipo.motor.nodoSql "Crea el esquema y carga los datos de demostración" "TDS 1433"
        }
    }

    views {
        systemContext personapi "c4-c1-contexto" "C4 nivel 1 · Contexto: el usuario, el cliente REST, el monolito PersonAPI y SQL Server persona_db como único almacén." {
            include *
            autolayout tb
        }

        container personapi "c4-c2-contenedores" "C4 nivel 2 · Contenedores: la aplicación ASP.NET Core MVC 10 como único artefacto desplegable, con sus dos canales (vistas Razor y API REST), y la base persona_db." {
            include *
            autolayout tb
        }

        component personapi.web "c4-c3-componentes" "C4 nivel 3 · Componentes del contenedor web: los cuatro módulos en los dos canales, con un servicio por entidad, las interfaces DAO con su implementación EF Core, el contexto y las entidades. Ningún controlador alcanza el contexto de datos." {
            include *
            autolayout tb 300 300
        }

        component personapi.web "c4-c3-componentes-canal-web" "C4 nivel 3 · Recorte del canal MVC: controladores web, vistas y ViewModels, y la cadena de servicios y DAO que consumen." {
            include personapi.web.home personapi.web.mvcPersonas personapi.web.mvcProfesiones personapi.web.mvcTelefonos personapi.web.mvcEstudios
            include personapi.web.vistas
            include personapi.web.svcPersona personapi.web.svcProfesion personapi.web.svcTelefono personapi.web.svcEstudio
            include personapi.web.daoPersona personapi.web.daoProfesion personapi.web.daoTelefono personapi.web.daoEstudio
            include personapi.web.contexto personapi.web.entidades
            include usuario db
            autolayout tb
        }

        component personapi.web "c4-c3-componentes-canal-json" "C4 nivel 3 · Recorte del canal REST: controladores de API, DTOs, Swagger y la cadena de servicios y DAO que consumen." {
            include personapi.web.apiPersonas personapi.web.apiProfesiones personapi.web.apiTelefonos personapi.web.apiEstudios
            include personapi.web.dtos personapi.web.swagger
            include personapi.web.svcPersona personapi.web.svcProfesion personapi.web.svcTelefono personapi.web.svcEstudio
            include personapi.web.daoPersona personapi.web.daoProfesion personapi.web.daoTelefono personapi.web.daoEstudio
            include personapi.web.contexto personapi.web.entidades
            include clienteRest db
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

    configuration {
        scope softwaresystem
    }
}
