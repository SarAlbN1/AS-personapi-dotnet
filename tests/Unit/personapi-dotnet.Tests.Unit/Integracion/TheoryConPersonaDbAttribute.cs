namespace personapi_dotnet.Tests.Unit.Integracion;

/// <summary>
/// Variante de <see cref="FactConPersonaDbAttribute"/> para pruebas con datos: sin
/// ConnectionStrings:PersonaDb, todos los casos se omiten en lugar de fallar.
/// </summary>
public sealed class TheoryConPersonaDbAttribute : TheoryAttribute
{
    public TheoryConPersonaDbAttribute()
    {
        if (string.IsNullOrWhiteSpace(FactConPersonaDbAttribute.CadenaDeConexion()))
        {
            Skip = "Sin ConnectionStrings:PersonaDb: se omite la prueba contra persona_db.";
        }
    }
}
