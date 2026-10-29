# Diagramas de arquitectura

[← Arquitectura](../README.md) · [SAD](../02-SAD-HLD.md) · [SDD](../../04-implementacion/03-SDD-LDD.md)

Fuentes **en texto** de los diagramas C4 (HU-016). La fuente es el `.dsl`: las imágenes
son material derivado y se regeneran a partir de él.

| Archivo | Contenido |
|---|---|
| [`personapi-c4.dsl`](personapi-c4.dsl) | Modelo C4 completo en Structurizr DSL: contexto, contenedores, componentes, recortes por canal y despliegue |

## Qué se dibuja

**La arquitectura del sistema**, con los cuatro módulos del dominio en los dos canales,
tal como la deciden el SRS, el SAD, el SDD y el informe.

Los diagramas describen el **diseño**, no el avance: no distinguen lo ya escrito de lo
que falta, igual que el resto de la documentación de arquitectura. El estado de cada
historia vive en **GitHub Projects**, que es la fuente única de estado ([regla 3 del
backlog](../../06-backlog/README.md)); la correspondencia entre cada elemento del diagrama
y su código o su historia está más abajo.

## Vistas

| Clave de la vista | Nivel | Qué muestra |
|---|---|---|
| `c4-c1-contexto` | C4 1 · Contexto | El usuario, el cliente REST, el monolito PersonAPI y SQL Server `persona_db` como único almacén |
| `c4-c2-contenedores` | C4 2 · Contenedores | La aplicación ASP.NET Core MVC 10 como **único artefacto desplegable**, sus dos canales y la base `persona_db` |
| `c4-c3-componentes` | C4 3 · Componentes | El contenedor web completo: cuatro controladores MVC y cuatro de API, un servicio por entidad, las cuatro interfaces DAO con su implementación EF Core, el contexto y las entidades |
| `c4-c3-componentes-canal-web` | C4 3 · Recorte | Solo el canal MVC, para explicarlo sin el ruido del otro canal |
| `c4-c3-componentes-canal-json` | C4 3 · Recorte | Solo el canal REST, con DTOs y Swagger |
| `c4-despliegue` | Despliegue | Los tres servicios de `docker-compose.yml` (`personapi`, `sqlserver`, `db-init`) en la red `personapi-net` |

Los dos recortes existen porque la vista completa tiene 22 componentes: sirven para la
sustentación, mientras `c4-c3-componentes` es la que va al informe.

## Cómo renderizar

**Playground (sin instalar nada).** Abrir <https://structurizr.com/dsl>, pegar el
contenido de `personapi-c4.dsl` y elegir la vista en el selector. Exportar cada diagrama
como PNG.

**Reproducible, con Docker.** `structurizr-cli` **no** exporta PNG —sus formatos son
`plantuml`, `mermaid`, `json`, `static`…— y la imagen `structurizr/cli` está
descontinuada: hoy solo imprime el aviso de migración y termina sin hacer nada. El camino
que funciona es validar con la imagen `structurizr/structurizr`, exportar a PlantUML y
convertir a PNG con PlantUML:

```bash
# 1. Validar el modelo (falla si hay un identificador fuera de alcance)
docker run --rm -v "$PWD/docs/wiki/02-arquitectura/diagramas:/w" \
  structurizr/structurizr validate -workspace /w/personapi-c4.dsl

# 2. Exportar a PlantUML
docker run --rm -v "$PWD/docs/wiki/02-arquitectura/diagramas:/w" \
  structurizr/structurizr export -workspace /w/personapi-c4.dsl \
  -format plantuml/structurizr -output /w/puml

# 3. Convertir a PNG. PLANTUML_LIMIT_SIZE es obligatorio: por defecto PlantUML
#    recorta a 4096 px y la vista de componentes mide mas, asi que sin el se
#    pierden Swagger y persona_db sin ningun aviso.
docker run --rm --user root -e PLANTUML_LIMIT_SIZE=8192 \
  -v "$PWD/docs/wiki/02-arquitectura/diagramas/puml:/data" \
  -v "$PWD/docs/informe/diagramas:/out" -w /data plantuml/plantuml \
  -tpng -charset UTF-8 -o /out \
  structurizr-c4-c1-contexto.puml structurizr-c4-c2-contenedores.puml structurizr-c4-c3-componentes.puml
```

La carpeta intermedia `puml/` es material temporal y no se versiona.

### Nombres de archivo que espera el informe

`docs/informe/main.tex`, en la sección «Modelo C4», referencia exactamente estos tres
archivos. El exportador antepone `structurizr-`, así que hay que renombrarlos:

```bash
cd docs/informe/diagramas
for v in c4-c1-contexto c4-c2-contenedores c4-c3-componentes; do
  mv "structurizr-$v.png" "$v.png"
done
```

```text
docs/informe/diagramas/c4-c1-contexto.png
docs/informe/diagramas/c4-c2-contenedores.png
docs/informe/diagramas/c4-c3-componentes.png
```

Mientras un archivo no exista, el informe pinta el marco de «pendiente» en su lugar y
compila igual, así que la ausencia se ve en el PDF en vez de pasar inadvertida.

## Imágenes generadas

