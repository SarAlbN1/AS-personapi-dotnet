using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// Deja los controladores MVC fuera del documento OpenAPI (HU-009): Swagger publica las
/// 20 operaciones de <c>/api</c> y nada más.
/// </summary>
/// <remarks>
/// El explorador de API incluye toda acción con ruta por atributo, aunque devuelva una
/// vista. Estudios y teléfonos usan esas rutas para llevar su clave en la URL, así que sin
/// esta convención Swagger mostraba sus diez acciones web como si fueran parte del contrato.
/// Se decide aquí, una vez, igual que el antiforgery, y no con un atributo en cada controlador.
/// </remarks>
public sealed class VistasFueraDeOpenApiConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.Attributes.OfType<IApiBehaviorMetadata>().Any())
        {
            return;
        }

        controller.ApiExplorer.IsVisible = false;
    }
}
