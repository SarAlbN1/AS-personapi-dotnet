using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// Convención de HU-006 para que los cuatro CRUD web muestren los resultados de los
/// servicios (SDD §2) igual y sin repetir la traducción en cada acción:
/// <list type="bullet">
/// <item><c>ValidationError</c> → la misma vista con el formulario intacto y el mensaje
/// en el resumen de validación (400).</item>
/// <item><c>Conflict</c> → la misma vista con el formulario intacto y un aviso de
/// conflicto (409).</item>
/// <item><c>NotFound</c> → la vista compartida <c>Estado</c> (404).</item>
/// </list>
/// Uso en un controlador MVC:
/// <code>
/// if (!ModelState.IsValid) return View(modelo);
/// var resultado = await _servicio.CreateAsync(...);
/// return this.VistaSiFalla(resultado, modelo) ?? this.RedirigirConExito("Persona creada.", nameof(Index));
/// </code>
/// </summary>
public static class ResultadoServicioVistaExtensions
{
    /// <summary>
    /// Devuelve la vista que corresponde a un resultado fallido, o <c>null</c> si el
    /// servicio tuvo éxito y el controlador debe seguir (normalmente, redirigir).
    /// </summary>
    /// <param name="modelo">El ViewModel recibido, para volver a pintar el formulario tal como se envió.</param>
    /// <param name="vista">Vista que se vuelve a pintar; por defecto, la de la acción actual.</param>
    public static IActionResult? VistaSiFalla(
        this Controller controller, ServiceResult resultado, object? modelo, string? vista = null)
    {
        switch (resultado.Status)
        {
            case ServiceResultStatus.Success:
                return null;

            case ServiceResultStatus.NotFound:
                return controller.VistaDeEstado(StatusCodes.Status404NotFound, resultado.Message);

            case ServiceResultStatus.ValidationError:
                controller.ModelState.AddModelError(string.Empty, resultado.Message ?? "Los datos no son válidos.");
                return ConCodigo(controller.View(vista, modelo), StatusCodes.Status400BadRequest);

            case ServiceResultStatus.Conflict:
                controller.ViewData[MensajesVista.Conflicto] = resultado.Message;
                return ConCodigo(controller.View(vista, modelo), StatusCodes.Status409Conflict);

            default:
                throw new ArgumentOutOfRangeException(nameof(resultado), resultado.Status, "Resultado de servicio desconocido.");
        }
    }

    /// <summary>
    /// Patrón POST-Redirect-GET: deja el mensaje en TempData para que la vista de destino
    /// lo muestre una sola vez y redirige.
    /// </summary>
    public static IActionResult RedirigirConExito(
        this Controller controller, string mensaje, string accion, object? valoresRuta = null)
    {
        controller.TempData[MensajesVista.Exito] = mensaje;
        return controller.RedirectToAction(accion, valoresRuta);
    }

    /// <summary>Pinta la vista compartida <c>Estado</c> con el código y el mensaje indicados.</summary>
    public static ViewResult VistaDeEstado(this Controller controller, int codigo, string? mensaje = null) =>
        ConCodigo(controller.View("Estado", EstadoViewModel.Para(codigo, mensaje)), codigo);

    private static ViewResult ConCodigo(ViewResult vista, int codigo)
    {
        vista.StatusCode = codigo;
        return vista;
    }
}
