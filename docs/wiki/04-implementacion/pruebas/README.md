# Evidencias de pruebas ejecutadas

[← Implementación](../README.md) · [Cómo se prueba](../pruebas.md) · [Índice de la wiki](../../Home.md)

Esta carpeta registra **lo que se ejecutó realmente** sobre PersonAPI. La distinción es deliberada:

| Documento | Responsabilidad |
|---|---|
| [Atributos de calidad verificables](../../02-arquitectura/atributos-de-calidad.md) | **Qué** debe cumplir la solución y con qué escenario se comprueba |
| [Pruebas y verificación](../pruebas.md) | **Cómo** se prueba: pirámide, alcance por capa y evidencia exigida en el PR |
| Páginas de esta carpeta | **Qué resultado se obtuvo**: fecha, entorno, comando y valor medido |

## Páginas

| Página | Qué registra | Estado |
|---|---|---|
| [Atributos de calidad medidos](atributos-de-calidad.md) | Una comprobación por cada atributo de la página de criterios, con el comando que la reproduce | Sin ejecutar |
| [Cobertura CRUD](cobertura-crud.md) | Las 20 operaciones REST y las 20 acciones MVC, una fila por operación | Ejecutada (HU-014) |
| [Revisión de entrega](revision-entrega.md) | Clon limpio, secretos, revisión arquitectónica (AK-01 a AK-07), informe y tag | Ejecutada (HU-015) |

## Reglas de registro

1. Una fila se completa **solo tras ejecutar**. Mientras no haya ejecución, el valor es `Pendiente`: no se escriben resultados estimados, supuestos ni copiados de otro proyecto.
2. Cada página anota **fecha, rama o commit, entorno** (versión del SDK, instancia de SQL Server, estado de `persona_db`) y el **comando o la ruta HTTP exacta** que reproduce el resultado.
3. Se registra el valor observado, no el deseado. Un criterio que no se cumple se anota como no cumplido y se enlaza la issue que lo corrige.
4. Las pruebas unitarias y de integración de cada servicio **no** se transcriben aquí: se ejecutan en el PR de su issue y su salida vive en ese PR. Aquí solo se registran las comprobaciones transversales que respaldan una afirmación de la arquitectura o de los criterios de éxito.
5. Una captura de pantalla acompaña, no sustituye, la salida del comando.

Los [criterios de éxito](../../01-producto/criterios-de-exito.md) solo pueden declararse cumplidos con una fila completada en esta carpeta o con evidencia equivalente en un PR.
