using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace personapi_dotnet.Tests.Unit.OpenApi;

/// <summary>
/// Comprobaciones de HU-009 sobre el documento que publica /swagger/v1/swagger.json, sin
/// base de datos. La regla de códigos se aplica a toda operación de /api; los textos y
/// ejemplos se exigen a los recursos de <see cref="RecursosDocumentados"/>, que ya son los cuatro.
/// </summary>
public class DocumentoOpenApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string[] RecursosDocumentados = ["personas", "profesiones", "telefonos", "estudios"];

    // Respuestas genéricas que Swashbuckle pone cuando la operación no trae descripción.
    private static readonly string[] DescripcionesGenericas =
        ["OK", "Created", "No Content", "Bad Request", "Not Found", "Conflict", "Internal Server Error"];

    private readonly WebApplicationFactory<Program> _factory;

    public DocumentoOpenApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task El_documento_es_OpenAPI_3_y_se_titula_PersonAPI()
    {
        using var documento = await DocumentoAsync();
        var raiz = documento.RootElement;

        Assert.StartsWith("3.", raiz.GetProperty("openapi").GetString());
        Assert.Equal("PersonAPI", raiz.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Los_cuatro_recursos_publican_las_veinte_operaciones()
    {
        using var documento = await DocumentoAsync();
        var operaciones = Operaciones(documento).Select(o => $"{o.Verbo} {o.Ruta}").ToHashSet();

        foreach (var (recurso, clave) in new[] { ("personas", "{cc}"), ("profesiones", "{id}"), ("telefonos", "{num}"), ("estudios", "{idProf}/{ccPer}") })
        {
            Assert.Contains($"get /api/{recurso}", operaciones);
            Assert.Contains($"post /api/{recurso}", operaciones);
            Assert.Contains($"get /api/{recurso}/{clave}", operaciones);
            Assert.Contains($"put /api/{recurso}/{clave}", operaciones);
            Assert.Contains($"delete /api/{recurso}/{clave}", operaciones);
        }
    }

    [Fact]
    public async Task Cada_operacion_de_la_api_documenta_los_codigos_del_contrato()
    {
        using var documento = await DocumentoAsync();

        foreach (var operacion in Operaciones(documento))
        {
            var esperados = operacion.Verbo switch
            {
                "get" when operacion.Ruta.Contains('{') => new[] { "200", "404" },
                "get" => ["200"],
                "post" => ["201", "400", "409"],
                "put" => ["204", "400", "404"],
                "delete" => ["204", "404"],
                _ => throw new InvalidOperationException($"Verbo no previsto: {operacion.Verbo}")
            };

            var codigos = operacion.Respuestas.EnumerateObject().Select(r => r.Name).ToList();
            foreach (var codigo in esperados.Append("500"))
            {
                Assert.True(codigos.Contains(codigo), $"{operacion} no documenta {codigo}.");
            }

            // Todo error se describe con el esquema de ProblemDetails (o su variante de validación).
            foreach (var error in operacion.Respuestas.EnumerateObject().Where(r => r.Name[0] is '4' or '5'))
            {
                var esquema = error.Value.GetProperty("content").EnumerateObject().First()
                    .Value.GetProperty("schema").GetProperty("$ref").GetString();
                Assert.True(
                    esquema is "#/components/schemas/ProblemDetails" or "#/components/schemas/ValidationProblemDetails",
                    $"{operacion} {error.Name} usa {esquema}.");
            }
        }
    }

    [Fact]
    public async Task Borrar_personas_y_profesiones_documenta_el_409_por_dependencias()
    {
        using var documento = await DocumentoAsync();

        foreach (var ruta in new[] { "/api/personas/{cc}", "/api/profesiones/{id}" })
        {
            var borrar = Operaciones(documento).Single(o => o.Verbo == "delete" && o.Ruta == ruta);
            Assert.True(borrar.Respuestas.TryGetProperty("409", out _), $"{borrar} no documenta 409.");
        }
    }

    [Fact]
    public async Task Los_recursos_documentados_tienen_resumen_y_una_descripcion_propia_por_codigo()
    {
        using var documento = await DocumentoAsync();

        foreach (var operacion in Operaciones(documento).Where(o => RecursosDocumentados.Any(o.EsDe)))
        {
            Assert.False(string.IsNullOrWhiteSpace(operacion.Resumen), $"{operacion} no tiene resumen.");

            foreach (var respuesta in operacion.Respuestas.EnumerateObject())
            {
                var descripcion = respuesta.Value.GetProperty("description").GetString();
                Assert.DoesNotContain(descripcion, DescripcionesGenericas);
            }
        }
    }

    [Theory]
    [InlineData("PersonaEntradaDto")]
    [InlineData("PersonaDto")]
    [InlineData("ProfesionDto")]
    [InlineData("TelefonoEntradaDto")]
    [InlineData("TelefonoDto")]
    [InlineData("EstudioEntradaDto")]
    [InlineData("EstudioDto")]
    public async Task Los_esquemas_de_entrada_y_salida_traen_ejemplo_en_cada_propiedad(string esquema)
    {
        using var documento = await DocumentoAsync();

        var propiedades = documento.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty(esquema).GetProperty("properties");

        foreach (var propiedad in propiedades.EnumerateObject())
        {
            Assert.True(propiedad.Value.TryGetProperty("example", out _), $"{esquema}.{propiedad.Name} no tiene ejemplo.");
        }
    }

    [Fact]
    public async Task Un_error_inesperado_en_la_api_responde_500_sin_detalles_internos()
    {
        // Fuera de Development y con una base inalcanzable: así es como un cliente vería
        // un fallo real. La cadena no tiene credenciales; basta con que no conecte.
        const string cadena = "Server=tcp:127.0.0.1,1;Database=persona_db;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=2";
        using var fabrica = _factory.WithWebHostBuilder(host =>
        {
            host.UseEnvironment("Production");
            host.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:PersonaDb"] = cadena }));
        });

        var respuesta = await fabrica.CreateClient().GetAsync("/api/personas");
        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Equal(500, JsonDocument.Parse(cuerpo).RootElement.GetProperty("status").GetInt32());
        foreach (var fuga in new[] { "Exception", "SqlClient", "127.0.0.1", "persona_db", "Server=", "   at " })
        {
            Assert.DoesNotContain(fuga, cuerpo);
        }
    }

    private async Task<JsonDocument> DocumentoAsync()
    {
        var json = await _factory.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
        return JsonDocument.Parse(json);
    }

    private static IEnumerable<Operacion> Operaciones(JsonDocument documento) =>
        documento.RootElement.GetProperty("paths").EnumerateObject()
            .Where(ruta => ruta.Name.StartsWith("/api/"))
            .SelectMany(ruta => ruta.Value.EnumerateObject().Select(verbo => new Operacion(
                verbo.Name,
                ruta.Name,
                verbo.Value.TryGetProperty("summary", out var resumen) ? resumen.GetString() : null,
                verbo.Value.GetProperty("responses"))));

    private sealed record Operacion(string Verbo, string Ruta, string? Resumen, JsonElement Respuestas)
    {
        public bool EsDe(string recurso) =>
            Ruta == $"/api/{recurso}" || Ruta.StartsWith($"/api/{recurso}/");

        public override string ToString() => $"{Verbo.ToUpperInvariant()} {Ruta}";
    }
}
