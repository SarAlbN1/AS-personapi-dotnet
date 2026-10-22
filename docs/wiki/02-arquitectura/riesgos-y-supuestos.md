# Riesgos y supuestos

[← Arquitectura](README.md)

Las elecciones tecnológicas y de alcance se consideran **cerradas por el equipo**. Esta página enumera riesgos de implementación, no decisiones arquitectónicas pendientes.

| Riesgo | Mitigación |
|---|---|
| La guía antigua mezcla .NET 6/7 y SQL Server 2019/2022 | Línea única de diseño: .NET 10 LTS, EF Core 10, SQL Server 2025; registrar que es modernización del procedimiento |
| El scaffolding sobrescribe cambios manuales | No editar entidades generadas como fuente de verdad; mantener DDL, usar clases parciales/DTO cuando aplique |
| Referencias circulares al serializar EF | DTOs de salida, proyecciones explícitas, no exponer navegaciones directamente |
| Borrado con FKs provoca error 500 | Capturar y traducir colisiones de integridad a `409` |
| Doble escritura simultánea de una PK | PK en SQL es la protección definitiva; manejar la excepción además de la validación previa |
| Tres integrantes cambian `Program.cs` o esquema simultáneamente | Contratos estabilizados, un propietario por issue, PR pequeños y revisión cruzada |
| SQL Server local no disponible en todos los sistemas | Documentar entorno soportado y conexión a instancia SQL Server accesible; no añadir Docker como requisito tácito |

Si un requisito docente posterior prohíbe la actualización tecnológica, debe registrarse como cambio de alcance y no ocultarse.
