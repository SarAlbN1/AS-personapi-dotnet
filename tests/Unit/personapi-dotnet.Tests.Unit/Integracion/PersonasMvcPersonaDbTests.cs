using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Comprobaciones de HU-007 contra persona_db real: el CRUD web escribe en la misma
/// base que el canal REST (escenario SRS §10.1) y el borrado bloqueado de la regla 10
/// se ve en la vista. Las filas base son las de scripts/02_dml.sql; las que crea la
/// prueba usan cédulas propias y se limpian por el canal REST.
/// </summary>
public partial class PersonasMvcPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>
{
    /// <summary>Ana tiene teléfono y estudio en el DML: es la persona que no se puede borrar.</summary>
    private const int CcConDependencias = 1010101010;

    private const int CcDePrueba = 900700700;

    private readonly WebApplicationFactory<Program> _factory;

    public PersonasMvcPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [FactConPersonaDb]
    public async Task Una_persona_creada_desde_MVC_se_consulta_por_REST_y_coincide()
    {
        var cliente = _factory.CreateClient();
        await cliente.DeleteAsync($"/api/personas/{CcDePrueba}");
        var token = await TokenAsync(cliente, "/Personas/Create");

        var alta = await cliente.PostAsync("/Personas/Create", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Cc"] = CcDePrueba.ToString(),
                ["Nombre"] = "Marta",
                ["Apellido"] = "Web",
                ["Genero"] = "F",
                ["Edad"] = "41",
                ["EsEdicion"] = "false",
                ["__RequestVerificationToken"] = token
            }));

        // El alta web redirige al detalle: la vista ya no es la del formulario.
        Assert.Equal(HttpStatusCode.OK, alta.StatusCode);
        Assert.Equal($"/Personas/Details/{CcDePrueba}", alta.RequestMessage!.RequestUri!.AbsolutePath);

        // Escenario SRS 10.1: lo creado por MVC se lee por el canal JSON y coincide.
        var porRest = await cliente.GetFromJsonAsync<PersonaDto>($"/api/personas/{CcDePrueba}");

        Assert.NotNull(porRest);
        Assert.Equal("Marta", porRest.Nombre);
        Assert.Equal("Web", porRest.Apellido);
        Assert.Equal("F", porRest.Genero);
        Assert.Equal(41, porRest.Edad);

        await cliente.DeleteAsync($"/api/personas/{CcDePrueba}");
    }

    [FactConPersonaDb]
    public async Task El_borrado_de_una_persona_con_dependientes_se_explica_y_no_elimina()
    {
        var cliente = _factory.CreateClient();
        var token = await TokenAsync(cliente, $"/Personas/Delete/{CcConDependencias}");

        var respuesta = await cliente.PostAsync(
            $"/Personas/Delete/{CcConDependencias}",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("No se puede eliminar la persona", html);

        // La fila sigue ahí, comprobado por el otro canal.
        var porRest = await cliente.GetAsync($"/api/personas/{CcConDependencias}");
        Assert.Equal(HttpStatusCode.OK, porRest.StatusCode);
    }

    [FactConPersonaDb]
    public async Task El_listado_web_muestra_las_personas_del_dml()
    {
        var html = await _factory.CreateClient().GetStringAsync("/Personas");

        Assert.Contains("data-tabla=\"personas\"", html);
        Assert.Contains(CcConDependencias.ToString(), html);
    }

    private static async Task<string> TokenAsync(HttpClient cliente, string ruta)
    {
        var html = await cliente.GetStringAsync(ruta);
        return TokenRegex().Match(html).Groups[1].Value;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
