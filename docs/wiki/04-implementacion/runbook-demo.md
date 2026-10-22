# Runbook de la demostración PersonAPI

[← Implementación](README.md) · [Criterios de éxito](../01-producto/criterios-de-exito.md)

> Guion planificado. Ningún paso se declara ejecutado hasta que se adjunte evidencia de una prueba real.

## Preparación

1. Clonar `personapi-dotnet` en un entorno con SDK .NET 10, SQL Server 2025 Express y herramienta de administración SQL.
2. Configurar una conexión local sin registrar credenciales en Git.
3. Ejecutar `scripts/01_ddl.sql` y después `scripts/02_dml.sql` desde SSMS (revisar que la base y las tablas estén correctas).
4. Desde la raíz, ejecutar `dotnet restore` y `dotnet build`.
5. Ejecutar el proyecto web desde Visual Studio o `dotnet run --project personapi-dotnet/personapi-dotnet.csproj`.
6. Abrir la URL local anunciada por Kestrel y su ruta `/swagger`; no asumir un puerto fijo.

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

## Respuesta ante fallos en la demo

| Síntoma | Verificación inicial |
|---|---|
| Fallo de conexión SQL | Instancia Express encendida, conexión configurada y BD `persona_db` existente |
| Swagger vacío | Registrados los API Controllers y generador OpenAPI; URL y perfil de ejecución correctos |
| 500 en CRUD | Revisar log, validez del esquema, FK/PK y traducción de errores SQL |
| Formularios no muestran relaciones | Verificar consultas del servicio y listas de selección |

## Entrega

Ejecutar los casos desde ambiente limpio, recopilar resultados verificables y publicar el tag `v1.0.0` solo cuando las verificaciones se cumplan. **No reemplazar pruebas con capturas fabricadas.**
