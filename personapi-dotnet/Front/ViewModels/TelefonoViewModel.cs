using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Teléfono tal como lo muestran y lo editan las vistas de HU-012. La identidad es el
/// número; el nombre del dueño y las opciones del selector los llena el controlador desde
/// los servicios y nunca se leen del formulario. El número duplicado y la existencia del
/// dueño los decide <c>ITelefonoService</c> y llegan por <c>VistaSiFalla</c>.
/// </summary>
public class TelefonoViewModel
{
    /// <summary>Longitud de telefono.num en el DDL (VARCHAR(15)).</summary>
    public const int NumLongitudMaxima = 15;

    /// <summary>Longitud de telefono.oper en el DDL (VARCHAR(45)).</summary>
    public const int OperLongitudMaxima = 45;

    // Cadena, no número, para conservar los ceros iniciales. A diferencia de los demás
    // textos, el número no se recorta (api-rest.md), así que su longitud sí se puede
    // medir aquí: se informa junto al campo y la petición no llega a la base.
    [Required(ErrorMessage = "El número es obligatorio.")]
    [StringLength(NumLongitudMaxima, ErrorMessage = "El número admite hasta 15 dígitos.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "El número solo admite dígitos.")]
    [Display(Name = "Número")]
    public string? Num { get; set; }

    [Required(ErrorMessage = "El operador es obligatorio.")]
    [StringLength(OperLongitudMaxima, ErrorMessage = "El operador admite hasta 45 caracteres.")]
    [Display(Name = "Operador")]
    public string? Oper { get; set; }

    // Anulable para que un selector sin elegir dé «obligatorio» y no se enlace como 0.
    [Required(ErrorMessage = "El dueño es obligatorio.")]
    [Display(Name = "Dueño")]
    public int? Duenio { get; set; }

    [BindNever, ValidateNever]
    public string? NombreDuenio { get; set; }

    [BindNever, ValidateNever]
    public IReadOnlyList<SelectListItem> Personas { get; set; } = [];
}
