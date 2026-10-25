using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;

/// <summary>
/// Acceso a datos de <see cref="Telefono"/> (SDD §2). Solo lee y escribe: las reglas de
/// negocio, como bloquear un borrado con dependencias, viven en 02-Aplicacion.
/// </summary>
public interface ITelefonoRepository
{
    Task<IReadOnlyList<Telefono>> GetAllAsync();

    Task<Telefono?> GetByIdAsync(string num);

    Task<bool> ExistsAsync(string num);

    Task AddAsync(Telefono entity);

    Task UpdateAsync(Telefono entity);

    Task DeleteAsync(Telefono entity);
}
