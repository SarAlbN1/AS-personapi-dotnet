using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Recorrido de /api/estudios contra persona_db real (HU-011). Las filas que crea la
/// prueba usan la profesión 3, que no tiene estudios en scripts/02_dml.sql, y se borran
/// al empezar y al terminar; los estudios sembrados solo se leen o se intentan modificar
/// sin éxito.
/// </summary>
public class EstudiosApiPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int ProfLibre = 3;
    private const int CcAna = 1010101010;
    private const int CcLuis = 1020202020;
    private const int CcSofia = 1030303030;

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EstudiosApiPersonaDbTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Listar_devuelve_200_con_los_estudios_sembrados()
    {
        var response = await _client.GetAsync("/api/estudios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var estudios = await response.Content.ReadFromJsonAsync<List<EstudioDto>>();
        Assert.Contains(estudios!, e => e.IdProf == 2 && e.CcPer == CcLuis && e.Fecha == new DateOnly(2015, 6, 20));
    }

    [FactConPersonaDb]
    public async Task Recorrido_crear_consultar_editar_borrar_por_las_dos_claves()
    {
        var creado = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcLuis, Fecha = new DateOnly(2019, 11, 30), Univer = "Universidad del Rosario" });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal($"/api/estudios/{ProfLibre}/{CcLuis}", creado.Headers.Location!.AbsolutePath);

        var consultado = await _client.GetFromJsonAsync<EstudioDto>($"/api/estudios/{ProfLibre}/{CcLuis}");
        Assert.Equal("Universidad del Rosario", consultado!.Univer);
        Assert.Equal(new DateOnly(2019, 11, 30), consultado.Fecha);

        var editado = await _client.PutAsJsonAsync($"/api/estudios/{ProfLibre}/{CcLuis}",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcLuis, Fecha = null, Univer = "Universidad EAFIT" });
        Assert.Equal(HttpStatusCode.NoContent, editado.StatusCode);

        var trasEditar = await LeerDeLaBaseAsync(ProfLibre, CcLuis);
        Assert.Null(trasEditar!.Fecha);
        Assert.Equal("Universidad EAFIT", trasEditar.Univer);

        var borrado = await _client.DeleteAsync($"/api/estudios/{ProfLibre}/{CcLuis}");
        Assert.Equal(HttpStatusCode.NoContent, borrado.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(ProfLibre, CcLuis));
    }

    [FactConPersonaDb]
    public async Task Crear_admite_fecha_y_universidad_nulas()
    {
        var response = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcSofia });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fila = await LeerDeLaBaseAsync(ProfLibre, CcSofia);
        Assert.Null(fila!.Fecha);
        Assert.Null(fila.Univer);
    }

    [FactConPersonaDb]
    public async Task Crear_con_profesion_o_persona_inexistente_devuelve_400_y_no_escribe()
    {
        var sinProfesion = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = 9099, CcPer = CcLuis });
        var sinPersona = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = 777000777 });

        Assert.Equal(HttpStatusCode.BadRequest, sinProfesion.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, sinPersona.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(9099, CcLuis));
        Assert.Null(await LeerDeLaBaseAsync(ProfLibre, 777000777));
    }

    [FactConPersonaDb]
    public async Task Crear_par_duplicado_devuelve_409_sin_cambiar_la_fila()
    {
        var response = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = 1, CcPer = CcAna, Univer = "Duplicada" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Pontificia Universidad Javeriana", (await LeerDeLaBaseAsync(1, CcAna))!.Univer);
    }

    [FactConPersonaDb]
    public async Task Editar_con_otro_par_en_el_cuerpo_devuelve_400_y_no_cambia_la_clave()
    {
        var response = await _client.PutAsJsonAsync($"/api/estudios/1/{CcAna}",
            new EstudioEntradaDto { IdProf = 2, CcPer = CcAna, Univer = "Otra" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Pontificia Universidad Javeriana", (await LeerDeLaBaseAsync(1, CcAna))!.Univer);
        Assert.Null(await LeerDeLaBaseAsync(2, CcAna));
    }

    [FactConPersonaDb]
    public async Task Consultar_editar_y_borrar_par_inexistente_devuelven_404()
    {
        var ruta = $"/api/estudios/{ProfLibre}/{CcAna}";

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(ruta)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync(ruta,
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcAna })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(ruta)).StatusCode);
    }

    [FactConPersonaDb]
    public async Task Fecha_invalida_o_sin_clave_devuelven_400_y_no_escriben()
    {
        foreach (var cuerpo in new[]
                 {
                     $$"""{"idProf":{{ProfLibre}},"ccPer":{{CcLuis}},"fecha":"2020-02-30"}""",
                     $$"""{"idProf":{{ProfLibre}},"ccPer":{{CcLuis}},"fecha":"15/12/2020"}""",
                     $$"""{"ccPer":{{CcLuis}},"univer":"Sin profesión"}"""
                 })
        {
            var response = await _client.PostAsync("/api/estudios",
                new StringContent(cuerpo, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        Assert.Null(await LeerDeLaBaseAsync(ProfLibre, CcLuis));
    }

    [FactConPersonaDb]
    public async Task Universidad_se_mide_sin_los_espacios_de_los_extremos()
    {
        var univer49 = new string('u', 49);

        var conEspacios = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcLuis, Univer = $"  {univer49}  " });
        var larga = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = ProfLibre, CcPer = CcSofia, Univer = new string('u', 51) });

        Assert.Equal(HttpStatusCode.Created, conEspacios.StatusCode);
        Assert.Equal(univer49, (await LeerDeLaBaseAsync(ProfLibre, CcLuis))!.Univer);
        Assert.Equal(HttpStatusCode.BadRequest, larga.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(ProfLibre, CcSofia));
    }

    private async Task<Estudio?> LeerDeLaBaseAsync(int idProf, int ccPer)
    {
        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IEstudioRepository>();
        return (await repositorio.GetAllAsync()).SingleOrDefault(e => e.IdProf == idProf && e.CcPer == ccPer);
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IEstudioRepository>();
        foreach (var cc in new[] { CcAna, CcLuis, CcSofia })
        {
            if (await repositorio.GetByIdAsync(ProfLibre, cc) is { } fila)
            {
                await repositorio.DeleteAsync(fila);
            }
        }
    }
}
