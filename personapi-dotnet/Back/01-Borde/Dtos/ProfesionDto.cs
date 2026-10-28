using System.ComponentModel.DataAnnotations;

namespace personapi_dotnet.Back.Borde.Dtos;

/// <summary>
/// Representación JSON de una profesión (api-rest.md). Es el contrato de entrada y salida
/// de /api/profesiones: no expone la entidad EF ni su colección de estudios.
/// </summary>
public class ProfesionDto
{
    /// <summary>Longitud de profesion.nom en el DDL (VARCHAR(90)).</summary>
    public const int NomLongitudMaxima = 90;

    /// <summary>Clave de la profesión. Obligatoria: la tabla no la genera.</summary>
    [Required(ErrorMessage = "El id es obligatorio.")]
    public int? Id { get; set; }

    /// <summary>Nombre de la profesión, hasta 90 caracteres.</summary>
    [Required(ErrorMessage = "El nombre (nom) es obligatorio.")]
    [StringLength(NomLongitudMaxima, ErrorMessage = "El nombre (nom) admite hasta 90 caracteres.")]
    public string? Nom { get; set; }

    /// <summary>Descripción opcional; en la base es VARCHAR(MAX), sin límite de longitud.</summary>
    public string? Des { get; set; }
}
