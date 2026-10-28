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

    /// <summary>Lista todas las personas.</summary>
    /// <response code="200">Las personas registradas; sin filas, <c>[]</c>.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PersonaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonaDto>>> GetAll() =>
        Ok(await _personas.GetAllAsync());

    /// <summary>Consulta una persona por su cédula.</summary>
    /// <param name="cc">Cédula de la persona (clave).</param>
    /// <response code="200">La persona encontrada.</response>
    /// <response code="404">No existe una persona con esa cédula.</response>
    [HttpGet("{cc:int}")]
    [ProducesResponseType<PersonaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int cc) =>
        Traducir(await _personas.GetByIdAsync(cc));

    /// <summary>Crea una persona.</summary>
    /// <param name="entrada">Datos de la persona; <c>genero</c> es <c>M</c> o <c>F</c>.</param>
    /// <response code="201">Persona creada; <c>Location</c> apunta a <c>/api/personas/{cc}</c>.</response>
    /// <response code="400">Falta un campo obligatorio, una longitud se excede, el género no es M ni F o la edad es negativa.</response>
    /// <response code="409">Ya existe una persona con esa cédula.</response>
    [HttpPost]
    [ProducesResponseType<PersonaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PersonaEntradaDto entrada)
    {
        var resultado = await _personas.CreateAsync(entrada);

        if (!resultado.IsSuccess)
        {
            return Traducir(resultado);
        }

        return CreatedAtAction(nameof(GetById), new { cc = resultado.Value!.Cc }, resultado.Value);
    }

    /// <summary>Actualiza una persona; la cédula no se puede cambiar.</summary>
    /// <param name="cc">Cédula de la persona (clave).</param>
    /// <param name="entrada">Datos completos de la persona, con la misma cédula de la ruta.</param>
    /// <response code="204">Persona actualizada.</response>
    /// <response code="400">Datos inválidos o la cédula del cuerpo no coincide con la de la ruta.</response>
    /// <response code="404">No existe una persona con esa cédula.</response>
    [HttpPut("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int cc, [FromBody] PersonaEntradaDto entrada) =>
        Traducir(await _personas.UpdateAsync(cc, entrada));

    /// <summary>Elimina una persona.</summary>
    /// <param name="cc">Cédula de la persona (clave).</param>
    /// <response code="204">Persona eliminada.</response>
    /// <response code="404">No existe una persona con esa cédula.</response>
    /// <response code="409">La persona tiene teléfonos o estudios asociados; no se elimina.</response>
    [HttpDelete("{cc:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
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
