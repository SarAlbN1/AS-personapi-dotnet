using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Recorrido web de /Estudios contra persona_db real (HU-013). Las filas que crea la prueba
/// usan su propia profesión, 9070, fuera de los datos de scripts/02_dml.sql y de los ids de
/// las pruebas de profesiones (9050 a 9069), para no cruzarse con EstudiosApiPersonaDbTests,
/// que usa la profesión 3. La profesión y sus estudios se crean al empezar y se borran al
/// terminar; los estudios sembrados solo se leen o se intentan modificar sin éxito.
/// </summary>
public partial class EstudiosMvcPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int ProfPrueba = 9070;
    private const int ProfInexistente = 9079;
    private const int CcAna = 1010101010;
    private const int CcCarlos = 1020202020;
    private const int CcLaura = 1030303030;
    private const int CcInexistente = 1099999999;

    private readonly WebApplicationFactory<Program> _factory;

    public EstudiosMvcPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await LimpiarAsync();
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IProfesionRepository>()
            .AddAsync(new Profesion { Id = ProfPrueba, Nom = "Prueba MVC de estudios" });
    }

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Index_lista_los_estudios_sembrados_con_las_dos_claves_en_los_enlaces()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/Estudios");
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains($"data-estudio=\"2/{CcCarlos}\"", html);
        Assert.Contains("Medicina", html);
        Assert.Contains("Carlos Pérez", html);
        foreach (var accion in new[] { "Details", "Edit", "Delete" })
        {
            Assert.Contains($"href=\"/Estudios/{accion}/2/{CcCarlos}\"", html);
        }
    }

    [FactConPersonaDb]
    public async Task Los_selectores_ofrecen_las_profesiones_y_personas_de_los_servicios()
    {
        var html = await TextoAsync(await _factory.CreateClient().GetAsync("/Estudios/Create"));

        Assert.Matches("<select[^>]*name=\"IdProf\"", html);
        Assert.Matches("<select[^>]*name=\"CcPer\"", html);
        Assert.Contains($"<option value=\"{ProfPrueba}\">{ProfPrueba} · Prueba MVC de estudios</option>", html);
        Assert.Contains("<option value=\"2\">2 · Medicina</option>", html);
        Assert.Contains($"<option value=\"{CcAna}\">{CcAna} · Ana Gómez</option>", html);
        Assert.DoesNotContain($"value=\"{ProfInexistente}\"", html);
    }

    [FactConPersonaDb]
    public async Task Recorrido_crear_ver_editar_y_eliminar_por_las_dos_claves()
    {
        var clave = $"{ProfPrueba}/{CcAna}";
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");

        var creado = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfPrueba, CcAna, "2021-05-10", "  Universidad del Valle  "));
        var listado = await TextoAsync(creado);

        // POST-Redirect-GET: el cliente sigue la redirección al listado.
        Assert.Equal(HttpStatusCode.OK, creado.StatusCode);
        Assert.Equal("/Estudios", creado.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", listado);
        Assert.Contains($"data-estudio=\"{clave}\"", listado);

        var enLaBase = await LeerDeLaBaseAsync(ProfPrueba, CcAna);
        Assert.Equal("Universidad del Valle", enLaBase!.Univer);
        Assert.Equal(new DateOnly(2021, 5, 10), enLaBase.Fecha);

        var detalle = await cliente.GetAsync($"/Estudios/Details/{clave}");
        var htmlDetalle = await TextoAsync(detalle);
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        Assert.Contains("Universidad del Valle", htmlDetalle);
        Assert.Contains("Ana Gómez", htmlDetalle);

        token = await TokenDeAsync(cliente, $"/Estudios/Edit/{clave}");
        var editado = await cliente.PostAsync($"/Estudios/Edit/{clave}",
            Formulario(token, ProfPrueba, CcAna, "2022-01-31", ""));

        Assert.Equal(HttpStatusCode.OK, editado.StatusCode);
        Assert.Equal($"/Estudios/Details/{clave}", editado.RequestMessage!.RequestUri!.AbsolutePath);
        var trasEditar = await LeerDeLaBaseAsync(ProfPrueba, CcAna);
        Assert.Equal(new DateOnly(2022, 1, 31), trasEditar!.Fecha);
        Assert.Null(trasEditar.Univer);

        token = await TokenDeAsync(cliente, $"/Estudios/Delete/{clave}");
        var eliminado = await cliente.PostAsync($"/Estudios/Delete/{clave}", Formulario(token));

        Assert.Equal(HttpStatusCode.OK, eliminado.StatusCode);
        Assert.Equal("/Estudios", eliminado.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Null(await LeerDeLaBaseAsync(ProfPrueba, CcAna));
    }

    [FactConPersonaDb]
    public async Task Un_estudio_creado_desde_MVC_se_lee_igual_por_REST()
    {
        // Escenario SRS 10.4: lo que guarda la web es lo que devuelve /api/estudios/{idProf}/{ccPer}.
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");
        await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfPrueba, CcCarlos, "2018-12-07", "Universidad EAFIT"));

        var porRest = await cliente.GetFromJsonAsync<EstudioDto>($"/api/estudios/{ProfPrueba}/{CcCarlos}");

        Assert.Equal(ProfPrueba, porRest!.IdProf);
        Assert.Equal(CcCarlos, porRest.CcPer);
        Assert.Equal(new DateOnly(2018, 12, 7), porRest.Fecha);
        Assert.Equal("Universidad EAFIT", porRest.Univer);
    }

    [FactConPersonaDb]
    public async Task Crear_un_par_existente_muestra_el_conflicto_y_conserva_el_formulario()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");

        var respuesta = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, 1, CcAna, "2001-01-01", "Repetida"));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("value=\"Repetida\"", html);
        // Los selectores se vuelven a llenar y conservan lo elegido.
        Assert.Contains("<option selected=\"selected\" value=\"1\">", html);
        Assert.Contains($"<option selected=\"selected\" value=\"{CcAna}\">", html);
        Assert.Equal("Pontificia Universidad Javeriana", (await LeerDeLaBaseAsync(1, CcAna))!.Univer);
    }

    [FactConPersonaDb]
    public async Task Crear_con_referencias_que_no_ofrecen_los_selectores_da_400_y_no_escribe()
    {
        // Un formulario manipulado puede enviar valores fuera de las opciones: el servicio
        // comprueba las dos referencias.
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");

        var sinProfesion = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfInexistente, CcAna, null, null));
        Assert.Equal(HttpStatusCode.BadRequest, sinProfesion.StatusCode);
        Assert.Contains($"No existe una profesión con id {ProfInexistente}.", await TextoAsync(sinProfesion));

        token = await TokenDeAsync(cliente, "/Estudios/Create");
        var sinPersona = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfPrueba, CcInexistente, null, null));
        Assert.Equal(HttpStatusCode.BadRequest, sinPersona.StatusCode);
        Assert.Contains($"No existe una persona con cédula {CcInexistente}.", await TextoAsync(sinPersona));

        Assert.Null(await LeerDeLaBaseAsync(ProfInexistente, CcAna));
        Assert.Null(await LeerDeLaBaseAsync(ProfPrueba, CcInexistente));
    }

    [FactConPersonaDb]
    public async Task Crear_sin_elegir_profesion_ni_persona_marca_los_campos_y_no_escribe()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");

        var respuesta = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, null, null, null, "Sin selección"));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("La profesión es obligatoria.", html);
        Assert.Contains("La persona es obligatoria.", html);
        Assert.Contains("value=\"Sin selección\"", html);
        Assert.Contains("2 · Medicina", html);
    }

    [FactConPersonaDb]
    public async Task La_longitud_de_la_universidad_se_mide_sin_los_espacios_de_los_extremos()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Estudios/Create");

        var larga = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfPrueba, CcLaura, null, new string('u', 51)));
        Assert.Equal(HttpStatusCode.BadRequest, larga.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", await TextoAsync(larga));
        Assert.Null(await LeerDeLaBaseAsync(ProfPrueba, CcLaura));

        token = await TokenDeAsync(cliente, "/Estudios/Create");
        var conEspacios = await cliente.PostAsync("/Estudios/Create",
            Formulario(token, ProfPrueba, CcLaura, null, $"  {new string('u', 50)}  "));
        Assert.Equal(HttpStatusCode.OK, conEspacios.StatusCode);
        Assert.Equal(50, (await LeerDeLaBaseAsync(ProfPrueba, CcLaura))!.Univer!.Length);
    }

    [FactConPersonaDb]
    public async Task Editar_con_un_par_de_formulario_distinto_al_de_la_ruta_no_cambia_nada()
    {
        // El par manipulado es el de otro estudio que existe: si el formulario repintado
        // apuntara a él, un segundo «Guardar» lo modificaría.
        var ruta = $"/Estudios/Edit/1/{CcAna}";
        var (cliente, token) = await ClienteConTokenAsync(ruta);

        var respuesta = await cliente.PostAsync(ruta, Formulario(token, 1, CcLaura, "2000-01-01", "Manipulada"));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", html);
        Assert.Contains($"action=\"{ruta}\"", html);
        Assert.DoesNotContain($"/Estudios/Edit/1/{CcLaura}\"", html);
        Assert.Matches($"name=\"CcPer\"[^>]*value=\"{CcAna}\"", html);
        Assert.Equal("Pontificia Universidad Javeriana", (await LeerDeLaBaseAsync(1, CcAna))!.Univer);
        Assert.Equal("Universidad de los Andes", (await LeerDeLaBaseAsync(1, CcLaura))!.Univer);
    }

    [FactConPersonaDb]
    public async Task Ver_editar_y_eliminar_un_par_inexistente_dan_404()
    {
        var cliente = _factory.CreateClient();

        // La profesión 3 existe pero no tiene estudios con Ana: falta solo el par.
        foreach (var accion in new[] { "Details", "Edit", "Delete" })
        {
            var respuesta = await cliente.GetAsync($"/Estudios/{accion}/3/{CcAna}");

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("data-mensaje=\"estado\"", await TextoAsync(respuesta));
        }
    }

    [FactConPersonaDb]
    public async Task Un_POST_sin_token_antiforgery_se_rechaza()
    {
        var respuesta = await _factory.CreateClient().PostAsync("/Estudios/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["IdProf"] = $"{ProfPrueba}",
                ["CcPer"] = $"{CcAna}"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(ProfPrueba, CcAna));
    }

    // Razor codifica los caracteres no ASCII («Pérez» sale como «P&#xE9;rez»): las
    // comprobaciones se hacen sobre el texto decodificado.
    private static async Task<string> TextoAsync(HttpResponseMessage respuesta) =>
        WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());

    private async Task<(HttpClient Cliente, string Token)> ClienteConTokenAsync(string ruta)
    {
        // El mismo cliente conserva la cookie antiforgery que acompaña al token del formulario.
        var cliente = _factory.CreateClient();
        return (cliente, await TokenDeAsync(cliente, ruta));
    }

    private static async Task<string> TokenDeAsync(HttpClient cliente, string ruta) =>
        TokenRegex().Match(await cliente.GetStringAsync(ruta)).Groups[1].Value;

    private static FormUrlEncodedContent Formulario(
        string token, int? idProf = null, int? ccPer = null, string? fecha = null, string? univer = null)
    {
        var campos = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        if (idProf is not null) campos["IdProf"] = idProf.Value.ToString();
        if (ccPer is not null) campos["CcPer"] = ccPer.Value.ToString();
        if (fecha is not null) campos["Fecha"] = fecha;
        if (univer is not null) campos["Univer"] = univer;
        return new FormUrlEncodedContent(campos);
    }

    private async Task<Estudio?> LeerDeLaBaseAsync(int idProf, int ccPer)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IEstudioRepository>().GetByIdAsync(idProf, ccPer);
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var estudios = scope.ServiceProvider.GetRequiredService<IEstudioRepository>();
        foreach (var cc in new[] { CcAna, CcCarlos, CcLaura })
        {
            if (await estudios.GetByIdAsync(ProfPrueba, cc) is { } fila)
            {
                await estudios.DeleteAsync(fila);
            }
        }

        var profesiones = scope.ServiceProvider.GetRequiredService<IProfesionRepository>();
        if (await profesiones.GetByIdAsync(ProfPrueba) is { } profesion)
        {
            await profesiones.DeleteAsync(profesion);
        }
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
