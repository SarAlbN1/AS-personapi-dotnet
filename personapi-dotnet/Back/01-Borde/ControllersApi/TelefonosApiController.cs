using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Canal JSON de teléfonos (SDD §5). La clave viaja como cadena en la ruta, sin
/// restricción numérica, para no perder los ceros iniciales de <c>num</c>.
/// </summary>
[ApiController]
[Route("api/telefonos")]
[Produces("application/json")]
public class TelefonosApiController : ControllerBase
{
    private readonly ITelefonoService _telefonos;

    public TelefonosApiController(ITelefonoService telefonos) => _telefonos = telefonos;

    /// <summary>Lista todos los teléfonos.</summary>
    /// <response code="200">Los teléfonos registrados; sin filas, <c>[]</c>.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TelefonoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TelefonoDto>>> GetAll() =>
        Ok(await _telefonos.GetAllAsync());

    /// <summary>Consulta un teléfono por su número.</summary>
    /// <param name="num">Número del teléfono, como cadena y con sus ceros iniciales.</param>
    /// <response code="200">El teléfono encontrado.</response>
    /// <response code="404">No existe un teléfono con ese número.</response>
    [HttpGet("{num}")]
    [ProducesResponseType<TelefonoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string num) =>
        Traducir(await _telefonos.GetByIdAsync(num));

    /// <summary>Registra un teléfono a nombre de una persona existente.</summary>
    /// <param name="entrada">Número, operador y cédula del dueño.</param>
    /// <response code="201">Teléfono creado; <c>Location</c> apunta a <c>/api/telefonos/{num}</c>.</response>
    /// <response code="400">Falta el número, el operador o el dueño, el número tiene caracteres que no son dígitos o supera los 15, el operador supera los 45, o el dueño no existe.</response>
    /// <response code="409">Ya existe un teléfono con ese número.</response>
    [HttpPost]
    [ProducesResponseType<TelefonoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TelefonoEntradaDto entrada)
    {
        var resultado = await _telefonos.CreateAsync(entrada);

        if (!resultado.IsSuccess)
        {
            return Traducir(resultado);
        }

        return CreatedAtAction(nameof(GetById), new { num = resultado.Value!.Num }, resultado.Value);
    }

    /// <summary>Actualiza el operador y el dueño de un teléfono; el número no se puede cambiar.</summary>
    /// <param name="num">Número del teléfono, como cadena y con sus ceros iniciales.</param>
    /// <param name="entrada">Datos completos del teléfono, con el mismo número de la ruta.</param>
    /// <response code="204">Teléfono actualizado.</response>
    /// <response code="400">Datos inválidos, el dueño no existe o el número del cuerpo no coincide exactamente con el de la ruta.</response>
    /// <response code="404">No existe un teléfono con ese número.</response>
    [HttpPut("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string num, [FromBody] TelefonoEntradaDto entrada) =>
        Traducir(await _telefonos.UpdateAsync(num, entrada));

    /// <summary>Elimina un teléfono. Nada depende de un teléfono, así que no hay conflicto al borrarlo.</summary>
    /// <param name="num">Número del teléfono, como cadena y con sus ceros iniciales.</param>
    /// <response code="204">Teléfono eliminado.</response>
    /// <response code="404">No existe un teléfono con ese número.</response>
    [HttpDelete("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string num) =>
        Traducir(await _telefonos.DeleteAsync(num));

    // Mismo mapeo que en personas, profesiones y estudios: los errores salen como ProblemDetails,
    // la forma que ya produce [ApiController] cuando el cuerpo no valida.
    private IActionResult Traducir(ServiceResult resultado) => resultado.Status switch
    {
        ServiceResultStatus.Success => NoContent(),
        ServiceResultStatus.NotFound => Problem(detail: resultado.Message, statusCode: StatusCodes.Status404NotFound),
        ServiceResultStatus.ValidationError => Problem(detail: resultado.Message, statusCode: StatusCodes.Status400BadRequest),
        ServiceResultStatus.Conflict => Problem(detail: resultado.Message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };

    private IActionResult Traducir<T>(ServiceResult<T> resultado) =>
        resultado.IsSuccess ? Ok(resultado.Value) : Traducir((ServiceResult)resultado);
}
