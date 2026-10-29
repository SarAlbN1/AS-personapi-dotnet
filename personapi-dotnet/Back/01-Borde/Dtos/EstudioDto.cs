namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación de salida de un estudio en el canal JSON (api-rest.md). La identidad es
/// el par <c>(idProf, ccPer)</c>, mapeado a <c>id_prof</c>/<c>cc_per</c>: no hay
/// identificador artificial (AK-03). No se exponen las navegaciones de la entidad EF.
/// </summary>
public class EstudioDto
{
    /// <summary>Id de la profesión, primera mitad de la clave.</summary>
    /// <example>1</example>
    public int IdProf { get; set; }

    /// <summary>Cédula de la persona, segunda mitad de la clave.</summary>
    /// <example>1010101010</example>
    public int CcPer { get; set; }

    /// <summary>Fecha de grado, si se conoce.</summary>
    /// <example>2020-12-15</example>
    public DateOnly? Fecha { get; set; }

    /// <summary>Universidad, si se conoce.</summary>
    /// <example>Pontificia Universidad Javeriana</example>
    public string? Univer { get; set; }
}
