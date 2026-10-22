# Bootstrap de `personapi-dotnet`

[← Proceso](README.md)

**Objetivo:** publicar repositorio académico y preparar trabajo de 3 integrantes. Este runbook no crea código funcional ni issues sin autorización.

## Prerrequisitos

Git y GitHub CLI (`gh`) instalados, autenticación activa, propietario de repo y usernames reales de los tres integrantes. El repositorio será **público** y su nombre debe ser `personapi-dotnet`.

## Pasos

```bash
git --version
gh --version
gh auth status
# Ejecutar en carpeta personapi-dotnet, ya con README, CLAUDE, CONTRIBUTING y docs/wiki/
git init -b main
git add .
git commit -m "chore(repo): inicializa documentación y estructura del laboratorio"
# Reemplazar OWNER por el usuario u organización real
gh repo create OWNER/personapi-dotnet --public --source=. --remote=origin --push
```

Si el repositorio ya existe, **no** repetir `git init` ni `gh repo create`: clonar y proponer cambios por PR. Antes de publicar, revisar que `.gitignore` excluya `bin/`, `obj/`, `.vs/`, `.env` y archivos de secretos.

## Configurar en GitHub

- Colaboradores: invitar a los otros dos con sus nombres de usuario reales.
- `main`: PR obligatorio, al menos una revisión cruzada, squash merge, sin force-push.
- Milestones `Sprint 1` y `Sprint 2`.
- GitHub Project: `PersonAPI - Laboratorio MVC DAO`; Status = Backlog, In Progress, In Review, Blocked, Done; Sprint = 1/2; Module = Core, Database, Personas, Profesiones, Telefonos, Estudios, API, QA, Deploy; Assignee = integrante real.
- Issues: crear las 15 **solo al aprobar el backlog arquitectónico**; no reutilizar números de otro proyecto.

## Verificación

```bash
gh repo view OWNER/personapi-dotnet --json url,visibility,defaultBranchRef
gh issue list --repo OWNER/personapi-dotnet
```

**Resultado esperado:** repositorio público y `main` visible; wiki navegable; reglas de contribución disponibles; tablero configurado. Si algo falla, registrar el problema en el PR/issue correspondiente, nunca declarar que está resuelto sin comprobar.
