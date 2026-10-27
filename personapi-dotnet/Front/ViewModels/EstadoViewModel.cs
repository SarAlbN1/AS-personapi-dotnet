namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Página compartida para respuestas sin formulario: un recurso que no existe (404), una
/// ruta desconocida o una solicitud rechazada, como un POST sin token antiforgery (400).
/// </summary>
public class EstadoViewModel
{
    public int Codigo { get; init; }

    public string Titulo { get; init; } = null!;

    public string Mensaje { get; init; } = null!;

    public static EstadoViewModel Para(int codigo, string? mensaje = null) => codigo switch
    {
        404 => new() { Codigo = codigo, Titulo = "No encontrado", Mensaje = mensaje ?? "La página o el registro solicitado no existe." },
        400 => new() { Codigo = codigo, Titulo = "Solicitud no válida", Mensaje = mensaje ?? "La solicitud no se pudo procesar. Vuelva al formulario e inténtelo de nuevo." },
        _ => new() { Codigo = codigo, Titulo = "Error", Mensaje = mensaje ?? "No se pudo completar la solicitud." }
    };
}
