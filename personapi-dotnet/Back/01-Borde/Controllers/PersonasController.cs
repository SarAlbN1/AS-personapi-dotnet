using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// CRUD web de personas (HU-007): las cinco acciones de SDD §6 sobre <b>el mismo</b>
/// <see cref="IPersonaService"/> que usa <c>PersonasApiController</c>, de modo que las
/// reglas de SDD §7 no se escriben dos veces (AD-02, AD-03, reglas 2 y 12).
/// </summary>
/// <remarks>
/// No referencia <c>PersonaDbContext</c> ni ejecuta SQL (AK-01, regla 3): depende solo
/// de la interfaz del servicio. La traducción de los resultados a lo que ve el usuario
/// es la convención de HU-006 (<c>VistaSiFalla</c> y <c>RedirigirConExito</c>), y el
/// token antiforgery de los POST lo exige <c>AntiforgeryEnVistasConvention</c>, así que
/// aquí no hace falta <c>[ValidateAntiForgeryToken]</c>.
/// </remarks>
public class PersonasController : Controller
{
    private readonly IPersonaService _personas;

    public PersonasController(IPersonaService personas) => _personas = personas;

    /// <summary>Listado completo. Un listado vacío es una tabla vacía, no un error.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var personas = await _personas.GetAllAsync();

        return View(new PersonaListadoViewModel
        {
            Personas = personas.Select(PersonaDetalleViewModel.Desde).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var resultado = await _personas.GetByIdAsync(id);

        return this.VistaSiFalla(resultado, null)
            ?? View(PersonaDetalleViewModel.Desde(resultado.Value!));
    }

    [HttpGet]
    public IActionResult Create() => View(new PersonaFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(PersonaFormViewModel modelo)
    {
        // Forma inválida: se vuelve a pintar el formulario sin llamar al servicio, así
        // que no se escribe nada en la base.
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _personas.CreateAsync(modelo.AEntrada());

        return this.VistaSiFalla(resultado, modelo)
            ?? this.RedirigirConExito(
                $"Persona {modelo.Cc} creada.", nameof(Details), new { id = modelo.Cc });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resultado = await _personas.GetByIdAsync(id);

        return this.VistaSiFalla(resultado, null)
            ?? View(PersonaFormViewModel.Desde(resultado.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, PersonaFormViewModel modelo)
    {
        // La vista envía la cédula oculta; si no coincide con la ruta, el servicio lo
        // rechaza con ValidationError, igual que el PUT del canal REST.
        modelo.EsEdicion = true;

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _personas.UpdateAsync(id, modelo.AEntrada());

        return this.VistaSiFalla(resultado, modelo)
            ?? this.RedirigirConExito(
                $"Persona {id} actualizada.", nameof(Details), new { id });
    }

    /// <summary>Confirmación de baja: muestra la persona antes de eliminarla.</summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _personas.GetByIdAsync(id);

        return this.VistaSiFalla(resultado, null)
            ?? View(PersonaDetalleViewModel.Desde(resultado.Value!));
    }

    /// <summary>
    /// Baja efectiva. Si la persona tiene teléfonos o estudios, el servicio devuelve
    /// <c>Conflict</c> (regla 10) y la confirmación se vuelve a pintar con la
    /// explicación, sin eliminar nada.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(Delete))]
    public async Task<IActionResult> DeleteConfirmado(int id)
    {
        var resultado = await _personas.DeleteAsync(id);

        if (resultado.IsSuccess)
        {
            return this.RedirigirConExito($"Persona {id} eliminada.", nameof(Index));
        }

        // Para volver a mostrar la confirmación con el aviso hace falta la persona otra
        // vez; si ya no existe, VistaSiFalla resuelve el 404 y el modelo no se usa.
        var persona = await _personas.GetByIdAsync(id);
        var modelo = persona.IsSuccess ? PersonaDetalleViewModel.Desde(persona.Value!) : null;

        return this.VistaSiFalla(resultado, modelo)!;
    }
}
