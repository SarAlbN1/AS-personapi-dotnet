using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Implementations;

public class EstudioRepository : IEstudioRepository
{
    private readonly PersonaDbContext _context;

    public EstudioRepository(PersonaDbContext context) => _context = context;

    public async Task<IReadOnlyList<Estudio>> GetAllAsync() =>
        await _context.Estudios.AsNoTracking()
            .OrderBy(e => e.IdProf).ThenBy(e => e.CcPer)
            .ToListAsync();

    // FindAsync recibe los valores en el orden de la PK: (id_prof, cc_per).
    public async Task<Estudio?> GetByIdAsync(int idProf, int ccPer) =>
        await _context.Estudios.FindAsync(idProf, ccPer);

    public Task<bool> ExistsAsync(int idProf, int ccPer) =>
        _context.Estudios.AnyAsync(e => e.IdProf == idProf && e.CcPer == ccPer);

    public async Task AddAsync(Estudio entity)
    {
        _context.Estudios.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Estudio entity)
    {
        _context.Estudios.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Estudio entity)
    {
        _context.Estudios.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
