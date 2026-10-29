namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación de salida de un teléfono en el canal JSON (api-rest.md). <c>Num</c> es
/// cadena, no número: la clave de la tabla es <c>VARCHAR(15)</c> y hay que conservar los
/// ceros iniciales. No se expone la entidad EF, que arrastra la navegación a persona.
/// </summary>
public class TelefonoDto
{
    /// <summary>Número del teléfono, clave del recurso.</summary>
    /// <example>3001234567</example>
    public string Num { get; set; } = null!;

    /// <summary>Operador.</summary>
    /// <example>Claro</example>
    public string Oper { get; set; } = null!;

    /// <summary>Cédula de la persona dueña; referencia <c>persona.cc</c>.</summary>
    /// <example>1010101010</example>
    public int Duenio { get; set; }
}
