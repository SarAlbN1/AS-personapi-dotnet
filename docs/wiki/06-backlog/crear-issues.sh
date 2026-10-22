#!/usr/bin/env bash
# Crea de forma reproducible las etiquetas, milestones e issues de PersonAPI a partir de backlog.tsv.
# Simulacion por defecto: sin --apply no se ejecuta ninguna accion de escritura en GitHub.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TSV="$SCRIPT_DIR/backlog.tsv"
EQUIPO="$SCRIPT_DIR/equipo.tsv"
REPO=""
APPLY=0

usage() {
  cat <<'USAGE'
Uso: crear-issues.sh --repo OWNER/REPO [--apply] [--tsv RUTA]

  --repo    Repositorio destino, por ejemplo OWNER/personapi-dotnet (obligatorio).
  --apply   Ejecuta la creacion real. Sin este indicador solo simula (dry-run).
  --tsv     Ruta alternativa al TSV (por defecto backlog.tsv junto al script).
  --equipo  Ruta alternativa al mapeo marcador -> usuario de GitHub (por defecto equipo.tsv).

Requiere gh autenticado con permiso de escritura sobre el repositorio.
Los marcadores A/B/C se traducen con equipo.tsv. Si una fila de equipo.tsv no tiene
usuario de GitHub, la issue se crea SIN asignar y el script lo advierte: no se inventan
cuentas. No crea el GitHub Project: el estado se gestiona en el tablero.
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo) REPO="${2:-}"; shift 2 ;;
    --apply) APPLY=1; shift ;;
    --tsv) TSV="${2:-}"; shift 2 ;;
    --equipo) EQUIPO="${2:-}"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Argumento no reconocido: $1" >&2; usage; exit 2 ;;
  esac
done

[[ -n "$REPO" ]] || { echo "ERROR: falta --repo OWNER/REPO" >&2; exit 2; }
[[ -f "$TSV" ]] || { echo "ERROR: no existe el TSV: $TSV" >&2; exit 2; }
command -v gh >/dev/null || { echo "ERROR: gh no esta instalado" >&2; exit 2; }

run() { # ejecuta o simula
  if [[ $APPLY -eq 1 ]]; then
    "$@"
  else
    printf '  [dry-run]'; printf ' %q' "$@"; printf '\n'
  fi
}

# ---------- 1. Validacion del TSV ----------
echo "== Validacion de $TSV =="
awk -F'\t' '
  NR==1 { if (NF!=15) { printf "ERROR: la cabecera tiene %d columnas, se esperan 15\n", NF; bad=1 } next }
  { filas++
    if (NF!=15) { printf "ERROR: %s tiene %d columnas\n", $1, NF; bad=1 }
    if ($1 in seen) { printf "ERROR: ID duplicado %s\n", $1; bad=1 } ; seen[$1]=1
    if ($1 !~ /^HU-[0-9][0-9][0-9]$/) { printf "ERROR: ID mal formado: %s\n", $1; bad=1 }
    if ($4 != 1 && $4 != 2) { printf "ERROR: %s sprint invalido: %s\n", $1, $4; bad=1 }
    if ($6 != "P0" && $6 != "P1") { printf "ERROR: %s prioridad invalida: %s\n", $1, $6; bad=1 }
    if ($7 !~ /^[0-9]+$/) { printf "ERROR: %s puntos no numericos: %s\n", $1, $7; bad=1 }
    if ($8 !~ /^[ABC]$/) { printf "ERROR: %s responsable invalido: %s\n", $1, $8; bad=1 }
    if ($5 !~ /^(Base|Datos|Personas|Profesiones|Telefonos|Estudios|Pruebas|Entrega)$/) { printf "ERROR: %s modulo fuera de la taxonomia Module: %s\n", $1, $5; bad=1 }
    if ($3 !~ /^EP-0[0-5]$/) { printf "ERROR: %s epica invalida: %s\n", $1, $3; bad=1 }
    if ($13 == "") { printf "ERROR: %s sin criterios de aceptacion\n", $1; bad=1 }
    sprint[$4]++; porpersona[$8"-"$4]++; issues[$8]++; puntos[$8]+=$7; total+=$7
    ids[$1]=1; deps[$1]=$10
  }
  END {
    if (filas != 15) { printf "ERROR: se esperan 15 historias, hay %d\n", filas; bad=1 }
    if (sprint[1] != 6) { printf "ERROR: Sprint 1 tiene %d historias, se esperan 6\n", sprint[1]; bad=1 }
    if (sprint[2] != 9) { printf "ERROR: Sprint 2 tiene %d historias, se esperan 9\n", sprint[2]; bad=1 }
    split("A B C", p, " ")
    for (i=1; i<=3; i++) {
      k=p[i]
      if (porpersona[k"-1"] != 2) { printf "ERROR: %s tiene %d issues en Sprint 1, se esperan 2\n", k, porpersona[k"-1"]; bad=1 }
      if (porpersona[k"-2"] != 3) { printf "ERROR: %s tiene %d issues en Sprint 2, se esperan 3\n", k, porpersona[k"-2"]; bad=1 }
    }
    # dependencias: existentes y solo hacia historias anteriores (evita ciclos y bloqueos regresivos)
    for (id in deps) {
      n = split(deps[id], d, /; */)
      for (j=1; j<=n; j++) {
        dep=d[j]; gsub(/^ +| +$/, "", dep)
        if (dep == "") continue
        if (!(dep in ids)) { printf "ERROR: %s depende de %s, que no existe\n", id, dep; bad=1 }
        else if (dep >= id) { printf "ERROR: %s depende de %s, que no es anterior\n", id, dep; bad=1 }
      }
    }
    printf "  historias: %d | Sprint 1: %d | Sprint 2: %d | puntos: %d\n", filas, sprint[1], sprint[2], total
    for (i=1; i<=3; i++) { k=p[i]; printf "  %s: %d issues, %d puntos\n", k, issues[k], puntos[k] }
    if (bad) { print "VALIDACION FALLIDA"; exit 1 }
    print "  validacion OK"
  }
