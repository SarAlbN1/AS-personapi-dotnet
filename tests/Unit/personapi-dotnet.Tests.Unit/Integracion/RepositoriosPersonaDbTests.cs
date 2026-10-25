using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Comprobaciones de HU-003: cada repositorio, resuelto por la inyección de dependencias
/// de la aplicación, lee filas reales de persona_db. Las claves esperadas son las de
/// scripts/02_dml.sql.
/// </summary>
public class RepositoriosPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RepositoriosPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [FactConPersonaDb]
    public async Task PersonaRepository_lee_personas_reales()
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IPersonaRepository>();

        var personas = await repositorio.GetAllAsync();
        var ana = await repositorio.GetByIdAsync(1010101010);

        Assert.Contains(personas, p => p.Cc == 1020202020);
        Assert.NotNull(ana);
        Assert.Equal("Gómez", ana.Apellido);
        Assert.False(await repositorio.ExistsAsync(1));
    }

    [FactConPersonaDb]
    public async Task ProfesionRepository_lee_profesiones_reales()
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IProfesionRepository>();

        var profesiones = await repositorio.GetAllAsync();
        var medicina = await repositorio.GetByIdAsync(2);

        Assert.Contains(profesiones, p => p.Id == 1);
        Assert.NotNull(medicina);
        Assert.Equal("Medicina", medicina.Nom);
        Assert.False(await repositorio.ExistsAsync(99));
    }

    [FactConPersonaDb]
    public async Task TelefonoRepository_lee_telefonos_reales()
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<ITelefonoRepository>();

        var telefonos = await repositorio.GetAllAsync();
        var telefono = await repositorio.GetByIdAsync("3001234567");

        Assert.Contains(telefonos, t => t.Num == "3109876543");
        Assert.NotNull(telefono);
        Assert.Equal(1010101010, telefono.Duenio);
        Assert.False(await repositorio.ExistsAsync("0000000000"));
    }

    [FactConPersonaDb]
    public async Task EstudioRepository_lee_por_clave_compuesta()
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IEstudioRepository>();

        var estudios = await repositorio.GetAllAsync();
        var estudio = await repositorio.GetByIdAsync(idProf: 1, ccPer: 1030303030);

        Assert.Contains(estudios, e => e.IdProf == 2 && e.CcPer == 1020202020);
        Assert.NotNull(estudio);
        Assert.Equal("Universidad de los Andes", estudio.Univer);
        // El orden importa: (1, 1020202020) no existe aunque ambos valores sí aparezcan.
        Assert.False(await repositorio.ExistsAsync(idProf: 1, ccPer: 1020202020));
    }

    [Fact]
    public void El_contexto_y_los_repositorios_son_scoped()
    {
        // Cadena ficticia: construir el contexto no abre conexión, así que esta prueba no
        // necesita SQL Server.
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:PersonaDb", "Server=sin-conexion;Database=persona_db"));

        using var scopeA = factory.Services.CreateScope();
        using var scopeB = factory.Services.CreateScope();

        Assert.Same(
            scopeA.ServiceProvider.GetRequiredService<PersonaDbContext>(),
            scopeA.ServiceProvider.GetRequiredService<PersonaDbContext>());
        Assert.NotSame(
            scopeA.ServiceProvider.GetRequiredService<PersonaDbContext>(),
            scopeB.ServiceProvider.GetRequiredService<PersonaDbContext>());

        AssertScoped<IPersonaRepository>(scopeA, scopeB);
        AssertScoped<IProfesionRepository>(scopeA, scopeB);
        AssertScoped<ITelefonoRepository>(scopeA, scopeB);
        AssertScoped<IEstudioRepository>(scopeA, scopeB);
    }

    private static void AssertScoped<T>(IServiceScope scopeA, IServiceScope scopeB) where T : class
    {
        Assert.Same(scopeA.ServiceProvider.GetRequiredService<T>(), scopeA.ServiceProvider.GetRequiredService<T>());
        Assert.NotSame(scopeA.ServiceProvider.GetRequiredService<T>(), scopeB.ServiceProvider.GetRequiredService<T>());
    }
}
