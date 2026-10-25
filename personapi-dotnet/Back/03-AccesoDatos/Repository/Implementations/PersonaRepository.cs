using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Implementations;

public class PersonaRepository : IPersonaRepository
{
    private readonly PersonaDbContext _context;

    public PersonaRepository(PersonaDbContext context) => _context = context;

    public async Task<IReadOnlyList<Persona>> GetAllAsync() =>
        await _context.Personas.AsNoTracking().OrderBy(e => e.Cc).ToListAsync();

    public async Task<Persona?> GetByIdAsync(int cc) =>
        await _context.Personas.FindAsync(cc);

    public Task<bool> ExistsAsync(int cc) =>
        _context.Personas.AnyAsync(e => e.Cc == cc);

    public async Task AddAsync(Persona entity)
    {
        _context.Personas.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Persona entity)
    {
        _context.Personas.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Persona entity)
    {
        _context.Personas.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
