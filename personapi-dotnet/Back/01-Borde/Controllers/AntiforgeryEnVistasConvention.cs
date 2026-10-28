using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// Convención de antiforgery de HU-006 (SDD §6): todo POST, PUT, PATCH o DELETE de un
/// controlador MVC exige el token que el tag helper <c>&lt;form method="post"&gt;</c>
/// inserta solo. Así ningún CRUD web depende de acordarse de poner
/// <c>[ValidateAntiForgeryToken]</c> en cada acción.
/// </summary>
/// <remarks>
/// Los controladores con <c>[ApiController]</c> quedan fuera: la API REST no usa
/// formularios ni cookies, y exigir el token rompería Swagger y los clientes JSON.
/// </remarks>
public sealed class AntiforgeryEnVistasConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.Attributes.OfType<IApiBehaviorMetadata>().Any())
        {
            return;
        }

        controller.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
}
