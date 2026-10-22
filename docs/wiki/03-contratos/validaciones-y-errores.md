# Validaciones y tratamiento de errores

[← Contratos](README.md)

**Entrada:** comprobar campos obligatorios, longitudes, tipos, identificadores, rango razonable de edad y dominio `genero` (`M`/`F`) antes de escribir. Mantener las restricciones finales en SQL.

**Persona:** `cc` único; no eliminar con teléfonos o estudios asociados.

**Profesión:** `id` único; no eliminar si existen estudios asociados.

**Teléfono:** `num` único y `duenio` debe referenciar persona existente. No cambiar arbitrariamente la PK al actualizar: si el identificador de ruta no coincide con el cuerpo, `400`.

**Estudio:** PK compuesta única; ambas FKs existentes, `fecha` válida; al actualizar no cambiar la pareja identificadora sin operación explícita de eliminación/creación.

**Manejo:** API con códigos 400/404/409 documentados, MVC con mensajes visibles y estado de formulario preservado. No revelar trazas SQL ni secretos al cliente. Las violaciones de SQL conocidas se traducen en conflicto o error de entrada; lo inesperado se registra y se responde sin exponer detalles internos.
