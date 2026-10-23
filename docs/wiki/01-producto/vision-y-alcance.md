# Visión y alcance

[← Producto](README.md)

## Problema que resuelve

Demostrar que una aplicación CRUD puede mantener separadas la presentación, la lógica de negocio y la persistencia, conservando un único despliegue. No se busca construir un sistema de gestión de personas de producción.

## Capacidades dentro del alcance

- CRUD **completo en vistas MVC Razor** y **API REST** para `persona`, `profesion`, `telefono` y `estudios`.
- 5 operaciones REST por recurso: listar, obtener por clave, crear, actualizar y eliminar, para un total de **20**.
- Swagger UI y documento OpenAPI 3.
- Reglas de dominio: duplicados, relaciones existentes y protección de eliminación con dependencias.
- SQL Server `persona_db`, DDL/DML versionados, Entity Framework Core Database First, DAO y servicios.
- README reproducible, código fuente, informe académico y URL del tag del repositorio público `personapi-dotnet`.

## Modelo de negocio

| Tabla | Clave | Relaciones |
|---|---|---|
| `persona` | `cc` | 1:N con teléfono y estudios |
| `profesion` | `id` | 1:N con estudios |
| `telefono` | `num` | `duenio` → persona.cc |
| `estudios` | (`id_prof`, `cc_per`) | FK a profesion.id y persona.cc |

## Fuera del alcance

Autenticación, autorización, microservicios, eventos, colas, gateway, frontend SPA y despliegue productivo. No añadir capacidades ajenas al laboratorio para simular complejidad arquitectónica.

Docker **no** figura en esta lista: es una propuesta de despliegue del equipo, documentada en el [informe](../../informe/main.tex), cuya obligatoriedad depende del enunciado original. Lo que queda fuera es tratarlo como requisito confirmado del docente sin ese respaldo, no la propuesta en sí.

**Diseño aceptado:** monolito en capas con dos interfaces de entrada (HTML/JSON) y un conjunto compartido de servicios y repositorios.
