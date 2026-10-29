using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Escenarios de aceptación de SRS §10 contra persona_db real (HU-014), cruzando los dos
/// canales cuando el módulo web ya está integrado. Trabajan sobre filas propias (persona
/// 914000010, profesión 9142 y teléfono 9140000010) para no depender del estado que dejen
/// otras pruebas, y las borran al empezar y al terminar. El escenario 1 y la parte MVC del
/// 5 se añaden cuando se integre el módulo web de personas (HU-007).
/// </summary>
public class EscenariosSrsPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int CcPrueba = 914000010;
    private const int IdPrueba = 9142;
    private const string NumPrueba = "9140000010";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EscenariosSrsPersonaDbTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Escenario_2_una_profesion_creada_por_REST_se_ve_en_MVC()
    {
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdPrueba, Nom = "Profesión de escenario", Des = "Creada por REST" })).StatusCode);

        var listado = await HtmlAsync("/Profesiones");
        Assert.Contains($"data-profesion=\"{IdPrueba}\"", listado);
        Assert.Contains("Profesión de escenario", listado);

        var detalle = await HtmlAsync($"/Profesiones/Details/{IdPrueba}");
        Assert.Contains("Creada por REST", detalle);
    }

    [FactConPersonaDb]
    public async Task Escenario_3_un_telefono_queda_asociado_a_su_persona_y_se_consulta_la_relacion()
    {
        await CrearPersonaYProfesionAsync();

        var alta = await _client.PostAsJsonAsync("/api/telefonos",
            new TelefonoEntradaDto { Num = NumPrueba, Oper = "Claro", Duenio = CcPrueba });

        Assert.Equal(HttpStatusCode.Created, alta.StatusCode);
        Assert.Equal($"/api/telefonos/{NumPrueba}", alta.Headers.Location!.AbsolutePath);

        // La relación se consulta desde el teléfono: su dueño es la persona creada.
        var telefono = await _client.GetFromJsonAsync<TelefonoDto>($"/api/telefonos/{NumPrueba}");
        Assert.Equal(NumPrueba, telefono!.Num);
        Assert.Equal("Claro", telefono.Oper);
        Assert.Equal(CcPrueba, telefono.Duenio);

        var dueno = await _client.GetFromJsonAsync<PersonaDto>($"/api/personas/{telefono.Duenio}");
        Assert.Equal("Escenario", dueno!.Nombre);

        // Y aparece en el listado entre los teléfonos de esa persona.
        var telefonos = await _client.GetFromJsonAsync<List<TelefonoDto>>("/api/telefonos");
        Assert.Contains(telefonos!, t => t.Num == NumPrueba && t.Duenio == CcPrueba);
    }

    [FactConPersonaDb]
    public async Task Escenario_3_un_telefono_creado_por_MVC_con_el_selector_se_consulta_por_REST()
    {
        await CrearPersonaYProfesionAsync();
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        // El dueño se elige en el selector, que ya ofrece a la persona recién creada.
        Assert.Contains($"<option value=\"{CcPrueba}\">{CcPrueba} · Escenario Srs</option>",
            await HtmlAsync("/Telefonos/Create"));

        var alta = await cliente.PostAsync("/Telefonos/Create", Formulario(token, new()
        {
            ["Num"] = NumPrueba, ["Oper"] = "Claro", ["Duenio"] = $"{CcPrueba}"
        }));
        Assert.Equal(HttpStatusCode.OK, alta.StatusCode);

        var porRest = await _client.GetFromJsonAsync<TelefonoDto>($"/api/telefonos/{NumPrueba}");
        Assert.Equal(CcPrueba, porRest!.Duenio);
        Assert.Equal("Claro", porRest.Oper);
        Assert.Contains($"{CcPrueba} · Escenario Srs", await HtmlAsync($"/Telefonos/Details/{NumPrueba}"));
    }

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

        // La misma fila por el canal web, con las dos mitades de la clave en la ruta.
        var detalle = await HtmlAsync($"/Estudios/Details/{IdPrueba}/{CcPrueba}");
        Assert.Contains($"{IdPrueba} · Profesión de escenario", detalle);
        Assert.Contains($"{CcPrueba} · Escenario Srs", detalle);
        Assert.Contains("2022-06-30", detalle);
        Assert.Contains("Universidad Javeriana", detalle);
    }

    [FactConPersonaDb]
    public async Task Escenarios_5_y_6_una_persona_con_telefono_y_estudio_no_se_borra_hasta_quitar_sus_dependientes()
    {
        await CrearPersonaYProfesionAsync();
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/telefonos",
            new TelefonoEntradaDto { Num = NumPrueba, Oper = "Claro", Duenio = CcPrueba })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/estudios",
            new EstudioEntradaDto { IdProf = IdPrueba, CcPer = CcPrueba })).StatusCode);

        // Escenario 5: con dependientes, el borrado responde 409 y la persona sigue.
        await BorradoBloqueadoAsync();

        // Escenario 6: primero los dependientes, después la persona. Quitar solo el
        // teléfono no basta: el estudio sigue bloqueando el borrado.
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/telefonos/{NumPrueba}")).StatusCode);
        await BorradoBloqueadoAsync();
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/estudios/{IdPrueba}/{CcPrueba}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/personas/{CcPrueba}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/personas/{CcPrueba}")).StatusCode);

        // La profesión ya no tiene estudios: también se puede borrar.
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/profesiones/{IdPrueba}")).StatusCode);
    }

    private async Task BorradoBloqueadoAsync()
    {
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/personas/{CcPrueba}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/personas/{CcPrueba}")).StatusCode);
    }

    // Las vistas codifican los acentos como entidades HTML: se decodifican para comparar.
    private async Task<string> HtmlAsync(string ruta)
    {
        var respuesta = await _client.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
    }

    // El mismo cliente conserva la cookie antiforgery que acompaña al token del formulario.
    private async Task<(HttpClient Cliente, string Token)> ClienteConTokenAsync(string ruta)
    {
        var cliente = _factory.CreateClient();
        var html = await cliente.GetStringAsync(ruta);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        return (cliente, token);
    }

    private static FormUrlEncodedContent Formulario(string token, Dictionary<string, string> campos)
    {
        campos["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(campos);
    }

    private async Task CrearPersonaYProfesionAsync()
    {
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/personas",
            new PersonaEntradaDto { Cc = CcPrueba, Nombre = "Escenario", Apellido = "Srs", Genero = "F", Edad = 30 })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdPrueba, Nom = "Profesión de escenario" })).StatusCode);
    }

    // Por el mismo canal REST y en orden de dependencias: teléfono, estudio, persona y profesión.
    // Un 404 aquí solo significa que no quedaba nada.
    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        await _client.DeleteAsync($"/api/telefonos/{NumPrueba}");
        await _client.DeleteAsync($"/api/estudios/{IdPrueba}/{CcPrueba}");
        await _client.DeleteAsync($"/api/personas/{CcPrueba}");
        await _client.DeleteAsync($"/api/profesiones/{IdPrueba}");
    }
}
