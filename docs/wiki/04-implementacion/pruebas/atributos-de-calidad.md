# Atributos de calidad medidos

[← Evidencias](README.md) · [Criterios](../../02-arquitectura/atributos-de-calidad.md) · [Índice de la wiki](../../Home.md)

Registro de la comprobación de cada atributo definido en [Atributos de calidad verificables](../../02-arquitectura/atributos-de-calidad.md). **Ninguna fila está ejecutada todavía:** la tabla es la plantilla que se completa durante la implementación, con el comando o la ruta que reproduce cada resultado.

## Entorno de la medición

Se completa en la misma ejecución que llena la tabla; mientras esté en blanco, los resultados no son interpretables.

| | |
|---|---|
| Fecha y hora | Pendiente |
| Rama y commit | Pendiente |
| SDK .NET (`dotnet --info`) | Pendiente |
| Instancia SQL Server | Pendiente |
| Estado de `persona_db` | Pendiente (indicar si viene de `01_ddl.sql` + `02_dml.sql` recién ejecutados o acumula datos previos) |
| Perfil de ejecución y URL de Kestrel | Pendiente |

## Resultado por atributo

| Atributo | Criterio de la página de calidad | Cómo se comprueba | Valor observado | Veredicto |
|---|---|---|---|---|
| Mantenibilidad | Ningún controlador MVC/API accede a `PersonaDbContext`; toda operación pasa por servicio y DAO | Revisión de `01-Presentacion/Controllers/` y `01-Presentacion/ControllersApi/`: ningún `using` ni dependencia del contexto, ninguna consulta EF Core | Pendiente | Pendiente |
| Correctitud | Datos inválidos o referencias inexistentes no se persisten | `POST /api/telefonos` con `duenio` inexistente y `POST /api/estudios` con FK inexistente; consulta posterior de la tabla | Pendiente | Pendiente |
| Consistencia | Crear por MVC y consultar por API devuelve el mismo registro | Crear persona en `Personas/Create`, leer `GET /api/personas/{cc}` y comparar con la fila de `persona` | Pendiente | Pendiente |
| Integridad | Borrar persona con teléfono o estudio no pierde relacionados | `DELETE /api/personas/{cc}` con dependencias: se espera `409` y las filas dependientes intactas | Pendiente | Pendiente |
| Interoperabilidad | 20 endpoints REST y códigos 200/201/204/400/404/409 según escenario | Recuento de operaciones en el documento OpenAPI 3 y ejecución de los casos positivos y negativos por recurso | Pendiente | Pendiente |
| Reproducibilidad | Desde un clon se ejecutan DDL/DML, se restaura, compila y arranca | Clon limpio, `01_ddl.sql`, `02_dml.sql`, `dotnet restore`, `dotnet build`, `dotnet run` siguiendo solo el README | Pendiente | Pendiente |
| Testabilidad | Casos positivos y negativos por recurso; una prueba fallida bloquea el cierre | Ejecución de la suite de pruebas del proyecto con su salida completa | Pendiente | Pendiente |
| Seguridad básica | Sin contraseñas ni cadenas de conexión con secretos en el repositorio | Revisión de `appsettings.json` versionado, historial y configuración local | Pendiente | Pendiente |

No se fijan umbrales de latencia ni de disponibilidad: el laboratorio no exige pruebas de carga ni escalabilidad, y una cifra sin entorno medido no es verificable.

## Hallazgos

Se registran aquí los defectos o desviaciones que aparezcan al medir, cada uno con la issue que lo atiende. Sin ejecuciones todavía, la sección está vacía.
