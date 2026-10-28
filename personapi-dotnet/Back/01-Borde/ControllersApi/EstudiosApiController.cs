using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Canal JSON de estudios (SDD §5). Detalle, edición y baja se identifican con los dos
/// segmentos de la PK compuesta, <c>{idProf}/{ccPer}</c>, nunca con un id artificial.
/// </summary>
[ApiController]
[Route("api/estudios")]
[Produces("application/json")]
public class EstudiosApiController : ControllerBase
{
    private readonly IEstudioService _estudios;

    public EstudiosApiController(IEstudioService estudios) => _estudios = estudios;

    /// <summary>Listado completo; sin filas devuelve <c>[]</c>.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EstudioDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EstudioDto>>> GetAll() =>
        Ok(await _estudios.GetAllAsync());

    /// <summary>Detalle por el par (profesión, persona).</summary>
    [HttpGet("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType<EstudioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int idProf, int ccPer) =>
        Traducir(await _estudios.GetByIdAsync(idProf, ccPer));

    /// <summary>Alta. Con profesión o persona inexistente responde <c>400</c>.</summary>
    [HttpPost]
    [ProducesResponseType<EstudioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EstudioEntradaDto entrada)
    {
        var resultado = await _estudios.CreateAsync(entrada);

        if (!resultado.IsSuccess)
        {
            return Traducir(resultado);
        }

        var creado = resultado.Value!;
        return CreatedAtAction(nameof(GetById), new { idProf = creado.IdProf, ccPer = creado.CcPer }, creado);
    }

    /// <summary>Edición de fecha y universidad; el par de la ruta no cambia.</summary>
    [HttpPut("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int idProf, int ccPer, [FromBody] EstudioEntradaDto entrada) =>
        Traducir(await _estudios.UpdateAsync(idProf, ccPer, entrada));

    /// <summary>Baja.</summary>
    [HttpDelete("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idProf, int ccPer) =>
        Traducir(await _estudios.DeleteAsync(idProf, ccPer));

    // Mismo mapeo que en personas, profesiones y teléfonos: los errores salen como
    // ProblemDetails, la forma que ya produce [ApiController] cuando el cuerpo no valida.
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
