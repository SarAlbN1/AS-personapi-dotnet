using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace personapi_dotnet.Tests.Unit;

/// <summary>
/// Comprobaciones de arranque de HU-001: el árbol Front/Back obliga a configurar
/// WebRootPath y ViewLocationFormats, y Swagger debe publicar un documento OpenAPI 3.
/// </summary>
public class ArranqueWebTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ArranqueWebTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Swagger_publica_un_documento_OpenAPI_3()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var version = documento.RootElement.GetProperty("openapi").GetString();

        Assert.StartsWith("3.", version);
    }

    [Fact]
    public async Task La_vista_de_inicio_se_resuelve_desde_Front_Views()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Los_estaticos_se_sirven_desde_Front_wwwroot()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/css/site.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
