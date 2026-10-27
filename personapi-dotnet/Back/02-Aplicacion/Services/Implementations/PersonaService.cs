using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.Aplicacion.Services.Implementations;

public class PersonaService : IPersonaService
{
    private static readonly string[] GenerosValidos = ["M", "F"];

    private readonly IPersonaRepository _personas;
    private readonly ITelefonoRepository _telefonos;
    private readonly IEstudioRepository _estudios;

    // Los repositorios de teléfono y estudio solo se usan para comprobar dependencias
    // antes de un borrado: la regla 10 prohíbe eliminar una persona que las tenga.
    public PersonaService(
        IPersonaRepository personas,
        ITelefonoRepository telefonos,
        IEstudioRepository estudios)
    {
        _personas = personas;
        _telefonos = telefonos;
        _estudios = estudios;
    }

    public async Task<IReadOnlyList<PersonaDto>> GetAllAsync() =>
        (await _personas.GetAllAsync()).Select(ADto).ToList();

    public async Task<ServiceResult<PersonaDto>> GetByIdAsync(int cc)
    {
        var persona = await _personas.GetByIdAsync(cc);

        return persona is null
            ? ServiceResult<PersonaDto>.NotFound($"No existe una persona con cédula {cc}.")
            : ServiceResult<PersonaDto>.Success(ADto(persona));
    }

    public async Task<ServiceResult<PersonaDto>> CreateAsync(PersonaEntradaDto entrada)
    {
        var error = Validar(entrada);
        if (error is not null)
        {
            return ServiceResult<PersonaDto>.ValidationError(error);
        }

        // La unicidad final la garantiza la PK: esta comprobación da el 409 con mensaje
        // útil, pero si dos solicitudes compiten manda la restricción de la base.
        var cc = entrada.Cc!.Value;
        if (await _personas.ExistsAsync(cc))
        {
            return ServiceResult<PersonaDto>.Conflict(
                $"Ya existe una persona con cédula {cc}.");
        }

        var persona = new Persona
        {
            Cc = cc,
            Nombre = entrada.Nombre.Trim(),
            Apellido = entrada.Apellido.Trim(),
            Genero = entrada.Genero.ToUpperInvariant(),
            Edad = entrada.Edad
        };

        await _personas.AddAsync(persona);

        return ServiceResult<PersonaDto>.Success(ADto(persona));
    }

    public async Task<ServiceResult> UpdateAsync(int cc, PersonaEntradaDto entrada)
    {
        var error = Validar(entrada);
        if (error is not null)
        {
            return ServiceResult.ValidationError(error);
        }

        // El PUT no cambia la identidad del recurso: ruta y cuerpo deben coincidir.
        if (cc != entrada.Cc)
        {
            return ServiceResult.ValidationError(
                $"La cédula de la ruta ({cc}) no coincide con la del cuerpo ({entrada.Cc}).");
        }

        var persona = await _personas.GetByIdAsync(cc);
        if (persona is null)
        {
            return ServiceResult.NotFound($"No existe una persona con cédula {cc}.");
        }

        persona.Nombre = entrada.Nombre.Trim();
        persona.Apellido = entrada.Apellido.Trim();
        persona.Genero = entrada.Genero.ToUpperInvariant();
        persona.Edad = entrada.Edad;

        await _personas.UpdateAsync(persona);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int cc)
    {
        var persona = await _personas.GetByIdAsync(cc);
        if (persona is null)
        {
            return ServiceResult.NotFound($"No existe una persona con cédula {cc}.");
        }

        var dependencias = await ContarDependencias(cc);
        if (dependencias is not null)
        {
            return ServiceResult.Conflict(dependencias);
        }

        await _personas.DeleteAsync(persona);

        return ServiceResult.Success();
    }

    private async Task<string?> ContarDependencias(int cc)
    {
        var telefonos = (await _telefonos.GetAllAsync()).Count(t => t.Duenio == cc);
        var estudios = (await _estudios.GetAllAsync()).Count(e => e.CcPer == cc);

        if (telefonos == 0 && estudios == 0)
        {
            return null;
        }

        return $"No se puede eliminar la persona {cc}: tiene {telefonos} teléfono(s) y " +
               $"{estudios} estudio(s) asociados. Elimine primero esos registros.";
    }

    private static string? Validar(PersonaEntradaDto entrada)
    {
        // El borde ya lo rechaza con [Required], pero el servicio no depende de eso:
        // MVC comparte estos servicios y la cédula es la identidad de la fila.
        if (entrada.Cc is null)
        {
            return "La cédula (cc) es obligatoria.";
        }

        // Mismo motivo: por REST [ApiController] corta antes, pero el servicio no puede
        // reventar con NullReferenceException si HU-007 le pasa un DTO sin revisar
        // ModelState. Nombre, apellido y género se recortan o se normalizan más abajo.
        if (string.IsNullOrWhiteSpace(entrada.Nombre) || string.IsNullOrWhiteSpace(entrada.Apellido))
        {
            return "El nombre y el apellido son obligatorios.";
        }

        if (string.IsNullOrWhiteSpace(entrada.Genero))
        {
            return "El género (genero) es obligatorio.";
        }

        if (!GenerosValidos.Contains(entrada.Genero.ToUpperInvariant()))
        {
            return "El género debe ser 'M' o 'F'.";
        }

        if (entrada.Edad is < 0)
        {
            return "La edad no puede ser negativa.";
        }

        return null;
    }

    private static PersonaDto ADto(Persona persona) => new()
    {
        Cc = persona.Cc,
        Nombre = persona.Nombre,
        Apellido = persona.Apellido,
        Genero = persona.Genero,
        Edad = persona.Edad
    };
}
