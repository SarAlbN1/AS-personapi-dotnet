using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Escenarios de aceptación de SRS §10 contra persona_db real (HU-014), por el canal REST.
/// Trabajan sobre filas propias (persona 914000010 y profesión 9142) para no depender del
/// estado que dejen otras pruebas, y las borran al empezar y al terminar. Las partes que
/// pasan por las vistas Razor y por teléfonos se añaden cuando se integren sus módulos.
/// </summary>
public class EscenariosSrsPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int CcPrueba = 914000010;
    private const int IdPrueba = 9142;

    private readonly HttpClient _client;

    public EscenariosSrsPersonaDbTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Escenario_4_un_estudio_vincula_persona_y_profesion_y_se_consulta_por_la_clave_compuesta()
    {
        await CrearPersonaYProfesionAsync();

        var alta = await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = IdPrueba, CcPer = CcPrueba, Fecha = new DateOnly(2022, 6, 30), Univer = "Universidad Javeriana" });

        Assert.Equal(HttpStatusCode.Created, alta.StatusCode);
        Assert.Equal($"/api/estudios/{IdPrueba}/{CcPrueba}", alta.Headers.Location!.AbsolutePath);

        var estudio = await _client.GetFromJsonAsync<EstudioDto>($"/api/estudios/{IdPrueba}/{CcPrueba}");
        Assert.Equal(IdPrueba, estudio!.IdProf);
        Assert.Equal(CcPrueba, estudio.CcPer);
        Assert.Equal(new DateOnly(2022, 6, 30), estudio.Fecha);
        Assert.Equal("Universidad Javeriana", estudio.Univer);

        // El par es la identidad: con las mitades invertidas no hay estudio.
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/estudios/{CcPrueba}/{IdPrueba}")).StatusCode);
    }

    [FactConPersonaDb]
    public async Task Escenarios_5_y_6_una_persona_con_estudios_no_se_borra_hasta_quitar_sus_dependientes()
    {
        await CrearPersonaYProfesionAsync();
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = IdPrueba, CcPer = CcPrueba })).StatusCode);

        // Escenario 5: con un estudio asociado, el borrado responde 409 y la persona sigue.
        var bloqueado = await _client.DeleteAsync($"/api/personas/{CcPrueba}");
        Assert.Equal(HttpStatusCode.Conflict, bloqueado.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/personas/{CcPrueba}")).StatusCode);

        // Escenario 6: primero el dependiente, después la persona.
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/estudios/{IdPrueba}/{CcPrueba}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/personas/{CcPrueba}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/personas/{CcPrueba}")).StatusCode);

        // La profesión ya no tiene estudios: también se puede borrar.
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/profesiones/{IdPrueba}")).StatusCode);
    }

    private async Task CrearPersonaYProfesionAsync()
    {
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/personas",
            new PersonaEntradaDto { Cc = CcPrueba, Nombre = "Escenario", Apellido = "Srs", Genero = "F", Edad = 30 })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdPrueba, Nom = "Profesión de escenario" })).StatusCode);
    }

    // Por el mismo canal REST y en orden de dependencias: estudio, persona y profesión.
    // Un 404 aquí solo significa que no quedaba nada.
    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        await _client.DeleteAsync($"/api/estudios/{IdPrueba}/{CcPrueba}");
        await _client.DeleteAsync($"/api/personas/{CcPrueba}");
        await _client.DeleteAsync($"/api/profesiones/{IdPrueba}");
    }
}
