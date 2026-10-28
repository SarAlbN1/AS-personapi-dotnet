using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using personapi_dotnet.Back.Borde.Controllers;

namespace personapi_dotnet.Tests.Unit.Presentacion;

/// <summary>
/// Comprobaciones de HU-006 sin base de datos: layout y navegación, antiforgery en los
/// POST web y la convención que muestra ValidationError, Conflict y NotFound
/// conservando el formulario. Usa la página de ejemplo /Home/Convenciones, cuyo
/// resultado de servicio es simulado.
/// </summary>
public partial class PresentacionMvcTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string RutaDemo = "/Home/Convenciones";

    private readonly WebApplicationFactory<Program> _factory;

    public PresentacionMvcTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task El_layout_navega_a_los_cuatro_modulos()
    {
        var html = await _factory.CreateClient().GetStringAsync("/");

        foreach (var modulo in new[] { "Personas", "Profesiones", "Telefonos", "Estudios" })
        {
            Assert.Contains($"href=\"/{modulo}\"", html);
        }

        Assert.Contains("href=\"/swagger\"", html);
    }

    [Fact]
    public async Task El_formulario_lleva_el_token_antiforgery()
    {
        var html = await _factory.CreateClient().GetStringAsync(RutaDemo);

        Assert.Matches(TokenRegex(), html);
    }

    [Fact]
    public async Task Un_POST_web_sin_token_se_rechaza_con_400_y_la_vista_de_estado()
    {
        var respuesta = await _factory.CreateClient().PostAsync(RutaDemo, Formulario("Ana", "Exito"));

        var html = await respuesta.Content.ReadAsStringAsync();

        // Razor codifica los caracteres no ASCII («válida» sale como v&#xE1;lida).
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"estado\"", html);
        Assert.Contains("Solicitud no v", html);
    }

    [Fact]
    public async Task ValidationError_devuelve_400_con_el_mensaje_y_el_formulario_intacto()
    {
        var (cliente, token) = await ClienteConTokenAsync();

        var respuesta = await cliente.PostAsync(RutaDemo, Formulario("Ana Gómez", "ErrorDeValidacion", token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"validacion\"", html);
        Assert.Contains("no cumple una regla de dominio", html);
        Assert.Contains("value=\"Ana G", html);
        Assert.DoesNotContain("data-mensaje=\"conflicto\"", html);
    }

    [Fact]
    public async Task Conflict_devuelve_409_con_el_aviso_y_el_formulario_intacto()
    {
        var (cliente, token) = await ClienteConTokenAsync();

        var respuesta = await cliente.PostAsync(RutaDemo, Formulario("Medicina", "Conflicto", token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("Ya existe un registro", html);
        Assert.Contains("value=\"Medicina\"", html);
        // El escenario elegido sigue seleccionado tras el error.
        Assert.Matches("<option selected=\"selected\" value=\"2\">", html);
    }

    [Fact]
    public async Task NotFound_devuelve_404_con_la_vista_de_estado()
    {
        var (cliente, token) = await ClienteConTokenAsync();

        var respuesta = await cliente.PostAsync(RutaDemo, Formulario("Ana", "NoEncontrado", token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"estado\"", html);
        Assert.Contains("ya no existe", html);
    }

    [Fact]
    public async Task La_validacion_de_forma_no_llega_al_servicio_y_marca_el_campo()
    {
        var (cliente, token) = await ClienteConTokenAsync();

        var respuesta = await cliente.PostAsync(RutaDemo, Formulario("", "Conflicto", token));
        var html = await respuesta.Content.ReadAsStringAsync();

        // ModelState inválido: se vuelve a pintar el formulario sin invocar el "servicio".
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El nombre es obligatorio.", html);
        Assert.DoesNotContain("data-mensaje=\"conflicto\"", html);
    }

    [Fact]
    public async Task Success_redirige_y_muestra_el_mensaje_una_sola_vez()
    {
        var (cliente, token) = await ClienteConTokenAsync();

        var respuesta = await cliente.PostAsync(RutaDemo, Formulario("Ana", "Exito", token));
        var html = await respuesta.Content.ReadAsStringAsync();

        // El cliente sigue la redirección (POST-Redirect-GET).
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(RutaDemo, respuesta.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", html);

        var recarga = await cliente.GetStringAsync(RutaDemo);
        Assert.DoesNotContain("data-mensaje=\"exito\"", recarga);
    }

    [Fact]
    public async Task Una_ruta_web_desconocida_muestra_la_vista_de_estado_404()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/ModuloPendiente");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"estado\"", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Una_ruta_api_desconocida_no_recibe_la_vista_HTML()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/api/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.DoesNotContain("<html", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public void La_convencion_de_antiforgery_no_se_aplica_a_los_ApiController()
    {
        var convencion = new AntiforgeryEnVistasConvention();
        var api = new ControllerModel(typeof(ControladorApiDePrueba).GetTypeInfo(), [new ApiControllerAttribute()]);
        var mvc = new ControllerModel(typeof(HomeController).GetTypeInfo(), []);

        convencion.Apply(api);
        convencion.Apply(mvc);

        Assert.DoesNotContain(api.Filters, f => f is AutoValidateAntiforgeryTokenAttribute);
        Assert.Contains(mvc.Filters, f => f is AutoValidateAntiforgeryTokenAttribute);
    }

    private async Task<(HttpClient Cliente, string Token)> ClienteConTokenAsync()
    {
        // El mismo cliente conserva la cookie antiforgery que acompaña al token del formulario.
        var cliente = _factory.CreateClient();
        var html = await cliente.GetStringAsync(RutaDemo);
        return (cliente, TokenRegex().Match(html).Groups[1].Value);
    }

    private static FormUrlEncodedContent Formulario(string nombre, string escenario, string? token = null)
    {
        var campos = new Dictionary<string, string> { ["Nombre"] = nombre, ["Escenario"] = escenario };
        if (token is not null)
        {
            campos["__RequestVerificationToken"] = token;
        }

        return new FormUrlEncodedContent(campos);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    private sealed class ControladorApiDePrueba : ControllerBase;
}