' "$TSV"

echo
if [[ $APPLY -eq 1 ]]; then
  echo "== MODO APPLY: se escribira en $REPO =="
else
  echo "== MODO DRY-RUN: no se escribira nada en $REPO (usa --apply para crear) =="
fi
echo

# ---------- 2. Etiquetas ----------
echo "== Etiquetas =="
existentes_labels="$(gh label list --repo "$REPO" --limit 200 2>/dev/null | cut -f1 || true)"
# Pares etiqueta=color. Lista simple por portabilidad: bash 3.2 (macOS) no tiene arrays asociativos.
LABELS_COLORES="
area:repo=5319e7
area:core=1d76db
area:database=0e8a16
area:personas=fbca04
area:profesiones=d93f0b
area:telefonos=c2e0c6
area:estudios=bfd4f2
area:api=006b75
area:mvc=f9d0c4
area:qa=b60205
area:testing=e99695
area:docs=0075ca
priority:P0=b60205
priority:P1=fbca04
"
while IFS= read -r par; do
  [ -z "$par" ] && continue
  label="${par%%=*}"; color="${par##*=}"
  if printf '%s\n' "$existentes_labels" | grep -qxF "$label"; then
    echo "  existe: $label"
  else
    echo "  crear: $label"
    run gh label create "$label" --repo "$REPO" --color "$color"
  fi
done <<EOF_LABELS
$LABELS_COLORES
EOF_LABELS

# ---------- 3. Milestones ----------
echo
echo "== Milestones =="
milestones_json="$(gh api "repos/$REPO/milestones?state=all" --jq '.[].title' 2>/dev/null || true)"
for ms in "Sprint 1" "Sprint 2"; do
  if grep -qxF "$ms" <<<"$milestones_json"; then
    echo "  existe: $ms"
  else
    echo "  crear: $ms"
    run gh api "repos/$REPO/milestones" -f title="$ms" -f state=open
  fi
done

# ---------- 3b. Mapeo de responsables ----------
# bash 3.2 (macOS) no tiene arrays asociativos: una variable por marcador.
GH_USER_A=""; GH_USER_B=""; GH_USER_C=""
gh_user_de() { # $1 = marcador A|B|C
  case "$1" in
    A) printf '%s' "$GH_USER_A" ;;
    B) printf '%s' "$GH_USER_B" ;;
    C) printf '%s' "$GH_USER_C" ;;
  esac
}
if [[ -f "$EQUIPO" ]]; then
  echo
  echo "== Responsables ($EQUIPO) =="
  while IFS=$'\t' read -r marcador nombre usuario; do
    [[ "$marcador" == "marcador" || -z "$marcador" ]] && continue
    usuario="$(printf '%s' "${usuario:-}" | tr -d '\r\n ')"
    case "$marcador" in
      A) GH_USER_A="$usuario" ;;
      B) GH_USER_B="$usuario" ;;
      C) GH_USER_C="$usuario" ;;
      *) echo "  AVISO: marcador no reconocido en $EQUIPO: $marcador"; continue ;;
    esac
    if [[ -n "$usuario" ]]; then
      echo "  $marcador = $nombre -> @$usuario"
    else
      echo "  $marcador = $nombre -> SIN usuario de GitHub: sus issues se crearan sin asignar"
    fi
  done < "$EQUIPO"
