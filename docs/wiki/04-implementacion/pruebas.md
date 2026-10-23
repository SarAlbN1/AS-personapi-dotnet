# Pruebas y verificación

[← Implementación](README.md) · [Evidencias registradas](pruebas/README.md)

## Pirámide mínima

1. **Servicios:** pruebas de reglas (duplicados, FKs, conflictos) con DAOs controlables.
2. **Integración SQL:** comprobar que las restricciones reales impiden duplicados y huérfanos; no basta simular el repositorio.
3. **API:** comprobar los cinco verbos/rutas por recurso y estados HTTP, mediante Swagger o cliente automatizado.
4. **MVC:** comprobar cinco acciones/vistas por entidad, selectores de FK, validaciones y mensajes.
5. **E2E:** crear persona y profesión, asignar teléfono/estudio, consultar por ambos canales, comprobar borrado bloqueado y limpieza ordenada.

## Evidencia requerida en PR

- Comandos ejecutados y resultado de compilación/pruebas.
- Endpoint y caso concreto, resultado esperado vs obtenido.
- Captura o salida suficiente si la prueba es manual.
- Lista de criterios de aceptación satisfechos; fallos conocidos enlazados a issue.

No cerrar por tener únicamente pruebas unitarias verdes si la issue promete un recorrido completo con base real.

## Dónde queda el resultado

Esta página define **cómo** se prueba; los [atributos de calidad](../02-arquitectura/atributos-de-calidad.md) definen **qué** debe cumplirse. El resultado medido de las comprobaciones transversales se registra en [`pruebas/`](pruebas/README.md), con fecha, entorno y comando. La salida de las pruebas de cada issue vive en su PR.
