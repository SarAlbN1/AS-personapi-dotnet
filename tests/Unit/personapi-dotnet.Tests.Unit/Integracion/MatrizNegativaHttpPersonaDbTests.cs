using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Matriz negativa de HU-014 por HTTP contra persona_db real: clave duplicada 409,
/// referencia inválida 400, clave ausente 404 y borrado bloqueado 409, en cada recurso
/// REST integrado. Cada caso comprueba el código, que el cuerpo es un ProblemDetails con
/// mensaje y que la fila afectada no cambió. Solo lee filas de scripts/02_dml.sql y usa
/// claves que no existen (cc 914000002, id 9141, num 9140000002); ninguna escritura
/// prospera, así que no hay nada que limpiar.
/// </summary>
public class MatrizNegativaHttpPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int CcAna = 1010101010;
    private const int CcLaura = 1030303030;
    private const int CcInexistente = 914000002;
    private const int IdInexistente = 9141;
    private const string NumInexistente = "9140000002";

    private readonly HttpClient _client;

    public MatrizNegativaHttpPersonaDbTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    /// <summary>
    /// Recurso, caso, verbo, ruta, cuerpo, código esperado y la fila que no debe cambiar.
    /// Se compara solo esa fila, no el listado: otras clases de prueba escriben en paralelo.
    /// </summary>
    public static TheoryData<string, string, string, string, object?, HttpStatusCode, string> Casos => new()
    {
        // Personas no reciben referencias, así que no tienen caso de 400 por referencia.
        { "personas", "clave duplicada", "POST", "/api/personas",
            new PersonaEntradaDto { Cc = CcAna, Nombre = "Duplicada", Apellido = "Prueba", Genero = "F" },
            HttpStatusCode.Conflict, $"/api/personas/{CcAna}" },
        { "personas", "clave ausente", "GET", $"/api/personas/{CcInexistente}", null,
            HttpStatusCode.NotFound, $"/api/personas/{CcInexistente}" },
        { "personas", "clave ausente", "PUT", $"/api/personas/{CcInexistente}",
            new PersonaEntradaDto { Cc = CcInexistente, Nombre = "Nadie", Apellido = "Prueba", Genero = "M" },
            HttpStatusCode.NotFound, $"/api/personas/{CcInexistente}" },
        { "personas", "clave ausente", "DELETE", $"/api/personas/{CcInexistente}", null,
            HttpStatusCode.NotFound, $"/api/personas/{CcInexistente}" },
        { "personas", "borrado bloqueado", "DELETE", $"/api/personas/{CcAna}", null,
            HttpStatusCode.Conflict, $"/api/personas/{CcAna}" },

        // Profesiones tampoco reciben referencias.
        { "profesiones", "clave duplicada", "POST", "/api/profesiones",
            new ProfesionDto { Id = 2, Nom = "Duplicada" },
            HttpStatusCode.Conflict, "/api/profesiones/2" },
        { "profesiones", "clave ausente", "GET", $"/api/profesiones/{IdInexistente}", null,
            HttpStatusCode.NotFound, $"/api/profesiones/{IdInexistente}" },
        { "profesiones", "clave ausente", "PUT", $"/api/profesiones/{IdInexistente}",
            new ProfesionDto { Id = IdInexistente, Nom = "Ninguna" },
            HttpStatusCode.NotFound, $"/api/profesiones/{IdInexistente}" },
        { "profesiones", "clave ausente", "DELETE", $"/api/profesiones/{IdInexistente}", null,
            HttpStatusCode.NotFound, $"/api/profesiones/{IdInexistente}" },
        { "profesiones", "borrado bloqueado", "DELETE", "/api/profesiones/1", null,
            HttpStatusCode.Conflict, "/api/profesiones/1" },

        // Nada depende de un teléfono, así que no hay caso de borrado bloqueado.
        { "telefonos", "clave duplicada", "POST", "/api/telefonos",
            new TelefonoEntradaDto { Num = "3001234567", Oper = "Duplicado", Duenio = CcLaura },
            HttpStatusCode.Conflict, "/api/telefonos/3001234567" },
        { "telefonos", "referencia inválida (dueño)", "POST", "/api/telefonos",
            new TelefonoEntradaDto { Num = NumInexistente, Oper = "Claro", Duenio = CcInexistente },
            HttpStatusCode.BadRequest, $"/api/telefonos/{NumInexistente}" },
        { "telefonos", "clave ausente", "GET", $"/api/telefonos/{NumInexistente}", null,
            HttpStatusCode.NotFound, $"/api/telefonos/{NumInexistente}" },
        { "telefonos", "clave ausente", "PUT", $"/api/telefonos/{NumInexistente}",
            new TelefonoEntradaDto { Num = NumInexistente, Oper = "Ninguno", Duenio = CcAna },
            HttpStatusCode.NotFound, $"/api/telefonos/{NumInexistente}" },
        { "telefonos", "clave ausente", "DELETE", $"/api/telefonos/{NumInexistente}", null,
            HttpStatusCode.NotFound, $"/api/telefonos/{NumInexistente}" },

        // Nada depende de un estudio, así que no hay caso de borrado bloqueado.
        { "estudios", "clave duplicada", "POST", "/api/estudios",
            new EstudioEntradaDto { IdProf = 1, CcPer = CcAna, Univer = "Duplicado" },
            HttpStatusCode.Conflict, $"/api/estudios/1/{CcAna}" },
        { "estudios", "referencia inválida (profesión)", "POST", "/api/estudios",
            new EstudioEntradaDto { IdProf = IdInexistente, CcPer = CcLaura },
            HttpStatusCode.BadRequest, $"/api/estudios/{IdInexistente}/{CcLaura}" },
        { "estudios", "referencia inválida (persona)", "POST", "/api/estudios",
            new EstudioEntradaDto { IdProf = 3, CcPer = CcInexistente },
            HttpStatusCode.BadRequest, $"/api/estudios/3/{CcInexistente}" },
        { "estudios", "clave ausente", "GET", $"/api/estudios/3/{CcAna}", null,
            HttpStatusCode.NotFound, $"/api/estudios/3/{CcAna}" },
        { "estudios", "clave ausente", "PUT", $"/api/estudios/3/{CcAna}",
            new EstudioEntradaDto { IdProf = 3, CcPer = CcAna, Univer = "Ninguna" },
            HttpStatusCode.NotFound, $"/api/estudios/3/{CcAna}" },
        { "estudios", "clave ausente", "DELETE", $"/api/estudios/3/{CcAna}", null,
            HttpStatusCode.NotFound, $"/api/estudios/3/{CcAna}" },
    };

    [TheoryConPersonaDb]
    [MemberData(nameof(Casos))]
    public async Task Cada_caso_negativo_responde_su_codigo_con_ProblemDetails_y_no_escribe(
        string recurso, string caso, string verbo, string ruta, object? cuerpo, HttpStatusCode esperado, string fila)
    {
        var antes = await LeerAsync(fila);

        var peticion = new HttpRequestMessage(new HttpMethod(verbo), ruta);
        if (cuerpo is not null)
        {
            peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType());
        }

        var respuesta = await _client.SendAsync(peticion);

        Assert.True(esperado == respuesta.StatusCode,
            $"{recurso} · {caso} · {verbo} {ruta}: se esperaba {(int)esperado} y llegó {(int)respuesta.StatusCode}.");
        using var problema = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal((int)esperado, problema.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problema.RootElement.GetProperty("detail").GetString()));
        Assert.Equal(antes, await LeerAsync(fila));
    }

    private async Task<string> LeerAsync(string fila)
    {
        var respuesta = await _client.GetAsync(fila);
        // Un 404 trae un traceId distinto en cada petición: basta con el código.
        return respuesta.StatusCode == HttpStatusCode.NotFound
            ? "404"
            : $"{(int)respuesta.StatusCode} {await respuesta.Content.ReadAsStringAsync()}";
    }
}
