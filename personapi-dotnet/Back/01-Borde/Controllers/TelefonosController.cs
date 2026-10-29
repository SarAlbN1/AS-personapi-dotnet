using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Front.ViewModels;

namespace personapi_dotnet.Back.Borde.Controllers;

/// <summary>
/// CRUD web de teléfonos (HU-012, SDD §6). Usa el mismo <see cref="ITelefonoService"/> que
/// <c>TelefonosApiController</c>; el servicio de personas solo alimenta el selector de dueño
/// y los nombres que se muestran. El número viaja como cadena en la ruta
/// (<c>/Telefonos/{acción}/{num}</c>), sin restricción numérica, para no perder los ceros
/// iniciales.
/// </summary>
public class TelefonosController : Controller
{
    private readonly ITelefonoService _telefonos;
    private readonly IPersonaService _personas;

    public TelefonosController(ITelefonoService telefonos, IPersonaService personas)
    {
        _telefonos = telefonos;
        _personas = personas;
    }

    public async Task<IActionResult> Index()
    {
        var telefonos = await _telefonos.GetAllAsync();
        var personas = (await _personas.GetAllAsync()).ToDictionary(p => p.Cc, NombreCompleto);

        return View(telefonos.Select(t =>
        {
            var modelo = AViewModel(t);
            modelo.NombreDuenio = personas.GetValueOrDefault(t.Duenio);
            return modelo;
        }).ToList());
    }

    [HttpGet("Telefonos/Details/{num}")]
    public async Task<IActionResult> Details(string num)
    {
        var resultado = await _telefonos.GetByIdAsync(num);
        return this.VistaSiFalla(resultado, null) ?? View(await ConNombreAsync(AViewModel(resultado.Value!)));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await ConOpcionesAsync(new TelefonoViewModel()));

    [HttpPost]
    public async Task<IActionResult> Create(TelefonoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(await ConOpcionesAsync(modelo));
        }

        // Un dueño fuera del selector (formulario manipulado) llega al servicio, que comprueba
        // la referencia y responde ValidationError.
        var resultado = await _telefonos.CreateAsync(ADto(modelo));
        return this.VistaSiFalla(resultado, await ConOpcionesAsync(modelo))
            ?? this.RedirigirConExito($"Teléfono {modelo.Num} creado.", nameof(Index));
    }

    [HttpGet("Telefonos/Edit/{num}")]
    public async Task<IActionResult> Edit(string num)
    {
        var resultado = await _telefonos.GetByIdAsync(num);
        return this.VistaSiFalla(resultado, null) ?? View(await ConOpcionesAsync(AViewModel(resultado.Value!)));
    }

    // El número de la ruta es la identidad. [FromRoute] hace falta porque el formulario
    // también envía «Num» y el binding lo leería antes que la ruta. Si no coinciden, el
    // servicio responde ValidationError, igual que el PUT de la API: el número no se cambia.
    [HttpPost("Telefonos/Edit/{num}")]
    public async Task<IActionResult> Edit([FromRoute] string num, TelefonoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(await ConOpcionesAsync(ConNumDeLaRuta(num, modelo)));
        }

        var resultado = await _telefonos.UpdateAsync(num, ADto(modelo));
        return this.VistaSiFalla(resultado, await ConOpcionesAsync(ConNumDeLaRuta(num, modelo)))
            ?? this.RedirigirConExito($"Teléfono {num} actualizado.", nameof(Details), new { num });
    }

    [HttpGet("Telefonos/Delete/{num}")]
    public async Task<IActionResult> Delete(string num)
    {
        var resultado = await _telefonos.GetByIdAsync(num);
        return this.VistaSiFalla(resultado, null) ?? View(await ConNombreAsync(AViewModel(resultado.Value!)));
    }

    // Nada depende de un teléfono, así que la baja no tiene conflicto: solo puede faltar (404).
    [HttpPost("Telefonos/Delete/{num}"), ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string num)
    {
        var resultado = await _telefonos.DeleteAsync(num);
        return this.VistaSiFalla(resultado, null)
            ?? this.RedirigirConExito($"Teléfono {num} eliminado.", nameof(Index));
    }

    // Al repintar la edición, el formulario vuelve a apuntar al teléfono de la ruta. El número
    // del cuerpo puede venir manipulado y ser el de otro teléfono que existe: si la vista lo
    // usara, un segundo «Guardar» lo modificaría. Se quita también de ModelState, porque los
    // tag helpers pintan el valor enviado antes que el del modelo.
    private TelefonoViewModel ConNumDeLaRuta(string num, TelefonoViewModel modelo)
    {
        modelo.Num = num;
        ModelState.Remove(nameof(TelefonoViewModel.Num));
        return modelo;
    }

    // Las opciones salen del servicio de personas: el usuario solo puede elegir dueños que
    // existen en el momento de pintar el formulario.
    private async Task<TelefonoViewModel> ConOpcionesAsync(TelefonoViewModel modelo)
    {
        modelo.Personas = (await _personas.GetAllAsync())
            .Select(p => new SelectListItem($"{p.Cc} · {NombreCompleto(p)}", p.Cc.ToString()))
            .ToList();
        return modelo;
    }

    private async Task<TelefonoViewModel> ConNombreAsync(TelefonoViewModel modelo)
    {
        var persona = await _personas.GetByIdAsync(modelo.Duenio!.Value);
        modelo.NombreDuenio = persona.Value is { } p ? NombreCompleto(p) : null;
        return modelo;
    }

    private static string NombreCompleto(PersonaDto persona) => $"{persona.Nombre} {persona.Apellido}";

    private static TelefonoViewModel AViewModel(TelefonoDto dto) =>
        new() { Num = dto.Num, Oper = dto.Oper, Duenio = dto.Duenio };

    private static TelefonoEntradaDto ADto(TelefonoViewModel modelo) =>
        new() { Num = modelo.Num!, Oper = modelo.Oper!, Duenio = modelo.Duenio };
}
