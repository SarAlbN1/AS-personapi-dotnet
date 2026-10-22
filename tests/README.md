# tests

Ubicación prevista para los proyectos de prueba automatizada de PersonAPI.

| Carpeta | Alcance previsto |
|---|---|
| `Unit/` | Reglas de servicio (duplicados, FK, conflictos) con DAO dobles. Niveles 1 de [pruebas.md](../docs/wiki/04-implementacion/pruebas.md). |
| `Integration/` | Restricciones reales de SQL Server y recorrido de API. Niveles 2 y 3 de `pruebas.md`. |

**Estado: sin proyectos.** No se crean `.csproj` de prueba en el bootstrap; corresponden a
su propia issue, junto con su registro en `personapi-dotnet.sln`.

## Discrepancia documental abierta

El [SDD](../docs/wiki/04-implementacion/03-SDD-LDD.md) y la
[estructura del repositorio](../docs/wiki/04-implementacion/estructura-del-repositorio.md)
**no declaran** una carpeta `tests/`, mientras que
[pruebas.md](../docs/wiki/04-implementacion/pruebas.md) exige pruebas de servicios,
integración SQL, API y MVC. Esta carpeta se deja preparada sin modificar el SDD: la
ubicación definitiva debe decidirse en la issue de pruebas y quedar reflejada en el SDD
por PR revisado, no de forma silenciosa.