Exportadas con los comandos de arriba y versionadas junto al informe, que es quien las
incluye:

| Vista | Imagen |
|---|---|
| Contexto | [`c4-c1-contexto.png`](../../../informe/diagramas/c4-c1-contexto.png) |
| Contenedores | [`c4-c2-contenedores.png`](../../../informe/diagramas/c4-c2-contenedores.png) |
| Componentes | [`c4-c3-componentes.png`](../../../informe/diagramas/c4-c3-componentes.png) |
| Despliegue | [`c4-despliegue.png`](../../../informe/diagramas/c4-despliegue.png) |

Se regeneran desde el `.dsl` cada vez que el modelo cambie; no se editan a mano. Al
regenerarlas, **comprobar que la imagen de componentes no quede recortada**: mide unos
5000 px de ancho y el límite por defecto de PlantUML la corta en silencio.

## Lo que el modelo garantiza

- **Un solo contenedor desplegable y una sola base** (AD-01, regla 1).
- **MVC y REST comparten el servicio de cada entidad:** los dos controladores de un módulo
  apuntan al mismo componente, así que el diagrama no permite afirmar lógica duplicada
  (AD-02, reglas 2 y 12).
- **Los servicios dependen de interfaces DAO,** no de implementaciones (AD-03, AD-04).
- **Ningún controlador tiene una flecha hacia `PersonaDbContext`:** el único camino a los
  datos pasa por los repositorios EF Core (AK-01, regla 3).
- **Las dependencias entre módulos son las que exigen las reglas de SDD §7** y están
  rotuladas como tales: `PersonaService` consulta teléfonos y estudios para bloquear el
  borrado; `ProfesionService` consulta estudios; `TelefonoService` valida la persona dueña;
  `EstudioService` valida persona y profesión. No hay acoplamiento sin regla que lo exija.
- **Las vistas no inventan claves:** los selectores de teléfonos y estudios se alimentan
  del servicio dueño de cada entidad.
- **Nada fuera de `versiones.md`:** .NET 10, ASP.NET Core MVC 10, EF Core 10, SQL Server
  2025 Express, Swagger/OpenAPI 3 y, para el despliegue, las imágenes que ya declara
  `docker-compose.yml`.

## Correspondencia con el repositorio

Cada elemento dibujado corresponde a un archivo del repositorio o a la historia que lo
entrega, según el SDD. Comprobado contra `main` en `7d144fb` (28-10-2026).

| Componente del diagrama | Dónde vive | Historia |
|---|---|---|
| `HomeController` y convención de errores | `HomeController`, `AntiforgeryEnVistasConvention`, `ResultadoServicioVistaExtensions` | HU-006 |
| Vistas Razor y ViewModels | `Front/Views/`, `Front/ViewModels/` | HU-006 y el CRUD de cada módulo |
| `PersonasController` | `Back/01-Borde/Controllers/` | HU-007 |
| `ProfesionesController` | `Back/01-Borde/Controllers/` | HU-008 |
| `TelefonosController` | `Back/01-Borde/Controllers/` | HU-012 |
| `EstudiosController` | `Back/01-Borde/Controllers/` | HU-013 |
| `PersonasApiController` | `Back/01-Borde/ControllersApi/` | HU-004 |
| `ProfesionesApiController` | `Back/01-Borde/ControllersApi/` | HU-005 |
| `TelefonosApiController` | `Back/01-Borde/ControllersApi/` | HU-010 |
| `EstudiosApiController` | `Back/01-Borde/ControllersApi/` | HU-011 |
| DTOs | `Back/01-Borde/Dtos/` | la historia REST de cada entidad |
| Swagger / OpenAPI 3 | configuración de Swashbuckle en `Program.cs` | HU-001 y HU-009 |
| `PersonaService`, `ProfesionService`, `TelefonoService`, `EstudioService` | `Back/02-Aplicacion/Services/` | la historia REST de cada entidad |
| Las cuatro interfaces DAO y sus implementaciones | `Back/03-AccesoDatos/Repository/` | HU-003 |
| `PersonaDbContext` y entidades | `Back/04-Persistencia/Entities/` | HU-003 |
| `persona_db` | `scripts/01_ddl.sql`, `scripts/02_dml.sql` | HU-002 |
| `personapi`, `sqlserver`, `db-init` | `docker-compose.yml`, `Dockerfile` | despliegue declarado en el informe |

**Cuándo se actualiza este modelo.** Cuando cambie una decisión de arquitectura, no cuando
avance una historia: si una historia entrega lo que el diagrama ya declara, el diagrama no
cambia. Si el código se aparta del diseño, se reporta a quien mantiene el SAD o el SDD en
vez de redibujar el diagrama para que encaje.

## Nota de coherencia con el informe

El informe presenta el despliegue con Docker Compose y los archivos versionados coinciden
con su figura (servicios, imágenes, puertos y red), mientras el [README del
backlog](../../06-backlog/README.md) mantiene que ninguna historia lo toma como criterio de
aceptación. La vista de despliegue se incluye porque el informe la declara y describe lo
que dicen esos archivos, **sin afirmar** que se haya ejecutado en una entrega.
