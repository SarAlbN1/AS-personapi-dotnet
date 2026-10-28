using System.Reflection;
using Microsoft.OpenApi;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Configuración del documento OpenAPI 3 que publica Swagger UI (HU-009, AD-07). Los
/// códigos de cada operación salen de sus <c>[ProducesResponseType]</c>; los resúmenes,
/// las descripciones por código y los ejemplos, de los comentarios XML de los
/// controladores y de los DTO. La convención completa está en
/// <c>docs/wiki/03-contratos/api-rest.md</c>.
/// </summary>
public static class DocumentacionOpenApi
{
    public static IServiceCollection AddDocumentacionOpenApi(this IServiceCollection services)
    {
        services.ConfigureSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PersonAPI",
                Version = "v1",
                Description =
                    "CRUD REST de personas, profesiones, teléfonos y estudios sobre persona_db. " +
                    "Los errores se devuelven como ProblemDetails (RFC 9457) y nunca incluyen " +
                    "trazas, SQL ni la cadena de conexión."
            });

            // El SDK genera el XML junto al ensamblado (GenerateDocumentationFile). Si no
            // está, el documento sigue siendo válido, solo que sin textos ni ejemplos.
            var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xml))
            {
                options.IncludeXmlComments(xml);
            }
        });

        return services;
    }
}
