using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;

/// <summary>
/// Acceso a datos de <see cref="Estudio"/> (SDD §2). La clave es la PK compuesta
/// (id_prof, cc_per) del DDL. Solo lee y escribe: las reglas de negocio viven en
/// 02-Aplicacion.
/// </summary>
public interface IEstudioRepository
{
    Task<IReadOnlyList<Estudio>> GetAllAsync();

    Task<Estudio?> GetByIdAsync(int idProf, int ccPer);

    Task<bool> ExistsAsync(int idProf, int ccPer);

    Task AddAsync(Estudio entity);

    Task UpdateAsync(Estudio entity);

    Task DeleteAsync(Estudio entity);
}
