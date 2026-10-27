using Microsoft.Extensions.Configuration;

namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Prueba que necesita persona_db real. Lee ConnectionStrings:PersonaDb igual que la
/// aplicación (User Secrets del proyecto web o variable ConnectionStrings__PersonaDb) y,
/// si no está definida, la prueba se omite en lugar de fallar.
/// </summary>
public sealed class FactConPersonaDbAttribute : FactAttribute
{
    public FactConPersonaDbAttribute()
    {
        if (string.IsNullOrWhiteSpace(CadenaDeConexion()))
        {
            Skip = "Sin ConnectionStrings:PersonaDb: se omite la prueba contra persona_db.";
        }
    }

    public static string? CadenaDeConexion() =>
        new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build()
            .GetConnectionString("PersonaDb");
}
