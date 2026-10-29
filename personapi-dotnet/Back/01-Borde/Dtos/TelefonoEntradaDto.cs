using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Cuerpo de entrada de <c>POST</c> y <c>PUT</c> de teléfonos. Las anotaciones cubren la
/// forma del mensaje; la existencia del dueño y la unicidad del número las comprueba
/// <c>TelefonoService</c>, porque son reglas de negocio (SDD §7).
/// </summary>
public class TelefonoEntradaDto
{
    // Solo dígitos: `num` es la clave del recurso y viaja en la ruta. Un número con
    // caracteres como '/' se creaba con 201 y después quedaba inalcanzable por GET y
    // DELETE, porque la barra no pasa por la plantilla de ruta. Sigue siendo cadena
    // para conservar los ceros iniciales (api-rest.md).
    /// <summary>Número del teléfono: solo dígitos, hasta 15, como cadena para conservar los ceros iniciales.</summary>
    /// <example>3157778899</example>
    [Required(AllowEmptyStrings = false)]
    [StringLength(15)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "El número (num) solo admite dígitos.")]
    public string Num { get; set; } = null!;

    /// <summary>Operador, hasta 45 caracteres.</summary>
    /// <example>Movistar</example>
    [Required(AllowEmptyStrings = false)]
    [StringLength(45)]
    public string Oper { get; set; } = null!;

    // Anulable a propósito, igual que `cc` en personas: con `int` no anulable un cuerpo
    // sin `duenio` se enlaza como 0 y la validación hablaría de la persona 0.
    /// <summary>Cédula de una persona existente.</summary>
    /// <example>1020202020</example>
    [Required]
    public int? Duenio { get; set; }
}
