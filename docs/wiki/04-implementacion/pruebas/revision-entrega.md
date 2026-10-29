# Revisión de entrega (HU-015)

[← Evidencias](README.md) · [Entrega](../../05-proceso/entrega.md) · [Reglas arquitectónicas](../../02-arquitectura/reglas-arquitectonicas.md) · [Runbook](../runbook-demo.md)

Comprobaciones previas al tag `v1.0.0`, ejecutadas el 2026-10-29 sobre `main` en **`403dd4e`**: el código y el informe con todas las historias integradas, de HU-001 a HU-014, HU-016 y HU-017. El PR de cierre solo cambia documentación (README, wiki y `scripts/README.md`), así que estas comprobaciones valen para el commit que lleve el tag.

## 1. Clon limpio y base creada desde cero

Clon nuevo desde GitHub, sin User Secrets propios ni base previa, con un `.env` local que no se versiona.

| Camino | Resultado |
|---|---|
| **Docker Compose:** `docker compose up --build -d`, con un volumen nuevo | Los tres servicios levantaron. `db-init` ejecutó `01_ddl.sql` y `02_dml.sql` y terminó con 0. En `http://localhost:8080` respondieron con 200 `/`, `/Personas`, `/Profesiones`, `/Telefonos`, `/Estudios` y `/swagger`. El documento OpenAPI 3.0.4 tiene **20 operaciones, todas de `/api`**, y cada listado REST devuelve 3 filas. |
| **Recorrido E2E de los 8 pasos del runbook** sobre ese despliegue | Los 8 pasos dieron lo esperado, mezclando los dos canales: alta web y lectura REST, `PUT` REST visto en la web, `409` por REST y aviso de conflicto en la web, borrado ordenado y Swagger. |
| **Camino local del README:** `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet run`, con `ConnectionStrings__PersonaDb` apuntando a la base recién creada | SDK 10.0.402. 0 advertencias y 0 errores. Pasan **207/207**; sin cadena, 92 superadas y 95 omitidas, 187 en total, porque la teoría omitida cuenta como una sola prueba. Con `dotnet run`, `/`, `/Personas`, `/Telefonos/Details/3001234567` y `/swagger` dan 200, y el documento tiene 20 operaciones. La base queda con 3 filas por tabla. |
| **Comandos `sqlcmd` del README** contra la instancia `SQLEXPRESS` local, con SQL Browser detenido | Con `-S "localhost\SQLEXPRESS"`, el `sqlcmd` 1.10 (el que instala el manual) no conecta: busca la tubería `SQLLocal\SQLEXPRESS`. El `sqlcmd` clásico (ODBC 18) sí conecta. Con `-S "lpc:localhost\SQLEXPRESS"` conectan los dos, y los dos scripts terminan con 0. El README y `scripts/README.md` usan `lpc:`, igual que el manual del informe. |

Al terminar se ejecutó `docker compose down -v` y se borró el clon con su `.env`.

## 2. Secretos y credenciales

Sobre las **131 confirmaciones** de todas las ramas:

| Comprobación | Resultado |
|---|---|
| Archivos sensibles añadidos alguna vez (`.env`, `secrets.json`, `.pfx`, `.pem`, `.key`, `appsettings.*.json`, `.mdf`, `.bak`) | Ninguno. |
| Contraseñas, tokens y claves en el contenido añadido | Solo dos marcadores: `<contrasena-local-no-versionada>` y `<contrasena-local>`. |
| Cadenas de conexión con usuario y contraseña | Ninguna con valor real. `docker-compose.yml` usa `${SA_PASSWORD}` y `appsettings.json` deja `PersonaDb` vacía. |
| Capturas del informe y texto del PDF | Sin credenciales ni rutas locales. Revisadas en #31, #37 y #43. |

Comando que reproduce la búsqueda de contenido:

```bash
git log --all --remotes -p --format= | grep -aE '^\+' \
  | grep -aiE 'password\s*=\s*[^;"<$ {]|pwd\s*=\s*[^;"<$ ]|SA_PASSWORD=[^$ "]+|ghp_[A-Za-z0-9]{20}|github_pat_|AKIA[0-9A-Z]{16}|PRIVATE KEY|AccountKey='
```

## 3. Architecture killers

