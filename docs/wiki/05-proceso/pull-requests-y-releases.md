# Pull requests, revisión y versiones

[← Proceso](README.md)

## Plantilla mínima de PR

```markdown
## Issue
Closes #<numero-real> · LAB-NN

## Cambio realizado
- ...

## Contratos / arquitectura afectados
- Tablas, rutas, interfaces, capas o vistas: ...
- SRS/SAD/SDD/ADR actualizado cuando corresponda: ...

## Verificación
- [ ] dotnet build
- [ ] Pruebas del módulo y casos negativos
- [ ] Evidencia de operación E2E (si aplica)
- [ ] Sin controladores acoplados a DbContext
- [ ] Sin secretos ni dependencias fuera de alcance

## Resultado
- ...
```

**No usar `Closes #NN` si todavía no existe un número real de issue.** Una issue por PR funcional; cambios compartidos solo cuando son indispensables y están referenciados.

## Protección de `main` y revisión cruzada obligatoria

Esta sección es la **fuente única** de la política de integración. El resto de documentos la referencian y no la repiten.

### Reglas de integración

1. Todo cambio destinado a `main` pasa por una pull request. **Push directo a `main` prohibido.**
2. Cada PR necesita **al menos una aprobación de otro integrante** con permiso de escritura o superior.
3. **Nadie aprueba su propio PR.** La revisión del autor no cuenta como aprobación.
4. La regla aplica a los **tres integrantes, incluidos propietario y administradores**: los permisos administrativos no sirven para omitir la revisión.
5. **Una aprobación válida basta para integrar.** La condición es una aprobación de otro integrante vigente sobre el último commit, y que no quede ningún *Request changes* activo. Las conversaciones de revisión se responden, pero un hilo abierto **no bloquea** la integración: quien considere que una observación debe impedir la fusión lo hace con *Request changes*, no dejando el hilo sin resolver. Al aprobar, el revisor da por atendidas o no bloqueantes las observaciones abiertas.
6. Si se empujan commits que invalidan una aprobación previa, se solicita **nueva revisión**.
7. Prohibido el *force-push* para eludir el proceso, y prohibido eliminar la rama `main`.
8. Prohibido deshabilitar o modificar las reglas de protección de GitHub sin acuerdo explícito del equipo.
9. **Que GitHub muestre `MERGEABLE` o `CLEAN` no es una aprobación.** Significa solo que no hay conflictos de merge.
10. Cada historia se implementa en su propia rama y se integra con **squash merge**.
11. **Aprobación registrada por un agente.** Un asistente automatizado puede registrar la aprobación con la cuenta del revisor **solo** si se cumplen todas estas condiciones:
    1. El revisor la pide expresamente, para ese PR concreto, después de conocer el resultado de la revisión.
    2. La cuenta que aprueba no es la del autor del PR (la regla 3 no cambia).
    3. Antes de aprobar, el agente ejecutó sobre el último commit del PR las verificaciones que el [DoD](dor-y-dod.md) exige para ese tipo de cambio y publicó sus hallazgos en el PR.
    4. Ningún commit del PR fue creado ni subido con la cuenta del revisor, y el agente que aprueba no redactó ni modificó el contenido del PR.

    La aprobación cuenta como del revisor, que responde por ella como si la hubiera registrado él.
12. **Integración registrada por un agente.** Un asistente automatizado puede fusionar el PR con la cuenta del revisor **solo** si se cumplen todas estas condiciones:
    1. El revisor lo pide expresamente, para ese PR concreto.
    2. El PR ya tiene una aprobación válida, vigente sobre el último commit, de una cuenta distinta de la del autor.
    3. La cuenta que fusiona no es la del autor del PR.
    4. Se cumplen las reglas 5 a 10: ningún *Request changes* activo y ninguna aprobación invalidada por commits nuevos.
    5. La fusión se hace **solo** con *squash merge* y borrando la rama integrada, como pide la [plantilla de PR](../../../.github/pull_request_template.md): `gh pr merge <número> --squash --delete-branch`. Nunca *merge commit* ni *rebase*, aunque el repositorio los tenga habilitados.
    6. La fusión no usa permisos administrativos, auto-merge ni ninguna vía que omita la protección de `main`. Si GitHub la rechaza, el agente se detiene e informa.

    La integración cuenta como del revisor, que responde por ella.

### Procedimiento por historia

1. Tomar una HU asignada en GitHub Projects.
2. Actualizar `main` local desde el remoto (`git switch main && git pull --ff-only`).
3. Crear la rama de la HU con el formato de [ramas y commits](git-ramas-y-commits.md).
4. Implementar **solo** su alcance.
5. Ejecutar las pruebas y verificaciones que exige el [DoD](dor-y-dod.md).
6. Abrir PR hacia `main` con la [plantilla](../../../.github/pull_request_template.md).
7. Solicitar revisión a otro integrante.
8. Atender las observaciones y responder las conversaciones.
9. Obtener una aprobación válida (de otra persona).
10. Integrar con squash merge cuando se cumpla todo lo anterior.

### Configuración verificada en GitHub

Comprobada sobre `main` el 22 de octubre de 2026 con `gh api repos/{owner}/{repo}/branches/main/protection`. Es el estado real en ese momento, no una intención:

| Protección | Estado |
|---|---|
| Pull request obligatoria antes de integrar | activa |
| Aprobaciones requeridas | 1 |
| Descartar aprobaciones obsoletas al llegar commits nuevos | activa |
| Exigir aprobación del último push revisable | activa |
| Resolución de conversaciones antes de integrar | activa |
| Historia lineal obligatoria | activa |
| Sin excepciones para administradores (`enforce_admins`) | activa |
| Force push | bloqueado |
| Eliminación de la rama | bloqueada |
| Bloqueo total de la rama (`lock_branch`) | desactivado, como se quiere |
| Commits firmados | no exigidos |
| Comprobaciones de estado obligatorias | ninguna, porque todavía no hay CI |

**Pendiente de aplicar en GitHub:** desde que la regla 5 ya no exige resolver conversaciones, la fila «Resolución de conversaciones antes de integrar» queda como protección a desactivar (*Require conversation resolution before merging*). La desactiva quien administra el repositorio, y esta fila se actualiza con la fecha de esa comprobación. Mientras siga activa, GitHub seguirá exigiendo resolver los hilos.

La integración preferida es **squash and merge**. Si una protección se modifica en GitHub, esta tabla debe actualizarse en el mismo PR que la cambie: no se afirma como activa una protección que no se haya comprobado.

### Revisión de contenido

- Autor distinto de revisor; quien encuentra un defecto lo informa con evidencia, quien es responsable lo corrige en su rama.
- El PR se revisa contra criterios de aceptación y [reglas arquitectónicas](../02-arquitectura/reglas-arquitectonicas.md), no solo estilo.
- `main` siempre debe compilar. Borrar la rama tras integrar.
- No atribuir tareas a asistentes como sustituto de responsabilidad humana; seguir las reglas de transparencia académica que indique el docente.

## Versionado y entrega

- `v0.1.0` y `v0.2.0` son **opcionales** para cortes internos de Sprint 1 y Sprint 2.
- Tag final **`v1.0.0`** apunta a commit integrado y verificado en `main`.

```bash
git switch main
git pull --ff-only
git tag -a v1.0.0 -m "PersonAPI Laboratorio 1: entrega final"
git push origin v1.0.0
```

Antes del tag, verificar README de instalación, DDL/DML, código fuente, documento académico con portada/marco conceptual/diseño/procedimiento/conclusiones/referencias y los flujos de demo. El enlace entregado debe apuntar al tag publicado.
