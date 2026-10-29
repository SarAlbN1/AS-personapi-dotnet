using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Recorrido web de /Profesiones contra persona_db real (HU-008). Usa ids 9060 a 9069,
/// fuera de los datos de scripts/02_dml.sql y de los de la API (9050 a 9059), y los borra
/// al empezar y al terminar; las filas sembradas solo se leen o se intentan borrar sin éxito.
/// </summary>
public partial class ProfesionesMvcPersonaDbTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const int IdPrueba = 9061;
    private const int IdOtro = 9062;
    private const int IdPorRest = 9063;
    private const int IdInexistente = 9069;

    private readonly WebApplicationFactory<Program> _factory;

    public ProfesionesMvcPersonaDbTests(WebApplicationFactory<Program> factory) => _factory = factory;

    public Task InitializeAsync() => LimpiarAsync();

    public Task DisposeAsync() => LimpiarAsync();

    [FactConPersonaDb]
    public async Task Index_lista_las_profesiones_sembradas()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/Profesiones");
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("data-profesion=\"2\"", html);
        Assert.Contains("Medicina", html);
    }

    [FactConPersonaDb]
    public async Task Recorrido_crear_ver_editar_y_eliminar()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Create");

        var creada = await cliente.PostAsync("/Profesiones/Create",
            Formulario(token, IdPrueba, "  Arquitectura  ", "Diseño de edificios"));
        var listado = await creada.Content.ReadAsStringAsync();

        // POST-Redirect-GET: el cliente sigue la redirección al listado.
        Assert.Equal(HttpStatusCode.OK, creada.StatusCode);
        Assert.Equal("/Profesiones", creada.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", listado);
        Assert.Contains($"data-profesion=\"{IdPrueba}\"", listado);
        Assert.Equal("Arquitectura", (await LeerDeLaBaseAsync(IdPrueba))!.Nom);

        var detalle = await cliente.GetAsync($"/Profesiones/Details/{IdPrueba}");
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        Assert.Contains("Arquitectura", await detalle.Content.ReadAsStringAsync());

        token = await TokenDeAsync(cliente, $"/Profesiones/Edit/{IdPrueba}");
        var editada = await cliente.PostAsync($"/Profesiones/Edit/{IdPrueba}",
            Formulario(token, IdPrueba, "Arquitectura de software", ""));

        Assert.Equal(HttpStatusCode.OK, editada.StatusCode);
        Assert.Equal($"/Profesiones/Details/{IdPrueba}", editada.RequestMessage!.RequestUri!.AbsolutePath);
        var trasEditar = await LeerDeLaBaseAsync(IdPrueba);
        Assert.Equal("Arquitectura de software", trasEditar!.Nom);
        Assert.Null(trasEditar.Des);

        token = await TokenDeAsync(cliente, $"/Profesiones/Delete/{IdPrueba}");
        var eliminada = await cliente.PostAsync($"/Profesiones/Delete/{IdPrueba}", Formulario(token));

        Assert.Equal(HttpStatusCode.OK, eliminada.StatusCode);
        Assert.Equal("/Profesiones", eliminada.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));
    }

    [FactConPersonaDb]
    public async Task Crear_con_id_duplicado_muestra_el_conflicto_y_conserva_el_formulario()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Create");

        var respuesta = await cliente.PostAsync("/Profesiones/Create", Formulario(token, 2, "Duplicada", "Otra"));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("value=\"Duplicada\"", html);
        Assert.Equal("Medicina", (await LeerDeLaBaseAsync(2))!.Nom);
    }

    [FactConPersonaDb]
    public async Task Crear_sin_nombre_marca_el_campo_y_no_escribe()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Create");

        var respuesta = await cliente.PostAsync("/Profesiones/Create", Formulario(token, IdPrueba, "   ", "Sin nombre"));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El nombre es obligatorio.", html);
        Assert.Contains("Sin nombre", html);
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));
    }

    [FactConPersonaDb]
    public async Task La_longitud_del_nombre_se_mide_sin_los_espacios_de_los_extremos()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Create");

        var largo = await cliente.PostAsync("/Profesiones/Create",
            Formulario(token, IdPrueba, new string('n', 91), null));
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", await largo.Content.ReadAsStringAsync());
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));

        token = await TokenDeAsync(cliente, "/Profesiones/Create");
        var conEspacios = await cliente.PostAsync("/Profesiones/Create",
            Formulario(token, IdPrueba, $"  {new string('n', 90)}  ", null));
        Assert.Equal(HttpStatusCode.OK, conEspacios.StatusCode);
        Assert.Equal(90, (await LeerDeLaBaseAsync(IdPrueba))!.Nom.Length);
    }

    [FactConPersonaDb]
    public async Task Eliminar_una_profesion_con_estudios_muestra_la_explicacion_y_no_borra()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Delete/1");

        var respuesta = await cliente.PostAsync("/Profesiones/Delete/1", Formulario(token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("estudio(s) asociado(s)", html);
        Assert.NotNull(await LeerDeLaBaseAsync(1));
    }

    [FactConPersonaDb]
    public async Task Una_profesion_creada_por_REST_aparece_en_el_Index()
    {
        var cliente = _factory.CreateClient();

        var creada = await cliente.PostAsJsonAsync("/api/profesiones",
            new ProfesionDto { Id = IdPorRest, Nom = "Creada por REST" });
        var html = await cliente.GetStringAsync("/Profesiones");

        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
        Assert.Contains($"data-profesion=\"{IdPorRest}\"", html);
        Assert.Contains("Creada por REST", html);
    }

    [FactConPersonaDb]
    public async Task Ver_editar_y_eliminar_una_profesion_inexistente_dan_404()
    {
        var cliente = _factory.CreateClient();

        foreach (var accion in new[] { "Details", "Edit", "Delete" })
        {
            var respuesta = await cliente.GetAsync($"/Profesiones/{accion}/{IdInexistente}");

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("data-mensaje=\"estado\"", await respuesta.Content.ReadAsStringAsync());
        }
    }

    [FactConPersonaDb]
    public async Task Editar_con_un_id_de_formulario_distinto_al_de_la_ruta_no_cambia_nada()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Edit/2");

        var respuesta = await cliente.PostAsync("/Profesiones/Edit/2", Formulario(token, IdOtro, "Cambiada", null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("Medicina", (await LeerDeLaBaseAsync(2))!.Nom);
        Assert.Null(await LeerDeLaBaseAsync(IdOtro));
    }

    [FactConPersonaDb]
    public async Task Tras_un_id_manipulado_el_formulario_repintado_sigue_apuntando_a_la_ruta()
    {
        // El id manipulado es el de otra profesión que sí existe: si el formulario repintado
        // apuntara a ella, un segundo «Guardar» la modificaría.
        var (cliente, token) = await ClienteConTokenAsync("/Profesiones/Edit/2");

        var respuesta = await cliente.PostAsync("/Profesiones/Edit/2", Formulario(token, 1, "Manipulada", null));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("action=\"/Profesiones/Edit/2\"", html);
        Assert.DoesNotContain("/Profesiones/Edit/1\"", html);
        Assert.Matches("name=\"Id\"[^>]*value=\"2\"", html);
        Assert.Equal("Ingeniería de Sistemas", (await LeerDeLaBaseAsync(1))!.Nom);
        Assert.Equal("Medicina", (await LeerDeLaBaseAsync(2))!.Nom);
    }

    [FactConPersonaDb]
    public async Task Un_POST_sin_token_antiforgery_se_rechaza()
    {
        var respuesta = await _factory.CreateClient().PostAsync("/Profesiones/Create",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["Id"] = $"{IdPrueba}", ["Nom"] = "Sin token" }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Null(await LeerDeLaBaseAsync(IdPrueba));
    }

    private async Task<(HttpClient Cliente, string Token)> ClienteConTokenAsync(string ruta)
    {
        // El mismo cliente conserva la cookie antiforgery que acompaña al token del formulario.
        var cliente = _factory.CreateClient();
        return (cliente, await TokenDeAsync(cliente, ruta));
    }

    private static async Task<string> TokenDeAsync(HttpClient cliente, string ruta) =>
        TokenRegex().Match(await cliente.GetStringAsync(ruta)).Groups[1].Value;

    private static FormUrlEncodedContent Formulario(string token, int? id = null, string? nom = null, string? des = null)
    {
        var campos = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        if (id is not null) campos["Id"] = id.Value.ToString();
        if (nom is not null) campos["Nom"] = nom;
        if (des is not null) campos["Des"] = des;
        return new FormUrlEncodedContent(campos);
    }

    private async Task<Back.Persistencia.Entities.Profesion?> LeerDeLaBaseAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IProfesionRepository>().GetByIdAsync(id);
    }

    private async Task LimpiarAsync()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IProfesionRepository>();
        foreach (var id in new[] { IdPrueba, IdOtro, IdPorRest })
        {
            if (await repositorio.GetByIdAsync(id) is { } fila)
            {
                await repositorio.DeleteAsync(fila);
            }
        }
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
