using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Estudio tal como lo muestran y lo editan las vistas de HU-013. La identidad es el par
/// (IdProf, CcPer); los nombres y las opciones de los selectores los llena el controlador
/// desde los servicios y nunca se leen del formulario. La existencia de las dos
/// referencias, el par duplicado y la longitud de la universidad los decide
/// <c>IEstudioService</c> y llegan por <c>VistaSiFalla</c>.
/// </summary>
public class EstudioViewModel
{
    /// <summary>Longitud de estudios.univer en el DDL (VARCHAR(50)).</summary>
    public const int UniverLongitudMaxima = 50;

    // Anulables para que un selector sin elegir dé «obligatorio» y no se enlace como 0.
    [Required(ErrorMessage = "La profesión es obligatoria.")]
    [Display(Name = "Profesión")]
    public int? IdProf { get; set; }

    [Required(ErrorMessage = "La persona es obligatoria.")]
    [Display(Name = "Persona")]
    public int? CcPer { get; set; }

    [Display(Name = "Fecha de grado")]
    [DataType(DataType.Date)]
    public DateOnly? Fecha { get; set; }

    // Sin [StringLength]: mediría antes del recorte. El servicio mide la universidad ya
    // recortada; la vista solo limita lo que se puede escribir con maxlength.
    [Display(Name = "Universidad")]
    public string? Univer { get; set; }

    [BindNever, ValidateNever]
    public string? NomProfesion { get; set; }

    [BindNever, ValidateNever]
    public string? NombrePersona { get; set; }

    [BindNever, ValidateNever]
    public IReadOnlyList<SelectListItem> Profesiones { get; set; } = [];

    [BindNever, ValidateNever]
    public IReadOnlyList<SelectListItem> Personas { get; set; } = [];
}
