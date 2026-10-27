# tests

Ubicación prevista para los proyectos de prueba automatizada de PersonAPI.

| Carpeta | Alcance previsto |
|---|---|
| `Unit/` | Reglas de servicio (duplicados, FK, conflictos) con DAO dobles. Niveles 1 de [pruebas.md](../docs/wiki/04-implementacion/pruebas.md). |
| `Integration/` | Restricciones reales de SQL Server y recorrido de API. Niveles 2 y 3 de `pruebas.md`. |

**Estado:** `Unit/personapi-dotnet.Tests.Unit/` existe desde HU-001, registrado en
`personapi-dotnet.sln`, con xUnit y `Microsoft.AspNetCore.Mvc.Testing`. Contiene las
comprobaciones de arranque (vistas, estáticos y documento OpenAPI 3); las pruebas de
reglas de servicio las añade cada historia en este mismo proyecto. `Integration/` sigue
sin proyecto: corresponde a HU-014, que necesita `persona_db` real.

Desde HU-003, `Integracion/` contiene la lectura de `persona_db` a través de cada
repositorio (`[FactConPersonaDb]`) y la comprobación del ciclo `scoped`. Toma
`ConnectionStrings:PersonaDb` de los User Secrets del proyecto web o de la variable
`ConnectionStrings__PersonaDb`; si no está definida, esas pruebas se **omiten** y el resto
se ejecuta igual. Se dejan en este proyecto porque HU-014 construye sus pruebas sobre él
sin crear una estructura paralela.

## Discrepancia documental abierta

El [SDD](../docs/wiki/04-implementacion/03-SDD-LDD.md) y la
[estructura del repositorio](../docs/wiki/04-implementacion/estructura-del-repositorio.md)
**no declaran** una carpeta `tests/`, mientras que
[pruebas.md](../docs/wiki/04-implementacion/pruebas.md) exige pruebas de servicios,
integración SQL, API y MVC. Esta carpeta se deja preparada sin modificar el SDD: la
ubicación definitiva debe decidirse en la issue de pruebas y quedar reflejada en el SDD
por PR revisado, no de forma silenciosa.
