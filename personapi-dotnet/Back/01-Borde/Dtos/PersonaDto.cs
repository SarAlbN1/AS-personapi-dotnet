namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación de salida de una persona en el canal JSON (AD-10). No se exponen
/// entidades EF con navegaciones: este DTO es plano y su forma es la de
/// <c>docs/wiki/03-contratos/api-rest.md</c>.
/// </summary>
public class PersonaDto
{
    public int Cc { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Genero { get; set; } = null!;

    public int? Edad { get; set; }
}
