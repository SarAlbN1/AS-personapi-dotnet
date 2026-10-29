using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// CRUD web de estudios (HU-013, SDD §6). Usa el mismo <see cref="IEstudioService"/> que
/// <c>EstudiosApiController</c>; los servicios de persona y profesión solo alimentan los
/// selectores y los nombres que se muestran. Detalle, edición y baja llevan las dos
/// mitades de la clave en la ruta (<c>/Estudios/{acción}/{idProf}/{ccPer}</c>), nunca un
/// identificador artificial (AK-03).
/// </summary>
public class EstudiosController : Controller
{
    private readonly IEstudioService _estudios;
    private readonly IProfesionService _profesiones;
    private readonly IPersonaService _personas;

    public EstudiosController(IEstudioService estudios, IProfesionService profesiones, IPersonaService personas)
    {
        _estudios = estudios;
        _profesiones = profesiones;
        _personas = personas;
    }

    public async Task<IActionResult> Index()
    {
        var estudios = await _estudios.GetAllAsync();
        var profesiones = (await _profesiones.GetAllAsync()).ToDictionary(p => p.Id!.Value, p => p.Nom);
        var personas = (await _personas.GetAllAsync()).ToDictionary(p => p.Cc, NombreCompleto);

        return View(estudios.Select(e =>
        {
            var modelo = AViewModel(e);
            modelo.NomProfesion = profesiones.GetValueOrDefault(e.IdProf);
            modelo.NombrePersona = personas.GetValueOrDefault(e.CcPer);
            return modelo;
        }).ToList());
    }

    [HttpGet("Estudios/Details/{idProf:int}/{ccPer:int}")]
    public async Task<IActionResult> Details(int idProf, int ccPer)
    {
        var resultado = await _estudios.GetByIdAsync(idProf, ccPer);
        return this.VistaSiFalla(resultado, null) ?? View(await ConNombresAsync(AViewModel(resultado.Value!)));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await ConOpcionesAsync(new EstudioViewModel()));

    [HttpPost]
    public async Task<IActionResult> Create(EstudioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(await ConOpcionesAsync(modelo));
        }

        // Un valor fuera de los selectores (formulario manipulado) llega al servicio, que
        // comprueba las dos referencias y responde ValidationError.
        var resultado = await _estudios.CreateAsync(ADto(modelo));
        return this.VistaSiFalla(resultado, await ConOpcionesAsync(modelo))
            ?? this.RedirigirConExito(
                $"Estudio de la profesión {modelo.IdProf} para la persona {modelo.CcPer} creado.", nameof(Index));
    }

    [HttpGet("Estudios/Edit/{idProf:int}/{ccPer:int}")]
    public async Task<IActionResult> Edit(int idProf, int ccPer)
    {
        var resultado = await _estudios.GetByIdAsync(idProf, ccPer);
        return this.VistaSiFalla(resultado, null) ?? View(await ConNombresAsync(AViewModel(resultado.Value!)));
    }

    // El par de la ruta es la identidad. [FromRoute] hace falta porque el formulario también
    // envía IdProf y CcPer y el binding los leería antes que la ruta. Si no coinciden, el
    // servicio responde ValidationError, igual que el PUT de la API: el par no se cambia.
    [HttpPost("Estudios/Edit/{idProf:int}/{ccPer:int}")]
    public async Task<IActionResult> Edit([FromRoute] int idProf, [FromRoute] int ccPer, EstudioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(await ConNombresAsync(ConClaveDeLaRuta(idProf, ccPer, modelo)));
        }

        var resultado = await _estudios.UpdateAsync(idProf, ccPer, ADto(modelo));
        return this.VistaSiFalla(resultado, await ConNombresAsync(ConClaveDeLaRuta(idProf, ccPer, modelo)))
            ?? this.RedirigirConExito(
                $"Estudio de la profesión {idProf} para la persona {ccPer} actualizado.",
                nameof(Details), new { idProf, ccPer });
    }

    [HttpGet("Estudios/Delete/{idProf:int}/{ccPer:int}")]
    public async Task<IActionResult> Delete(int idProf, int ccPer)
    {
        var resultado = await _estudios.GetByIdAsync(idProf, ccPer);
        return this.VistaSiFalla(resultado, null) ?? View(await ConNombresAsync(AViewModel(resultado.Value!)));
    }

    // Nada depende de un estudio, así que la baja no tiene conflicto: solo puede faltar (404).
    [HttpPost("Estudios/Delete/{idProf:int}/{ccPer:int}"), ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int idProf, int ccPer)
    {
        var resultado = await _estudios.DeleteAsync(idProf, ccPer);
        return this.VistaSiFalla(resultado, null)
            ?? this.RedirigirConExito(
                $"Estudio de la profesión {idProf} para la persona {ccPer} eliminado.", nameof(Index));
    }

    // Al repintar la edición, el formulario vuelve a apuntar al estudio de la ruta. El par
    // del cuerpo puede venir manipulado y ser el de otro estudio que existe: si la vista lo
    // usara, un segundo «Guardar» lo modificaría. Se quita también de ModelState, porque los
    // tag helpers pintan el valor enviado antes que el del modelo.
    private EstudioViewModel ConClaveDeLaRuta(int idProf, int ccPer, EstudioViewModel modelo)
    {
        modelo.IdProf = idProf;
        modelo.CcPer = ccPer;
        ModelState.Remove(nameof(EstudioViewModel.IdProf));
        ModelState.Remove(nameof(EstudioViewModel.CcPer));
        return modelo;
    }

    // Las opciones salen de los servicios: el usuario solo puede elegir profesiones y
    // personas que existen en el momento de pintar el formulario.
    private async Task<EstudioViewModel> ConOpcionesAsync(EstudioViewModel modelo)
    {
        modelo.Profesiones = (await _profesiones.GetAllAsync())
            .Select(p => new SelectListItem($"{p.Id} · {p.Nom}", p.Id.ToString()))
            .ToList();
        modelo.Personas = (await _personas.GetAllAsync())
            .Select(p => new SelectListItem($"{p.Cc} · {NombreCompleto(p)}", p.Cc.ToString()))
            .ToList();
        return modelo;
    }

    private async Task<EstudioViewModel> ConNombresAsync(EstudioViewModel modelo)
    {
        var profesion = await _profesiones.GetByIdAsync(modelo.IdProf!.Value);
        var persona = await _personas.GetByIdAsync(modelo.CcPer!.Value);
        modelo.NomProfesion = profesion.Value?.Nom;
        modelo.NombrePersona = persona.Value is { } p ? NombreCompleto(p) : null;
        return modelo;
    }

    private static string NombreCompleto(PersonaDto persona) => $"{persona.Nombre} {persona.Apellido}";

    private static EstudioViewModel AViewModel(EstudioDto dto) =>
        new() { IdProf = dto.IdProf, CcPer = dto.CcPer, Fecha = dto.Fecha, Univer = dto.Univer };

    private static EstudioEntradaDto ADto(EstudioViewModel modelo) =>
        new() { IdProf = modelo.IdProf, CcPer = modelo.CcPer, Fecha = modelo.Fecha, Univer = modelo.Univer };
}
