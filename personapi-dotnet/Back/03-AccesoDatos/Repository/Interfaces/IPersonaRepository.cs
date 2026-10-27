using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;

/// <summary>
/// Acceso a datos de <see cref="Persona"/> (SDD §2). Solo lee y escribe: las reglas de
/// negocio, como bloquear un borrado con dependencias, viven en 02-Aplicacion.
/// </summary>
public interface IPersonaRepository
{
    Task<IReadOnlyList<Persona>> GetAllAsync();

    Task<Persona?> GetByIdAsync(int cc);

    Task<bool> ExistsAsync(int cc);

    Task AddAsync(Persona entity);

    Task UpdateAsync(Persona entity);

    Task DeleteAsync(Persona entity);
}
