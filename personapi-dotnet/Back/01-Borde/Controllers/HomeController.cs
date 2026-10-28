using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Página de ejemplo de HU-006: el formulario que demuestra la convención de errores.</summary>
    [HttpGet]
    public IActionResult Convenciones()
    {
        return View(new ConvencionesDemoViewModel());
    }

    /// <summary>
    /// Sigue el mismo flujo que un CRUD real (validación de forma, servicio, traducción del
    /// resultado), pero el resultado es <b>simulado</b>: aquí no hay servicio ni regla de
    /// negocio, solo el escenario que el usuario eligió en el formulario.
    /// </summary>
    [HttpPost]
    public IActionResult Convenciones(ConvencionesDemoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = modelo.Escenario switch
        {
            EscenarioDemo.ErrorDeValidacion => ServiceResult.ValidationError(
                $"«{modelo.Nombre}» no cumple una regla de dominio (resultado simulado)."),
            EscenarioDemo.Conflicto => ServiceResult.Conflict(
                $"Ya existe un registro con el nombre «{modelo.Nombre}» (resultado simulado)."),
            EscenarioDemo.NoEncontrado => ServiceResult.NotFound(
                $"El registro «{modelo.Nombre}» ya no existe (resultado simulado)."),
            _ => ServiceResult.Success()
        };

        return this.VistaSiFalla(resultado, modelo)
            ?? this.RedirigirConExito($"«{modelo.Nombre}» se guardó correctamente (resultado simulado).", nameof(Convenciones));
    }

    /// <summary>
    /// Destino de las respuestas de error sin cuerpo de las rutas MVC (ruta desconocida,
    /// POST sin token antiforgery). Lo invoca UseStatusCodePagesWithReExecute.
    /// </summary>
    /// <remarks>
    /// La re-ejecución conserva el verbo original: si el error vino de un POST, esta acción
    /// también recibe un POST y la convención de antiforgery la bloquearía. Solo muestra
    /// información, así que se exime del token. Lo mismo vale para <see cref="Error"/>.
    /// </remarks>
    [IgnoreAntiforgeryToken]
    public IActionResult Estado(int codigo)
    {
        return this.VistaDeEstado(codigo);
    }

    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
