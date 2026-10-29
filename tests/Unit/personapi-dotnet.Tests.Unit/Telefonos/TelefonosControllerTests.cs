using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Controllers;

namespace personapi_dotnet.Tests.Unit.Telefonos;

/// <summary>
/// Criterio 5 de HU-012: el controlador MVC depende solo de servicios. El de teléfonos hace
/// el trabajo; el de personas alimenta el selector de dueño.
/// </summary>
public class TelefonosControllerTests
{
    private static readonly Type[] Servicios = [typeof(ITelefonoService), typeof(IPersonaService)];

    [Fact]
    public void TelefonosController_depende_solo_de_los_servicios()
    {
        var constructores = typeof(TelefonosController).GetConstructors();

        var parametros = Assert.Single(constructores).GetParameters();
        Assert.Equal(Servicios, parametros.Select(p => p.ParameterType));
    }

    [Fact]
    public void TelefonosController_no_tiene_campos_de_acceso_a_datos()
    {
        var campos = typeof(TelefonosController)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.All(campos, c => Assert.Contains(c.FieldType, Servicios));
    }
}
