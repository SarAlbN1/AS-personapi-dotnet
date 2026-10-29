# Configuración y ejecución local

[← Implementación](README.md)

## Prerrequisitos

SDK .NET 10, SQL Server 2025 Express accesible, cliente SQL (SSMS 22 en Windows) y un editor compatible (Visual Studio 2026 Community). Dependencias NuGet compatibles con EF Core 10 y proveedor SQL Server.

## Secuencia de puesta en marcha

1. Clonar el repositorio público `personapi-dotnet`.
2. Configurar SQL Server Express local y ejecutar `scripts/01_ddl.sql`, luego `scripts/02_dml.sql`.
3. Definir `ConnectionStrings__PersonaDb` con variable de entorno o User Secrets. El valor exacto depende de la instancia; **no se publica la contraseña**.
4. Restaurar y compilar:

```bash
dotnet restore
dotnet build
```

5. Ejecutar desde el directorio del proyecto web:

```bash
dotnet run --project personapi-dotnet/personapi-dotnet.csproj
```

6. Usar la URL que muestre Kestrel; abrir la aplicación MVC y `/swagger` sobre ese mismo origen.
7. Comprobar CRUD de las cuatro entidades y persistencia en `persona_db`.

**Estado verificado.** La secuencia completa se ejecutó sobre un clon limpio de `main`, por este camino y por Docker Compose, contra una base creada desde cero. El detalle está en la [revisión de entrega](pruebas/revision-entrega.md) y en el [runbook](runbook-demo.md#ensayos-ejecutados). Sin la cadena definida, las pruebas que usan `persona_db` se omiten.

## Database First

Generar entidades en `Back/04-Persistencia/Entities` desde un esquema SQL existente. El ejemplo de comando debe adaptarse a la instancia real y a la configuración local; no pegar contraseñas en la shell ni en el repositorio. Regenerar tras cambios de DDL controlados.

**No fingir ejecución:** los pasos y rutas deben actualizarse con los comandos reales y evidencia del equipo durante el desarrollo.

**Alternativa con contenedores.** Esta página describe el camino local, que es el que se usa para verificar las historias. El [informe](../../informe/main.tex) documenta además el despliegue con `docker compose up --build -d`, que publica la aplicación en el puerto 8080 y levanta SQL Server y la carga de `scripts/*.sql` como servicios. Quien lo use lee la contraseña de un archivo `.env` **no versionado**, nunca de un valor escrito en el repositorio.
