using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Tests.Unit.Integracion;

namespace personapi_dotnet.Tests.Unit.Api;

/// <summary>
/// Comprobaciones de HU-004: las cinco operaciones de <c>/api/personas</c> contra
/// persona_db real, con sus casos positivos y negativos. Las filas base son las de
/// scripts/02_dml.sql; las que crea la prueba usan cédulas propias y se borran al final.
/// </summary>
public class PersonasApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int CcAna = 1010101010;
    private const int CcInexistente = 777000777;

    private readonly WebApplicationFactory<Program> _factory;

    public PersonasApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static PersonaEntradaDto Entrada(int cc, string genero = "M", int? edad = 30) => new()
    {
        Cc = cc,
        Nombre = "Prueba",
        Apellido = "HU004",
        Genero = genero,
        Edad = edad
    };

    [FactConPersonaDb]
    public async Task Listado_devuelve_200_y_las_personas_del_dml()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/personas");
        var personas = await respuesta.Content.ReadFromJsonAsync<List<PersonaDto>>();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.NotNull(personas);
        Assert.Contains(personas, p => p.Cc == CcAna && p.Apellido == "Gómez");
    }

    [FactConPersonaDb]
    public async Task Detalle_devuelve_200_con_la_persona_y_404_si_no_existe()
    {
        var cliente = _factory.CreateClient();

        var encontrada = await cliente.GetAsync($"/api/personas/{CcAna}");
        var ausente = await cliente.GetAsync($"/api/personas/{CcInexistente}");
        var persona = await encontrada.Content.ReadFromJsonAsync<PersonaDto>();

        Assert.Equal(HttpStatusCode.OK, encontrada.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
        Assert.NotNull(persona);
        Assert.Equal("Ana", persona.Nombre);
        Assert.Equal("F", persona.Genero);
    }

    [FactConPersonaDb]
    public async Task Ciclo_completo_crea_consulta_edita_y_elimina()
    {
        var cliente = _factory.CreateClient();
        const int cc = 900100100;
        await cliente.DeleteAsync($"/api/personas/{cc}");

        var creada = await cliente.PostAsJsonAsync("/api/personas", Entrada(cc));
        var creado = await creada.Content.ReadFromJsonAsync<PersonaDto>();

        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
        Assert.Equal($"/api/personas/{cc}", creada.Headers.Location?.AbsolutePath);
        Assert.NotNull(creado);
        Assert.Equal(cc, creado.Cc);

        // La fila existe de verdad: se consulta por el mismo canal tras el alta.
        var consultada = await cliente.GetAsync($"/api/personas/{cc}");
        Assert.Equal(HttpStatusCode.OK, consultada.StatusCode);

        var editada = await cliente.PutAsJsonAsync(
            $"/api/personas/{cc}",
            new PersonaEntradaDto { Cc = cc, Nombre = "Editada", Apellido = "HU004", Genero = "F", Edad = 41 });
        Assert.Equal(HttpStatusCode.NoContent, editada.StatusCode);

        var despues = await (await cliente.GetAsync($"/api/personas/{cc}")).Content
            .ReadFromJsonAsync<PersonaDto>();
        Assert.NotNull(despues);
        Assert.Equal("Editada", despues.Nombre);
        Assert.Equal("F", despues.Genero);
        Assert.Equal(41, despues.Edad);

        var eliminada = await cliente.DeleteAsync($"/api/personas/{cc}");
        var tras = await cliente.GetAsync($"/api/personas/{cc}");

        Assert.Equal(HttpStatusCode.NoContent, eliminada.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tras.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Alta_con_cedula_repetida_devuelve_409()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/personas", Entrada(CcAna));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Alta_invalida_devuelve_400()
    {
        var cliente = _factory.CreateClient();
        const int cc = 900200200;

        var generoFuraDeDominio = await cliente.PostAsJsonAsync("/api/personas", Entrada(cc, genero: "X"));
        var edadNegativa = await cliente.PostAsJsonAsync("/api/personas", Entrada(cc, edad: -1));
        var sinNombre = await cliente.PostAsJsonAsync(
            "/api/personas",
            new PersonaEntradaDto { Cc = cc, Nombre = "", Apellido = "HU004", Genero = "M", Edad = 20 });

        Assert.Equal(HttpStatusCode.BadRequest, generoFuraDeDominio.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, edadNegativa.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, sinNombre.StatusCode);

        // Ninguno de los tres intentos debe haber escrito la fila.
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/personas/{cc}")).StatusCode);
    }

    [FactConPersonaDb]
    public async Task Edicion_con_cedula_distinta_en_ruta_y_cuerpo_devuelve_400()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PutAsJsonAsync($"/api/personas/{CcAna}", Entrada(1020202020));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Edicion_y_borrado_de_cedula_inexistente_devuelven_404()
    {
        var cliente = _factory.CreateClient();

        var editada = await cliente.PutAsJsonAsync($"/api/personas/{CcInexistente}", Entrada(CcInexistente));
        var eliminada = await cliente.DeleteAsync($"/api/personas/{CcInexistente}");

        Assert.Equal(HttpStatusCode.NotFound, editada.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, eliminada.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Borrado_de_persona_con_dependientes_devuelve_409_y_no_cambia_la_base()
    {
        var cliente = _factory.CreateClient();

        // Ana tiene teléfono y estudio en el DML de HU-002.
        var respuesta = await cliente.DeleteAsync($"/api/personas/{CcAna}");
        var sigueAhi = await cliente.GetAsync($"/api/personas/{CcAna}");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, sigueAhi.StatusCode);
    }
}
