using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.Persistencia.Entities;
using Xunit.Abstractions;

namespace personapi_dotnet.Tests.Unit.Verificacion;

/// <summary>
/// Comprobación arquitectónica de HU-014 (criterio 5; AK-01, AD-01, AD-02, AD-03, AD-10).
/// Recorre el código fuente y los tipos compilados, así que también vigila a cada módulo
/// que se integre después sin que haya que tocar esta clase.
/// </summary>
public partial class ArquitecturaTests
{
    private const string EspacioEntidades = "personapi_dotnet.Back.Persistencia.Entities";
    private const string EspacioServicios = "personapi_dotnet.Back.Aplicacion.Services.Interfaces";

    private static readonly Assembly Web = typeof(PersonaDbContext).Assembly;
    private static readonly string Raiz = BuscarRaiz();
    private static readonly string Proyecto = Path.Combine(Raiz, "personapi-dotnet");

    // Únicos sitios donde puede aparecer el contexto: su definición, las implementaciones
    // DAO y la raíz de composición, que solo lo registra (AddDbContext).
    private static readonly string[] PermitidosContexto =
    [
        "Back/03-AccesoDatos/Repository/Implementations/",
        "Back/04-Persistencia/Entities/PersonaDbContext.cs",
        "Program.cs"
    ];

    private readonly ITestOutputHelper _salida;

    public ArquitecturaTests(ITestOutputHelper salida) => _salida = salida;

    [Fact]
    public void Ningun_archivo_fuera_de_los_DAO_referencia_PersonaDbContext()
    {
        var infractores = ArchivosDeCodigo()
            .Where(a => !PermitidosContexto.Any(p => a.Relativa.StartsWith(p)))
            .Where(a => ContextoRegex().IsMatch(a.Codigo))
            .Select(a => a.Relativa)
            .ToList();

        Assert.Empty(infractores);
    }

    [Fact]
    public void Program_solo_registra_el_contexto()
    {
        var program = ArchivosDeCodigo().Single(a => a.Relativa == "Program.cs");

        var usos = program.Codigo.Split('\n').Where(l => ContextoRegex().IsMatch(l)).ToList();

        var uso = Assert.Single(usos);
        Assert.Contains("AddDbContext<PersonaDbContext>", uso);
    }

    [Fact]
    public void Ningun_archivo_fuera_de_los_DAO_usa_EF_Core_ni_ejecuta_SQL()
    {
        var infractores = ArchivosDeCodigo()
            .Where(a => !a.Relativa.StartsWith("Back/03-AccesoDatos/Repository/Implementations/"))
            .Where(a => !a.Relativa.StartsWith("Back/04-Persistencia/"))
            .Where(a => a.Relativa != "Program.cs")
            .Where(a => AccesoDatosRegex().IsMatch(a.Codigo))
            .Select(a => $"{a.Relativa}: {AccesoDatosRegex().Match(a.Codigo).Value}")
            .ToList();

        Assert.Empty(infractores);
    }

    [Fact]
    public void Los_controladores_dependen_solo_de_interfaces_de_servicio()
    {
        foreach (var controlador in Controladores())
        {
            foreach (var parametro in controlador.GetConstructors().SelectMany(c => c.GetParameters()))
            {
                var tipo = parametro.ParameterType;
                Assert.True(
                    tipo.IsInterface && tipo.Namespace == EspacioServicios,
                    $"{controlador.Name} recibe {tipo.FullName}; solo puede recibir I*Service.");
            }
        }
    }

    [Fact]
    public void El_controlador_MVC_y_el_API_de_cada_entidad_usan_el_mismo_servicio()
    {
        var controladores = Controladores().ToDictionary(t => t.Name);

        foreach (var entidad in new[] { "Personas", "Profesiones", "Telefonos", "Estudios" })
        {
            // Mientras falte uno de los dos canales no hay par que comparar: la prueba se
            // vuelve efectiva sola en cuanto ambos están integrados.
            if (!controladores.TryGetValue($"{entidad}Controller", out var mvc)
                || !controladores.TryGetValue($"{entidad}ApiController", out var api))
            {
                continue;
            }

            Assert.Equal(Servicios(api), Servicios(mvc));
            _salida.WriteLine($"{entidad}: {mvc.Name} y {api.Name} usan {string.Join(", ", Servicios(mvc).Select(t => t.Name))}.");
        }
    }

    [Fact]
    public void Ni_DTO_ni_ViewModels_ni_servicios_exponen_entidades_EF()
    {
        var expuestos = new List<string>();

        foreach (var tipo in Web.GetTypes().Where(t =>
                     t.Namespace is "personapi_dotnet.Back.Borde.Dtos" or "personapi_dotnet.Front.ViewModels"))
        {
            expuestos.AddRange(tipo.GetProperties()
                .Where(p => MencionaEntidad(p.PropertyType))
                .Select(p => $"{tipo.Name}.{p.Name}"));
        }

        foreach (var servicio in Web.GetTypes().Where(t => t.IsInterface && t.Namespace == EspacioServicios))
        {
            expuestos.AddRange(servicio.GetMethods()
                .Where(m => MencionaEntidad(m.ReturnType) || m.GetParameters().Any(p => MencionaEntidad(p.ParameterType)))
                .Select(m => $"{servicio.Name}.{m.Name}"));
        }

        foreach (var controlador in Controladores())
        {
            expuestos.AddRange(controlador.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => MencionaEntidad(m.ReturnType) || m.GetParameters().Any(p => MencionaEntidad(p.ParameterType)))
                .Select(m => $"{controlador.Name}.{m.Name}"));
        }

