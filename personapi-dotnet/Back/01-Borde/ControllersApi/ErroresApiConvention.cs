using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Documenta el <c>500</c> en todas las operaciones de los controladores
/// <c>[ApiController]</c> (HU-009). Cualquier operación puede fallar de forma inesperada
/// (por ejemplo, sin conexión con la base); la respuesta es un ProblemDetails sin
/// detalles internos, así que se declara una vez aquí y no en cada acción.
/// </summary>
public sealed class ErroresApiConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (!controller.Attributes.OfType<IApiBehaviorMetadata>().Any())
        {
            return;
        }

        controller.Filters.Add(new ProducesResponseTypeAttribute(
            typeof(ProblemDetails), StatusCodes.Status500InternalServerError)
        {
            Description = "Error inesperado (por ejemplo, la base no responde). El ProblemDetails no incluye trazas, SQL ni la cadena de conexión."
        });
    }
}
