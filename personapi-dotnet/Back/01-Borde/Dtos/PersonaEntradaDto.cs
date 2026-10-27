using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Cuerpo de entrada de <c>POST</c> y <c>PUT</c> de personas. Las anotaciones cubren la
/// forma del mensaje (presencia y longitud, que son validación de entrada del borde);
/// el dominio de <c>genero</c> y el rango de <c>edad</c> los comprueba
/// <c>PersonaService</c>, porque son reglas de negocio (SDD §7).
/// </summary>
public class PersonaEntradaDto
{
    // Anulable a propósito: con `int` no anulable, un cuerpo sin `cc` se enlaza como 0 y
    // el alta crearía la persona con cédula 0. Con `int?` el valor ausente es null y
    // [Required] lo rechaza con 400 antes de llegar al servicio.
    [Required]
    public int? Cc { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(45)]
    public string Nombre { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(45)]
    public string Apellido { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(1)]
    public string Genero { get; set; } = null!;

    public int? Edad { get; set; }
}
