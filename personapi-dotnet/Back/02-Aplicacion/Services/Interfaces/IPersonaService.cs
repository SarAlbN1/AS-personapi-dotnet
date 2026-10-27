using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Aplicacion.Services.Interfaces;

/// <summary>
/// Reglas de negocio de persona (SDD §2 y §7), compartidas por REST y MVC. Trabaja con
/// DTOs, nunca con entidades EF, y devuelve resultados discriminados que el borde
/// traduce a códigos de estado o a mensajes de vista.
/// </summary>
public interface IPersonaService
{
    Task<IReadOnlyList<PersonaDto>> GetAllAsync();

    Task<ServiceResult<PersonaDto>> GetByIdAsync(int cc);

    Task<ServiceResult<PersonaDto>> CreateAsync(PersonaEntradaDto entrada);

    Task<ServiceResult> UpdateAsync(int cc, PersonaEntradaDto entrada);

    Task<ServiceResult> DeleteAsync(int cc);
}
