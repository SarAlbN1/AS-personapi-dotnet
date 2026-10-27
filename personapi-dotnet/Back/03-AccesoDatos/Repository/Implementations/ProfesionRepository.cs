using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Implementations;

public class ProfesionRepository : IProfesionRepository
{
    private readonly PersonaDbContext _context;

    public ProfesionRepository(PersonaDbContext context) => _context = context;

    public async Task<IReadOnlyList<Profesion>> GetAllAsync() =>
        await _context.Profesions.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

    public async Task<Profesion?> GetByIdAsync(int id) =>
        await _context.Profesions.FindAsync(id);

    public Task<bool> ExistsAsync(int id) =>
        _context.Profesions.AnyAsync(e => e.Id == id);

    public async Task AddAsync(Profesion entity)
    {
        _context.Profesions.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Profesion entity)
    {
        _context.Profesions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Profesion entity)
    {
        _context.Profesions.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
