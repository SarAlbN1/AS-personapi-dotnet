using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Recorrido de /api/profesiones contra persona_db real (HU-005). Usa ids 9050 a 9059,
/// fuera de los datos de scripts/02_dml.sql, y los borra al terminar; las filas sembradas
/// solo se leen o se intentan borrar sin éxito.
/// </summary>
public class ProfesionesApiPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int IdPrueba = 9051;
    private const int IdDesLarga = 9052;

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProfesionesApiPersonaDbTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Listar_devuelve_200_con_las_profesiones_sembradas()
    {
        var response = await _client.GetAsync("/api/profesiones");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profesiones = await response.Content.ReadFromJsonAsync<List<ProfesionDto>>();
        Assert.Contains(profesiones!, p => p.Id == 2 && p.Nom == "Medicina");
    }

    [FactConPersonaDb]
    public async Task Recorrido_crear_consultar_editar_borrar()
    {
        var creado = await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdPrueba, Nom = "Arquitectura", Des = null });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal($"/api/profesiones/{IdPrueba}", creado.Headers.Location!.AbsolutePath);

        var consultado = await _client.GetFromJsonAsync<ProfesionDto>($"/api/profesiones/{IdPrueba}");
        Assert.Equal("Arquitectura", consultado!.Nom);
        Assert.Null(consultado.Des);

        var editado = await _client.PutAsJsonAsync($"/api/profesiones/{IdPrueba}",
            new ProfesionDto { Id = IdPrueba, Nom = "Arquitectura de software", Des = "Diseño de sistemas" });
        Assert.Equal(HttpStatusCode.NoContent, editado.StatusCode);

        var trasEditar = await LeerDeLaBaseAsync(IdPrueba);
        Assert.Equal("Arquitectura de software", trasEditar!.Nom);
        Assert.Equal("Diseño de sistemas", trasEditar.Des);

        var borrado = await _client.DeleteAsync($"/api/profesiones/{IdPrueba}");
        Assert.Equal(HttpStatusCode.NoContent, borrado.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));
    }

    [FactConPersonaDb]
    public async Task Crear_con_id_duplicado_devuelve_409_sin_cambiar_la_fila()
    {
        var response = await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = 2, Nom = "Duplicada" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Medicina", (await LeerDeLaBaseAsync(2))!.Nom);
    }

    [FactConPersonaDb]
    public async Task Crear_sin_nom_devuelve_400()
    {
        var response = await _client.PostAsJsonAsync("/api/profesiones", new { id = IdPrueba, des = "Sin nombre" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));
    }

    [FactConPersonaDb]
    public async Task Editar_con_id_distinto_al_de_la_ruta_devuelve_400()
    {
        var response = await _client.PutAsJsonAsync("/api/profesiones/2",
            new ProfesionDto { Id = 3, Nom = "Medicina" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Consultar_editar_y_borrar_inexistente_devuelven_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/profesiones/9059")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/profesiones/9059",
            new ProfesionDto { Id = 9059, Nom = "Nada" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/profesiones/9059")).StatusCode);
    }

    [FactConPersonaDb]
    public async Task Borrar_profesion_con_estudios_devuelve_409_y_la_base_queda_igual()
    {
        // Solo los estudios de la profesión 1: otras clases de prueba crean y borran
        // estudios en paralelo con otras profesiones, y un conteo total sería inestable.
        var estudiosAntes = (await LeerEstudiosAsync()).Count(e => e.IdProf == 1);

        var response = await _client.DeleteAsync("/api/profesiones/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(await LeerDeLaBaseAsync(1));
        Assert.Equal(estudiosAntes, (await LeerEstudiosAsync()).Count(e => e.IdProf == 1));
    }

    [FactConPersonaDb]
    public async Task Des_admite_texto_largo()
    {
        var larga = new string('x', 20_000);

        var response = await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdDesLarga, Nom = "Texto largo", Des = larga });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(larga, (await LeerDeLaBaseAsync(IdDesLarga))!.Des);
    }

    private async Task<Back.Persistencia.Entities.Profesion?> LeerDeLaBaseAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IProfesionRepository>();
        return (await repositorio.GetAllAsync()).SingleOrDefault(p => p.Id == id);
    }

    private async Task<IReadOnlyList<Back.Persistencia.Entities.Estudio>> LeerEstudiosAsync()
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IEstudioRepository>().GetAllAsync();
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IProfesionRepository>();
        foreach (var id in new[] { IdPrueba, IdDesLarga })
        {
            if (await repositorio.GetByIdAsync(id) is { } fila)
            {
                await repositorio.DeleteAsync(fila);
            }
        }
    }
}
