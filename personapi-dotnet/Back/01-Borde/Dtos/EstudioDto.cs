namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación de salida de un estudio en el canal JSON (api-rest.md). La identidad es
/// el par <c>(idProf, ccPer)</c>, mapeado a <c>id_prof</c>/<c>cc_per</c>: no hay
/// identificador artificial (AK-03). No se exponen las navegaciones de la entidad EF.
/// </summary>
public class EstudioDto
{
    public int IdProf { get; set; }

    public int CcPer { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Univer { get; set; }
}
