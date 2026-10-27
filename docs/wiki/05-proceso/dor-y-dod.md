# Definition of Ready (DoR) y Definition of Done (DoD)

[← Proceso](README.md) · [Reglas arquitectónicas](../02-arquitectura/reglas-arquitectonicas.md)

## Ready: una issue puede comenzar si

- Tiene objetivo, módulo y alcance verificables.
- Existe como issue y está asignada en GitHub Projects a una persona y a Sprint 1 o Sprint 2.
- Tiene criterios de aceptación con evidencia observable.
- Identifica contratos REST, tablas, servicios o vistas afectados.
- No contradice el SRS, SAD, SDD ni las reglas aprobadas.
- Las dependencias necesarias están identificadas; el trabajo puede avanzar sin bloquear innecesariamente a los demás.

## Done: una issue está terminada solo si

1. Se cumplen todos sus criterios de aceptación.
2. `dotnet build` finaliza sin errores y pasan las pruebas relevantes.
3. Si toca persistencia, se verifica contra SQL Server real, sus llaves y restricciones.
4. Si toca REST, se prueban los endpoints afectados, estados HTTP y Swagger.
5. Si toca MVC, se comprueba el flujo en navegador y se comparte la lógica de servicios.
6. Se preserva la dependencia `Controller → Service → DAO → EF Core → SQL Server`.
7. No hay contraseñas versionadas ni datos sensibles en logs.
8. Los contratos y las páginas de la wiki afectadas se actualizaron en el mismo PR.
9. El PR tiene pruebas descritas, evidencia y **aprobación válida de otro integrante**, vigente sobre el último commit; la del autor no cuenta ([política de integración](pull-requests-y-releases.md#protección-de-main-y-revisión-cruzada-obligatoria)).
10. La rama se integra con squash a `main` y la issue se mueve a `Done` en GitHub Projects.

**No equivale a Done:** que compile sin persistir, un endpoint con valores hardcoded, un CRUD sin errores negativos, una UI sin backend real, ni que GitHub indique que el PR es fusionable. CI no es requisito del alcance base; la propuesta de contenedores del informe no es criterio de aceptación de ninguna historia.
