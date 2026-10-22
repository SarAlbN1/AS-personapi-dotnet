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

## Revisión cruzada (3 integrantes)

- Autor distinto de revisor; quien encuentra un defecto lo informa con evidencia, quien es responsable lo corrige en su rama.
- El PR se revisa contra criterios de aceptación y [reglas arquitectónicas](../02-arquitectura/reglas-arquitectonicas.md), no solo estilo.
- Nadie integra PR propio sin revisión; los cambios después de la aprobación requieren nueva revisión si la alteran.
- Preferir squash merge y borrar rama tras integrar; `main` siempre debe compilar.
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
