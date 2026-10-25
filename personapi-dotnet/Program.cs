using Microsoft.AspNetCore.Mvc.Razor;

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
