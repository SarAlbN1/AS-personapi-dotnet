using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// CRUD web de profesiones (HU-008, SDD §6). Usa el mismo <see cref="IProfesionService"/>
/// que <c>ProfesionesApiController</c>, así que las reglas de SDD §7 no se repiten aquí:
/// los resultados se traducen con la convención de HU-006.
/// </summary>
public class ProfesionesController : Controller
{
    private readonly IProfesionService _profesiones;

    public ProfesionesController(IProfesionService profesiones) => _profesiones = profesiones;

    public async Task<IActionResult> Index()
    {
        var profesiones = await _profesiones.GetAllAsync();
        return View(profesiones.Select(AViewModel).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var resultado = await _profesiones.GetByIdAsync(id);
        return this.VistaSiFalla(resultado, null) ?? View(AViewModel(resultado.Value!));
    }

    [HttpGet]
    public IActionResult Create() => View(new ProfesionViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(ProfesionViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _profesiones.CreateAsync(ADto(modelo));
        return this.VistaSiFalla(resultado, modelo)
            ?? this.RedirigirConExito($"Profesión {modelo.Id} creada.", nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resultado = await _profesiones.GetByIdAsync(id);
        return this.VistaSiFalla(resultado, null) ?? View(AViewModel(resultado.Value!));
    }

    // El id de la ruta es la identidad. [FromRoute] hace falta porque el formulario también
    // envía «Id» y el binding lo leería antes que la ruta. Si no coinciden, el servicio
    // responde ValidationError, igual que el PUT de la API.
    [HttpPost]
    public async Task<IActionResult> Edit([FromRoute] int id, ProfesionViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _profesiones.UpdateAsync(id, ADto(modelo));
        return this.VistaSiFalla(resultado, modelo)
            ?? this.RedirigirConExito($"Profesión {id} actualizada.", nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _profesiones.GetByIdAsync(id);
        return this.VistaSiFalla(resultado, null) ?? View(AViewModel(resultado.Value!));
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _profesiones.DeleteAsync(id);
        if (resultado.IsSuccess)
        {
            return this.RedirigirConExito($"Profesión {id} eliminada.", nameof(Index));
        }

        // El 409 (estudios asociados) se muestra sobre la misma confirmación, con los datos
        // actuales de la profesión. Si entretanto desapareció, VistaSiFalla da el 404.
        var actual = await _profesiones.GetByIdAsync(id);
        var modelo = actual.IsSuccess ? AViewModel(actual.Value!) : null;
        return this.VistaSiFalla(resultado, modelo)!;
    }

    private static ProfesionViewModel AViewModel(ProfesionDto dto) =>
        new() { Id = dto.Id, Nom = dto.Nom, Des = dto.Des };

    private static ProfesionDto ADto(ProfesionViewModel modelo) =>
        new() { Id = modelo.Id, Nom = modelo.Nom, Des = modelo.Des };
}
