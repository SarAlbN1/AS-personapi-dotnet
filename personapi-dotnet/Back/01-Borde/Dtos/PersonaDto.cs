namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación de salida de una persona en el canal JSON (AD-10). No se exponen
/// entidades EF con navegaciones: este DTO es plano y su forma es la de
/// <c>docs/wiki/03-contratos/api-rest.md</c>.
/// </summary>
public class PersonaDto
{
    /// <example>1010101010</example>
    public int Cc { get; set; }

    /// <example>Ana</example>
    public string Nombre { get; set; } = null!;

    /// <example>Gómez</example>
    public string Apellido { get; set; } = null!;

    /// <example>F</example>
    public string Genero { get; set; } = null!;

    /// <example>28</example>
    public int? Edad { get; set; }
}
