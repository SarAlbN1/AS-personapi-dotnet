using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Cuerpo de entrada de <c>POST</c> y <c>PUT</c> de teléfonos. Las anotaciones cubren la
/// forma del mensaje; la existencia del dueño y la unicidad del número las comprueba
/// <c>TelefonoService</c>, porque son reglas de negocio (SDD §7).
/// </summary>
public class TelefonoEntradaDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(15)]
    public string Num { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    [StringLength(45)]
    public string Oper { get; set; } = null!;

    [Required]
    public int Duenio { get; set; }
}
