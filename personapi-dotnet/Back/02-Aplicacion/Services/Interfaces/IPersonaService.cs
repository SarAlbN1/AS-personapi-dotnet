using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Aplicacion.Services.Interfaces;

/// <summary>
/// Reglas de negocio de persona (SDD §2 y §7), compartidas por REST y MVC. Trabaja con
/// DTOs, nunca con entidades EF, y devuelve resultados discriminados que el borde
/// traduce a códigos de estado o a mensajes de vista.
/// </summary>
public interface IPersonaService
{
    Task<ResultadoServicio<IReadOnlyList<PersonaDto>>> GetAllAsync();

    Task<ResultadoServicio<PersonaDto>> GetByIdAsync(int cc);

    Task<ResultadoServicio<PersonaDto>> CreateAsync(PersonaEntradaDto entrada);

    Task<ResultadoServicio> UpdateAsync(int cc, PersonaEntradaDto entrada);

    Task<ResultadoServicio> DeleteAsync(int cc);
}