else
  echo "  AVISO: no existe $EQUIPO; las issues se crearan sin asignar"
fi

# ---------- 4. Issues ----------
echo
echo "== Issues =="
titulos_existentes="$(gh issue list --repo "$REPO" --state all --limit 500 --json title --jq '.[].title' 2>/dev/null || true)"

creadas=0; omitidas=0
while IFS=$'\t' read -r id titulo epica sprint modulo prioridad puntos responsable labels dependencias trazabilidad objetivo aceptacion evidencia epica_doc; do
  [[ "$id" == "id" ]] && continue
  [[ -z "$id" ]] && continue

  if grep -qF "$id" <<<"$titulos_existentes"; then
    echo "  omitida (ya existe una issue con $id): $id"
    omitidas=$((omitidas+1))
    continue
  fi

  cuerpo_file="$(mktemp)"
  {
    echo "## Objetivo"
    echo
    echo "$objetivo"
    echo
    echo "## Identificacion"
    echo
    echo "| Campo | Valor |"
    echo "|---|---|"
    echo "| ID | \`$id\` |"
    echo "| Epica | $epica |"
    echo "| Sprint | $sprint |"
    echo "| Modulo | $modulo |"
    echo "| Prioridad | $prioridad |"
    echo "| Puntos | $puntos |"
    resp_gh="$(gh_user_de "$responsable")"
    if [[ -n "$resp_gh" ]]; then
      echo "| Responsable | @$resp_gh (integrante $responsable del plan de sprints) |"
    else
      echo "| Responsable | Integrante $responsable (pendiente de asignar al usuario real) |"
    fi
    echo
    echo "## Criterios de aceptacion"
    echo
    printf '%s\n' "$aceptacion" | tr '|' '\n' | sed -e 's/^ *//' -e 's/ *$//' -e '/^$/d' -e 's/^/- [ ] /'
    echo
    echo "## Evidencia minima"
    echo
    echo "$evidencia"
    echo
    echo "## Dependencias"
    echo
    if [[ -z "$dependencias" ]]; then
      echo "Ninguna."
    else
      echo "Bloqueantes: $dependencias"
    fi
    echo
    echo "## Trazabilidad"
    echo
    echo "$trazabilidad"
    echo
    echo "Alcance tecnico completo y contexto de la epica: [\`docs/wiki/06-backlog/$epica_doc\`](docs/wiki/06-backlog/$epica_doc)"
    echo
    echo "## Definition of Done"
    echo
    echo "Aplica el [DoD del proyecto](docs/wiki/05-proceso/dor-y-dod.md): criterios cumplidos, \`dotnet build\` y pruebas aplicables en verde, persistencia verificada cuando corresponda, direcciones de dependencia respetadas, documentacion afectada actualizada, PR revisado por otro integrante y fusionado con squash."
  } > "$cuerpo_file"

  echo "  crear: $id - $titulo"
  asignado="$(gh_user_de "$responsable")"
  if [[ $APPLY -eq 1 ]]; then
    gh issue create --repo "$REPO" \
      --title "$id · $titulo" \
      --body-file "$cuerpo_file" \
      --milestone "Sprint $sprint" \
      ${asignado:+--assignee "$asignado"} \
      $(printf -- '--label %s ' ${labels//,/ })
  else
    echo "    [dry-run] gh issue create --repo $REPO --title \"$id · $titulo\" --milestone \"Sprint $sprint\" --label ${labels}${asignado:+ --assignee $asignado}"
    echo "    [dry-run] cuerpo: $(wc -l < "$cuerpo_file" | tr -d ' ') lineas"
  fi
  rm -f "$cuerpo_file"
  creadas=$((creadas+1))
done < "$TSV"

echo
echo "== Resumen =="
echo "  issues a crear: $creadas | omitidas por duplicado: $omitidas"
if [[ $APPLY -eq 0 ]]; then
  echo "  No se ejecuto ninguna escritura. Repite con --apply cuando el equipo apruebe el backlog."
fi
echo "  Paso manual posterior: añadir las issues al Project 'PersonAPI - Laboratorio MVC DAO' y"
echo "  fijar los campos Sprint/Module/Priority/Points (Module: Base, Datos, Personas, Profesiones,"
echo "  Telefonos, Estudios, Pruebas, Entrega). La asignacion de personas sale de equipo.tsv."
