# Atributos de calidad verificables

[← Arquitectura](README.md)

| Atributo | Escenario de prueba | Criterio de aceptación |
|---|---|---|
| Mantenibilidad | Revisar controlador y referencias de proyecto | Ningún controlador accede directamente al `DbContext`; toda operación pasa por servicio y DAO |
| Correctitud | Insertar datos inválidos o referencias inexistentes | Error controlado; la base no guarda filas huérfanas |
| Consistencia | Crear por MVC y consultar por API | Ambos muestran el mismo registro persistido en `persona_db` |
| Integridad | Borrar persona con teléfono/estudio | Conflicto HTTP 409 o mensaje web; sin pérdida de relacionados |
| Interoperabilidad | Ejecutar cada recurso en Swagger | 20 endpoints REST y respuestas 200/201/204/400/404/409 según escenario |
| Reproducibilidad | Clonar, configurar, ejecutar DDL/DML y `dotnet run` | Aplicación inicia y los datos pueden verificarse siguiendo README |
| Testabilidad | Ejecutar pruebas unitarias/funcionales | Casos positivos y negativos para cada recurso; fallo de prueba bloquea cierre |
| Seguridad básica | Revisar repositorio y configuración | No contiene contraseñas reales ni connection strings con secretos |

No se fijan números de latencia ni disponibilidad sin medición del entorno. La guía no exige pruebas de carga ni escalabilidad distribuida.
