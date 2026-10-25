# Configuración y ejecución local

[← Implementación](README.md)

## Prerrequisitos

SDK .NET 10, SQL Server 2025 Express accesible, cliente SQL (SSMS 22 en Windows) y un editor compatible (Visual Studio 2026 Community). Dependencias NuGet compatibles con EF Core 10 y proveedor SQL Server.

## Secuencia de puesta en marcha (una vez implementada)

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

**Estado verificado (HU-001, SDK 10.0.402):** los pasos 4 a 6 se ejecutaron sin base de datos: `dotnet build` y `dotnet test` en verde, `/` responde 200, los estáticos 200 y `/swagger/v1/swagger.json` devuelve `"openapi": "3.0.4"` sin operaciones. El paso 2 está verificado en HU-002 sobre SQL Server 2025 Express, con reejecución y pruebas negativas ([scripts](../../../scripts/README.md)); los pasos 3 y 7 los verifica HU-003.

## Database First

Generar entidades en `Back/04-Persistencia/Entities` desde un esquema SQL existente. El ejemplo de comando debe adaptarse a la instancia real y a la configuración local; no pegar contraseñas en la shell ni en el repositorio. Regenerar tras cambios de DDL controlados.

**No fingir ejecución:** los pasos y rutas deben actualizarse con los comandos reales y evidencia del equipo durante el desarrollo.

**Alternativa con contenedores.** Esta página describe el camino local, que es el que se usa para verificar las historias. El [informe](../../informe/main.tex) documenta además el despliegue con `docker compose up --build -d`, que publica la aplicación en el puerto 8080 y levanta SQL Server y la carga de `scripts/*.sql` como servicios. Quien lo use lee la contraseña de un archivo `.env` **no versionado**, nunca de un valor escrito en el repositorio.
