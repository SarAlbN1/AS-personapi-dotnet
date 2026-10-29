using System.ComponentModel.DataAnnotations;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Formulario de alta y edición de personas (HU-007). Solo lleva las anotaciones de
/// <b>forma</b> que pide la convención de HU-006: presencia y longitud. El dominio
/// —género dentro de M/F, edad no negativa, cédula libre— lo decide
/// <c>PersonaService</c> y llega a la vista por <c>VistaSiFalla</c>, así que no se
/// duplica aquí.
/// </summary>
public class PersonaFormViewModel
{
    /// <summary>Géneros que ofrece el formulario; el dominio lo valida el servicio (SDD §7).</summary>
    public static readonly string[] Generos = ["M", "F"];

    // Anulable por el mismo motivo que PersonaEntradaDto.Cc: con `int` no anulable, un
    // formulario sin cédula se enlazaría como 0 y el alta crearía la persona con cc 0.
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [Display(Name = "Cédula")]
    public int? Cc { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre admite hasta 45 caracteres.")]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(45, ErrorMessage = "El apellido admite hasta 45 caracteres.")]
    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El género es obligatorio.")]
    [StringLength(1, ErrorMessage = "El género es un solo carácter.")]
    [Display(Name = "Género")]
    public string? Genero { get; set; }

    [Display(Name = "Edad")]
    public int? Edad { get; set; }

    /// <summary>
    /// En edición la cédula es la identidad de la fila y no se cambia: la vista la
    /// muestra bloqueada y la envía oculta para que ruta y cuerpo coincidan.
    /// </summary>
    public bool EsEdicion { get; set; }

    public static PersonaFormViewModel Desde(PersonaDto persona) => new()
    {
        Cc = persona.Cc,
        Nombre = persona.Nombre,
        Apellido = persona.Apellido,
        Genero = persona.Genero,
        Edad = persona.Edad,
        EsEdicion = true
    };

    /// <summary>
    /// Traduce el formulario al DTO de entrada que comparten MVC y REST. Se invoca solo
    /// con <c>ModelState</c> válido, así que los obligatorios ya no son nulos.
    /// </summary>
    public PersonaEntradaDto AEntrada() => new()
    {
        Cc = Cc,
        Nombre = Nombre!,
        Apellido = Apellido!,
        Genero = Genero!,
        Edad = Edad
    };
}
