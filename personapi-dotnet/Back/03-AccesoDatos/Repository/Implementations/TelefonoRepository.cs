using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.AccesoDatos.Repository.Implementations;

public class TelefonoRepository : ITelefonoRepository
{
    private readonly PersonaDbContext _context;

    public TelefonoRepository(PersonaDbContext context) => _context = context;

    public async Task<IReadOnlyList<Telefono>> GetAllAsync() =>
        await _context.Telefonos.AsNoTracking().OrderBy(e => e.Num).ToListAsync();

    public async Task<Telefono?> GetByIdAsync(string num) =>
        await _context.Telefonos.FindAsync(num);

    public Task<bool> ExistsAsync(string num) =>
        _context.Telefonos.AnyAsync(e => e.Num == num);

    public async Task AddAsync(Telefono entity)
    {
        _context.Telefonos.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Telefono entity)
    {
        _context.Telefonos.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Telefono entity)
    {
        _context.Telefonos.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