| Killer | Evidencia | Estado |
|---|---|---|
| AK-01 · Controlador con acceso directo a `DbContext` | `Verificacion/ArquitecturaTests.cs` (HU-014): `PersonaDbContext` solo aparece en `Repository/Implementations/`, en su definición y en el `AddDbContext` de `Program.cs`. Fuera de los DAO no hay `using Microsoft.EntityFrameworkCore`, `FromSql*`, `ExecuteSql*` ni `SqlConnection`. Las pruebas fallaron al meter a propósito un `PersonaDbContext` en un controlador, en #34 y otra vez en su revisión. | Cerrado |
| AK-02 · Solo vistas o solo API | Los cuatro módulos tienen los dos canales, y `ArquitecturaTests` comprueba que cada `XController` usa los servicios de su `XApiController`. [Cobertura CRUD](cobertura-crud.md) registra las 20 operaciones REST y las 20 acciones web. | Cerrado |
| AK-03 · Estudios con id ficticio | `HasKey(e => new { e.IdProf, e.CcPer })` en `PersonaDbContext`; las rutas REST y web llevan `{idProf}/{ccPer}`. El escenario 4 consulta por el par e invierte las mitades para obtener 404. | Cerrado |
| AK-04 · Huérfanos o borrado en cascada | `01_ddl.sql` declara las FK sin `CASCADE`, y EF usa `ClientSetNull`. `IntegridadSqlPersonaDbTests` prueba en la base los errores 547 y 2627, y los escenarios 5 y 6 prueban el 409 y el borrado ordenado. | Cerrado |
| AK-05 · Swagger sin los 20 endpoints | `DocumentoOpenApiTests` exige exactamente 20 operaciones, todas de `/api` (#42). Contadas en el documento servido desde el clon limpio, por los dos caminos. | Cerrado |
| AK-06 · El clon limpio no compila o no conecta | Sección 1. | Cerrado |
| AK-07 · Secretos en el repositorio público | Sección 2. | Cerrado |

## 4. Lista de revisión arquitectónica por módulo

| Pregunta | Personas | Profesiones | Teléfonos | Estudios |
|---|---|---|---|---|
| ¿Dependencias en la dirección de AD-02, AD-03 y AD-04? | Sí | Sí | Sí | Sí |
| ¿MVC y API consumen el mismo servicio? | `IPersonaService` | `IProfesionService` | `ITelefonoService` (+ `IPersonaService` para el selector) | `IEstudioService` (+ los de profesión y persona para los selectores) |
| ¿FK o clave compuesta protegidas en SQL? | PK `cc`, CHECK de género | PK `id` | PK `num`, FK `duenio` | PK `(id_prof, cc_per)`, dos FK |
| ¿Pruebas de éxito, fallos y persistencia? | API, MVC y escenarios | API, MVC y escenarios | API, MVC y escenarios | API, MVC y escenarios |
| ¿Documentos sincronizados? | README, `api-rest.md`, `Front/README.md` | ídem | ídem | ídem |

La evidencia de cada fila está en las pruebas de `tests/Unit/personapi-dotnet.Tests.Unit/` y en los PR de cada historia (HU-004 a HU-013).

## 5. Informe académico

`docs/informe/` de `403dd4e` es idéntico al de `8e567c9` (#43), que se compiló con `texlive/texlive:latest-full` y `latexmk -pdf -halt-on-error`.

| Comprobación | Resultado |
|---|---|
| Secciones obligatorias: portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones, referencias | Presentes. |
| El PDF se compila desde `main.tex` | 74 páginas, **0 errores**. `main.pdf` se versiona junto al `.tex` y no tiene recuadros de captura pendientes. |
| Versiones resueltas de SDK y paquetes | SDK 10.0.402; EF Core 10.0.14; Swashbuckle.AspNetCore 10.3.1; Microsoft.AspNetCore.Mvc.Testing 10.0.13. |
| URL del repositorio | `\repoURL` = `https://github.com/SarAlbN1/AS-personapi-dotnet`. |
| Coherencia con el modelo C4 | El pie del nivel 1 remite a `persona_db` en el nivel 2, como el criterio de HU-016. |

## 6. Tag

| Paso | Estado |
|---|---|
| HU-001 a HU-014, HU-016 y HU-017 en `main` | Hecho (`403dd4e`) |
| Secciones 1 a 5 sobre ese commit | Hecho |
| PR de cierre (HU-015) revisado y fusionado | Pendiente |
| `git tag -a v1.0.0 -m "Entrega Laboratorio 1"` sobre `main` después de esa fusión, y `git push origin v1.0.0` | Pendiente |
| URL del tag registrada en el README | Hecho en el PR de cierre: `https://github.com/SarAlbN1/AS-personapi-dotnet/releases/tag/v1.0.0` |
