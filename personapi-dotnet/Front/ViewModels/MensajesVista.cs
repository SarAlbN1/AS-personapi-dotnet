namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Claves compartidas entre los controladores MVC y el partial <c>_MensajesServicio</c>.
/// Viven aquí porque son parte del contrato que consume Razor.
/// </summary>
public static class MensajesVista
{
    /// <summary>TempData: confirmación tras un POST que terminó en redirección.</summary>
    public const string Exito = "MensajeExito";

    /// <summary>ViewData: conflicto devuelto por el servicio (clave duplicada, borrado bloqueado).</summary>
    public const string Conflicto = "MensajeConflicto";
}
