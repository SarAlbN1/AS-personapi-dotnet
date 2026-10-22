# Estilo y flujo end-to-end

[← Arquitectura](README.md)

## Diagrama lógico (HLD)

```mermaid
flowchart TB
    Browser[Navegador web] --> Mvc[MVC Controllers + Razor]
    Client[Swagger / cliente REST] --> Api[API Controllers]
    subgraph MONOLITO[ASP.NET Core MVC · monolito]
        Mvc --> Services[Interfaces y servicios de aplicación]
        Api --> Services
        Services --> Dao[Interfaces DAO + repositorios EF Core]
        Dao --> DbContext[PersonaDbContext + entidades]
    end
    DbContext --> SQL[(SQL Server · persona_db)]
```

No hay llamadas de red entre capas internas: son invocaciones de código en el mismo proceso.

## Flujo principal CRUD

```mermaid
sequenceDiagram
    actor U as Usuario
    participant P as MVC o API Controller
    participant S as Servicio
    participant R as DAO/Repository
    participant D as SQL Server
    U->>P: Crear registro
    P->>P: Validar entrada
    P->>S: CreateAsync(datos)
    S->>R: Verificar PK/FK y persistir
    R->>D: Consulta + INSERT
    D-->>R: Resultado
    R-->>S: Resultado
    S-->>P: Éxito o error de negocio
    P-->>U: Vista/redirect o HTTP 201/400/409
```

## Recorrido demostrable

Crear persona → crear profesión → asociar teléfono → asociar estudio → consultar por ambos canales → bloquear eliminación de persona con dependencias → eliminar dependientes → eliminar persona. Esto ejercita las cuatro tablas, reglas y capas.
