using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;

/// <summary>
/// Acceso a datos de <see cref="Profesion"/> (SDD §2). Solo lee y escribe: las reglas de
/// negocio, como bloquear un borrado con dependencias, viven en 02-Aplicacion.
/// </summary>
public interface IProfesionRepository
{
    Task<IReadOnlyList<Profesion>> GetAllAsync();

    Task<Profesion?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Profesion entity);

    Task UpdateAsync(Profesion entity);

    Task DeleteAsync(Profesion entity);
}
