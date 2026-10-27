using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Front.ViewModels;

/// <summary>
/// Formulario de la página de ejemplo de HU-006. No representa ninguna entidad: solo
/// sirve para ver cómo se pintan los errores de validación y los conflictos.
/// </summary>
public class ConvencionesDemoViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre admite hasta 45 caracteres.")]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    /// <summary>Resultado que simulará el servicio al enviar el formulario.</summary>
    [Display(Name = "Resultado simulado del servicio")]
    public EscenarioDemo Escenario { get; set; } = EscenarioDemo.Exito;
}

public enum EscenarioDemo
{
    [Display(Name = "Success: guarda y redirige")]
    Exito,

    [Display(Name = "ValidationError: regla de dominio incumplida")]
    ErrorDeValidacion,

    [Display(Name = "Conflict: clave duplicada o borrado bloqueado")]
    Conflicto,

    [Display(Name = "NotFound: el registro ya no existe")]
    NoEncontrado
}
