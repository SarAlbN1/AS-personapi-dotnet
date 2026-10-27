using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Canal JSON de personas (SDD §5). Solo hace binding, códigos de estado y traducción
/// del resultado del servicio: no conoce EF Core, DbContext ni SQL.
/// </summary>
[ApiController]
[Route("api/personas")]
[Produces("application/json")]
public class PersonasApiController : ControllerBase
{
    private readonly IPersonaService _personas;

    public PersonasApiController(IPersonaService personas) => _personas = personas;

    /// <summary>Listado completo; sin filas devuelve <c>[]</c>.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PersonaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonaDto>>> GetAll() =>
        Ok(await _personas.GetAllAsync());

    /// <summary>Detalle por cédula.</summary>
    [HttpGet("{cc:int}")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int cc) =>
        Traducir(await _personas.GetByIdAsync(cc));

    /// <summary>Alta. Devuelve <c>201</c> con <c>Location</c> del recurso creado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PersonaEntradaDto entrada)
    {
        var resultado = await _personas.CreateAsync(entrada);

        if (!resultado.IsSuccess)
        {
            return Traducir(resultado);
        }

        return CreatedAtAction(nameof(GetById), new { cc = resultado.Value!.Cc }, resultado.Value);
    }

    /// <summary>Edición completa; la cédula de la ruta manda sobre la identidad.</summary>
    [HttpPut("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int cc, [FromBody] PersonaEntradaDto entrada) =>
        Traducir(await _personas.UpdateAsync(cc, entrada));

    /// <summary>Baja. Con teléfonos o estudios asociados responde <c>409</c>.</summary>
    [HttpDelete("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int cc) =>
        Traducir(await _personas.DeleteAsync(cc));

    // Un único punto de traducción resultado -> código de estado, para que los cinco
    // métodos no repitan el mapeo. Los errores viajan como ProblemDetails, la forma que
    // ya produce [ApiController] cuando el modelo de entrada es inválido.
    private IActionResult Traducir(ServiceResult resultado) => resultado.Status switch
    {
        // Solo llegan aquí los resultados sin valor: actualizar y eliminar (204).
        ServiceResultStatus.Success => NoContent(),
        ServiceResultStatus.NotFound => Problem(detail: resultado.Message, statusCode: StatusCodes.Status404NotFound),
        ServiceResultStatus.ValidationError => Problem(detail: resultado.Message, statusCode: StatusCodes.Status400BadRequest),
        ServiceResultStatus.Conflict => Problem(detail: resultado.Message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };

    private IActionResult Traducir<T>(ServiceResult<T> resultado) =>
        resultado.IsSuccess ? Ok(resultado.Value) : Traducir((ServiceResult)resultado);
}
