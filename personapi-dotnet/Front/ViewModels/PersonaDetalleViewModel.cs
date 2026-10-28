using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Persona en modo lectura: alimenta el listado, el detalle y la confirmación de baja
/// (HU-007). Es plano a propósito; la vista no recibe entidades EF con navegaciones
/// (AD-10, regla 8).
/// </summary>
public class PersonaDetalleViewModel
{
    public int Cc { get; init; }

    public string Nombre { get; init; } = null!;

    public string Apellido { get; init; } = null!;

    public string Genero { get; init; } = null!;

    public int? Edad { get; init; }

    public string NombreCompleto => $"{Nombre} {Apellido}";

    /// <summary>El DDL guarda un carácter; la vista muestra la palabra.</summary>
    public string GeneroTexto => Genero switch
    {
        "M" => "Masculino",
        "F" => "Femenino",
        _ => Genero
    };

    /// <summary><c>edad</c> admite nulo en el DDL: se dice, no se pinta un cero.</summary>
    public string EdadTexto => Edad?.ToString() ?? "Sin registrar";

    public static PersonaDetalleViewModel Desde(PersonaDto persona) => new()
    {
        Cc = persona.Cc,
        Nombre = persona.Nombre,
        Apellido = persona.Apellido,
        Genero = persona.Genero,
        Edad = persona.Edad
    };
}
