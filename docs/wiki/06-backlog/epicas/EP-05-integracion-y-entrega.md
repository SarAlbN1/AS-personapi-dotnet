# EP-05 · Integración y entrega

[← Backlog](../README.md) · [Pruebas](../../04-implementacion/pruebas.md) · [Entrega](../../05-proceso/entrega.md)

**Propósito:** cerrar el laboratorio: documentar el contrato REST completo en Swagger, verificar los dos canales y la integridad real de la base, documentar la arquitectura y el despliegue, y publicar la entrega académica con el tag `v1.0.0`. Esta épica **no sustituye** las pruebas de cada historia de módulo; comprueba la integración de lo ya probado individualmente.

**Requisitos cubiertos:** RF-07 (API autodescriptiva), RF-09 (entrega Git), RNF-01, RNF-02, RNF-03, RNF-04, RNF-05, RNF-06. Cierra los *killers* AK-01 a AK-07.

**Decisiones aplicadas:** AD-07 (OpenAPI 3 + Swagger UI), AD-01, AD-08, AD-09.

**Componentes afectados:** configuración de Swagger en `Program.cs` y atributos de los controladores API, proyecto o suite de pruebas, `README.md`, `docs/wiki/04-implementacion/runbook-demo.md`, `docs/wiki/02-arquitectura/diagramas/`, tag Git, y los archivos de despliegue con contenedores (`Dockerfile`, `docker-compose.yml`, `.dockerignore`) que crea HU-017. El informe `docs/informe/main.tex` se consulta en **solo lectura** desde todas las historias **salvo HU-017**, que es la que redacta el manual técnico de despliegue y está asignada a quien mantiene el documento; ninguna otra historia, persona ni asistente escribe en el `.tex`.

**Historias:** HU-009, HU-014, HU-015, HU-016, HU-017 (todas Sprint 2) · **20 puntos**

**Criterio de éxito de la épica:** Swagger UI expone y ejecuta las 20 operaciones; los siete escenarios de SRS §10 están ejecutados con evidencia; un clon limpio compila, conecta y corre siguiendo el README; los tres diagramas C4 están publicados y corresponden al código real; el manual de despliegue permite reproducirlo sin consultar a los autores, con capturas reales y con el despliegue por Compose levantado; `v1.0.0` apunta a un commit verificado.

---

## HU-009 · Contrato OpenAPI 3 y Swagger UI con las 20 operaciones documentadas

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 3 · **Módulo** Base

### Objetivo
Documentar y hacer ejecutable el contrato REST completo desde Swagger UI, para que la API sea autodescriptiva y evaluable, y cerrar el riesgo AK-05.

### Alcance técnico
- Configuración de Swashbuckle, comentarios XML, agrupación por recurso y ejemplos de esquema por DTO.
- Convención de `ProducesResponseType` y documentación de `400`/`404`/`409` por operación, aplicada a los controladores existentes (personas y profesiones) y **especificada por escrito** para los de teléfonos y estudios, de modo que HU-010 y HU-011 la cumplan sin rediseñar su controlador.
- Respuestas de error uniformes sin trazas SQL ni secretos ([validaciones y errores](../../03-contratos/validaciones-y-errores.md)).
- **No incluye:** implementar endpoints (los aportan EP-01 a EP-04).

### Criterios de aceptación
1. El documento OpenAPI 3 describe cada operación con sus claves, verbos y códigos de estado, con la convención aplicada a personas y profesiones.
2. La convención queda especificada por escrito para teléfonos y estudios.
3. Existe al menos un ejemplo de entrada y de salida por recurso.
4. Los códigos `400`, `404` y `409` están documentados por operación y las respuestas de error no exponen trazas ni secretos.
5. **Comprobación final:** las 20 operaciones aparecen y se ejecutan desde `/swagger`; se verifica una vez integradas HU-010 y HU-011.

**Evidencia mínima:** documento OpenAPI exportado con el conteo de operaciones y capturas de Swagger UI ejecutando un caso por recurso.

### Dependencias
**Bloqueantes:** HU-004, HU-005 (necesita controladores API reales sobre los que fijar la convención).
**De verificación, no de construcción:** HU-010 y HU-011. El trabajo se desarrolla y revisa antes; el criterio 5 se marca cuando esas dos historias se integran. Esta distinción se declara en la issue para no crear un bloqueo artificial ni cerrar la historia sin evidencia.

