# Trabajo con asistentes de IA

[← Proceso](README.md) · [Instrucciones principales](../../../CLAUDE.md)

Los asistentes son apoyo técnico; el integrante asignado sigue siendo responsable de verificar lo integrado. Antes de editar: leer `CLAUDE.md`, issue, requisitos relevantes, SAD, SDD y contrato afectado.

## Secuencia obligatoria

1. Identificar issue **real**, responsable, sprint y aceptación.
2. Revisar el estado del repositorio y crear rama propia desde `main` actualizado.
3. Indicar qué carpetas, rutas y tablas cambiará; si altera una decisión aprobada, reportar el impacto antes de implementarlo.
4. Implementar con la separación MVC/API → Service → DAO → EF Core → SQL Server.
5. Añadir las pruebas razonables para el cambio y ejecutarlas (no declarar resultados no ejecutados).
6. Actualizar contratos de REST, persistencia y wiki si corresponde.
7. Abrir PR con referencia a issue real; **otro integrante** revisa y aprueba.
8. No marcar `Done` hasta integrar y verificar los criterios. El estado del tablero se mueve según las [transiciones de estado](issues-etiquetas-y-milestones.md#transiciones-de-estado), que el asistente puede aplicar cuando el hecho que las dispara ya ocurrió.

## Prohibido

- Ampliar el stack más allá de [versiones](../04-implementacion/versiones.md) sin cambio aprobado del SRS y del SAD.
- Acceder a `DbContext` desde controladores, duplicar reglas entre UI y REST, cambiar el modelo sin actualizar DDL.
- Inventar credenciales, versiones, IDs de issues, pruebas aprobadas, capturas o despliegues.
- Subir commits a `main` (está protegida: sin push directo, sin force-push, sin excepciones para administradores) o editar ramas de otro integrante por iniciativa propia.
- Aprobar o fusionar un PR propio, o presentar como aprobado un PR que solo está libre de conflictos. La aprobación la da otra persona; el asistente solo la registra a petición expresa del revisor y en las condiciones de la [regla 11 de la política de integración](pull-requests-y-releases.md#reglas-de-integración). Del mismo modo, solo fusiona un PR ya aprobado con la cuenta del revisor, distinta de la del autor, según la regla 12.
- Transformar sugerencias técnicas en nuevos requisitos sin aprobación del equipo.
- Dejar rastro del asistente en el repositorio: líneas de co-autoría, firmas, menciones de la herramienta o marcas de generación automática en commits, PR, documentos, informe o logs. El autor registrado es el integrante asignado ([CONTRIBUTING](../../../CONTRIBUTING.md)).

## Reporte al final de una tarea

```markdown
### LAB-NN — Resultado
**Archivos modificados:** ...
**Criterios comprobados:** ...
**Pruebas ejecutadas y salida:** ...
**Reglas arquitectónicas verificadas:** ...
**Bloqueos o desviaciones:** ...
**PR:** ...
```

Si no hay evidencia, describirlo como **pendiente**, no como aprobado. La fuente viva de estados y propietarios es GitHub Projects.
