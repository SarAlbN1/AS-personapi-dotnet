using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Implementations;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Implementations;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.ControllersApi;
using personapi_dotnet.Back.Persistencia.Entities;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "Front/wwwroot"
});

// Antiforgery automático en los POST de los controladores MVC; la API REST queda fuera (HU-006).
// Los [ApiController] documentan el 500 por convención (HU-009).
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new personapi_dotnet.Back.Borde.Controllers.AntiforgeryEnVistasConvention());
    options.Conventions.Add(new ErroresApiConvention());//ACTIVO MVC Y API
});

// ProblemDetails para los errores no controlados de /api (ver UseExceptionHandler abajo) y
// textos y ejemplos del documento OpenAPI 3 (HU-009).
builder.Services.AddProblemDetails();
builder.Services.AddDocumentacionOpenApi();

// El árbol separa Front/ de Back/: sin estas rutas Razor no encuentra las vistas.
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Front/Views/{1}/{0}" + RazorViewEngine.ViewExtension);//dodne buscar las vistas de RAZOR (HTML)
    options.ViewLocationFormats.Add("/Front/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
});

// La cadena llega por User Secrets o por ConnectionStrings__PersonaDb, nunca desde el repositorio.
// AddDbContext registra el contexto como scoped: uno por solicitud.
builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PersonaDb")));//cuando alguien pide ipersonarepository, se le inyecta el dbcontext, que a su vez tiene la cadena de conexión.

// Único registro de repositorios (acuerdo de archivos compartidos de HU-003). 
// Cada historia de API añade aquí solo la línea de su servicio. Conexion db con servicios
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();

// Servicios de aplicación: una línea por historia de módulo.
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IProfesionService, ProfesionService>();
builder.Services.AddScoped<ITelefonoService, TelefonoService>();
builder.Services.AddScoped<IEstudioService, EstudioService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())//problem details en el json 
{
    // Una excepción no controlada en /api responde 500 con un ProblemDetails sin traza ni
    // mensaje interno; en las rutas web, con la vista Error.
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/api"),
        api => api.UseExceptionHandler());
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/api"),
        web => web.UseExceptionHandler("/Home/Error"));
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Las respuestas de error sin cuerpo de las rutas web (ruta desconocida, POST sin token)
// se muestran con la vista Estado. /api queda fuera: allí los errores son ProblemDetails.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    web => web.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}"));

// UseStaticFiles y no MapStaticAssets: el manifiesto de MapStaticAssets se genera en
// compilación a partir de ./wwwroot, que aquí no existe. WebRootPath resuelve
// Front/wwwroot en ejecución.
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonAPI v1"));

// MapControllers para las rutas por atributos de la API; MapControllerRoute para las vistas MVC.
app.MapControllers();//las rutas de la API se definen por los atributos [Route] de cada controlador. La ruta de la API es /api/persona, /api/profesion, etc. (HU-003).

app.MapControllerRoute(//las rutas de las vistas MVC se definen por convención: /{controlador}/{accion}/{id?}. La ruta por defecto es /Home/Index. (HU-003).
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
