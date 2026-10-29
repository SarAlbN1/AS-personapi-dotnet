using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Aplicacion.Services.Interfaces;

/// <summary>
/// Reglas de negocio de estudio (SDD §2 y §7), compartidas por REST y MVC. La clave es
/// siempre el par (idProf, ccPer); el servicio trabaja con DTOs y devuelve resultados
/// discriminados.
/// </summary>
public interface IEstudioService
{
    Task<IReadOnlyList<EstudioDto>> GetAllAsync();

    Task<ServiceResult<EstudioDto>> GetByIdAsync(int idProf, int ccPer);

    Task<ServiceResult<EstudioDto>> CreateAsync(EstudioEntradaDto entrada);

    Task<ServiceResult> UpdateAsync(int idProf, int ccPer, EstudioEntradaDto entrada);

    Task<ServiceResult> DeleteAsync(int idProf, int ccPer);
}
