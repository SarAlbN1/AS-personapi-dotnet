# Back

Todo lo que ejecuta lógica. Las cuatro carpetas son back, incluida `01-Borde/`.

El número fija la posición en el flujo y la dirección permitida de las llamadas: una
capa solo invoca a la de número inmediatamente superior. **Saltar capas está
prohibido.**

```text
01-Borde  →  02-Aplicacion  →  03-AccesoDatos  →  04-Persistencia  →  SQL Server
```

| Nº | Capa | Qué hace | Nunca toca |
|---|---|---|---|
| [01](01-Borde/README.md) | Borde HTTP | Controllers MVC, ControllersApi REST, Dtos | `DbContext`, EF Core, SQL |
| [02](02-Aplicacion/README.md) | Aplicación | Reglas de negocio y casos de uso | Razor, `HttpContext`, EF Core |
| [03](03-AccesoDatos/README.md) | Acceso a datos | DAO con EF Core | HTTP, HTML, reglas de negocio |
| [04](04-Persistencia/README.md) | Persistencia | Entidades y `PersonaDbContext` | Servicios, presentación |

## Qué entra por aquí

Dos canales paralelos, un solo cerebro:

```text
Navegador       →  01-Borde/Controllers/      ┐
                                              ├→  02 → 03 → 04 → SQL Server
Swagger / REST  →  01-Borde/ControllersApi/   ┘
```

Ambos consumen los mismos `I*Service` de `02-Aplicacion`. Por eso una regla de negocio
se escribe **una sola vez** y los dos canales la respetan.

## Qué sale hacia el front

`01-Borde/Controllers/` entrega un **ViewModel** en memoria a
[`Front/Views/`](../Front/README.md). No hay llamada HTTP entre back y front: es una
invocación de código en el mismo proceso.

[← Mapa de capas](../README.md) · [Front](../Front/README.md)
