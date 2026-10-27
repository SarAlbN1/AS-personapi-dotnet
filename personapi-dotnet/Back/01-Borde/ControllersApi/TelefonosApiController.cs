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

    /// <summary>Listado completo; sin filas devuelve <c>[]</c>.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TelefonoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TelefonoDto>>> GetAll() =>
        Ok(await _telefonos.GetAllAsync());

    /// <summary>Detalle por número.</summary>
    [HttpGet("{num}")]
    [ProducesResponseType<TelefonoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string num) =>
        Traducir(await _telefonos.GetByIdAsync(num));

    /// <summary>Alta. Con un dueño inexistente responde <c>400</c>.</summary>
    [HttpPost]
    [ProducesResponseType<TelefonoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
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

    /// <summary>Edición de operador y dueño; el número de la ruta manda.</summary>
    [HttpPut("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string num, [FromBody] TelefonoEntradaDto entrada) =>
        Traducir(await _telefonos.UpdateAsync(num, entrada));

    /// <summary>Baja.</summary>
    [HttpDelete("{num}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string num) =>
        Traducir(await _telefonos.DeleteAsync(num));

    // Mismo mapeo que en personas y profesiones: los errores salen como ProblemDetails,
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
