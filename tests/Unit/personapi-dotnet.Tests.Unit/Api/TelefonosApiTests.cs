using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Tests.Unit.Integracion;

namespace personapi_dotnet.Tests.Unit.Api;

/// <summary>
/// Comprobaciones de HU-010: las cinco operaciones de <c>/api/telefonos</c> contra
/// persona_db real. Los dueños son las personas del DML de HU-002; los teléfonos que
/// crea la prueba usan números propios y se borran al final.
/// </summary>
public class TelefonosApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int CcAna = 1010101010;
    private const int CcInexistente = 777000777;
    private const string NumAna = "3001234567";

    private readonly WebApplicationFactory<Program> _factory;

    public TelefonosApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static TelefonoEntradaDto Entrada(string num, int duenio = CcAna, string oper = "Claro") =>
        new() { Num = num, Oper = oper, Duenio = duenio };

    [FactConPersonaDb]
    public async Task Listado_devuelve_200_y_los_telefonos_del_dml()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/telefonos");
        var telefonos = await respuesta.Content.ReadFromJsonAsync<List<TelefonoDto>>();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.NotNull(telefonos);
        Assert.Contains(telefonos, t => t.Num == NumAna && t.Duenio == CcAna);
    }

    [FactConPersonaDb]
    public async Task Detalle_devuelve_200_y_404_si_no_existe()
    {
        var cliente = _factory.CreateClient();

        var encontrado = await cliente.GetAsync($"/api/telefonos/{NumAna}");
        var ausente = await cliente.GetAsync("/api/telefonos/3999999999");
        var telefono = await encontrado.Content.ReadFromJsonAsync<TelefonoDto>();

        Assert.Equal(HttpStatusCode.OK, encontrado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
        Assert.NotNull(telefono);
        Assert.Equal("Claro", telefono.Oper);
    }

    [FactConPersonaDb]
    public async Task Ciclo_completo_crea_consulta_edita_y_elimina()
    {
        var cliente = _factory.CreateClient();
        const string num = "3150000001";
        await cliente.DeleteAsync($"/api/telefonos/{num}");

        var creado = await cliente.PostAsJsonAsync("/api/telefonos", Entrada(num));
        var cuerpo = await creado.Content.ReadFromJsonAsync<TelefonoDto>();

        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal($"/api/telefonos/{num}", creado.Headers.Location?.AbsolutePath);
        Assert.NotNull(cuerpo);
        Assert.Equal(num, cuerpo.Num);

        var editado = await cliente.PutAsJsonAsync(
            $"/api/telefonos/{num}",
            Entrada(num, duenio: 1020202020, oper: "Movistar"));
        Assert.Equal(HttpStatusCode.NoContent, editado.StatusCode);

        var despues = await (await cliente.GetAsync($"/api/telefonos/{num}")).Content
            .ReadFromJsonAsync<TelefonoDto>();
        Assert.NotNull(despues);
        Assert.Equal("Movistar", despues.Oper);
        Assert.Equal(1020202020, despues.Duenio);

        var eliminado = await cliente.DeleteAsync($"/api/telefonos/{num}");
        var tras = await cliente.GetAsync($"/api/telefonos/{num}");

        Assert.Equal(HttpStatusCode.NoContent, eliminado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tras.StatusCode);
    }

    [FactConPersonaDb]
    public async Task El_numero_conserva_los_ceros_iniciales()
    {
        var cliente = _factory.CreateClient();
        const string num = "0012345678";
        await cliente.DeleteAsync($"/api/telefonos/{num}");

        var creado = await cliente.PostAsJsonAsync("/api/telefonos", Entrada(num));
        var devuelto = await creado.Content.ReadFromJsonAsync<TelefonoDto>();
        var consultado = await (await cliente.GetAsync($"/api/telefonos/{num}")).Content
            .ReadFromJsonAsync<TelefonoDto>();
        var listado = await (await cliente.GetAsync("/api/telefonos")).Content
            .ReadFromJsonAsync<List<TelefonoDto>>();

        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal(num, devuelto!.Num);
        Assert.Equal(num, consultado!.Num);
        Assert.Contains(listado!, t => t.Num == num);

        await cliente.DeleteAsync($"/api/telefonos/{num}");
    }

    [FactConPersonaDb]
    public async Task Alta_con_numero_repetido_devuelve_409()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/telefonos", Entrada(NumAna));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Alta_y_edicion_con_duenio_inexistente_devuelven_400_sin_escribir()
    {
        var cliente = _factory.CreateClient();
        const string num = "3150000002";

        var creado = await cliente.PostAsJsonAsync("/api/telefonos", Entrada(num, duenio: CcInexistente));
        var editado = await cliente.PutAsJsonAsync(
            $"/api/telefonos/{NumAna}",
            Entrada(NumAna, duenio: CcInexistente));

        Assert.Equal(HttpStatusCode.BadRequest, creado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, editado.StatusCode);

        // Ni se creó el huérfano ni cambió el dueño del teléfono existente.
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/telefonos/{num}")).StatusCode);
        var ana = await (await cliente.GetAsync($"/api/telefonos/{NumAna}")).Content
            .ReadFromJsonAsync<TelefonoDto>();
        Assert.Equal(CcAna, ana!.Duenio);
    }

    [FactConPersonaDb]
    public async Task Alta_con_numero_demasiado_largo_devuelve_400()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/telefonos", Entrada("1234567890123456"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Edicion_con_numero_distinto_en_ruta_y_cuerpo_devuelve_400()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PutAsJsonAsync($"/api/telefonos/{NumAna}", Entrada("3109876543"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [FactConPersonaDb]
    public async Task Edicion_y_borrado_de_numero_inexistente_devuelven_404()
    {
        var cliente = _factory.CreateClient();
        const string num = "3999999999";

        var editado = await cliente.PutAsJsonAsync($"/api/telefonos/{num}", Entrada(num));
        var eliminado = await cliente.DeleteAsync($"/api/telefonos/{num}");

        Assert.Equal(HttpStatusCode.NotFound, editado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, eliminado.StatusCode);
    }
}
