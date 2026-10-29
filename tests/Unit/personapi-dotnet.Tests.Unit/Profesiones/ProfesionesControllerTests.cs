using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Controllers;

namespace personapi_dotnet.Tests.Unit.Profesiones;

/// <summary>Criterio 5 de HU-008: el controlador MVC solo conoce el servicio compartido.</summary>
public class ProfesionesControllerTests
{
    [Fact]
    public void ProfesionesController_depende_solo_de_IProfesionService()
    {
        var constructores = typeof(ProfesionesController).GetConstructors();

        var parametros = Assert.Single(constructores).GetParameters();
        Assert.Equal([typeof(IProfesionService)], parametros.Select(p => p.ParameterType));
    }

    [Fact]
    public void ProfesionesController_no_tiene_campos_de_acceso_a_datos()
    {
        var campos = typeof(ProfesionesController)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.All(campos, c => Assert.Equal(typeof(IProfesionService), c.FieldType));
    }
}
