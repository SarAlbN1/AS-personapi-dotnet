using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Controllers;

namespace personapi_dotnet.Tests.Unit.Estudios;

/// <summary>
/// Criterio 5 de HU-013: el controlador MVC depende solo de servicios. El de estudios hace
/// el trabajo; los de profesión y persona alimentan los selectores.
/// </summary>
public class EstudiosControllerTests
{
    private static readonly Type[] Servicios =
        [typeof(IEstudioService), typeof(IProfesionService), typeof(IPersonaService)];

    [Fact]
    public void EstudiosController_depende_solo_de_los_servicios()
    {
        var constructores = typeof(EstudiosController).GetConstructors();

        var parametros = Assert.Single(constructores).GetParameters();
        Assert.Equal(Servicios, parametros.Select(p => p.ParameterType));
    }

    [Fact]
    public void EstudiosController_no_tiene_campos_de_acceso_a_datos()
    {
        var campos = typeof(EstudiosController)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.All(campos, c => Assert.Contains(c.FieldType, Servicios));
    }
}
