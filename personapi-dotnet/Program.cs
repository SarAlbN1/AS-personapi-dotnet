using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Back.AccesoDatos.Repository.Implementations;
using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Implementations;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Persistencia.Entities;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "Front/wwwroot"
});

builder.Services.AddControllersWithViews();

// El árbol separa Front/ de Back/: sin estas rutas Razor no encuentra las vistas.
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Front/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Front/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
});

// La cadena llega por User Secrets o por ConnectionStrings__PersonaDb, nunca desde el repositorio.
// AddDbContext registra el contexto como scoped: uno por solicitud.
builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PersonaDb")));

// Único registro de repositorios (acuerdo de archivos compartidos de HU-003). Cada
// historia de API añade aquí solo la línea de su servicio.
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();

builder.Services.AddScoped<IProfesionService, ProfesionService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
    app.UseHsts();
}

// UseStaticFiles y no MapStaticAssets: el manifiesto de MapStaticAssets se genera en
// compilación a partir de ./wwwroot, que aquí no existe. WebRootPath resuelve
// Front/wwwroot en ejecución.
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonAPI v1"));

// MapControllers para las rutas por atributos de la API; MapControllerRoute para las vistas MVC.
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
