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
/// Recorrido web de /Telefonos contra persona_db real (HU-012). Las filas que crea la prueba
/// usan números propios (prefijo 912 y uno con ceros iniciales), fuera de los datos de
/// scripts/02_dml.sql y de los números de las demás pruebas; se borran al empezar y al
/// terminar. Los teléfonos sembrados solo se leen o se intentan modificar sin éxito.
/// </summary>
public partial class TelefonosMvcPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string NumPrueba = "9120000001";
    private const string NumConCeros = "0091200002";
    private const string NumRest = "9120000003";
    private const string NumAna = "3001234567";
    private const string NumCarlos = "3109876543";
    private const int CcAna = 1010101010;
    private const int CcCarlos = 1020202020;
    private const int CcLaura = 1030303030;
    private const int CcInexistente = 1099999999;

    private static readonly string[] NumsDePrueba = [NumPrueba, NumConCeros, NumRest];

    private readonly WebApplicationFactory<Program> _factory;

    public TelefonosMvcPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Index_lista_los_telefonos_sembrados_con_el_nombre_del_dueno()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/Telefonos");
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains($"data-telefono=\"{NumCarlos}\"", html);
        Assert.Contains("Movistar", html);
        Assert.Contains($"{CcCarlos} · Carlos Pérez", html);
        foreach (var accion in new[] { "Details", "Edit", "Delete" })
        {
            Assert.Contains($"href=\"/Telefonos/{accion}/{NumCarlos}\"", html);
        }
    }

    [FactConPersonaDb]
    public async Task El_selector_de_dueno_ofrece_las_personas_del_servicio()
    {
        var html = await TextoAsync(await _factory.CreateClient().GetAsync("/Telefonos/Create"));

        Assert.Matches("<select[^>]*name=\"Duenio\"", html);
        Assert.Contains($"<option value=\"{CcAna}\">{CcAna} · Ana Gómez</option>", html);
        Assert.Contains($"<option value=\"{CcCarlos}\">{CcCarlos} · Carlos Pérez</option>", html);
        Assert.DoesNotContain($"value=\"{CcInexistente}\"", html);
    }

    [FactConPersonaDb]
    public async Task Recorrido_crear_ver_editar_y_eliminar_por_el_numero()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        var creado = await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumPrueba, "  Claro  ", CcAna));
        var listado = await TextoAsync(creado);

        // POST-Redirect-GET: el cliente sigue la redirección al listado.
        Assert.Equal(HttpStatusCode.OK, creado.StatusCode);
        Assert.Equal("/Telefonos", creado.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", listado);
        Assert.Contains($"data-telefono=\"{NumPrueba}\"", listado);

        var enLaBase = await LeerDeLaBaseAsync(NumPrueba);
        Assert.Equal("Claro", enLaBase!.Oper);
        Assert.Equal(CcAna, enLaBase.Duenio);

        var detalle = await cliente.GetAsync($"/Telefonos/Details/{NumPrueba}");
        var htmlDetalle = await TextoAsync(detalle);
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        Assert.Contains("Claro", htmlDetalle);
        Assert.Contains("Ana Gómez", htmlDetalle);

        // La edición cambia operador y dueño; el número no.
        token = await TokenDeAsync(cliente, $"/Telefonos/Edit/{NumPrueba}");
        var editado = await cliente.PostAsync($"/Telefonos/Edit/{NumPrueba}", Formulario(token, NumPrueba, "Tigo", CcLaura));

        Assert.Equal(HttpStatusCode.OK, editado.StatusCode);
        Assert.Equal($"/Telefonos/Details/{NumPrueba}", editado.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("Laura", await TextoAsync(editado));
        var trasEditar = await LeerDeLaBaseAsync(NumPrueba);
        Assert.Equal("Tigo", trasEditar!.Oper);
        Assert.Equal(CcLaura, trasEditar.Duenio);

        token = await TokenDeAsync(cliente, $"/Telefonos/Delete/{NumPrueba}");
        var eliminado = await cliente.PostAsync($"/Telefonos/Delete/{NumPrueba}", Formulario(token));

        Assert.Equal(HttpStatusCode.OK, eliminado.StatusCode);
        Assert.Equal("/Telefonos", eliminado.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Null(await LeerDeLaBaseAsync(NumPrueba));
    }

    [FactConPersonaDb]
    public async Task Los_ceros_iniciales_se_conservan_en_la_ruta_y_en_la_base()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");
        await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumConCeros, "Claro", CcCarlos));

        Assert.Equal(NumConCeros, (await LeerDeLaBaseAsync(NumConCeros))!.Num);
        var detalle = await cliente.GetAsync($"/Telefonos/Details/{NumConCeros}");
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        Assert.Contains($"Teléfono {NumConCeros}", await TextoAsync(detalle));
    }

    [FactConPersonaDb]
    public async Task Un_telefono_creado_desde_MVC_se_lee_igual_por_REST()
    {
        // Criterio 4: lo que guarda la web es lo que devuelve /api/telefonos/{num}.
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");
        await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumRest, "Movistar", CcLaura));

        var porRest = await cliente.GetFromJsonAsync<TelefonoDto>($"/api/telefonos/{NumRest}");

        Assert.Equal(NumRest, porRest!.Num);
        Assert.Equal("Movistar", porRest.Oper);
        Assert.Equal(CcLaura, porRest.Duenio);
    }

    [FactConPersonaDb]
    public async Task Crear_un_numero_existente_muestra_el_conflicto_y_conserva_el_formulario()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        var respuesta = await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumAna, "Repetido", CcLaura));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains($"Ya existe un teléfono con número {NumAna}.", html);
        Assert.Contains("value=\"Repetido\"", html);
        // El selector se vuelve a llenar y conserva lo elegido.
        Assert.Contains($"<option selected=\"selected\" value=\"{CcLaura}\">", html);
        var sembrado = await LeerDeLaBaseAsync(NumAna);
        Assert.Equal("Claro", sembrado!.Oper);
        Assert.Equal(CcAna, sembrado.Duenio);
    }

    [FactConPersonaDb]
    public Task Un_numero_de_mas_de_15_digitos_se_informa_junto_al_campo_y_no_escribe() =>
        NumeroInvalidoAsync("9120000000000001", "El número admite hasta 15 dígitos.");

    [FactConPersonaDb]
    public Task Un_numero_con_caracteres_que_no_son_digitos_se_informa_junto_al_campo_y_no_escribe() =>
        NumeroInvalidoAsync("912-000-0004", "El número solo admite dígitos.");

    private async Task NumeroInvalidoAsync(string num, string mensaje)
    {
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        var respuesta = await cliente.PostAsync("/Telefonos/Create", Formulario(token, num, "Claro", CcAna));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Matches($"data-valmsg-for=\"Num\"[^>]*>{Regex.Escape(mensaje)}<", html);
        Assert.Contains($"value=\"{num}\"", html);
        Assert.Null(await LeerDeLaBaseAsync(num));
    }

    [FactConPersonaDb]
    public async Task Crear_con_un_dueno_que_no_ofrece_el_selector_da_400_y_no_escribe()
    {
        // Un formulario manipulado puede enviar un dueño fuera de las opciones: el servicio
        // comprueba la referencia.
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        var respuesta = await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumPrueba, "Claro", CcInexistente));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains($"No existe una persona con cédula {CcInexistente} para ser dueña del teléfono.",
            await TextoAsync(respuesta));
        Assert.Null(await LeerDeLaBaseAsync(NumPrueba));
    }

    [FactConPersonaDb]
    public async Task Crear_sin_datos_marca_los_campos_y_no_escribe()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Telefonos/Create");

        var respuesta = await cliente.PostAsync("/Telefonos/Create", Formulario(token, NumPrueba));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El operador es obligatorio.", html);
        Assert.Contains("El dueño es obligatorio.", html);
        Assert.Contains($"{CcAna} · Ana Gómez", html);
        Assert.Null(await LeerDeLaBaseAsync(NumPrueba));
    }

    [FactConPersonaDb]
    public async Task Editar_con_un_numero_de_formulario_distinto_al_de_la_ruta_no_cambia_nada()
    {
        // El número manipulado es el de otro teléfono que existe: si el formulario repintado
        // apuntara a él, un segundo «Guardar» lo modificaría.
        var ruta = $"/Telefonos/Edit/{NumAna}";
        var (cliente, token) = await ClienteConTokenAsync(ruta);

        var respuesta = await cliente.PostAsync(ruta, Formulario(token, NumCarlos, "Manipulado", CcLaura));
        var html = await TextoAsync(respuesta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", html);
        Assert.Contains($"action=\"{ruta}\"", html);
        Assert.DoesNotContain($"/Telefonos/Edit/{NumCarlos}\"", html);
        Assert.Matches($"name=\"Num\"[^>]*value=\"{NumAna}\"", html);
        Assert.Equal("Claro", (await LeerDeLaBaseAsync(NumAna))!.Oper);
        Assert.Equal("Movistar", (await LeerDeLaBaseAsync(NumCarlos))!.Oper);
    }

    [FactConPersonaDb]
    public async Task Ver_editar_y_eliminar_un_numero_inexistente_dan_404()
    {
        var cliente = _factory.CreateClient();

        foreach (var accion in new[] { "Details", "Edit", "Delete" })
        {
            var respuesta = await cliente.GetAsync($"/Telefonos/{accion}/9129999999");

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("data-mensaje=\"estado\"", await TextoAsync(respuesta));
        }
    }

    [FactConPersonaDb]
    public async Task Un_POST_sin_token_antiforgery_se_rechaza()
    {
        var respuesta = await _factory.CreateClient().PostAsync("/Telefonos/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Num"] = NumPrueba,
                ["Oper"] = "Claro",
                ["Duenio"] = $"{CcAna}"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(NumPrueba));
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
        string token, string? num = null, string? oper = null, int? duenio = null)
    {
        var campos = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        if (num is not null) campos["Num"] = num;
        if (oper is not null) campos["Oper"] = oper;
        if (duenio is not null) campos["Duenio"] = duenio.Value.ToString();
        return new FormUrlEncodedContent(campos);
    }

    private async Task<Telefono?> LeerDeLaBaseAsync(string num)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ITelefonoRepository>().GetByIdAsync(num);
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var telefonos = scope.ServiceProvider.GetRequiredService<ITelefonoRepository>();
        foreach (var num in NumsDePrueba)
        {
            if (await telefonos.GetByIdAsync(num) is { } fila)
            {
                await telefonos.DeleteAsync(fila);
            }
        }
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
