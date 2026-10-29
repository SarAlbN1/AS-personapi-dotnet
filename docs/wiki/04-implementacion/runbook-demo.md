# Runbook de la demostración PersonAPI

[← Implementación](README.md) · [Criterios de éxito](../01-producto/criterios-de-exito.md)

> Guion planificado. Ningún paso se declara ejecutado hasta que se adjunte evidencia de una prueba real. Los ensayos ya ejecutados están en [Ensayos ejecutados](#ensayos-ejecutados).

## Preparación

1. Clonar `personapi-dotnet` en un entorno con SDK .NET 10, SQL Server 2025 Express y herramienta de administración SQL.
2. Configurar una conexión local sin registrar credenciales en Git.
3. Ejecutar `scripts/01_ddl.sql` y después `scripts/02_dml.sql` desde SSMS (revisar que la base y las tablas estén correctas).
4. Desde la raíz, ejecutar `dotnet restore` y `dotnet build`.
5. Ejecutar el proyecto web desde Visual Studio o `dotnet run --project personapi-dotnet/personapi-dotnet.csproj`.
6. Abrir la URL local anunciada por Kestrel y su ruta `/swagger`; no asumir un puerto fijo.

**Alternativa con Docker.** Desde la raíz del clon, copiar `.env.example` a `.env`, escribir `SA_PASSWORD` y ejecutar `docker compose up --build -d`. Levanta SQL Server 2025, carga los dos scripts con `db-init` y publica la aplicación en `http://localhost:8080`. `docker compose down -v` la retira junto con su volumen.

**Ojo con User Secrets.** Un segundo clon en el mismo equipo comparte el `UserSecretsId` del proyecto, así que lee la cadena que ya estaba guardada. Para que el clon apunte a otra base, se define `ConnectionStrings__PersonaDb`, que tiene prioridad.

## Recorrido E2E recomendado

| Paso | Acción | Evidencia esperada |
|---|---|---|
| 1 | Crear una persona desde `Personas/Create` | Aparece en listado y `GET /api/personas/{cc}` |
| 2 | Crear una profesión desde MVC | La muestra `GET /api/profesiones/{id}` |
| 3 | Crear teléfono de la persona en MVC o API | FK `duenio` persistida y consulta REST satisfactoria |
| 4 | Crear estudio que vincule persona y profesión | `GET /api/estudios/{idProf}/{ccPer}` responde el registro |
| 5 | Editar persona y profesión mediante REST | Las vistas MVC muestran los cambios de SQL Server |
| 6 | Intentar borrar persona con dependencias | API devuelve `409`, MVC presenta mensaje; registros permanecen |
| 7 | Eliminar estudio y teléfono, luego eliminar persona | Borrado controlado `204` y posterior consulta `404` |
| 8 | Revisar Swagger | Los 20 endpoints están documentados y son probables |

## Casos negativos mínimos

- Alta duplicada de persona / profesión / teléfono / estudio → `409`.
- Consulta de identificador inexistente → `404`.
- Estudio con persona o profesión inexistente y teléfono con dueño inexistente → `400`.
- Ruta `PUT` y clave del cuerpo inconsistentes → `400`.
- Se conserva el estado de los demás registros tras un intento fallido.

## Ensayos ejecutados

| Fecha | Qué | Resultado |
|---|---|---|
| 2026-10-28 | **Clon limpio de `main` (`05c02ac`) por Docker.** Clon nuevo desde GitHub, `.env` con una contraseña aleatoria y `docker compose up --build -d`. | Los tres servicios levantaron. `db-init` ejecutó `01_ddl.sql` y `02_dml.sql` y terminó con 0. En `:8080` respondieron `/`, `/Profesiones`, `/Estudios` y `/swagger` con 200, con 20 operaciones en el documento (5 por recurso) y 3 filas en cada listado REST. La vuelta CRUD de personas dio `201`, `200`, `204`, `204` y `404`, y el borrado de una persona con dependientes, `409`. `/Personas` y `/Telefonos` dieron `404`, porque sus módulos web aún no estaban en `main`. |
| 2026-10-28 | **El mismo clon por el camino local del README.** `dotnet restore`, `dotnet build` y `dotnet test` con `ConnectionStrings__PersonaDb` apuntando a la base recién creada del contenedor; después, `dotnet run`. | SDK 10.0.402. Compiló con 0 advertencias y 0 errores. Pasaron 119 de 119 pruebas, y sin cadena, 56 superadas y 63 omitidas. Con `dotnet run`, `/`, `/Profesiones`, `/Estudios` y `/swagger` respondieron con 200, el documento tenía 20 operaciones y `GET /api/telefonos/3001234567` devolvió la fila sembrada. |
| 2026-10-28 | **Recorrido R1 de las 20 operaciones REST y de los casos negativos**, sobre `8434ffa`. | Las 39 comprobaciones dieron el código esperado. El detalle está en [cobertura CRUD](pruebas/cobertura-crud.md). |
| 2026-10-29 | **Recorrido R2: los 8 pasos de la tabla de arriba**, sobre `c47f4be`, con los cuatro módulos web en `main`. Los formularios se enviaron como lo hace el navegador: GET del formulario, token antiforgery con su cookie y POST. Usó claves propias (persona `917000001`, profesión `9171`, teléfono `9170000001`). | Los 8 pasos dieron lo esperado. 1: la persona creada en la web redirige a su detalle y `GET /api/personas/917000001` la devuelve. 2: la profesión creada en la web se lee por REST. 3: el dueño aparece en el selector y el teléfono queda con `duenio` 917000001. 4: el estudio se crea con los dos selectores y responde por `(9171, 917000001)`. 5: tras los `PUT` (204), los detalles web muestran «Editada por REST». 6: `DELETE` REST da 409 y la confirmación web, 409 con «No se puede eliminar la persona 917000001: tiene 1 teléfono(s) y 1 estudio(s) asociados»; la persona sigue. 7: las bajas web de teléfono, estudio y persona redirigen; después la persona da 404 y la profesión se borra con 204. 8: `/swagger` da 200, con 20 operaciones. La base volvió a 3 filas por tabla. |

**Estado del recorrido E2E:** los 8 pasos se ejecutaron en R2, y `EscenariosSrsPersonaDbTests` los cubre de forma automática.

El ensayo final se repite sobre el `main` que lleve el tag.

## Respuesta ante fallos en la demo

| Síntoma | Verificación inicial |
|---|---|
| Fallo de conexión SQL | Instancia Express encendida, conexión configurada y BD `persona_db` existente |
| Swagger vacío | Registrados los API Controllers y generador OpenAPI; URL y perfil de ejecución correctos |
| 500 en CRUD | Revisar log, validez del esquema, FK/PK y traducción de errores SQL |
| Formularios no muestran relaciones | Verificar consultas del servicio y listas de selección |

## Entrega

Ejecutar los casos desde ambiente limpio, recopilar resultados verificables y publicar el tag `v1.0.0` solo cuando las verificaciones se cumplan. **No reemplazar pruebas con capturas fabricadas.**