### Trazabilidad
RF-07 · RNF-03 · AD-07 · AK-05 · [api-rest.md](../../03-contratos/api-rest.md#swagger-y-verificación) · [SDD §4](../../04-implementacion/03-SDD-LDD.md#4-dependencias-en-programcs), §5 · criterio de éxito 4.

### Definition of Done
DoD común + conteo de 20 operaciones comprobado en el documento OpenAPI y PR revisado.

---

## HU-014 · Verificación integral: integridad SQL, CRUD dual y recorrido E2E

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 5 · **Módulo** Pruebas

### Objetivo
Ejecutar y registrar la verificación de los dos canales, la integridad real de la base y los siete escenarios de aceptación, para sostener la demostración con evidencia y no con afirmaciones.

### Alcance técnico
- Pirámide de [pruebas.md](../../04-implementacion/pruebas.md): reglas de servicio con DAOs controlables, integración contra SQL Server real, comprobación de los 20 endpoints, de las 20 acciones web y de los escenarios E2E.
- Escenarios 1 a 7 de [SRS §10](../../01-producto/01-SRS.md#10-escenarios-mínimos-de-aceptación), incluido el borrado bloqueado y la limpieza ordenada de dependientes.
- Comprobación arquitectónica automatizable: ningún archivo fuera de `Back/03-AccesoDatos/Repository/Implementations` referencia `PersonaDbContext`.
- Actualización de [runbook-demo.md](../../04-implementacion/runbook-demo.md) con el recorrido realmente ejecutado.
- Construcción de las pruebas sobre el proyecto de pruebas creado en HU-001; no se crea una estructura de pruebas paralela.
- **No incluye:** corregir defectos de módulos ajenos. Se reportan como issue o comentario según [CLAUDE.md](../../../../CLAUDE.md) y el [proceso](../../05-proceso/pull-requests-y-releases.md); el responsable del módulo los corrige en su rama, regla de propiedad que se aplica durante toda la historia aunque no sea un criterio de aceptación.

### Criterios de aceptación
1. Los escenarios 1 a 7 de SRS §10 se ejecutan contra `persona_db` real y su resultado queda registrado.
2. La matriz negativa se ejecuta para los cuatro recursos: clave duplicada `409`, referencia inválida `400`, clave ausente `404` y borrado bloqueado `409`.
3. Las pruebas de integración demuestran que las restricciones de SQL Server impiden duplicados y huérfanos, no solo con repositorios simulados.
4. Se recorren y registran las 20 operaciones REST y las 20 acciones web (cinco por entidad) con su resultado.
5. **Comprobación arquitectónica completa**, con evidencia por cada punto:
   - ningún archivo fuera de las implementaciones DAO referencia `PersonaDbContext` ni ejecuta SQL (AK-01, regla 3);
   - el controlador MVC y el controlador API de cada entidad consumen **el mismo** `I*Service`, sin reglas duplicadas (AD-02, AD-03, regla 2 y 12, [criterio de éxito 12](../../01-producto/criterios-de-exito.md));
   - las respuestas HTTP y las vistas usan DTO y ViewModels, sin exponer entidades EF con navegaciones (AD-10, regla 8);
   - la solución sigue siendo **un único artefacto desplegable** sobre una sola base `persona_db` (AD-01, regla 1).
6. `runbook-demo.md` queda actualizado con el recorrido ejecutado.

**Evidencia mínima:** salida de compilación y pruebas, matriz de casos con resultado esperado frente al obtenido y runbook actualizado.

### Dependencias
**HU-007, HU-008, HU-010, HU-011, HU-012, HU-013.** Es el punto de convergencia del Sprint 2 por naturaleza: no puede verificarse un CRUD dual que no existe. Mitigación acordada: la suite se construye de forma incremental a medida que cada módulo se integra, en lugar de esperar al final del sprint. El conteo de Swagger se toma de HU-009.

### Trazabilidad
RNF-01, RNF-02, RNF-06 · AD-01, AD-02, AD-03, AD-10 · AK-01, AK-02, AK-03, AK-04 · [pruebas.md](../../04-implementacion/pruebas.md) · [SRS §10](../../01-producto/01-SRS.md#10-escenarios-mínimos-de-aceptación) · [SDD §9](../../04-implementacion/03-SDD-LDD.md#9-comprobación-de-implementación) · reglas 3, 9, 10, 11 · criterios de éxito 6, 10, 11, 13.

### Definition of Done
DoD común + evidencia de persistencia real, matriz de casos completa y PR revisado.

---

## HU-015 · Cierre de entrega: clon limpio verificado, revisión arquitectónica, informe y tag `v1.0.0`

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante C · **Prioridad** P0 · **Puntos** 4 · **Módulo** Entrega

### Objetivo
Verificar la reproducibilidad desde un clon limpio, cerrar la revisión arquitectónica y publicar la entrega académica exigida por el laboratorio.

### Alcance técnico
- README con restauración, configuración, compilación, ejecución y scripts, **verificado ejecutándolo** sobre un clon limpio y una base creada desde cero (AK-06).
- Recorrido de la [lista de revisión arquitectónica](../../02-arquitectura/reglas-arquitectonicas.md#lista-de-revisión-arquitectónica-de-un-pr) por módulo y cierre documentado de AK-01 a AK-07.
- Comprobación de ausencia de secretos en el historial (AK-07).
- **Verificación** del informe académico, cuya fuente de verdad es `docs/informe/main.tex` (LaTeX, compilado en Overleaf): se comprueba que contiene portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones, y referencias, con las versiones resueltas (`dotnet --info`, paquetes) de [versiones](../../04-implementacion/versiones.md). **El `.tex` no se toca en esta historia:** si falta contenido, se solicita a quien mantiene el documento y esa persona lo redacta en el `.tex`; no se crea un informe paralelo en Markdown ni se versiona el PDF como fuente.
- Tag `v1.0.0` conforme a [entrega.md](../../05-proceso/entrega.md) y [releases](../../05-proceso/pull-requests-y-releases.md).
- **No incluye:** nueva funcionalidad, ni edición de `docs/informe/main.tex`. Si aparece un defecto, se abre issue; el tag no se publica sobre un estado no verificado.

### Criterios de aceptación
1. Los pasos del README se verifican ejecutándolos sobre un clon limpio y una base creada desde cero.
2. La lista de revisión arquitectónica se recorre por módulo y cada killer AK-01 a AK-07 queda cerrado con evidencia.
3. Se comprueba que no hay secretos ni cadenas con credenciales en el historial del repositorio.
4. Se verifica que `docs/informe/main.tex` contiene portada, marco conceptual, diseño, procedimiento, conclusiones y lecciones, y referencias, con las versiones resueltas, y que el PDF entregado se compila de ese `.tex` sin haberlo modificado desde el backlog.
5. `v1.0.0` se crea sobre un commit integrado y verificado de `main` y su URL queda registrada, solo después de la evidencia de HU-014.
6. Las páginas de la wiki afectadas se actualizan en el mismo PR.

**Evidencia mínima:** log de la instalación desde clon limpio, lista de revisión firmada, verificación de ausencia de secretos y URL del tag.

### Dependencias
**HU-009** (conteo de Swagger para el criterio 4 del laboratorio), **HU-014** (evidencia de verificación) y **HU-016 y HU-017**, cuyo resultado —los diagramas C4 y el manual técnico de despliegue— vive en `docs/informe/main.tex`, que esta historia verifica antes de publicar el tag. Es la última historia del backlog por definición.

### Trazabilidad
RF-09 · RNF-04, RNF-05 · AK-06, AK-07 · AD-01, AD-08, AD-09 · [entrega.md](../../05-proceso/entrega.md) · [versiones](../../04-implementacion/versiones.md) · criterios de éxito 1, 3, 14, 15.

### Definition of Done
DoD común + reproducibilidad comprobada en entorno limpio, informe entregado y tag publicado tras la verificación.

---

## HU-016 · Diagramas C4 del sistema: contexto, contenedores y componentes

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante A · **Prioridad** P1 · **Puntos** 3 · **Módulo** Entrega

### Objetivo
Entregar los diagramas C4 de nivel 1 a 3 del monolito, derivados del SAD y del SDD y verificados contra el código real, para sustentar la arquitectura en la entrega y en la sustentación.

### Alcance técnico
- **Nivel 1 · Contexto:** usuario, monolito PersonAPI y SQL Server `persona_db` como único almacén.
- **Nivel 2 · Contenedores:** la aplicación ASP.NET Core MVC 10 como **único artefacto desplegable** (AD-01) y la base `persona_db`, con los dos canales —vistas Razor y API REST— y sus protocolos.
- **Nivel 3 · Componentes** del contenedor web: controladores MVC, controladores API, servicios, interfaces DAO e implementaciones EF Core, con las dependencias en la dirección que fijan AD-02, AD-03 y AD-04.
- Fuentes **en texto** versionadas en `docs/wiki/02-arquitectura/diagramas/` e imágenes exportadas enlazadas desde la página de arquitectura de la wiki.
- **No incluye:** modificar SAD, SDD ni contratos, ni dibujar elementos que esos documentos no declaren. Tampoco añade tecnologías fuera de [versiones.md](../../04-implementacion/versiones.md). Una discrepancia entre el diseño y el código se **reporta** como issue a quien mantiene el documento; no se resuelve dibujando otra cosa.

### Criterios de aceptación
1. El diagrama de contexto muestra usuario, monolito y `persona_db`, sin elementos que el SAD no declare.
2. El diagrama de contenedores muestra un solo artefacto desplegable y una sola base, con los dos canales y sus protocolos.
3. El diagrama de componentes muestra controladores MVC y API, servicios, interfaces DAO e implementaciones EF Core, y **ninguna flecha de un controlador a `PersonaDbContext`** (AK-01, regla 3).
4. Cada elemento dibujado corresponde a un componente real del repositorio o a una decisión `AD-*` vigente; la correspondencia se comprueba elemento por elemento contra el árbol del repositorio.
5. Las fuentes de los tres diagramas están versionadas como texto y las imágenes quedan enlazadas desde la wiki.
6. Los diagramas no modifican SAD, SDD ni contratos.

**Evidencia mínima:** fuentes versionadas, imágenes exportadas, enlace desde la wiki y la comprobación elemento por elemento.

### Dependencias
**HU-001** (estructura de capas) y **HU-003** (entidades, contexto y contratos DAO reales). No bloquea a HU-014: la verificación no espera material documental. Su dependiente es **HU-015**, porque los diagramas se incluyen en el informe que la entrega verifica.

### Trazabilidad
AD-01 · AD-02 · AD-03 · AD-04 · AD-10 · [SAD §5](../../02-arquitectura/02-SAD-HLD.md#5-vista-por-módulos) · [SDD §1, §2](../../04-implementacion/03-SDD-LDD.md) · reglas 1, 2, 3, 6.

### Definition of Done
DoD común + diagramas publicados y enlazados, correspondencia con el repositorio comprobada y PR revisado.

---

## HU-017 · Manual técnico de despliegue en el informe académico

**Épica** EP-05 · **Sprint** 2 · **Responsable** Integrante B · **Prioridad** P1 · **Puntos** 5 · **Módulo** Entrega

### Objetivo
Redactar en `docs/informe/main.tex` el manual técnico de despliegue del monolito —requisitos, configuración, base de datos, ejecución y verificación— para que la entrega académica permita reproducir el despliegue sin consultar a los autores.

### Alcance técnico
- **Instalación paso a paso de cada herramienta**, con su captura y con la **versión resuelta** de [versiones.md](../../04-implementacion/versiones.md): SDK .NET 10, SQL Server 2025 Express, la herramienta de consulta (SSMS o `sqlcmd`), Git y Docker Desktop con Compose v2. Incluye cómo comprobar la versión instalada (`dotnet --version`, `docker --version`, `docker compose version`). No basta con enumerar requisitos previos.
- Cadena de conexión por **User Secrets** o variable de entorno. **Sin credenciales en el texto**, ni contraseñas, ni capturas que las muestren ([regla de secretos](../../../../CLAUDE.md)).
- Creación de `persona_db` ejecutando `scripts/01_ddl.sql` y después `scripts/02_dml.sql`, con el comportamiento esperado ante reejecución.
- Restauración, compilación, ejecución, URL de la aplicación y de `/swagger`, verificación posterior al despliegue y fallos frecuentes con su causa.
- **Archivos de despliegue con contenedores que el informe ya declara (§8.14) y que no existen en el repositorio:** `Dockerfile`, `docker-compose.yml` y `.dockerignore`, con las imágenes ya fijadas en [versiones.md](../../04-implementacion/versiones.md) y los servicios `personapi`, `sqlserver` y `db-init`. La contraseña de `sa` se lee de un `.env` **no versionado**; el repositorio solo lleva `.env.example`. No se añade ni se retira ninguna tecnología: las tres imágenes y Compose v2 ya están en `versiones.md` y en el informe.
- **Capturas reales** tomadas durante la ejecución en entorno limpio, que sustituyen los recuadros `\captura{...}` pendientes del procedimiento.
- **Propiedad del documento:** esta historia es la única que escribe en el `.tex` y está asignada a quien lo mantiene. HU-015 sigue limitándose a verificar y citar.
- **No incluye:** decidir la tecnología de despliegue. El manual **documenta lo que el informe ya declara** y no añade ni retira tecnologías respecto de ese documento ni de [versiones.md](../../04-implementacion/versiones.md). El **despliegue con Docker se verifica en esta historia y no es criterio de ninguna otra**; el camino que valida los criterios de aceptación de las demás historias sigue siendo la ejecución local, como fija [SRS §2](../../01-producto/01-SRS.md#2-alcance-confirmado). Tampoco duplica el README del repositorio: lo referencia.

### Criterios de aceptación
1. El manual describe, paso a paso y con su captura, la instalación de cada herramienta con la versión resuelta de `versiones.md`: SDK .NET 10, SQL Server 2025 Express, la herramienta de consulta (SSMS o `sqlcmd`), Git y Docker Desktop con Compose v2. Indica cómo comprobar la versión instalada (`dotnet --version`, `docker --version`, `docker compose version`).
2. Configuración de la cadena de conexión documentada sin ninguna credencial.
3. Creación de la base documentada con los dos scripts en orden y el comportamiento ante reejecución.
4. Restauración, compilación, ejecución y URL documentadas, coherentes con lo que el informe declara.
5. `docker compose up --build -d` levanta la aplicación en el puerto 8080 con `persona_db` creada desde `scripts/01_ddl.sql` y `scripts/02_dml.sql`, y el manual lo documenta con capturas reales. No se añade ni retira ninguna tecnología respecto de `versiones.md` y del informe.
6. Verificación posterior al despliegue y fallos frecuentes con su causa.
7. Cada paso del manual incluye al menos una captura real tomada durante la ejecución en entorno limpio. Las capturas sustituyen los recuadros `\captura{...}` pendientes del procedimiento, y ninguna muestra contraseñas ni cadenas de conexión con credenciales; si una pantalla las contiene, se recorta o se tapa antes de versionarla.
8. Los pasos se ejecutan en un entorno limpio antes de cerrar la historia y el resultado se registra; **lo no ejecutado se declara pendiente**, no se presenta como verificado.
9. El manual referencia el README en lugar de duplicarlo.

**Evidencia mínima:** fragmento del informe compilado con el manual y sus capturas, log del despliegue ejecutado en entorno limpio —local y `docker compose up --build -d`— y comprobación de ausencia de credenciales en el texto, en las capturas y en `.env.example`.

### Dependencias
**HU-001** (solución ejecutable) y **HU-002** (scripts DDL/DML). Dependiente: **HU-015**, por la misma razón que HU-016: el manual vive en el informe que la entrega verifica.

### Trazabilidad
RF-09 · RNF-04 · RNF-05 · AD-01 · AD-09 · [versiones.md](../../04-implementacion/versiones.md) · [entrega.md](../../05-proceso/entrega.md) · criterios de éxito 1, 3.

### Definition of Done
DoD común + manual en el `.tex` compilando con sus capturas, `Dockerfile`, `docker-compose.yml` y `.dockerignore` versionados y levantando la aplicación con Compose, pasos ejecutados en entorno limpio con su log y PR revisado.
