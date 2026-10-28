using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Aplicacion.Services.Interfaces;

/// <summary>
/// Casos de uso de profesiones, compartidos por REST (HU-005) y MVC (HU-008). Aplica las
/// reglas de SDD §7: id único, nombre obligatorio y sin borrado con estudios asociados.
/// </summary>
public interface IProfesionService
{
    Task<IReadOnlyList<ProfesionDto>> GetAllAsync();

    Task<ServiceResult<ProfesionDto>> GetByIdAsync(int id);

    Task<ServiceResult<ProfesionDto>> CreateAsync(ProfesionDto dto);

    Task<ServiceResult> UpdateAsync(int id, ProfesionDto dto);

    Task<ServiceResult> DeleteAsync(int id);
}
