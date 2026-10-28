using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Criterio 3 de HU-014: las restricciones de SQL Server, y no solo los servicios, impiden
/// duplicados, huérfanos y borrados con dependientes. Escribe directamente con los
/// repositorios, sin pasar por las comprobaciones de los servicios, para que la única
/// defensa sea la base. Ninguna escritura llega a confirmarse; aun así, las filas que
/// podrían quedar (cc 914000001, id 9140, num 9140000001) se borran al empezar y al terminar.
/// </summary>
public class IntegridadSqlPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    // Números de error de SQL Server: 2627 violación de PRIMARY KEY, 547 de FOREIGN KEY o CHECK.
    private const int PkDuplicada = 2627;
    private const int RestriccionVulnerada = 547;

    private const int CcInexistente = 914000001;
    private const int IdInexistente = 9140;
    private const string NumPrueba = "9140000001";

    private readonly WebApplicationFactory<Program> _factory;

    public IntegridadSqlPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task La_PK_de_persona_impide_duplicar_la_cedula()
    {
        var existente = (await Leer<IPersonaRepository, IReadOnlyList<Persona>>(r => r.GetAllAsync())).First();

        var error = await RechazoAsync<IPersonaRepository>(r => r.AddAsync(
            new Persona { Cc = existente.Cc, Nombre = "Duplicada", Apellido = "Prueba", Genero = "F" }));

        Assert.Equal(PkDuplicada, error.Number);
        Assert.Equal(existente.Nombre, (await Leer<IPersonaRepository, Persona?>(r => r.GetByIdAsync(existente.Cc)))!.Nombre);
    }

    [FactConPersonaDb]
    public async Task La_PK_de_profesion_impide_duplicar_el_id()
    {
        var existente = (await Leer<IProfesionRepository, IReadOnlyList<Profesion>>(r => r.GetAllAsync())).First();

        var error = await RechazoAsync<IProfesionRepository>(r => r.AddAsync(
            new Profesion { Id = existente.Id, Nom = "Duplicada" }));

        Assert.Equal(PkDuplicada, error.Number);
    }

    [FactConPersonaDb]
    public async Task La_PK_de_telefono_impide_duplicar_el_numero()
    {
        var existente = (await Leer<ITelefonoRepository, IReadOnlyList<Telefono>>(r => r.GetAllAsync())).First();

        var error = await RechazoAsync<ITelefonoRepository>(r => r.AddAsync(
            new Telefono { Num = existente.Num, Oper = "Duplicado", Duenio = existente.Duenio }));

        Assert.Equal(PkDuplicada, error.Number);
    }

    [FactConPersonaDb]
    public async Task La_PK_compuesta_de_estudios_impide_duplicar_el_par()
    {
        var existente = (await Leer<IEstudioRepository, IReadOnlyList<Estudio>>(r => r.GetAllAsync())).First();

        var error = await RechazoAsync<IEstudioRepository>(r => r.AddAsync(
            new Estudio { IdProf = existente.IdProf, CcPer = existente.CcPer, Univer = "Duplicado" }));

        Assert.Equal(PkDuplicada, error.Number);
    }

    [FactConPersonaDb]
    public async Task La_FK_impide_un_telefono_sin_dueno()
    {
        var error = await RechazoAsync<ITelefonoRepository>(r => r.AddAsync(
            new Telefono { Num = NumPrueba, Oper = "Huérfano", Duenio = CcInexistente }));

        Assert.Equal(RestriccionVulnerada, error.Number);
        Assert.Contains("telefono_persona_fk", error.Message);
        Assert.Null(await Leer<ITelefonoRepository, Telefono?>(r => r.GetByIdAsync(NumPrueba)));
    }

    [FactConPersonaDb]
    public async Task Las_FK_impiden_un_estudio_con_persona_o_profesion_inexistente()
    {
        var persona = (await Leer<IPersonaRepository, IReadOnlyList<Persona>>(r => r.GetAllAsync())).First();
        var profesion = (await Leer<IProfesionRepository, IReadOnlyList<Profesion>>(r => r.GetAllAsync())).First();

        var sinPersona = await RechazoAsync<IEstudioRepository>(r => r.AddAsync(
            new Estudio { IdProf = profesion.Id, CcPer = CcInexistente }));
        var sinProfesion = await RechazoAsync<IEstudioRepository>(r => r.AddAsync(
            new Estudio { IdProf = IdInexistente, CcPer = persona.Cc }));

        Assert.Equal(RestriccionVulnerada, sinPersona.Number);
        Assert.Contains("estudio_persona_fk", sinPersona.Message);
        Assert.Equal(RestriccionVulnerada, sinProfesion.Number);
        Assert.Contains("estudio_profesion_fk", sinProfesion.Message);
    }

    [FactConPersonaDb]
    public async Task El_CHECK_impide_un_genero_fuera_de_M_y_F()
    {
        var error = await RechazoAsync<IPersonaRepository>(r => r.AddAsync(
            new Persona { Cc = CcInexistente, Nombre = "Genero", Apellido = "Invalido", Genero = "X" }));

        Assert.Equal(RestriccionVulnerada, error.Number);
        Assert.Contains("CK_persona_genero", error.Message);
        Assert.Null(await Leer<IPersonaRepository, Persona?>(r => r.GetByIdAsync(CcInexistente)));
    }

    [FactConPersonaDb]
    public async Task La_base_impide_borrar_una_persona_con_telefonos()
    {
        var duenio = (await Leer<ITelefonoRepository, IReadOnlyList<Telefono>>(r => r.GetAllAsync())).First().Duenio;

        var error = await RechazoAsync<IPersonaRepository>(async r => await r.DeleteAsync((await r.GetByIdAsync(duenio))!));

        Assert.Equal(RestriccionVulnerada, error.Number);
        Assert.NotNull(await Leer<IPersonaRepository, Persona?>(r => r.GetByIdAsync(duenio)));
    }

    [FactConPersonaDb]
    public async Task La_base_impide_borrar_una_profesion_con_estudios()
    {
        var idProf = (await Leer<IEstudioRepository, IReadOnlyList<Estudio>>(r => r.GetAllAsync())).First().IdProf;

        var error = await RechazoAsync<IProfesionRepository>(async r => await r.DeleteAsync((await r.GetByIdAsync(idProf))!));

        Assert.Equal(RestriccionVulnerada, error.Number);
        Assert.Contains("estudio_profesion_fk", error.Message);
        Assert.NotNull(await Leer<IProfesionRepository, Profesion?>(r => r.GetByIdAsync(idProf)));
    }

    /// <summary>
    /// Ejecuta la escritura en un ámbito propio (un DbContext limpio) y devuelve el error de
    /// SQL Server que la rechazó. Falla si la base la acepta.
    /// </summary>
    private async Task<SqlException> RechazoAsync<TRepositorio>(Func<TRepositorio, Task> escritura)
        where TRepositorio : notnull
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<TRepositorio>();

        var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() => escritura(repositorio));
        return Assert.IsType<SqlException>(excepcion.InnerException);
    }

    private async Task<TResultado> Leer<TRepositorio, TResultado>(Func<TRepositorio, Task<TResultado>> lectura)
        where TRepositorio : notnull
    {
        using var scope = _factory.Services.CreateScope();
        return await lectura(scope.ServiceProvider.GetRequiredService<TRepositorio>());
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var servicios = scope.ServiceProvider;

        if (await servicios.GetRequiredService<ITelefonoRepository>().GetByIdAsync(NumPrueba) is { } telefono)
        {
            await servicios.GetRequiredService<ITelefonoRepository>().DeleteAsync(telefono);
        }

        if (await servicios.GetRequiredService<IPersonaRepository>().GetByIdAsync(CcInexistente) is { } persona)
        {
            await servicios.GetRequiredService<IPersonaRepository>().DeleteAsync(persona);
        }

        if (await servicios.GetRequiredService<IProfesionRepository>().GetByIdAsync(IdInexistente) is { } profesion)
        {
            await servicios.GetRequiredService<IProfesionRepository>().DeleteAsync(profesion);
        }
    }
}
