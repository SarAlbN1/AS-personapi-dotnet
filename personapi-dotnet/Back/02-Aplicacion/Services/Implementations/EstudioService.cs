using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.Aplicacion.Services.Implementations;

public class EstudioService : IEstudioService
{
    private readonly IEstudioRepository _estudios;
    private readonly IProfesionRepository _profesiones;
    private readonly IPersonaRepository _personas;

    // Los repositorios de profesión y persona solo se consultan para validar las dos FK:
    // las restricciones de la base son la defensa final, pero el servicio da el 400 antes.
    public EstudioService(
        IEstudioRepository estudios,
        IProfesionRepository profesiones,
        IPersonaRepository personas)
    {
        _estudios = estudios;
        _profesiones = profesiones;
        _personas = personas;
    }

    public async Task<IReadOnlyList<EstudioDto>> GetAllAsync() =>
        (await _estudios.GetAllAsync()).Select(ADto).ToList();

    public async Task<ServiceResult<EstudioDto>> GetByIdAsync(int idProf, int ccPer)
    {
        var estudio = await _estudios.GetByIdAsync(idProf, ccPer);

        return estudio is null
            ? ServiceResult<EstudioDto>.NotFound(NoExiste(idProf, ccPer))
            : ServiceResult<EstudioDto>.Success(ADto(estudio));
    }

    public async Task<ServiceResult<EstudioDto>> CreateAsync(EstudioEntradaDto entrada)
    {
        if (Validar(entrada) is string error)
        {
            return ServiceResult<EstudioDto>.ValidationError(error);
        }

        var idProf = entrada.IdProf!.Value;
        var ccPer = entrada.CcPer!.Value;

        // Una referencia inexistente es un error de entrada (400), no un conflicto: así lo
        // fija api-rest.md para «referencias inexistentes al crear/asociar».
        if (!await _profesiones.ExistsAsync(idProf))
        {
            return ServiceResult<EstudioDto>.ValidationError($"No existe una profesión con id {idProf}.");
        }

        if (!await _personas.ExistsAsync(ccPer))
        {
            return ServiceResult<EstudioDto>.ValidationError($"No existe una persona con cédula {ccPer}.");
        }

        if (await _estudios.ExistsAsync(idProf, ccPer))
        {
            return ServiceResult<EstudioDto>.Conflict(
                $"Ya existe un estudio de la profesión {idProf} para la persona {ccPer}.");
        }

        var estudio = new Estudio
        {
            IdProf = idProf,
            CcPer = ccPer,
            Fecha = entrada.Fecha,
            Univer = RecortarUniversidad(entrada.Univer)
        };

        await _estudios.AddAsync(estudio);

        return ServiceResult<EstudioDto>.Success(ADto(estudio));
    }

    public async Task<ServiceResult> UpdateAsync(int idProf, int ccPer, EstudioEntradaDto entrada)
    {
        if (Validar(entrada) is string error)
        {
            return ServiceResult.ValidationError(error);
        }

        // El PUT no cambia la identidad del recurso (api-rest.md): solo fecha y univer.
        if (entrada.IdProf != idProf || entrada.CcPer != ccPer)
        {
            return ServiceResult.ValidationError(
                $"La clave de la ruta ({idProf}, {ccPer}) no coincide con la del cuerpo " +
                $"({entrada.IdProf}, {entrada.CcPer}); el par identificador no se puede cambiar.");
        }

        var estudio = await _estudios.GetByIdAsync(idProf, ccPer);
        if (estudio is null)
        {
            return ServiceResult.NotFound(NoExiste(idProf, ccPer));
        }

        estudio.Fecha = entrada.Fecha;
        estudio.Univer = RecortarUniversidad(entrada.Univer);

        await _estudios.UpdateAsync(estudio);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int idProf, int ccPer)
    {
        var estudio = await _estudios.GetByIdAsync(idProf, ccPer);
        if (estudio is null)
        {
            return ServiceResult.NotFound(NoExiste(idProf, ccPer));
        }

        // Nada depende de un estudio: no hay caso de conflicto al borrarlo.
        await _estudios.DeleteAsync(estudio);

        return ServiceResult.Success();
    }

    private static string? Validar(EstudioEntradaDto entrada)
    {
        // El borde ya lo rechaza con [Required]; el servicio no depende de eso porque el
        // MVC de HU-013 lo comparte, y el par es la identidad de la fila.
        if (entrada.IdProf is null)
        {
            return "La profesión (idProf) es obligatoria.";
        }

        if (entrada.CcPer is null)
        {
            return "La persona (ccPer) es obligatoria.";
        }

        // La longitud se mide sin los espacios de los extremos, como en los demás módulos.
        if (RecortarUniversidad(entrada.Univer) is { Length: > EstudioEntradaDto.UniverLongitudMaxima })
        {
            return $"La universidad (univer) admite hasta {EstudioEntradaDto.UniverLongitudMaxima} caracteres.";
        }

        return null;
    }

    // univer es opcional: recortada, una universidad solo de espacios se guarda como NULL.
    private static string? RecortarUniversidad(string? univer) =>
        string.IsNullOrWhiteSpace(univer) ? null : univer.Trim();

    private static string NoExiste(int idProf, int ccPer) =>
        $"No existe un estudio de la profesión {idProf} para la persona {ccPer}.";

    private static EstudioDto ADto(Estudio estudio) => new()
    {
        IdProf = estudio.IdProf,
        CcPer = estudio.CcPer,
        Fecha = estudio.Fecha,
        Univer = estudio.Univer
    };
}