        Assert.Empty(expuestos);
    }

    [Fact]
    public void Ninguna_vista_recibe_una_entidad_EF()
    {
        var entidades = Web.GetTypes()
            .Where(t => t.Namespace == EspacioEntidades && t != typeof(PersonaDbContext))
            .Select(t => t.Name)
            .ToHashSet();

        var infractores = Directory.EnumerateFiles(Path.Combine(Proyecto, "Front", "Views"), "*.cshtml", SearchOption.AllDirectories)
            .SelectMany(v => File.ReadLines(v).Select(l => (Vista: v, Linea: l)))
            .Select(x => (x.Vista, Modelo: ModeloRegex().Match(x.Linea)))
            .Where(x => x.Modelo.Success)
            .Where(x => IdentificadorRegex().Matches(x.Modelo.Groups[1].Value)
                .Any(m => entidades.Contains(m.Value) || m.Value == "Entities"))
            .Select(x => $"{Path.GetRelativePath(Proyecto, x.Vista)}: {x.Modelo.Value}")
            .ToList();

        Assert.Empty(infractores);
    }

    [Fact]
    public void La_solucion_es_un_unico_artefacto_sobre_una_sola_base()
    {
        // Un solo proyecto desplegable en la solución; el resto son pruebas.
        var proyectos = ProyectoRegex().Matches(File.ReadAllText(Path.Combine(Raiz, "personapi-dotnet.sln")))
            .Select(m => m.Groups[1].Value.Replace('\\', '/'))
            .Where(p => !p.StartsWith("tests/"))
            .ToList();
        Assert.Equal(["personapi-dotnet/personapi-dotnet.csproj"], proyectos);

        // Un solo DbContext en el ensamblado.
        var contextos = Web.GetTypes().Where(t => t.IsSubclassOf(typeof(DbContext))).ToList();
        Assert.Equal([typeof(PersonaDbContext)], contextos);

        // Una sola cadena de conexión, PersonaDb, y nadie lee otra.
        using var appsettings = JsonDocument.Parse(File.ReadAllText(Path.Combine(Proyecto, "appsettings.json")));
        var cadenas = appsettings.RootElement.GetProperty("ConnectionStrings").EnumerateObject().Select(c => c.Name);
        Assert.Equal(["PersonaDb"], cadenas);

        var lecturas = ArchivosDeCodigo()
            .SelectMany(a => CadenaRegex().Matches(a.Codigo).Select(m => m.Groups[1].Value))
            .Distinct();
        Assert.Equal(["PersonaDb"], lecturas);
    }

    private static IEnumerable<Type> Controladores() =>
        Web.GetTypes().Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract);

    private static IEnumerable<Type> Servicios(Type controlador) =>
        controlador.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType).OrderBy(t => t.FullName);

    private static bool MencionaEntidad(Type tipo)
    {
        if (tipo.Namespace == EspacioEntidades)
        {
            return true;
        }

        if (tipo.HasElementType && MencionaEntidad(tipo.GetElementType()!))
        {
            return true;
        }

        return tipo.IsGenericType && tipo.GetGenericArguments().Any(MencionaEntidad);
    }

    /// <summary>Archivos .cs y .cshtml del proyecto web, sin bin/obj y sin comentarios.</summary>
    private static IEnumerable<(string Relativa, string Codigo)> ArchivosDeCodigo() =>
        Directory.EnumerateFiles(Proyecto, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs") || f.EndsWith(".cshtml"))
            .Select(f => (Relativa: Path.GetRelativePath(Proyecto, f).Replace('\\', '/'), Ruta: f))
            .Where(f => !f.Relativa.StartsWith("bin/") && !f.Relativa.StartsWith("obj/"))
            .Select(f => (f.Relativa, Codigo: SinComentarios(File.ReadAllText(f.Ruta))));

    // Los comentarios pueden nombrar PersonaDbContext para decir que no se usa: no cuentan.
    private static string SinComentarios(string codigo) =>
        ComentarioRegex().Replace(codigo, string.Empty);

    private static string BuscarRaiz()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "personapi-dotnet.sln")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("No se encontró personapi-dotnet.sln por encima del directorio de pruebas.");
    }

    [GeneratedRegex(@"\bPersonaDbContext\b")]
    private static partial Regex ContextoRegex();

    [GeneratedRegex(@"\b(using\s+Microsoft\.EntityFrameworkCore|Microsoft\.Data\.SqlClient|SqlConnection|SqlCommand|FromSql\w*|ExecuteSql\w*|SqlQuery\w*|DbSet<)")]
    private static partial Regex AccesoDatosRegex();

    [GeneratedRegex(@"//[^\n]*|/\*.*?\*/|@\*.*?\*@", RegexOptions.Singleline)]
    private static partial Regex ComentarioRegex();

    [GeneratedRegex(@"^\s*@model\s+(.+)$")]
    private static partial Regex ModeloRegex();

    [GeneratedRegex(@"[A-Za-z_][A-Za-z0-9_]*")]
    private static partial Regex IdentificadorRegex();

    [GeneratedRegex(@"Project\(""\{FAE04EC0[^}]*\}""\)\s*=\s*""[^""]*"",\s*""([^""]+\.csproj)""")]
    private static partial Regex ProyectoRegex();

    [GeneratedRegex(@"GetConnectionString\(""([^""]+)""\)")]
    private static partial Regex CadenaRegex();
}
