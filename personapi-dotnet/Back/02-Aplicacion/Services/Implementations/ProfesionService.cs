using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.Aplicacion.Services.Implementations;

public class ProfesionService : IProfesionService
{
    private readonly IProfesionRepository _profesiones;
    private readonly IEstudioRepository _estudios;

    public ProfesionService(IProfesionRepository profesiones, IEstudioRepository estudios)
    {
        _profesiones = profesiones;
        _estudios = estudios;
    }

    public async Task<IReadOnlyList<ProfesionDto>> GetAllAsync() =>
        (await _profesiones.GetAllAsync()).Select(ADto).ToList();

    public async Task<ServiceResult<ProfesionDto>> GetByIdAsync(int id)
    {
        var profesion = await _profesiones.GetByIdAsync(id);
        return profesion is null
            ? ServiceResult<ProfesionDto>.NotFound(NoExiste(id))
            : ServiceResult<ProfesionDto>.Success(ADto(profesion));
    }

    public async Task<ServiceResult<ProfesionDto>> CreateAsync(ProfesionDto dto)
    {
        if (dto.Id is not int id)
        {
            return ServiceResult<ProfesionDto>.ValidationError("El id es obligatorio.");
        }

        var nom = dto.Nom?.Trim();
        if (ValidarNombre(nom) is string error)
        {
            return ServiceResult<ProfesionDto>.ValidationError(error);
        }

        if (await _profesiones.ExistsAsync(id))
        {
            return ServiceResult<ProfesionDto>.Conflict($"Ya existe una profesión con id {id}.");
        }

        var profesion = new Profesion { Id = id, Nom = nom!, Des = dto.Des };
        await _profesiones.AddAsync(profesion);
        return ServiceResult<ProfesionDto>.Success(ADto(profesion));
    }

    public async Task<ServiceResult> UpdateAsync(int id, ProfesionDto dto)
    {
        if (dto.Id is not int idCuerpo)
        {
            return ServiceResult.ValidationError("El id es obligatorio.");
        }

        // El PUT no cambia la identidad del recurso (api-rest.md).
        if (idCuerpo != id)
        {
            return ServiceResult.ValidationError($"El id del cuerpo ({idCuerpo}) no coincide con el de la ruta ({id}).");
        }

        var nom = dto.Nom?.Trim();
        if (ValidarNombre(nom) is string error)
        {
            return ServiceResult.ValidationError(error);
        }

        var profesion = await _profesiones.GetByIdAsync(id);
        if (profesion is null)
        {
            return ServiceResult.NotFound(NoExiste(id));
        }

        profesion.Nom = nom!;
        profesion.Des = dto.Des;
        await _profesiones.UpdateAsync(profesion);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var profesion = await _profesiones.GetByIdAsync(id);
        if (profesion is null)
        {
            return ServiceResult.NotFound(NoExiste(id));
        }

        // La FK estudio_profesion_fk es NO ACTION: el servicio detecta el conflicto antes de
        // escribir, y la base sigue siendo la defensa final.
        var estudios = await _estudios.GetAllAsync();
        var asociados = estudios.Count(e => e.IdProf == id);
        if (asociados > 0)
        {
            return ServiceResult.Conflict(
                $"La profesión {id} tiene {asociados} estudio(s) asociado(s); elimínelos antes de borrarla.");
        }

        await _profesiones.DeleteAsync(profesion);
        return ServiceResult.Success();
    }

    // Recibe el nombre ya recortado, como personas y teléfonos: la longitud se mide sin los
    // espacios de los extremos.
    private static string? ValidarNombre(string? nom)
    {
        if (string.IsNullOrWhiteSpace(nom))
        {
            return "El nombre (nom) es obligatorio.";
        }

        return nom.Length > ProfesionDto.NomLongitudMaxima
            ? $"El nombre (nom) admite hasta {ProfesionDto.NomLongitudMaxima} caracteres."
            : null;
    }

    private static string NoExiste(int id) => $"No existe una profesión con id {id}.";

    private static ProfesionDto ADto(Profesion profesion) =>
        new() { Id = profesion.Id, Nom = profesion.Nom, Des = profesion.Des };
}
