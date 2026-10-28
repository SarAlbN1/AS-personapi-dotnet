using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Profesión tal como la muestran y la editan las vistas de HU-008. Solo lleva las
/// anotaciones de forma: el id duplicado, la longitud del nombre ya recortado y el borrado
/// con estudios los decide <c>IProfesionService</c> y llegan por <c>VistaSiFalla</c>.
/// </summary>
public class ProfesionViewModel
{
    /// <summary>Longitud de profesion.nom en el DDL (VARCHAR(90)).</summary>
    public const int NomLongitudMaxima = 90;

    // Anulable para que un campo vacío dé «obligatorio» y no se enlace como 0.
    [Required(ErrorMessage = "El id es obligatorio.")]
    [Display(Name = "Id")]
    public int? Id { get; set; }

    // Sin [StringLength]: mediría antes del recorte. El servicio mide el nombre ya
    // recortado; la vista solo limita lo que se puede escribir con maxlength.
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string? Nom { get; set; }

    [Display(Name = "Descripción")]
    public string? Des { get; set; }
}
