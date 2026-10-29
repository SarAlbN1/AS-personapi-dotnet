#!/bin/sh
# Regenera las imagenes del modelo C4 desde personapi-c4.dsl.
#
#   ./docs/wiki/02-arquitectura/diagramas/render.sh
#
# Necesita Docker. Valida el modelo, exporta a PlantUML y convierte a PNG con los
# nombres que espera docs/informe/main.tex.
#
# PLANTUML_LIMIT_SIZE no es opcional: por defecto PlantUML recorta a 4096 px, la vista
# de componentes mide mas de 5000 y el recorte no produce ningun aviso.
set -e

RAIZ=$(cd "$(dirname "$0")/../../../.." && pwd)
cd "$RAIZ"
D=docs/wiki/02-arquitectura/diagramas
SALIDA=docs/informe/diagramas

VISTAS="c4-c1-contexto c4-c2-contenedores c4-c3-componentes c4-despliegue"

echo "== Validando el modelo =="
docker run --rm -v "$PWD/$D:/w" structurizr/structurizr \
  validate -workspace /w/personapi-c4.dsl

echo "== Exportando a PlantUML =="
rm -rf "$D/puml"
docker run --rm -v "$PWD/$D:/w" structurizr/structurizr \
  export -workspace /w/personapi-c4.dsl -format plantuml/structurizr -output /w/puml >/dev/null

echo "== Convirtiendo a PNG =="
mkdir -p "$SALIDA"
PUML=""
for v in $VISTAS; do PUML="$PUML structurizr-$v.puml"; done
# shellcheck disable=SC2086
docker run --rm --user root -e PLANTUML_LIMIT_SIZE=8192 \
  -v "$PWD/$D/puml:/data" -v "$PWD/$SALIDA:/out" -w /data \
  plantuml/plantuml -tpng -charset UTF-8 -o /out $PUML >/dev/null

# El exportador antepone structurizr-; main.tex espera los nombres sin prefijo.
cd "$SALIDA"
for v in $VISTAS; do mv -f "structurizr-$v.png" "$v.png"; done
cd "$RAIZ"
rm -rf "$D/puml"

# El nivel 4 (codigo) no sale del .dsl: Structurizr no dibuja ese nivel. Es un
# diagrama de clases escrito a mano en PlantUML, al lado del modelo.
echo "== Nivel 4 (codigo), escrito a mano =="
docker run --rm --user root -e PLANTUML_LIMIT_SIZE=8192 \
  -v "$RAIZ/$D:/data" -v "$RAIZ/$SALIDA:/out" -w /data \
  plantuml/plantuml -tpng -charset UTF-8 -o /out c4-c4-codigo-personaservice.puml >/dev/null

echo "== Listo =="
file "$SALIDA"/*.png | sed 's/, 8-bit.*//'
echo
echo "Revisar que ninguna imagen quede recortada: si una llega justo a 4096 px de ancho,"
echo "volver a ejecutar con un PLANTUML_LIMIT_SIZE mayor."
