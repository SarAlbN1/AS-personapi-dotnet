# Criterios de éxito

[← Producto](README.md)

La demo se considera completa cuando se obtienen evidencias de:

1. El repositorio público se llama `personapi-dotnet` y se puede clonar.
2. DDL crea `persona_db` y cuatro tablas con PK/FK; DML carga datos coherentes.
3. La app compila y se conecta a SQL Server sin secretos dentro de Git.
4. Cada recurso tiene sus **cinco acciones REST** operativas, comprobadas en Swagger.
5. Cada recurso tiene **Index, Details, Create, Edit y Delete** operativos mediante Razor.
6. Persona puede crearse por MVC y leerse por REST, comprobando una **persistencia compartida**.
7. Un teléfono solo puede vincularse a una persona existente.
8. Un estudio solo puede crearse con persona y profesión existentes; se consulta por la clave compuesta.
9. La creación de un identificador duplicado devuelve conflicto; el recurso inexistente devuelve 404.
10. Borrar una persona con teléfonos o estudios produce **409 en REST** y una explicación en MVC.
11. Se eliminan primero los dependientes; después la persona se puede borrar.
12. Controladores no acceden a `DbContext`; MVC y API invocan servicios, que dependen de interfaces DAO.
13. Pruebas verifican situaciones positivas/negativas con datos reales, y no solo métodos simulados.
14. README explica restauración, configuración, compilación y ejecución; se entrega URL del tag `v1.0.0`.
15. El informe académico contiene portada, marco conceptual, diseño, procedimiento, conclusiones/lecciones y referencias.

Cada afirmación de éxito requiere una prueba, una captura o una revisión de código. Ningún punto se da por cumplido por estar escrito aquí.
