using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Controllers;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Tests.Unit.Personas;

/// <summary>
/// CRUD web de personas (HU-007) sin base de datos: el servicio se sustituye por un
/// doble, porque lo que se comprueba aquí es el borde web —las cinco acciones, la
/// traducción de resultados de HU-006 y que un formulario inválido no llegue al
/// servicio—, no las reglas de SDD §7, que son del servicio.
/// Las pruebas contra <c>persona_db</c> están en
/// <c>Integracion/PersonasMvcPersonaDbTests</c>.
/// </summary>
public partial class PersonasMvcTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly PersonaServicioDoble _servicio = new();

    public PersonasMvcTests(WebApplicationFactory<Program> factory)
    {
        _servicio.Filas.Add(new PersonaDto
        {
            Cc = 1010101010, Nombre = "Ana", Apellido = "Gómez", Genero = "F", Edad = 30
        });

        // El último registro gana: la aplicación real sigue usando PersonaService.
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddSingleton<IPersonaService>(_servicio)));
    }

    [Fact]
    public async Task El_listado_muestra_las_personas_del_servicio()
    {
        var html = await _factory.CreateClient().GetStringAsync("/Personas");

        Assert.Contains("data-tabla=\"personas\"", html);
        Assert.Contains("1010101010", html);
        // Razor codifica los no ASCII: «Gómez» sale como G&#xF3;mez.
        Assert.Contains("Femenino", html);
        Assert.Contains("href=\"/Personas/Details/1010101010\"", html);
    }

    [Fact]
    public async Task Un_listado_vacio_no_es_un_error_sino_un_aviso()
    {
        _servicio.Filas.Clear();

        var respuesta = await _factory.CreateClient().GetAsync("/Personas");
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"vacio\"", html);
    }

    [Fact]
    public async Task El_detalle_de_una_cedula_inexistente_devuelve_404_con_la_vista_de_estado()
    {
        var respuesta = await _factory.CreateClient().GetAsync("/Personas/Details/999");
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"estado\"", html);
        Assert.Contains("No existe una persona con c", html);
    }

    [Fact]
    public async Task Un_formulario_invalido_se_repinta_y_no_llega_al_servicio()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Create");

        var respuesta = await cliente.PostAsync("/Personas/Create", Formulario(token, cc: "2020202020", nombre: ""));
        var html = await respuesta.Content.ReadAsStringAsync();

        // ModelState inválido: ni se invoca al servicio, así que no se escribe nada.
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El nombre es obligatorio.", html);
        Assert.Empty(_servicio.Altas);
        Assert.Single(_servicio.Filas);
    }

    [Fact]
    public async Task Una_cedula_duplicada_devuelve_409_con_el_aviso_y_el_formulario_intacto()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Create");

        var respuesta = await cliente.PostAsync("/Personas/Create", Formulario(token, cc: "1010101010", nombre: "Otra"));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("Ya existe una persona con c", html);
        // El formulario conserva lo enviado.
        Assert.Contains("value=\"Otra\"", html);
        Assert.Single(_servicio.Filas);
    }

    [Fact]
    public async Task Un_alta_valida_redirige_al_detalle_y_avisa_una_sola_vez()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Create");

        var respuesta = await cliente.PostAsync("/Personas/Create", Formulario(token, cc: "2020202020", nombre: "Luis"));
        var html = await respuesta.Content.ReadAsStringAsync();

        // POST-Redirect-GET: el cliente sigue la redirección hasta el detalle.
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("/Personas/Details/2020202020", respuesta.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", html);
        Assert.Contains(_servicio.Filas, p => p.Cc == 2020202020 && p.Nombre == "Luis");

        var recarga = await cliente.GetStringAsync("/Personas/Details/2020202020");
        Assert.DoesNotContain("data-mensaje=\"exito\"", recarga);
    }

    [Fact]
    public async Task La_edicion_envia_al_servicio_la_cedula_de_la_ruta_y_la_del_cuerpo()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Edit/1010101010");

        await cliente.PostAsync(
            "/Personas/Edit/1010101010", Formulario(token, cc: "1010101010", nombre: "Ana María", edicion: true));

        // El servicio es quien compara ruta y cuerpo (misma regla que el PUT REST): el
        // borde solo le pasa las dos cosas.
        var (ruta, entrada) = Assert.Single(_servicio.Ediciones);
        Assert.Equal(1010101010, ruta);
        Assert.Equal(1010101010, entrada.Cc);
        Assert.Equal("Ana María", entrada.Nombre);
    }

    [Fact]
    public async Task Una_cedula_manipulada_en_la_edicion_no_cambia_el_registro_editado()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Edit/1010101010");

        // El campo oculto llega con otra cédula; manda la de la ruta.
        var respuesta = await cliente.PostAsync(
            "/Personas/Edit/1010101010",
            Formulario(token, cc: "2020202020", nombre: "Manipulada", edicion: true));

        var (ruta, entrada) = Assert.Single(_servicio.Ediciones);
        Assert.Equal(1010101010, ruta);
        Assert.Equal(1010101010, entrada.Cc);
        // Y la redirección lleva al registro de la ruta, no al del formulario.
        Assert.Equal("/Personas/Details/1010101010", respuesta.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.DoesNotContain(_servicio.Filas, p => p.Cc == 2020202020);
    }

    [Fact]
    public async Task El_borrado_bloqueado_muestra_la_explicacion_y_no_elimina()
    {
        _servicio.ResultadoDeBaja = ServiceResult.Conflict(
            "No se puede eliminar la persona 1010101010: tiene 2 teléfono(s) y 1 estudio(s) asociados. " +
            "Elimine primero esos registros.");
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Delete/1010101010");

        var respuesta = await cliente.PostAsync("/Personas/Delete/1010101010", Formulario(token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"conflicto\"", html);
        Assert.Contains("tiene 2 tel", html);
        Assert.Contains("Elimine primero esos registros.", html);
        // Se vuelve a pintar la confirmación, con la persona todavía en su sitio.
        Assert.Contains("data-detalle=\"persona\"", html);
        Assert.Single(_servicio.Filas);
    }

    [Fact]
    public async Task Un_borrado_sin_dependencias_elimina_y_vuelve_al_listado()
    {
        var (cliente, token) = await ClienteConTokenAsync("/Personas/Delete/1010101010");

        var respuesta = await cliente.PostAsync("/Personas/Delete/1010101010", Formulario(token));
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("/Personas", respuesta.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("data-mensaje=\"exito\"", html);
        Assert.Empty(_servicio.Filas);
    }

    [Fact]
    public async Task Un_POST_sin_token_antiforgery_se_rechaza_con_400()
    {
        var respuesta = await _factory.CreateClient()
            .PostAsync("/Personas/Create", Formulario(token: null, cc: "3030303030", nombre: "Sin token"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("data-mensaje=\"estado\"", await respuesta.Content.ReadAsStringAsync());
        Assert.Empty(_servicio.Altas);
    }

    [Fact]
    public void El_controlador_depende_solo_del_servicio_de_personas()
    {
        var dependencias = typeof(PersonasController)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToList();

        // AK-01 y regla 3: nada de DbContext ni de repositorios en el borde web.
        Assert.Equal([typeof(IPersonaService)], dependencias);
    }

    private async Task<(HttpClient Cliente, string Token)> ClienteConTokenAsync(string ruta)
    {
        // El mismo cliente conserva la cookie que acompaña al token del formulario.
        var cliente = _factory.CreateClient();
        var html = await cliente.GetStringAsync(ruta);
        return (cliente, TokenRegex().Match(html).Groups[1].Value);
    }

    private static FormUrlEncodedContent Formulario(
        string? token,
        string cc = "1010101010",
        string nombre = "Ana",
        string apellido = "Gómez",
        string genero = "F",
        string edad = "30",
        bool edicion = false)
    {
        var campos = new Dictionary<string, string>
        {
            ["Cc"] = cc,
            ["Nombre"] = nombre,
            ["Apellido"] = apellido,
            ["Genero"] = genero,
            ["Edad"] = edad,
            ["EsEdicion"] = edicion ? "true" : "false"
        };

        if (token is not null)
        {
            campos["__RequestVerificationToken"] = token;
        }

        return new FormUrlEncodedContent(campos);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    /// <summary>
    /// Doble de <see cref="IPersonaService"/>: guarda en memoria y devuelve los mismos
    /// resultados discriminados que el servicio real, para poder comprobar la traducción
    /// del borde sin SQL Server. No reimplementa las reglas de dominio.
    /// </summary>
    private sealed class PersonaServicioDoble : IPersonaService
    {
        public List<PersonaDto> Filas { get; } = [];

        public List<PersonaEntradaDto> Altas { get; } = [];

        public List<(int Ruta, PersonaEntradaDto Entrada)> Ediciones { get; } = [];

        /// <summary>Si se fija, <c>DeleteAsync</c> lo devuelve sin eliminar nada.</summary>
        public ServiceResult? ResultadoDeBaja { get; set; }

        public Task<IReadOnlyList<PersonaDto>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<PersonaDto>>(Filas.ToList());

        public Task<ServiceResult<PersonaDto>> GetByIdAsync(int cc)
        {
            var persona = Filas.SingleOrDefault(p => p.Cc == cc);

            return Task.FromResult(persona is null
                ? ServiceResult<PersonaDto>.NotFound($"No existe una persona con cédula {cc}.")
                : ServiceResult<PersonaDto>.Success(persona));
        }

        public Task<ServiceResult<PersonaDto>> CreateAsync(PersonaEntradaDto entrada)
        {
            Altas.Add(entrada);

            if (Filas.Any(p => p.Cc == entrada.Cc))
            {
                return Task.FromResult(ServiceResult<PersonaDto>.Conflict(
                    $"Ya existe una persona con cédula {entrada.Cc}."));
            }

            var persona = new PersonaDto
            {
                Cc = entrada.Cc!.Value,
                Nombre = entrada.Nombre,
                Apellido = entrada.Apellido,
                Genero = entrada.Genero,
                Edad = entrada.Edad
            };
            Filas.Add(persona);

            return Task.FromResult(ServiceResult<PersonaDto>.Success(persona));
        }

        public Task<ServiceResult> UpdateAsync(int cc, PersonaEntradaDto entrada)
        {
            Ediciones.Add((cc, entrada));

            return Task.FromResult(Filas.Any(p => p.Cc == cc)
                ? ServiceResult.Success()
                : ServiceResult.NotFound($"No existe una persona con cédula {cc}."));
        }

        public Task<ServiceResult> DeleteAsync(int cc)
        {
            if (ResultadoDeBaja is not null)
            {
                return Task.FromResult(ResultadoDeBaja);
            }

            var persona = Filas.SingleOrDefault(p => p.Cc == cc);
            if (persona is null)
            {
                return Task.FromResult(ServiceResult.NotFound($"No existe una persona con cédula {cc}."));
            }

            Filas.Remove(persona);

            return Task.FromResult(ServiceResult.Success());
        }
    }
}
