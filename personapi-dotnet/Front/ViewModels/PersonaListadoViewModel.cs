namespace personapi_dotnet.Front.ViewModels;

/// <summary>Modelo de <c>Personas/Index</c>: la tabla y su estado vacío.</summary>
public class PersonaListadoViewModel
{
    public IReadOnlyList<PersonaDetalleViewModel> Personas { get; init; } = [];

    public bool EstaVacio => Personas.Count == 0;
}
