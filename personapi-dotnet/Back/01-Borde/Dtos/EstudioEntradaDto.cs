using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Cuerpo de entrada de <c>POST</c> y <c>PUT</c> de estudios. Las anotaciones cubren la
/// forma del mensaje; la existencia de la profesión y la persona y la unicidad del par
/// las comprueba <c>EstudioService</c>, porque son reglas de negocio (SDD §7).
/// </summary>
public class EstudioEntradaDto
{
    /// <summary>Longitud de estudios.univer en el DDL (VARCHAR(50)).</summary>
    public const int UniverLongitudMaxima = 50;

    // Las dos mitades de la clave son anulables a propósito, igual que `cc` en personas:
    // con `int` un cuerpo sin ellas se enlazaría como 0 y el error hablaría de la
    // profesión 0 o de la persona 0.
    [Required(ErrorMessage = "La profesión (idProf) es obligatoria.")]
    public int? IdProf { get; set; }

    [Required(ErrorMessage = "La persona (ccPer) es obligatoria.")]
    public int? CcPer { get; set; }

    // DateOnly: una fecha que no existe en el calendario ("2020-02-30") o con otro
    // formato no se enlaza y [ApiController] responde 400. Nula está permitida por el DDL.
    public DateOnly? Fecha { get; set; }

    // Sin [StringLength]: la longitud se mide después de recortar los espacios de los
    // extremos, y eso lo hace EstudioService. Una anotación aquí mediría antes del recorte
    // y [ApiController] respondería 400 a un valor que el servicio acepta.
    public string? Univer { get; set; }
}
